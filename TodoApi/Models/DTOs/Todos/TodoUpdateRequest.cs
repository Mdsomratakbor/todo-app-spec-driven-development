using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models.DTOs.Todos;

public class TodoUpdateRequest
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public bool? IsCompleted { get; set; }

    public Guid? CategoryId { get; set; }

    public string? Priority { get; set; }
}
