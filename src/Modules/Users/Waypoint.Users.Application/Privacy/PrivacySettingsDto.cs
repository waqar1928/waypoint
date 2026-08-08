using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Users.Domain;

namespace Waypoint.Users.Application.Privacy;

public sealed record PrivacySettingsDto(VisibilityLevel ProfileVisibility, VisibilityLevel DreamVisibility);

public sealed record GetPrivacySettingsQuery : IRequest<PrivacySettingsDto>;

public sealed record UpdatePrivacySettingsCommand(VisibilityLevel ProfileVisibility, VisibilityLevel DreamVisibility)
    : IRequest<PrivacySettingsDto>;

public sealed class UpdatePrivacySettingsCommandValidator : AbstractValidator<UpdatePrivacySettingsCommand>
{
    public UpdatePrivacySettingsCommandValidator()
    {
        RuleFor(x => x.ProfileVisibility).IsInEnum();
        RuleFor(x => x.DreamVisibility).IsInEnum();
    }
}

public sealed class GetPrivacySettingsQueryHandler(IUsersRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetPrivacySettingsQuery, PrivacySettingsDto>
{
    public async Task<PrivacySettingsDto> Handle(GetPrivacySettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var settings = await repository.GetPrivacySettingsAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Privacy settings not found.");

        return new PrivacySettingsDto(settings.ProfileVisibility, settings.DreamVisibility);
    }
}

public sealed class UpdatePrivacySettingsCommandHandler(IUsersRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdatePrivacySettingsCommand, PrivacySettingsDto>
{
    public async Task<PrivacySettingsDto> Handle(
        UpdatePrivacySettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var settings = await repository.GetPrivacySettingsAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Privacy settings not found.");

        settings.ProfileVisibility = request.ProfileVisibility;
        settings.DreamVisibility = request.DreamVisibility;
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.SavePrivacySettingsAsync(settings, cancellationToken);

        return new PrivacySettingsDto(settings.ProfileVisibility, settings.DreamVisibility);
    }
}
