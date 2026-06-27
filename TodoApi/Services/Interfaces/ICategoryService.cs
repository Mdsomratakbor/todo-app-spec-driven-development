using TodoApi.Models.DTOs.Categories;

namespace TodoApi.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse> CreateAsync(CategoryRequest request);
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request);
    Task DeleteAsync(Guid id);
}
