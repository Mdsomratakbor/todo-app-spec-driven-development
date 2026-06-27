using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentResponse.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models.DTOs.Auth;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
        {
            throw new ConflictException("Username is already taken");
        }

        var user = new IdentityUser
        {
            UserName = request.Username,
            Email = request.Username
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BusinessException(errors);
        }

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Invalid username or password");
        }

        return GenerateAuthResponse(user);
    }

    private AuthResponse GenerateAuthResponse(IdentityUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var expiryHours = int.Parse(jwtSection["ExpiryInHours"] ?? "24");
        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = user.UserName!,
            ExpiresAt = expiresAt
        };
    }
}
