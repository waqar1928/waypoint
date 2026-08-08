using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Users.Application.Profiles;

public sealed record UpdateMyProfileCommand(string DisplayName, string? Bio, string TimeZone) : IRequest<ProfileDto>;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    private static readonly HashSet<string> KnownTimeZones = new(TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id));

    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Bio).MaximumLength(500);
        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .Must(BeAKnownTimeZone)
            .WithMessage("Enter a valid time zone.");
    }

    private static bool BeAKnownTimeZone(string timeZone) =>
        KnownTimeZones.Contains(timeZone) || TryFindTimeZone(timeZone);

    private static bool TryFindTimeZone(string timeZone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}

public sealed class UpdateMyProfileCommandHandler(IUsersRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateMyProfileCommand, ProfileDto>
{
    public async Task<ProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var profile = await repository.GetProfileAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Profile not found.");

        profile.DisplayName = request.DisplayName;
        profile.Bio = request.Bio;
        profile.TimeZone = request.TimeZone;
        profile.UpdatedBy = userId;

        await repository.SaveProfileAsync(profile, cancellationToken);

        return new ProfileDto(
            profile.UserId,
            profile.DisplayName,
            profile.Bio,
            profile.AvatarUrl,
            profile.TimeZone,
            profile.Locale,
            profile.OnboardingCompletedAt);
    }
}
