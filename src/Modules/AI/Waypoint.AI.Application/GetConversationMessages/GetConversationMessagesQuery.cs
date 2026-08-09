using MediatR;
using Waypoint.Common;

namespace Waypoint.AI.Application.GetConversationMessages;

public sealed record GetConversationMessagesQuery(Guid ConversationId) : IRequest<IReadOnlyList<MessageDto>>;

public sealed class GetConversationMessagesQueryHandler(IAiRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetConversationMessagesQuery, IReadOnlyList<MessageDto>>
{
    public async Task<IReadOnlyList<MessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var conversation = await repository.GetConversationByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null || conversation.UserId != userId)
        {
            throw new NotFoundException("Conversation not found.");
        }

        var messages = await repository.GetMessagesForConversationAsync(request.ConversationId, cancellationToken);
        return messages.Select(MessageDto.From).ToList();
    }
}
