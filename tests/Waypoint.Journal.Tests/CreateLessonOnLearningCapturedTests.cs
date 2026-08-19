using NSubstitute;
using Waypoint.Common;
using Waypoint.Journal.Application;
using Waypoint.Journal.Application.Registration;
using Waypoint.Journal.Domain;
using Xunit;

namespace Waypoint.Journal.Tests;

public class CreateLessonOnLearningCapturedTests
{
    private readonly IJournalRepository _repository = Substitute.For<IJournalRepository>();

    [Fact]
    public async Task Writes_a_lesson_type_entry_for_the_event_s_user_and_dream()
    {
        var userId = Guid.NewGuid();
        var dreamId = Guid.NewGuid();
        var handler = new CreateLessonOnLearningCaptured(_repository);

        await handler.Handle(
            new LearningCapturedIntegrationEvent(userId, dreamId, "People want a simpler tool"),
            CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<JournalEntry>(e =>
                e.UserId == userId && e.DreamId == dreamId &&
                e.EntryType == JournalEntryType.Lesson && e.Body == "People want a simpler tool"),
            Arg.Any<CancellationToken>());
    }
}
