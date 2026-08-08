using FluentAssertions;
using Waypoint.Dreams.Application.SelectDreamDirection;
using Xunit;

namespace Waypoint.Dreams.Tests;

public class SelectDreamDirectionCommandValidatorTests
{
    private readonly SelectDreamDirectionCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var command = new SelectDreamDirectionCommand(
            "Cut waste for small manufacturers",
            "Help small manufacturers reduce material waste.",
            "Because I want to help real businesses survive.",
            "Small manufacturing shop owners",
            "They lack visibility into waste.",
            "Measurable waste reduction.",
            "I want visible impact.",
            "Less industry-wide waste.",
            IsBusinessShaped: true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_title_fails()
    {
        var command = new SelectDreamDirectionCommand(
            "", "A statement", null, null, null, null, null, null, false);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SelectDreamDirectionCommand.Title));
    }

    [Fact]
    public void Empty_statement_fails()
    {
        var command = new SelectDreamDirectionCommand(
            "A title", "", null, null, null, null, null, null, false);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SelectDreamDirectionCommand.Statement));
    }
}
