using Cartographer.Core.Abstractions;
using FluentResponse.Exceptions;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Extensions;
using TodoApi.Models.DTOs.Todos;
using TodoApi.Models.Entities;
using TodoApi.Models.Enums;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TodoService> _logger;

    public TodoService(AppDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<TodoService> logger)
    {
        _db = db;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private string CurrentUserId =>
        _httpContextAccessor.HttpContext?.User.GetUserId() ?? string.Empty;

    public async Task<TodoResponse> CreateAsync(TodoRequest request)
    {
        var userId = CurrentUserId;

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);

            if (!categoryExists)
            {
                _logger.LogWarning("User {UserId} attempted to create todo with non-existent category {CategoryId}", userId, request.CategoryId);
                throw new NotFoundException("Category not found");
            }
        }

        var priority = TodoPriority.Medium;
        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            if (!Enum.TryParse<TodoPriority>(request.Priority, ignoreCase: true, out var parsed))
                throw new BusinessException("Priority must be one of: low, medium, high");
            priority = parsed;
        }

        var now = DateTime.UtcNow;
        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            Priority = priority,
            DueDate = request.DueDate,
            CategoryId = request.CategoryId,
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();

        await _db.Entry(todo).Reference(t => t.Category).LoadAsync();

        _logger.LogInformation("User {UserId} created todo {TodoId}: {TodoTitle}", userId, todo.Id, todo.Title);

        return _mapper.Map<TodoResponse>(todo);
    }

    public async Task<TodoResponse> GetByIdAsync(Guid id)
    {
        var userId = CurrentUserId;

        var todo = await _db.Todos
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            _logger.LogWarning("User {UserId} requested non-existent todo {TodoId}", userId, id);
            throw new NotFoundException("Todo not found");
        }

        return _mapper.Map<TodoResponse>(todo);
    }

    public async Task<List<TodoResponse>> GetAllAsync()
    {
        var userId = CurrentUserId;

        var todos = await _db.Todos
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("User {UserId} listed {Count} todos", userId, todos.Count);

        return todos.Select(t => _mapper.Map<TodoResponse>(t)).ToList();
    }

    public async Task<List<TodoResponse>> GetAllAsync(TodoFilterRequest filter)
    {
        var userId = CurrentUserId;

        var query = _db.Todos
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (filter.IsCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.DueBefore.HasValue)
            query = query.Where(t => t.DueDate <= filter.DueBefore.Value);

        if (filter.DueAfter.HasValue)
            query = query.Where(t => t.DueDate >= filter.DueAfter.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(search) ||
                (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Priority))
        {
            if (Enum.TryParse<TodoPriority>(filter.Priority, ignoreCase: true, out var priority))
                query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(filter.SortBy) &&
            filter.SortBy.Equals("priority", StringComparison.OrdinalIgnoreCase))
        {
            var descending = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            query = descending
                ? query.OrderByDescending(t => t.Priority)
                : query.OrderBy(t => t.Priority);
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedAt);
        }

        var todos = await query.ToListAsync();

        _logger.LogInformation("User {UserId} searched todos with filters - completed:{IsCompleted} category:{CategoryId} priority:{Priority} search:{Search} -> {Count} results",
            userId, filter.IsCompleted, filter.CategoryId, filter.Priority, filter.Search, todos.Count);

        return todos.Select(t => _mapper.Map<TodoResponse>(t)).ToList();
    }

    public async Task<TodoResponse> UpdateAsync(Guid id, TodoUpdateRequest request)
    {
        var userId = CurrentUserId;

        var todo = await _db.Todos
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            _logger.LogWarning("User {UserId} attempted to update non-existent todo {TodoId}", userId, id);
            throw new NotFoundException("Todo not found");
        }

        if (request.Title != null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BusinessException("Title is required");
            }
            todo.Title = request.Title;
        }

        if (request.Description != null)
            todo.Description = request.Description;

        if (request.DueDate != null)
            todo.DueDate = request.DueDate;

        if (request.IsCompleted.HasValue)
            todo.IsCompleted = request.IsCompleted.Value;

        if (request.Priority != null)
        {
            if (!Enum.TryParse<TodoPriority>(request.Priority, ignoreCase: true, out var priority))
                throw new BusinessException("Priority must be one of: low, medium, high");
            todo.Priority = priority;
        }

        if (request.CategoryId != null)
        {
            if (request.CategoryId.Value != Guid.Empty)
            {
                var categoryExists = await _db.Categories
                    .AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);

                if (!categoryExists)
                {
                    _logger.LogWarning("User {UserId} attempted to update todo {TodoId} with non-existent category {CategoryId}", userId, id, request.CategoryId);
                    throw new NotFoundException("Category not found");
                }
            }
            todo.CategoryId = request.CategoryId.Value == Guid.Empty ? null : request.CategoryId;
        }

        todo.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _db.Entry(todo).Reference(t => t.Category).LoadAsync();

        _logger.LogInformation("User {UserId} updated todo {TodoId}", userId, id);

        return _mapper.Map<TodoResponse>(todo);
    }

    public async Task DeleteAsync(Guid id)
    {
        var userId = CurrentUserId;

        var todo = await _db.Todos
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            _logger.LogWarning("User {UserId} attempted to delete non-existent todo {TodoId}", userId, id);
            throw new NotFoundException("Todo not found");
        }

        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deleted todo {TodoId}", userId, id);
    }
}
