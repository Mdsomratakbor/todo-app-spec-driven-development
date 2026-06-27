using System.Security.Claims;

namespace TodoApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
