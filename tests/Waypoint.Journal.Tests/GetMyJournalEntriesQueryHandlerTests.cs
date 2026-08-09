using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Journal.Application;
using Waypoint.Journal.Application.GetMyJournalEntries;
using Waypoint.Journal.Domain;
using Xunit;

namespace Waypoint.Journal.Tests;

public class GetMyJournalEntriesQueryHandlerTests
{
    private readonly IJournalRepository _repository = Substitute.For<IJournalRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private GetMyJournalEntriesQueryHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Requests_at_most_20_recent_entries_for_the_current_user()
    {
        _currentUser.UserId.Returns(_userId);
        _repository.GetRecentForUserAsync(_userId, 20, Arg.Any<CancellationToken>()).Returns([]);

        await CreateHandler().Handle(new GetMyJournalEntriesQuery(), CancellationToken.None);

        await _repository.Received(1).GetRecentForUserAsync(_userId, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Maps_entries_to_dtos()
    {
        _currentUser.UserId.Returns(_userId);
        var entry = JournalEntry.Create(_userId, null, JournalEntryType.Win, "Landed the first customer.");
        _repository.GetRecentForUserAsync(_userId, 20, Arg.Any<CancellationToken>()).Returns([entry]);

        var result = await CreateHandler().Handle(new GetMyJournalEntriesQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(entry.Id);
        result[0].EntryType.Should().Be(JournalEntryType.Win);
        result[0].Body.Should().Be("Landed the first customer.");
    }

    [Fact]
    public async Task Throws_when_not_signed_in()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateHandler().Handle(new GetMyJournalEntriesQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
