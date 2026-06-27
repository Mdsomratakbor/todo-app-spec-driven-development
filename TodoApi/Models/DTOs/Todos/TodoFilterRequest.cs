namespace TodoApi.Models.DTOs.Todos;

public class TodoFilterRequest
{
    public bool? IsCompleted { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? DueBefore { get; set; }
    public DateTime? DueAfter { get; set; }
    public string? Search { get; set; }
    public string? Priority { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
