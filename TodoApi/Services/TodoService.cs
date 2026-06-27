using Cartographer.Core.Abstractions;
using FluentResponse.Exceptions;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Extensions;
using TodoApi.Models.DTOs.Todos;
using TodoApi.Models.Entities;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TodoService(AppDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
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
                throw new NotFoundException("Category not found");
            }
        }

        var now = DateTime.UtcNow;
        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            DueDate = request.DueDate,
            CategoryId = request.CategoryId,
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();

        await _db.Entry(todo).Reference(t => t.Category).LoadAsync();

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
        {
            query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

        if (filter.DueBefore.HasValue)
        {
            query = query.Where(t => t.DueDate <= filter.DueBefore.Value);
        }

        if (filter.DueAfter.HasValue)
        {
            query = query.Where(t => t.DueDate >= filter.DueAfter.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(search) ||
                (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        var todos = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

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
        {
            todo.Description = request.Description;
        }

        if (request.DueDate != null)
        {
            todo.DueDate = request.DueDate;
        }

        if (request.IsCompleted.HasValue)
        {
            todo.IsCompleted = request.IsCompleted.Value;
        }

        if (request.CategoryId != null)
        {
            if (request.CategoryId.Value != Guid.Empty)
            {
                var categoryExists = await _db.Categories
                    .AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);

                if (!categoryExists)
                {
                    throw new NotFoundException("Category not found");
                }
            }
            todo.CategoryId = request.CategoryId.Value == Guid.Empty ? null : request.CategoryId;
        }

        todo.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _db.Entry(todo).Reference(t => t.Category).LoadAsync();

        return _mapper.Map<TodoResponse>(todo);
    }

    public async Task DeleteAsync(Guid id)
    {
        var userId = CurrentUserId;

        var todo = await _db.Todos
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            throw new NotFoundException("Todo not found");
        }

        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync();
    }
}
