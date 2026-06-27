using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models.DTOs.Todos;

public class TodoRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid? CategoryId { get; set; }
}
