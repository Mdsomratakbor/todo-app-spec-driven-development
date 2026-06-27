using System.Security.Claims;
using TodoApi.Extensions;

namespace TodoApi.Tests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_WithUser_ReturnsId()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        }));

        var result = principal.GetUserId();

        Assert.Equal("user-123", result);
    }

    [Fact]
    public void GetUserId_WithoutIdentity_ReturnsEmptyString()
    {
        var principal = new ClaimsPrincipal();

        var result = principal.GetUserId();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetUserId_WithoutNameIdentifierClaim_ReturnsEmptyString()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        }));

        var result = principal.GetUserId();

        Assert.Equal(string.Empty, result);
    }
}
