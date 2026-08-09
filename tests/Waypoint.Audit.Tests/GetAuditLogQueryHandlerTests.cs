using FluentAssertions;
using NSubstitute;
using Waypoint.Audit.Application;
using Waypoint.Audit.Application.GetAuditLog;
using Waypoint.Audit.Domain;
using Xunit;

namespace Waypoint.Audit.Tests;

public class GetAuditLogQueryHandlerTests
{
    private readonly IAuditLogRepository _repository = Substitute.For<IAuditLogRepository>();

    private GetAuditLogQueryHandler CreateHandler() => new(_repository);

    private static AuditLogEntry BuildEntry(string action) => new()
    {
        EntityType = "User",
        EntityId = Guid.NewGuid(),
        Action = action,
        ActorUserId = Guid.NewGuid(),
        OccurredAt = DateTimeOffset.UtcNow,
    };

    [Fact]
    public async Task Default_take_of_200_is_passed_through_to_the_repository()
    {
        _repository.GetRecentAsync(200, Arg.Any<CancellationToken>()).Returns([]);

        await CreateHandler().Handle(new GetAuditLogQuery(), CancellationToken.None);

        await _repository.Received(1).GetRecentAsync(200, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 1)] // below the floor clamps up to 1
    [InlineData(-50, 1)]
    [InlineData(1000, 500)] // above the ceiling clamps down to 500
    [InlineData(50, 50)] // within range passes through unchanged
    public async Task Take_is_clamped_to_the_1_to_500_range(int requested, int expectedClamped)
    {
        _repository.GetRecentAsync(expectedClamped, Arg.Any<CancellationToken>()).Returns([]);

        await CreateHandler().Handle(new GetAuditLogQuery(requested), CancellationToken.None);

        await _repository.Received(1).GetRecentAsync(expectedClamped, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Entries_are_mapped_to_dtos_preserving_repository_order()
    {
        var first = BuildEntry("LoginSucceeded");
        var second = BuildEntry("LockedByAdmin");
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([first, second]);

        var result = await CreateHandler().Handle(new GetAuditLogQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(first.Id);
        result[0].Action.Should().Be("LoginSucceeded");
        result[1].Id.Should().Be(second.Id);
        result[1].Action.Should().Be("LockedByAdmin");
    }
}
