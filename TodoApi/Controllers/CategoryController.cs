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

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
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
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
