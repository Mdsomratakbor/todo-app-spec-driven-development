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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Register request received for username {Username}", request.Username);
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
        _logger.LogInformation("Login request received for username {Username}", request.Username);
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
