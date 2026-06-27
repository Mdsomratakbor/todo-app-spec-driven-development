using FluentResponse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models.DTOs.Todos;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/todos")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly ILogger<TodoController> _logger;

    public TodoController(ITodoService todoService, ILogger<TodoController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoRequest request)
    {
        _logger.LogInformation("Create todo request: {TodoTitle}", request.Title);
        var result = await _todoService.CreateAsync(request);

        var response = new ApiResponse<TodoResponse>
        {
            Success = true,
            Data = result,
            StatusCode = 201
        };

        return CreatedAtAction(null, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TodoFilterRequest filter)
    {
        var hasFilters = filter.IsCompleted.HasValue
            || filter.CategoryId.HasValue
            || filter.DueBefore.HasValue
            || filter.DueAfter.HasValue
            || !string.IsNullOrWhiteSpace(filter.Search);

        _logger.LogDebug("List todos request (filters: {HasFilters})", hasFilters);

        var result = hasFilters
            ? await _todoService.GetAllAsync(filter)
            : await _todoService.GetAllAsync();

        var response = new ApiResponse<List<TodoResponse>>
        {
            Success = true,
            Data = result,
            StatusCode = 200
        };

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogDebug("Get todo by id: {TodoId}", id);
        var result = await _todoService.GetByIdAsync(id);

        var response = new ApiResponse<TodoResponse>
        {
            Success = true,
            Data = result,
            StatusCode = 200
        };

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TodoUpdateRequest request)
    {
        _logger.LogInformation("Update todo request for {TodoId}", id);
        var result = await _todoService.UpdateAsync(id, request);

        var response = new ApiResponse<TodoResponse>
        {
            Success = true,
            Data = result,
            StatusCode = 200
        };

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Delete todo request for {TodoId}", id);
        await _todoService.DeleteAsync(id);
        return NoContent();
    }
}
