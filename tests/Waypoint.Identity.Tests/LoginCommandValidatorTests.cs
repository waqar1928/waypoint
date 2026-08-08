using FluentAssertions;
using Waypoint.Identity.Application.Login;
using Xunit;

namespace Waypoint.Identity.Tests;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(new LoginCommand("alex@example.com", "anything"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var result = _validator.Validate(new LoginCommand("", "anything"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Empty_password_fails()
    {
        var result = _validator.Validate(new LoginCommand("alex@example.com", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
