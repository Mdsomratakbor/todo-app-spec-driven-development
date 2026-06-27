using System.ComponentModel.DataAnnotations;
using TodoApi.Models.DTOs.Auth;
using TodoApi.Models.DTOs.Categories;
using TodoApi.Models.DTOs.Todos;

namespace TodoApi.Tests.Models;

public class ModelValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void RegisterRequest_WithEmptyUsername_HasValidationError()
    {
        var request = new RegisterRequest { Username = "", Password = "password123" };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Username"));
    }

    [Fact]
    public void RegisterRequest_WithWhitespaceUsername_HasValidationError()
    {
        var request = new RegisterRequest { Username = "   ", Password = "password123" };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Username"));
    }

    [Fact]
    public void RegisterRequest_WithUsernameExceedingMaxLength_HasValidationError()
    {
        var request = new RegisterRequest { Username = new string('a', 257), Password = "password123" };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Username"));
    }

    [Fact]
    public void CategoryRequest_WithEmptyName_HasValidationError()
    {
        var request = new CategoryRequest { Name = "" };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void CategoryRequest_WithWhitespaceName_HasValidationError()
    {
        var request = new CategoryRequest { Name = "   " };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void CategoryRequest_WithNameExceedingMaxLength_HasValidationError()
    {
        var request = new CategoryRequest { Name = new string('a', 51) };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void TodoRequest_WithEmptyTitle_HasValidationError()
    {
        var request = new TodoRequest { Title = "" };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TodoRequest_WithWhitespaceTitle_HasValidationError()
    {
        var request = new TodoRequest { Title = "   " };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TodoRequest_WithTitleExceedingMaxLength_HasValidationError()
    {
        var request = new TodoRequest { Title = new string('a', 201) };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }
}
