using Cartographer.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TodoApi.Data;
using TodoApi.Models.DTOs.Todos;
using TodoApi.Models.Entities;
using TodoApi.Models.Enums;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

public class TodoServiceTests
{
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private const string TestUserId = "user-1";

    public TodoServiceTests()
    {
        _mapper = new Mock<IMapper>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId)
        }));

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mapper.Setup(x => x.Map<TodoResponse>(It.IsAny<Todo>()))
            .Returns((Todo t) => new TodoResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                DueDate = t.DueDate,
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name,
                Priority = t.Priority.ToString().ToLower(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_WithTitleOnly_ReturnsTodoResponse()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.CreateAsync(new TodoRequest { Title = "Buy groceries" });

        Assert.NotNull(result);
        Assert.Equal("Buy groceries", result.Title);
        Assert.False(result.IsCompleted);
        Assert.Null(result.Description);
        Assert.Null(result.DueDate);
        Assert.Null(result.CategoryId);
    }

    [Fact]
    public async Task CreateAsync_WithAllFields_ReturnsTodoResponse()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Personal", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.CreateAsync(new TodoRequest
        {
            Title = "Team meeting",
            Description = "Discuss Q3 goals",
            DueDate = DateTime.UtcNow.AddDays(7),
            CategoryId = categoryId
        });

        Assert.Equal("Team meeting", result.Title);
        Assert.Equal("Discuss Q3 goals", result.Description);
        Assert.NotNull(result.DueDate);
        Assert.Equal(categoryId, result.CategoryId);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentCategory_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.CreateAsync(new TodoRequest
            {
                Title = "Test",
                CategoryId = Guid.NewGuid()
            }));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingTodo_ReturnsTodoResponse()
    {
        using var db = CreateDbContext();
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Test",
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetByIdAsync(todoId);

        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_WithOtherUsersTodo_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Secret",
            UserId = "other-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.GetByIdAsync(todoId));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUserTodosOrderedByCreatedAtDesc()
    {
        using var db = CreateDbContext();
        var older = new Todo { Id = Guid.NewGuid(), Title = "Older", UserId = TestUserId, CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow };
        var newer = new Todo { Id = Guid.NewGuid(), Title = "Newer", UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Todos.AddRange(older, newer);
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Title);
        Assert.Equal("Older", result[1].Title);
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsOwnTodos()
    {
        using var db = CreateDbContext();
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Mine", UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Theirs", UserId = "other-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Mine", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_WithFilter_ReturnsFilteredResults()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Work", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Complete report", Description = "Quarterly report", IsCompleted = true, CategoryId = categoryId, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Buy milk", IsCompleted = false, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { IsCompleted = true });

        Assert.Single(result);
        Assert.Equal("Complete report", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersByTitle()
    {
        using var db = CreateDbContext();
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Grocery shopping", UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Team meeting", UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { Search = "grocery" });

        Assert.Single(result);
        Assert.Contains("grocery", result[0].Title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersByDescription()
    {
        using var db = CreateDbContext();
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Task A", Description = "This is about budgeting", UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Task B", UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { Search = "budgeting" });

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllAsync_WithNoResults_ReturnsEmptyList()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { IsCompleted = true });

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesFields()
    {
        using var db = CreateDbContext();
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Old title",
            Description = "Old description",
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.UpdateAsync(todoId, new TodoUpdateRequest
        {
            Title = "New title",
            Description = "New description"
        });

        Assert.Equal("New title", result.Title);
        Assert.Equal("New description", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new TodoUpdateRequest { Title = "Nope" }));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingTodo_Deletes()
    {
        using var db = CreateDbContext();
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Delete me",
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        await service.DeleteAsync(todoId);

        Assert.Empty(db.Todos);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ToggleCompletion_FlipsStatus()
    {
        using var db = CreateDbContext();
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Task",
            IsCompleted = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.UpdateAsync(todoId, new TodoUpdateRequest { IsCompleted = true });

        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task UpdateAsync_AssignCategory_ReturnsCategoryName()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Work", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Task",
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.UpdateAsync(todoId, new TodoUpdateRequest { CategoryId = categoryId });

        Assert.Equal(categoryId, result.CategoryId);
        Assert.Equal("Work", result.CategoryName);
    }

    [Fact]
    public async Task UpdateAsync_RemoveCategory_SetsCategoryIdNull()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Work", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Task",
            CategoryId = categoryId,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.UpdateAsync(todoId, new TodoUpdateRequest { CategoryId = Guid.Empty });

        Assert.Null(result.CategoryId);
        Assert.Null(result.CategoryName);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidCategory_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Task",
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.UpdateAsync(todoId, new TodoUpdateRequest { CategoryId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task GetAllAsync_FilterByCategory_ReturnsFiltered()
    {
        using var db = CreateDbContext();
        var catA = Guid.NewGuid();
        var catB = Guid.NewGuid();
        db.Categories.Add(new Category { Id = catA, Name = "A", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        db.Categories.Add(new Category { Id = catB, Name = "B", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "In A", CategoryId = catA, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "In B", CategoryId = catB, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { CategoryId = catA });

        Assert.Single(result);
        Assert.Equal("In A", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_FilterByDueDateRange_ReturnsFiltered()
    {
        using var db = CreateDbContext();
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Past", DueDate = DateTime.UtcNow.AddDays(-5), UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Future", DueDate = DateTime.UtcNow.AddDays(5), UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "No due", UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest
        {
            DueAfter = DateTime.UtcNow.AddDays(-1),
            DueBefore = DateTime.UtcNow.AddDays(10)
        });

        Assert.Single(result);
        Assert.Equal("Future", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_WithCombinedFilters_AppliesAndLogic()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Work", UserId = TestUserId, CreatedAt = DateTime.UtcNow });

        db.Todos.Add(new Todo
        {
            Id = Guid.NewGuid(),
            Title = "Meeting notes",
            Description = "Team standup",
            IsCompleted = false,
            CategoryId = categoryId,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.Todos.Add(new Todo
        {
            Id = Guid.NewGuid(),
            Title = "Complete report",
            IsCompleted = true,
            CategoryId = categoryId,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.Todos.Add(new Todo
        {
            Id = Guid.NewGuid(),
            Title = "Buy groceries",
            IsCompleted = false,
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest
        {
            IsCompleted = false,
            CategoryId = categoryId,
            Search = "meeting"
        });

        Assert.Single(result);
        Assert.Equal("Meeting notes", result[0].Title);
    }

    [Fact]
    public async Task CreateAsync_DefaultPriority_IsMedium()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.CreateAsync(new TodoRequest { Title = "Test" });

        Assert.Equal("medium", result.Priority);
    }

    [Fact]
    public async Task CreateAsync_WithExplicitPriority_SetsPriority()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.CreateAsync(new TodoRequest { Title = "High priority", Priority = "high" });

        Assert.Equal("high", result.Priority);
    }

    [Fact]
    public async Task UpdateAsync_ChangesPriority()
    {
        using var db = CreateDbContext();
        var todoId = Guid.NewGuid();
        db.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Task",
            UserId = TestUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.UpdateAsync(todoId, new TodoUpdateRequest { Priority = "low" });

        Assert.Equal("low", result.Priority);
    }

    [Fact]
    public async Task GetAllAsync_FilterByPriority_ReturnsFiltered()
    {
        using var db = CreateDbContext();
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "High task", Priority = TodoPriority.High, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Low task", Priority = TodoPriority.Low, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { Priority = "high" });

        Assert.Single(result);
        Assert.Equal("High task", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_SortByPriorityAsc_ReturnsSorted()
    {
        using var db = CreateDbContext();
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Medium", Priority = TodoPriority.Medium, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "High", Priority = TodoPriority.High, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Low", Priority = TodoPriority.Low, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { SortBy = "priority", SortDirection = "asc" });

        Assert.Equal(3, result.Count);
        Assert.Equal("Low", result[0].Title);
        Assert.Equal("Medium", result[1].Title);
        Assert.Equal("High", result[2].Title);
    }

    [Fact]
    public async Task GetAllAsync_SortByPriorityDesc_ReturnsSorted()
    {
        using var db = CreateDbContext();
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Medium", Priority = TodoPriority.Medium, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "High", Priority = TodoPriority.High, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = "Low", Priority = TodoPriority.Low, UserId = TestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<TodoService>>());

        var result = await service.GetAllAsync(new TodoFilterRequest { SortBy = "priority", SortDirection = "desc" });

        Assert.Equal(3, result.Count);
        Assert.Equal("High", result[0].Title);
        Assert.Equal("Medium", result[1].Title);
        Assert.Equal("Low", result[2].Title);
    }
}
