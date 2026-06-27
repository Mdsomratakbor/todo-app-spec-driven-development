using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TodoApi.Models.DTOs.Auth;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        _userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsADevelopmentSigningKeyThatIsAtLeast32CharactersLong!",
            ["Jwt:Issuer"] = "TodoApi",
            ["Jwt:Audience"] = "TodoApp",
            ["Jwt:ExpiryInHours"] = "24"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_WithUniqueUsername_ReturnsAuthResponse()
    {
        _userManager.Setup(x => x.FindByNameAsync("newuser"))
            .ReturnsAsync((IdentityUser?)null);

        _userManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), "password123"))
            .ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_userManager.Object, _configuration);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Username = "newuser",
            Password = "password123"
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("newuser", result.Username);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ThrowsInvalidOperationException()
    {
        var existingUser = new IdentityUser { UserName = "existing", Id = "id-1" };
        _userManager.Setup(x => x.FindByNameAsync("existing"))
            .ReturnsAsync(existingUser);

        var service = new AuthService(_userManager.Object, _configuration);

        var exception = await Assert.ThrowsAsync<FluentResponse.Exceptions.ConflictException>(() =>
            service.RegisterAsync(new RegisterRequest
            {
                Username = "existing",
                Password = "password123"
            }));

        Assert.Contains("already taken", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithFailedCreation_ThrowsBusinessException()
    {
        _userManager.Setup(x => x.FindByNameAsync("newuser"))
            .ReturnsAsync((IdentityUser?)null);

        _userManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), "short"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Passwords must be at least 6 characters" }));

        var service = new AuthService(_userManager.Object, _configuration);

        var exception = await Assert.ThrowsAsync<FluentResponse.Exceptions.BusinessException>(() =>
            service.RegisterAsync(new RegisterRequest
            {
                Username = "newuser",
                Password = "short"
            }));

        Assert.Contains("at least 6 characters", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        var user = new IdentityUser { UserName = "testuser", Id = "id-1" };
        _userManager.Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.CheckPasswordAsync(user, "correctpassword"))
            .ReturnsAsync(true);

        var service = new AuthService(_userManager.Object, _configuration);

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "testuser",
            Password = "correctpassword"
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ThrowsUnauthorizedAccessException()
    {
        _userManager.Setup(x => x.FindByNameAsync("unknown"))
            .ReturnsAsync((IdentityUser?)null);

        var service = new AuthService(_userManager.Object, _configuration);

        var exception = await Assert.ThrowsAsync<FluentResponse.Exceptions.UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Username = "unknown",
                Password = "any"
            }));

        Assert.Contains("Invalid username or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new IdentityUser { UserName = "testuser", Id = "id-1" };
        _userManager.Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.CheckPasswordAsync(user, "wrongpassword"))
            .ReturnsAsync(false);

        var service = new AuthService(_userManager.Object, _configuration);

        var exception = await Assert.ThrowsAsync<FluentResponse.Exceptions.UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            }));

        Assert.Contains("Invalid username or password", exception.Message);
    }
}
