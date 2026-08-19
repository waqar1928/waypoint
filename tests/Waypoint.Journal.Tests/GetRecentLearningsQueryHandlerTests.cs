using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Journal.Application;
using Waypoint.Journal.Application.GetRecentLearnings;
using Waypoint.Journal.Domain;
using Xunit;

namespace Waypoint.Journal.Tests;

public class GetRecentLearningsQueryHandlerTests
{
    private readonly IJournalRepository _repository = Substitute.For<IJournalRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private GetRecentLearningsQueryHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Returns_only_lesson_entries_from_the_repository()
    {
        _currentUser.UserId.Returns(_userId);
        var lesson = JournalEntry.Create(_userId, Guid.NewGuid(), JournalEntryType.Lesson, "Speed matters more than price");
        _repository.GetRecentLessonsForUserAsync(_userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([lesson]);

        var result = await CreateHandler().Handle(new GetRecentLearningsQuery(), CancellationToken.None);

        result.Should().ContainSingle(e => e.Body == "Speed matters more than price" && e.EntryType == JournalEntryType.Lesson);
    }

    [Fact]
    public async Task Throws_when_not_signed_in()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateHandler().Handle(new GetRecentLearningsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
