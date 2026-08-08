using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Dreams.Application.SelectDreamDirection;

namespace Waypoint.Dreams.Application.UpdateDreamStatement;

public sealed record UpdateDreamStatementCommand(
    string Title,
    string Statement,
    string? Purpose,
    string? WhoItHelps,
    string? Problem,
    string? Outcome,
    string? Motivation,
    string? Impact) : IRequest<DreamDto>;

public sealed class UpdateDreamStatementCommandValidator : AbstractValidator<UpdateDreamStatementCommand>
{
    public UpdateDreamStatementCommandValidator()
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

public sealed class UpdateDreamStatementCommandHandler(IDreamRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateDreamStatementCommand, DreamDto>
{
    public async Task<DreamDto> Handle(UpdateDreamStatementCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await repository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        dream.Title = request.Title;
        dream.UpdatedBy = userId;

        dream.Statement.Statement = request.Statement;
        dream.Statement.Purpose = request.Purpose;
        dream.Statement.WhoItHelps = request.WhoItHelps;
        dream.Statement.Problem = request.Problem;
        dream.Statement.Outcome = request.Outcome;
        dream.Statement.Motivation = request.Motivation;
        dream.Statement.Impact = request.Impact;
        dream.Statement.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.SaveAsync(dream, cancellationToken);

        return SelectDreamDirectionCommandHandler.ToDto(dream);
    }
}
