using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.CreateMentorProfile;

public sealed record CreateMentorProfileCommand(
    List<string> Expertise, int? YearsExperience, string? Availability) : IRequest<MentorProfileDto>;

public sealed class CreateMentorProfileCommandValidator : AbstractValidator<CreateMentorProfileCommand>
{
    public CreateMentorProfileCommandValidator()
    {
        RuleFor(x => x.Expertise).NotEmpty().WithMessage("Pick at least one area of expertise.");
        RuleForEach(x => x.Expertise).MaximumLength(50);
        RuleFor(x => x.YearsExperience).GreaterThanOrEqualTo(0).When(x => x.YearsExperience.HasValue);
        RuleFor(x => x.Availability).MaximumLength(50);
    }
}

public sealed class CreateMentorProfileCommandHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateMentorProfileCommand, MentorProfileDto>
{
    public async Task<MentorProfileDto> Handle(CreateMentorProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var existing = await repository.GetMentorProfileByUserIdAsync(userId, cancellationToken);
        var mentor = await PersonResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);

        if (existing is not null)
        {
            existing.Expertise = request.Expertise;
            existing.YearsExperience = request.YearsExperience;
            existing.Availability = request.Availability;
            existing.UpdatedBy = userId;
            await repository.SaveMentorProfileAsync(existing, cancellationToken);
            return MentorProfileDto.From(existing, mentor);
        }

        var profile = MentorProfile.Create(userId, request.Expertise, request.YearsExperience, request.Availability);
        await repository.AddMentorProfileAsync(profile, cancellationToken);
        return MentorProfileDto.From(profile, mentor);
    }
}
