using FluentAssertions;
using Waypoint.Users.Application.Profiles;
using Xunit;

namespace Waypoint.Users.Tests;

public class UpdateMyProfileCommandValidatorTests
{
    private readonly UpdateMyProfileCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(new UpdateMyProfileCommand("Alex Rivera", "Building something new.", "America/New_York"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_display_name_fails()
    {
        var result = _validator.Validate(new UpdateMyProfileCommand("", null, "UTC"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateMyProfileCommand.DisplayName));
    }

    [Fact]
    public void Unknown_time_zone_fails()
    {
        var result = _validator.Validate(new UpdateMyProfileCommand("Alex", null, "Not/A_Real_Zone"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateMyProfileCommand.TimeZone));
    }

    [Fact]
    public void Bio_over_500_characters_fails()
    {
        var result = _validator.Validate(new UpdateMyProfileCommand("Alex", new string('a', 501), "UTC"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateMyProfileCommand.Bio));
    }
}
