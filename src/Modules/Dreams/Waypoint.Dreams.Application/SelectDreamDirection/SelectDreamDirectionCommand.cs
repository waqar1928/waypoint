using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Application.SelectDreamDirection;

public sealed record SelectDreamDirectionCommand(
    string Title,
    string Statement,
    string? Purpose,
    string? WhoItHelps,
    string? Problem,
    string? Outcome,
    string? Motivation,
    string? Impact,
    bool IsBusinessShaped) : IRequest<DreamDto>;

public sealed class SelectDreamDirectionCommandValidator : AbstractValidator<SelectDreamDirectionCommand>
{
    public SelectDreamDirectionCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Statement).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Purpose).MaximumLength(2000);
        RuleFor(x => x.WhoItHelps).MaximumLength(2000);
        RuleFor(x => x.Problem).MaximumLength(2000);
        RuleFor(x => x.Outcome).MaximumLength(2000);
        RuleFor(x => x.Motivation).MaximumLength(2000);
        RuleFor(x => x.Impact).MaximumLength(2000);
    }
}

/// <summary>
/// Creates a user's Dream + Dream Statement. Phase 2 keeps this to one
/// dream per user — the product supports many eventually (docs/01), but
/// the onboarding UX is deliberately built to encourage focus first.
/// </summary>
public sealed class SelectDreamDirectionCommandHandler(
    IDreamRepository repository, ICurrentUserAccessor currentUser, IPublisher publisher, IAuditSink auditSink)
    : IRequestHandler<SelectDreamDirectionCommand, DreamDto>
{
    public async Task<DreamDto> Handle(SelectDreamDirectionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        if (await repository.ExistsForUserAsync(userId, cancellationToken))
        {
            throw new ConflictException("You already have a dream in progress.");
        }

        var dream = Dream.Create(userId, request.Title, request.IsBusinessShaped);
        dream.Statement = DreamStatement.Create(
            dream.Id, request.Statement, request.Purpose, request.WhoItHelps,
            request.Problem, request.Outcome, request.Motivation, request.Impact);

        await repository.SaveAsync(dream, cancellationToken);

        await auditSink.RecordAsync(
            new AuditEntry("Dream", dream.Id, "Created", userId, null, DateTimeOffset.UtcNow), cancellationToken);
        await publisher.Publish(new OnboardingCompletedIntegrationEvent(userId), cancellationToken);

        return ToDto(dream);
    }

    internal static DreamDto ToDto(Dream dream) => new(
        dream.Id, dream.Title, dream.Stage, dream.IsBusinessShaped,
        dream.Statement.Statement, dream.Statement.Purpose, dream.Statement.WhoItHelps,
        dream.Statement.Problem, dream.Statement.Outcome, dream.Statement.Motivation, dream.Statement.Impact);
}
