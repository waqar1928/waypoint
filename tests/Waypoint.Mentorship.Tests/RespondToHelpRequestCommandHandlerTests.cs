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
    private readonly Guid _userId = Guid.NewGuid();

    private RespondToHelpRequestCommandHandler CreateHandler() => new(_repository, _profileSummaryProvider, _currentUser);

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
