using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Journal.Application;
using Waypoint.Journal.Application.CreateJournalEntry;
using Waypoint.Journal.Domain;
using Xunit;

namespace Waypoint.Journal.Tests;

public class CreateJournalEntryCommandHandlerTests
{
    private readonly IJournalRepository _repository = Substitute.For<IJournalRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private CreateJournalEntryCommandHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Creates_entry_owned_by_the_current_user_and_persists_it()
    {
        _currentUser.UserId.Returns(_userId);
        var dreamId = Guid.NewGuid();

        var result = await CreateHandler().Handle(
            new CreateJournalEntryCommand(JournalEntryType.Win, "Shipped the first version today.", dreamId),
            CancellationToken.None);

        result.EntryType.Should().Be(JournalEntryType.Win);
        result.Body.Should().Be("Shipped the first version today.");

        await _repository.Received(1).AddAsync(
            Arg.Is<JournalEntry>(e =>
                e.UserId == _userId &&
                e.DreamId == dreamId &&
                e.EntryType == JournalEntryType.Win &&
                e.Body == "Shipped the first version today."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dream_id_is_optional()
    {
        _currentUser.UserId.Returns(_userId);

        await CreateHandler().Handle(
            new CreateJournalEntryCommand(JournalEntryType.Lesson, "Learned something.", null),
            CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<JournalEntry>(e => e.DreamId == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_not_signed_in()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateHandler().Handle(
            new CreateJournalEntryCommand(JournalEntryType.Daily, "Body", null), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}

public class CreateJournalEntryCommandValidatorTests
{
    private readonly CreateJournalEntryCommandValidator _validator = new();

    [Fact]
    public void Rejects_empty_body()
    {
        var result = _validator.Validate(new CreateJournalEntryCommand(JournalEntryType.Daily, "", null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rejects_body_over_5000_characters()
    {
        var tooLong = new string('a', 5001);

        var result = _validator.Validate(new CreateJournalEntryCommand(JournalEntryType.Daily, tooLong, null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Accepts_a_valid_entry()
    {
        var result = _validator.Validate(new CreateJournalEntryCommand(JournalEntryType.Weekly, "A reasonable reflection.", Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }
}
