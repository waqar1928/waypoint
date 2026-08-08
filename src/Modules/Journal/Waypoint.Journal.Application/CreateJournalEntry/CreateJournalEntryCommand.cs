using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Journal.Domain;

namespace Waypoint.Journal.Application.CreateJournalEntry;

public sealed record CreateJournalEntryCommand(JournalEntryType EntryType, string Body, Guid? DreamId)
    : IRequest<JournalEntryDto>;

public sealed class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.EntryType).IsInEnum();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(5000);
    }
}

public sealed class CreateJournalEntryCommandHandler(IJournalRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateJournalEntryCommand, JournalEntryDto>
{
    public async Task<JournalEntryDto> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var entry = JournalEntry.Create(userId, request.DreamId, request.EntryType, request.Body);

        await repository.AddAsync(entry, cancellationToken);

        return new JournalEntryDto(entry.Id, entry.EntryType, entry.Body, entry.CreatedAt);
    }
}
