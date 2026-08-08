using FluentAssertions;
using Waypoint.Identity.Application.Register;
using Xunit;

namespace Waypoint.Identity.Tests;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var command = new RegisterUserCommand("Alex Rivera", "alex@example.com", "GoodPass123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "alex@example.com", "GoodPass123", "DisplayName")]
    [InlineData("Alex", "not-an-email", "GoodPass123", "Email")]
    [InlineData("Alex", "alex@example.com", "short1A", "Password")]
    [InlineData("Alex", "alex@example.com", "nouppercase123", "Password")]
    [InlineData("Alex", "alex@example.com", "NoDigitsHere", "Password")]
    public void Invalid_command_fails_on_expected_field(
        string displayName, string email, string password, string expectedInvalidField)
    {
        var command = new RegisterUserCommand(displayName, email, password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedInvalidField);
    }
}
