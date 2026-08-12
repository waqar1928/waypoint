using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Mentorship.Application;
using Waypoint.Mentorship.Application.RespondToHelpRequest;
using Waypoint.Mentorship.Domain;
using Xunit;

namespace Waypoint.Mentorship.Tests;

public class RespondToHelpRequestCommandHandlerTests
{
    private readonly IMentorshipRepository _repository = Substitute.For<IMentorshipRepository>();
    private readonly IProfileSummaryProvider _profileSummaryProvider = Substitute.For<IProfileSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly INotificationSink _notificationSink = Substitute.For<INotificationSink>();
    private readonly Guid _userId = Guid.NewGuid();

    private RespondToHelpRequestCommandHandler CreateHandler() =>
        new(_repository, _profileSummaryProvider, _currentUser, _notificationSink);

    [Fact]
    public async Task First_response_transitions_status_from_open_to_answered()
    {
        _currentUser.UserId.Returns(_userId);
        var helpRequest = HelpRequest.Create(Guid.NewGuid(), null, HelpRequestCategory.Marketing, "Title", "Body");
        _repository.GetHelpRequestByIdAsync(helpRequest.Id, Arg.Any<CancellationToken>()).Returns(helpRequest);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Mentor Mo", null));

        await CreateHandler().Handle(new RespondToHelpRequestCommand(helpRequest.Id, "Try this"), CancellationToken.None);

        helpRequest.Status.Should().Be(HelpRequestStatus.Answered);
        await _repository.Received(1).SaveHelpRequestAsync(helpRequest, Arg.Any<CancellationToken>());
        await _repository.Received(1).AddResponseAsync(
            Arg.Is<HelpRequestResponse>(r => r.HelpRequestId == helpRequest.Id && r.ResponderUserId == _userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Notifies_the_requester_when_someone_else_responds()
    {
        var requesterId = Guid.NewGuid();
        _currentUser.UserId.Returns(_userId);
        var helpRequest = HelpRequest.Create(requesterId, null, HelpRequestCategory.Marketing, "Title", "Body");
        _repository.GetHelpRequestByIdAsync(helpRequest.Id, Arg.Any<CancellationToken>()).Returns(helpRequest);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Mentor Mo", null));

        await CreateHandler().Handle(new RespondToHelpRequestCommand(helpRequest.Id, "Try this"), CancellationToken.None);

        await _notificationSink.Received(1).SendAsync(
            Arg.Is<NotificationToSend>(n => n.RecipientUserId == requesterId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_notify_when_the_requester_responds_to_their_own_request()
    {
        // The requester can reply in their own thread (e.g. adding context) — that shouldn't
        // generate a self-notification.
        _currentUser.UserId.Returns(_userId);
        var helpRequest = HelpRequest.Create(_userId, null, HelpRequestCategory.Marketing, "Title", "Body");
        _repository.GetHelpRequestByIdAsync(helpRequest.Id, Arg.Any<CancellationToken>()).Returns(helpRequest);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Requester", null));

        await CreateHandler().Handle(new RespondToHelpRequestCommand(helpRequest.Id, "More context"), CancellationToken.None);

        await _notificationSink.DidNotReceive().SendAsync(Arg.Any<NotificationToSend>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_help_request_is_already_closed()
    {
        _currentUser.UserId.Returns(_userId);
        var helpRequest = HelpRequest.Create(Guid.NewGuid(), null, HelpRequestCategory.Marketing, "Title", "Body");
        helpRequest.Status = HelpRequestStatus.Closed;
        _repository.GetHelpRequestByIdAsync(helpRequest.Id, Arg.Any<CancellationToken>()).Returns(helpRequest);

        var act = () => CreateHandler().Handle(new RespondToHelpRequestCommand(helpRequest.Id, "Too late"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _repository.DidNotReceive().AddResponseAsync(Arg.Any<HelpRequestResponse>(), Arg.Any<CancellationToken>());
    }
}
