using FluentResponse.Models;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models.DTOs.Auth;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        var response = new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = result,
            StatusCode = 201
        };

        return CreatedAtAction(null, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        var response = new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = result,
            StatusCode = 200
        };

        return Ok(response);
    }
}
