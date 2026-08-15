using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Mentorship.Application;
using Waypoint.Mentorship.Application.CloseHelpRequest;
using Waypoint.Mentorship.Domain;
using Xunit;

namespace Waypoint.Mentorship.Tests;

public class CloseHelpRequestCommandHandlerTests
{
    private readonly IMentorshipRepository _repository = Substitute.For<IMentorshipRepository>();
    private readonly IProfileSummaryProvider _profileSummaryProvider = Substitute.For<IProfileSummaryProvider>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private CloseHelpRequestCommandHandler CreateHandler() =>
        new(_repository, _profileSummaryProvider, _dreamSummaryProvider, _currentUser);

    [Fact]
    public async Task Throws_when_closing_someone_elses_help_request()
    {
        _currentUser.UserId.Returns(_userId);
        var othersRequest = HelpRequest.Create(Guid.NewGuid(), null, HelpRequestCategory.Career, "Title", "Body");
        _repository.GetHelpRequestByIdAsync(othersRequest.Id, Arg.Any<CancellationToken>()).Returns(othersRequest);

        var act = () => CreateHandler().Handle(new CloseHelpRequestCommand(othersRequest.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Closes_own_help_request()
    {
        _currentUser.UserId.Returns(_userId);
        var request = HelpRequest.Create(_userId, null, HelpRequestCategory.Career, "Title", "Body");
        _repository.GetHelpRequestByIdAsync(request.Id, Arg.Any<CancellationToken>()).Returns(request);
        _repository.GetResponseCountsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Sam Rivera", null));

        var result = await CreateHandler().Handle(new CloseHelpRequestCommand(request.Id), CancellationToken.None);

        result.Status.Should().Be(HelpRequestStatus.Closed);
        await _repository.Received(1).SaveHelpRequestAsync(request, Arg.Any<CancellationToken>());
    }
}
