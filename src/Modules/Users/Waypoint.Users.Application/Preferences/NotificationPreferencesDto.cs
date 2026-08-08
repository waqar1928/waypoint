using MediatR;
using Waypoint.Common;

namespace Waypoint.Users.Application.Preferences;

public sealed record NotificationPreferencesDto(
    bool EmailProductUpdates, bool EmailCoachNudges, bool EmailCommunityActivity);

public sealed record GetNotificationPreferencesQuery : IRequest<NotificationPreferencesDto>;

public sealed record UpdateNotificationPreferencesCommand(
    bool EmailProductUpdates, bool EmailCoachNudges, bool EmailCommunityActivity)
    : IRequest<NotificationPreferencesDto>;

public sealed class GetNotificationPreferencesQueryHandler(IUsersRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    public async Task<NotificationPreferencesDto> Handle(
        GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var prefs = await repository.GetNotificationPreferencesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Notification preferences not found.");

        return new NotificationPreferencesDto(
            prefs.EmailProductUpdates, prefs.EmailCoachNudges, prefs.EmailCommunityActivity);
    }
}

public sealed class UpdateNotificationPreferencesCommandHandler(
    IUsersRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferencesDto>
{
    public async Task<NotificationPreferencesDto> Handle(
        UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var prefs = await repository.GetNotificationPreferencesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Notification preferences not found.");

        prefs.EmailProductUpdates = request.EmailProductUpdates;
        prefs.EmailCoachNudges = request.EmailCoachNudges;
        prefs.EmailCommunityActivity = request.EmailCommunityActivity;
        prefs.UpdatedBy = userId;
        prefs.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.SaveNotificationPreferencesAsync(prefs, cancellationToken);

        return new NotificationPreferencesDto(
            prefs.EmailProductUpdates, prefs.EmailCoachNudges, prefs.EmailCommunityActivity);
    }
}
