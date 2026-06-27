using Cartographer.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TodoApi.Data;
using TodoApi.Extensions;
using TodoApi.Models.DTOs.Categories;
using TodoApi.Models.Entities;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private const string TestUserId = "user-1";

    public CategoryServiceTests()
    {
        _mapper = new Mock<IMapper>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId)
        }));

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mapper.Setup(x => x.Map<CategoryResponse>(It.IsAny<Category>()))
            .Returns((Category c) => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                TodoCount = 0
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
    public async Task CreateAsync_WithValidRequest_ReturnsCategoryResponse()
    {
        using var db = CreateDbContext();
        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var result = await service.CreateAsync(new CategoryRequest { Name = "Work" });

        Assert.NotNull(result);
        Assert.Equal("Work", result.Name);
        Assert.Equal(0, result.TodoCount);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsConflictException()
    {
        using var db = CreateDbContext();
        db.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Work", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var exception = await Assert.ThrowsAsync<FluentResponse.Exceptions.ConflictException>(() =>
            service.CreateAsync(new CategoryRequest { Name = "work" }));

        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAllAsync_WithCategories_ReturnsOrderedList()
    {
        using var db = CreateDbContext();
        db.Categories.AddRange(
            new Category { Id = Guid.NewGuid(), Name = "Zeta", UserId = TestUserId, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Alpha", UserId = TestUserId, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha", result[0].Name);
        Assert.Equal("Zeta", result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_WithNoCategories_ReturnsEmptyList()
    {
        using var db = CreateDbContext();
        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsOwnCategories()
    {
        using var db = CreateDbContext();
        db.Categories.AddRange(
            new Category { Id = Guid.NewGuid(), Name = "Mine", UserId = TestUserId, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Theirs", UserId = "other-user", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Mine", result[0].Name);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsUpdatedCategory()
    {
        using var db = CreateDbContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Old", UserId = TestUserId, CreatedAt = DateTime.UtcNow };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var result = await service.UpdateAsync(category.Id, new CategoryRequest { Name = "Renamed" });

        Assert.Equal("Renamed", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ThrowsConflictException()
    {
        using var db = CreateDbContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Work", UserId = TestUserId, CreatedAt = DateTime.UtcNow };
        db.Categories.Add(category);
        db.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Personal", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var exception = await Assert.ThrowsAsync<FluentResponse.Exceptions.ConflictException>(() =>
            service.UpdateAsync(category.Id, new CategoryRequest { Name = "personal" }));

        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new CategoryRequest { Name = "Nope" }));
    }

    [Fact]
    public async Task DeleteAsync_WithNoTodos_DeletesCategory()
    {
        using var db = CreateDbContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "DeleteMe", UserId = TestUserId, CreatedAt = DateTime.UtcNow };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        await service.DeleteAsync(category.Id);

        Assert.Empty(db.Categories);
    }

    [Fact]
    public async Task DeleteAsync_WithTodos_ThrowsConflictException()
    {
        using var db = CreateDbContext();
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "HasTodos", UserId = TestUserId, CreatedAt = DateTime.UtcNow });
        db.Todos.Add(new Todo
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            UserId = TestUserId,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        var exception = await Assert.ThrowsAsync<FluentResponse.Exceptions.ConflictException>(() =>
            service.DeleteAsync(categoryId));

        Assert.Contains("associated todo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        using var db = CreateDbContext();
        var service = new CategoryService(db, _mapper.Object, _httpContextAccessor.Object, Mock.Of<ILogger<CategoryService>>());

        await Assert.ThrowsAsync<FluentResponse.Exceptions.NotFoundException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
    }
}
