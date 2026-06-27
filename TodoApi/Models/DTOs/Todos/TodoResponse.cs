namespace TodoApi.Models.DTOs.Todos;

public class TodoResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Priority { get; set; } = "medium";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
