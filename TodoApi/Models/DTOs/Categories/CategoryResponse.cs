namespace TodoApi.Models.DTOs.Categories;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TodoCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
