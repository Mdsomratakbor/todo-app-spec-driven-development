using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models.DTOs.Categories;

public class CategoryRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}
