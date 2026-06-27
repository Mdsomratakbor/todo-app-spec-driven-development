using TodoApi.Models.Entities;

namespace TodoApi.Tests.Entities;

public class CategoryTests
{
    [Fact]
    public void Create_Category_HasDefaultValues()
    {
        var category = new Category
        {
            Name = "Work",
            UserId = "user-1"
        };

        Assert.Equal("Work", category.Name);
        Assert.Equal("user-1", category.UserId);
        Assert.Equal(Guid.Empty, category.Id);
        Assert.Equal(default, category.CreatedAt);
    }

    [Fact]
    public void Create_Category_WithSpecificId()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 6, 27, 10, 0, 0, DateTimeKind.Utc);

        var category = new Category
        {
            Id = id,
            Name = "Personal",
            UserId = "user-1",
            CreatedAt = createdAt
        };

        Assert.Equal(id, category.Id);
        Assert.Equal("Personal", category.Name);
        Assert.Equal("user-1", category.UserId);
        Assert.Equal(createdAt, category.CreatedAt);
    }

    [Fact]
    public void Category_Name_MaxLength()
    {
        var name = new string('a', 50);
        var category = new Category { Name = name, UserId = "user-1" };
        Assert.Equal(50, category.Name.Length);
    }
}
