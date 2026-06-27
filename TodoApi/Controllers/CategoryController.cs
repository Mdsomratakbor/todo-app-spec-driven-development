using FluentResponse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models.DTOs.Categories;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        _logger.LogInformation("Create category request received: {CategoryName}", request.Name);
        var result = await _categoryService.CreateAsync(request);

        var response = new ApiResponse<CategoryResponse>
        {
            Success = true,
            Data = result,
            StatusCode = 201
        };

        return CreatedAtAction(null, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogDebug("List categories request received");
        var result = await _categoryService.GetAllAsync();

        var response = new ApiResponse<List<CategoryResponse>>
        {
            Success = true,
            Data = result,
            StatusCode = 200
        };

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequest request)
    {
        _logger.LogInformation("Update category request for {CategoryId}: {CategoryName}", id, request.Name);
        var result = await _categoryService.UpdateAsync(id, request);

        var response = new ApiResponse<CategoryResponse>
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
        _logger.LogInformation("Delete category request for {CategoryId}", id);
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
