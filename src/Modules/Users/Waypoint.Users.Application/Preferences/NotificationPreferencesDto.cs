using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Users.Application.Preferences;

public sealed record NotificationPreferencesDto(
    bool EmailProductUpdates,
    bool EmailCoachNudges,
    bool EmailCommunityActivity,
    bool PushEnabled,
    bool PushDetailedContent,
    bool PushDailyReminderEnabled,
    TimeOnly? QuietHoursStart,
    TimeOnly? QuietHoursEnd);

public sealed record GetNotificationPreferencesQuery : IRequest<NotificationPreferencesDto>;

public sealed record UpdateNotificationPreferencesCommand(
    bool EmailProductUpdates,
    bool EmailCoachNudges,
    bool EmailCommunityActivity,
    bool PushEnabled,
    bool PushDetailedContent,
    bool PushDailyReminderEnabled,
    TimeOnly? QuietHoursStart,
    TimeOnly? QuietHoursEnd)
    : IRequest<NotificationPreferencesDto>;

/// <summary>Both quiet-hours fields must be set together, or both left unset - a start with no end
/// (or vice versa) can't be evaluated by QuietHoursEvaluator and almost certainly means the
/// frontend form sent a partial update. PushDetailedContent/PushDailyReminderEnabled being true
/// while PushEnabled is false is allowed rather than rejected (the worker only ever acts on a
/// candidate when PushEnabled AND PushDailyReminderEnabled are both true - see
/// IPushReminderAudienceProvider), so there's no inconsistent *reachable* state even without
/// cross-field validation here; keeping this validator narrow avoids the form fighting the user
/// over an ordering of checkbox clicks that's momentarily "inconsistent" only until they finish.</summary>
public sealed class UpdateNotificationPreferencesCommandValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.QuietHoursStart.HasValue == x.QuietHoursEnd.HasValue)
            .WithMessage("Set both a quiet hours start and end, or leave both empty.");
    }
}

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
            prefs.EmailProductUpdates, prefs.EmailCoachNudges, prefs.EmailCommunityActivity,
            prefs.PushEnabled, prefs.PushDetailedContent, prefs.PushDailyReminderEnabled,
            prefs.QuietHoursStart, prefs.QuietHoursEnd);
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
        prefs.PushEnabled = request.PushEnabled;
        prefs.PushDetailedContent = request.PushDetailedContent;
        prefs.PushDailyReminderEnabled = request.PushDailyReminderEnabled;
        prefs.QuietHoursStart = request.QuietHoursStart;
        prefs.QuietHoursEnd = request.QuietHoursEnd;
        prefs.UpdatedBy = userId;

        await repository.SaveNotificationPreferencesAsync(prefs, cancellationToken);

        return new NotificationPreferencesDto(
            prefs.EmailProductUpdates, prefs.EmailCoachNudges, prefs.EmailCommunityActivity,
            prefs.PushEnabled, prefs.PushDetailedContent, prefs.PushDailyReminderEnabled,
            prefs.QuietHoursStart, prefs.QuietHoursEnd);
    }
}
