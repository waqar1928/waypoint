using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.UpdateMentorVerification;

/// <summary>Admin-only — see docs/09-phased-plan.md Phase 8 "mentor verification." The mentor
/// profile itself (Phase 7) has no self-service path to "verified"; only this command moves it.</summary>
public sealed record UpdateMentorVerificationCommand(Guid MentorProfileId, VerificationStatus Status) : IRequest<MentorProfileDto>;

public sealed class UpdateMentorVerificationCommandValidator : AbstractValidator<UpdateMentorVerificationCommand>
{
    public UpdateMentorVerificationCommandValidator()
    {
        RuleFor(x => x.MentorProfileId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdateMentorVerificationCommandHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider, IAuditSink auditSink, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateMentorVerificationCommand, MentorProfileDto>
{
    public async Task<MentorProfileDto> Handle(UpdateMentorVerificationCommand request, CancellationToken cancellationToken)
    {
        var profile = await repository.GetMentorProfileByIdAsync(request.MentorProfileId, cancellationToken)
            ?? throw new NotFoundException("Mentor profile not found.");

        profile.VerificationStatus = request.Status;
        await repository.SaveMentorProfileAsync(profile, cancellationToken);

        await auditSink.RecordAsync(
            new AuditEntry("MentorProfile", profile.Id, $"VerificationStatusChangedTo{request.Status}", currentUser.UserId, null, DateTimeOffset.UtcNow),
            cancellationToken);

        var mentor = await PersonResolver.ResolveAsync(profileSummaryProvider, profile.UserId, cancellationToken);
        return MentorProfileDto.From(profile, mentor);
    }
}
