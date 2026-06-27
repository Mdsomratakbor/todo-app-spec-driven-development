using Microsoft.AspNetCore.Identity;
using TodoApi.Models.Entities;

namespace TodoApi.Tests.Entities;

public class TodoTests
{
    [Fact]
    public void Create_Todo_HasDefaultValues()
    {
        var todo = new Todo
        {
            Title = "Buy groceries",
            UserId = "user-1"
        };

        Assert.Equal("Buy groceries", todo.Title);
        Assert.Equal("user-1", todo.UserId);
        Assert.False(todo.IsCompleted);
        Assert.Null(todo.Description);
        Assert.Null(todo.DueDate);
        Assert.Null(todo.CategoryId);
        Assert.Equal(Guid.Empty, todo.Id);
        Assert.Equal(default, todo.CreatedAt);
        Assert.Equal(default, todo.UpdatedAt);
    }

    [Fact]
    public void Create_Todo_WithAllFields()
    {
        var categoryId = Guid.NewGuid();
        var dueDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var todo = new Todo
        {
            Title = "Complete project",
            Description = "Finish the report",
            IsCompleted = false,
            DueDate = dueDate,
            UserId = "user-1",
            CategoryId = categoryId
        };

        Assert.Equal("Complete project", todo.Title);
        Assert.Equal("Finish the report", todo.Description);
        Assert.False(todo.IsCompleted);
        Assert.Equal(dueDate, todo.DueDate);
        Assert.Equal(categoryId, todo.CategoryId);
    }

    [Fact]
    public void Todo_Title_MaxLength()
    {
        var title = new string('a', 200);
        var todo = new Todo { Title = title, UserId = "user-1" };
        Assert.Equal(200, todo.Title.Length);
    }

    [Fact]
    public void Todo_Description_MaxLength()
    {
        var description = new string('a', 2000);
        var todo = new Todo { Title = "Test", Description = description, UserId = "user-1" };
        Assert.Equal(2000, todo.Description.Length);
    }

    [Fact]
    public void Todo_HasNavigationProperties()
    {
        var todo = new Todo
        {
            Title = "Test",
            UserId = "user-1",
            Category = new Category { Name = "Work", UserId = "user-1" },
            User = new IdentityUser { Id = "user-1", UserName = "testuser" }
        };

        Assert.NotNull(todo.Category);
        Assert.NotNull(todo.User);
        Assert.Equal("Work", todo.Category.Name);
        Assert.Equal("testuser", todo.User.UserName);
    }
}
