using MediatR;
using Waypoint.Common;
using Waypoint.Journal.Domain;

namespace Waypoint.Journal.Application.Registration;

/// <summary>
/// Reacts to LearningCapturedIntegrationEvent (published by Experiments when a result is recorded,
/// and by Actions when someone adds an optional reflection on completion) by writing a Lesson-type
/// Journal entry. This is what makes "learnings" show up in one place instead of staying locked
/// inside whichever experiment or action produced them - see IntegrationEvents.cs's doc comment.
/// Neither Experiments nor Actions references Journal directly; this is the cross-module side
/// effect, same pattern as CreateProfileOnUserRegistered.
/// </summary>
public sealed class CreateLessonOnLearningCaptured(IJournalRepository repository)
    : INotificationHandler<LearningCapturedIntegrationEvent>
{
    public Task Handle(LearningCapturedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var entry = JournalEntry.Create(
            notification.UserId, notification.DreamId, JournalEntryType.Lesson, notification.Body);
        return repository.AddAsync(entry, cancellationToken);
    }
}
