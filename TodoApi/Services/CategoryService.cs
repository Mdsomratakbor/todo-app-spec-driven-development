using Cartographer.Core.Abstractions;
using FluentResponse.Exceptions;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Extensions;
using TodoApi.Models.DTOs.Categories;
using TodoApi.Models.Entities;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<CategoryService> logger)
    {
        _db = db;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private string CurrentUserId =>
        _httpContextAccessor.HttpContext?.User.GetUserId() ?? string.Empty;

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request)
    {
        var userId = CurrentUserId;

        var existing = await _db.Categories
            .AnyAsync(c => c.UserId == userId && c.Name.ToLower() == request.Name.ToLower());

        if (existing)
        {
            _logger.LogWarning("User {UserId} attempted to create duplicate category {CategoryName}", userId, request.Name);
            throw new ConflictException("A category with this name already exists");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {UserId} created category {CategoryId} ({CategoryName})", userId, category.Id, category.Name);

        var response = _mapper.Map<CategoryResponse>(category);
        response.TodoCount = 0;

        return response;
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request)
    {
        var userId = CurrentUserId;
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            _logger.LogWarning("User {UserId} attempted to update non-existent category {CategoryId}", userId, id);
            throw new NotFoundException("Category not found");
        }

        var duplicate = await _db.Categories
            .AnyAsync(c => c.Id != id && c.UserId == userId && c.Name.ToLower() == request.Name.ToLower());

        if (duplicate)
        {
            _logger.LogWarning("User {UserId} attempted to rename category {CategoryId} to duplicate name {CategoryName}", userId, id, request.Name);
            throw new ConflictException("A category with this name already exists");
        }

        category.Name = request.Name;
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated category {CategoryId} to {CategoryName}", userId, id, category.Name);

        var response = _mapper.Map<CategoryResponse>(category);
        response.TodoCount = await _db.Todos.CountAsync(t => t.CategoryId == id && t.UserId == userId);

        return response;
    }

    public async Task DeleteAsync(Guid id)
    {
        var userId = CurrentUserId;
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            _logger.LogWarning("User {UserId} attempted to delete non-existent category {CategoryId}", userId, id);
            throw new NotFoundException("Category not found");
        }

        var todoCount = await _db.Todos.CountAsync(t => t.CategoryId == id && t.UserId == userId);
        if (todoCount > 0)
        {
            _logger.LogWarning("User {UserId} attempted to delete category {CategoryId} with {TodoCount} associated todos", userId, id, todoCount);
            throw new ConflictException($"Category has {todoCount} associated todo(s). Remove or reassign them before deleting.");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deleted category {CategoryId}", userId, id);
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var userId = CurrentUserId;

        var categories = await _db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var todoCounts = await _db.Todos
            .Where(t => t.UserId == userId && t.CategoryId != null)
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CategoryId, g => g.Count);

        var responses = categories.Select(c =>
        {
            var r = _mapper.Map<CategoryResponse>(c);
            r.TodoCount = todoCounts.GetValueOrDefault(c.Id, 0);
            return r;
        }).ToList();

        _logger.LogInformation("User {UserId} listed {Count} categories", userId, responses.Count);

        return responses;
    }
}
