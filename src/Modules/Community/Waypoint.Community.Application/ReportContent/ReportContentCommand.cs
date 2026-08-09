using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Community.Application.ReportContent;

public static class ReportableEntityTypes
{
    public const string Post = "post";
    public const string Comment = "comment";
    public const string HelpRequest = "help_request";
}

public static class ReportReasons
{
    public static readonly string[] Allowed = ["spam", "harassment", "other"];
}

public sealed record ReportContentCommand(string EntityType, Guid EntityId, string Reason, string? Details) : IRequest;

public sealed class ReportContentCommandValidator : AbstractValidator<ReportContentCommand>
{
    public ReportContentCommandValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.Reason).Must(r => ReportReasons.Allowed.Contains(r))
            .WithMessage($"Reason must be one of: {string.Join(", ", ReportReasons.Allowed)}.");
        RuleFor(x => x.Details).MaximumLength(1000);
    }
}

public sealed class ReportContentCommandHandler(IContentReportSink reportSink, ICurrentUserAccessor currentUser)
    : IRequestHandler<ReportContentCommand>
{
    public async Task Handle(ReportContentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        await reportSink.RecordAsync(
            new ContentReport(request.EntityType, request.EntityId, userId, request.Reason, request.Details, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
