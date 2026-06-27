using TodoApi.Models.DTOs.Todos;

namespace TodoApi.Services.Interfaces;

public interface ITodoService
{
    Task<TodoResponse> CreateAsync(TodoRequest request);
    Task<TodoResponse> GetByIdAsync(Guid id);
    Task<List<TodoResponse>> GetAllAsync();
    Task<List<TodoResponse>> GetAllAsync(TodoFilterRequest filter);
    Task<TodoResponse> UpdateAsync(Guid id, TodoUpdateRequest request);
    Task DeleteAsync(Guid id);
}
