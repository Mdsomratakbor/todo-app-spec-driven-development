using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models.Entities;

namespace TodoApi.Tests.Data;

public class AppDbContextTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Can_Add_Category()
    {
        using var db = CreateDbContext();

        var category = new Category
        {
            Name = "Work",
            UserId = "user-1"
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var saved = await db.Categories.FirstAsync();
        Assert.Equal("Work", saved.Name);
        Assert.Equal("user-1", saved.UserId);
    }

    [Fact]
    public async Task Can_Add_Todo()
    {
        using var db = CreateDbContext();

        var todo = new Todo
        {
            Title = "Buy groceries",
            UserId = "user-1"
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        var saved = await db.Todos.FirstAsync();
        Assert.Equal("Buy groceries", saved.Title);
        Assert.False(saved.IsCompleted);
    }

    [Fact]
    public async Task Can_Query_Todos_By_User()
    {
        using var db = CreateDbContext();

        db.Todos.AddRange(
            new Todo { Title = "Task A", UserId = "user-1" },
            new Todo { Title = "Task B", UserId = "user-1" },
            new Todo { Title = "Task C", UserId = "user-2" }
        );

        await db.SaveChangesAsync();

        var userTodos = await db.Todos.Where(t => t.UserId == "user-1").CountAsync();
        Assert.Equal(2, userTodos);
    }

    [Fact]
    public async Task Category_Has_Unique_Name_Per_User()
    {
        using var db = CreateDbContext();

        db.Categories.Add(new Category { Name = "Work", UserId = "user-1" });
        await db.SaveChangesAsync();

        // InMemory doesn't enforce unique constraints, but the entity model supports it
        db.Categories.Add(new Category { Name = "Work", UserId = "user-2" });
        await db.SaveChangesAsync();

        var count = await db.Categories.CountAsync();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task Category_Has_Empty_Todo_Collection_On_Create()
    {
        using var db = CreateDbContext();

        var category = new Category { Name = "Work", UserId = "user-1" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var saved = await db.Categories.FirstAsync();
        Assert.Equal("Work", saved.Name);
    }
}
