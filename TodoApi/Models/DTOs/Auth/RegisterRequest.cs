using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 256 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}
