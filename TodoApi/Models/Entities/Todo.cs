using Microsoft.AspNetCore.Identity;

namespace TodoApi.Models.Entities;

public class Todo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }

    public Category? Category { get; set; }
    public IdentityUser? User { get; set; }
}
