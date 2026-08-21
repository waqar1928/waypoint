using FluentAssertions;
using Waypoint.Users.Application.Preferences;

namespace Waypoint.Users.Tests;

public class UpdateNotificationPreferencesCommandValidatorTests
{
    private static readonly UpdateNotificationPreferencesCommandValidator Validator = new();

    private static UpdateNotificationPreferencesCommand MakeCommand(TimeOnly? start, TimeOnly? end) =>
        new(true, true, false, true, false, true, start, end);

    [Fact]
    public void Both_quiet_hours_unset_is_valid()
    {
        Validator.Validate(MakeCommand(null, null)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Both_quiet_hours_set_is_valid()
    {
        Validator.Validate(MakeCommand(new TimeOnly(22, 0), new TimeOnly(7, 0))).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Start_set_without_end_is_invalid()
    {
        Validator.Validate(MakeCommand(new TimeOnly(22, 0), null)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void End_set_without_start_is_invalid()
    {
        Validator.Validate(MakeCommand(null, new TimeOnly(7, 0))).IsValid.Should().BeFalse();
    }
}
