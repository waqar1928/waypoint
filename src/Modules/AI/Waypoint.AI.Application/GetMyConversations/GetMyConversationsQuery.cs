using MediatR;
using Waypoint.Common;

namespace Waypoint.AI.Application.GetMyConversations;

public sealed record GetMyConversationsQuery : IRequest<IReadOnlyList<ConversationSummaryDto>>;

public sealed class GetMyConversationsQueryHandler(IAiRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyConversationsQuery, IReadOnlyList<ConversationSummaryDto>>
{
    public async Task<IReadOnlyList<ConversationSummaryDto>> Handle(
        GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var conversations = await repository.GetConversationsForUserAsync(userId, cancellationToken);
        return conversations.OrderByDescending(c => c.UpdatedAt).Select(ConversationSummaryDto.From).ToList();
    }
}
