using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Users.Application;
using Waypoint.Users.Application.Profiles;
using Waypoint.Users.Domain;
using Xunit;

namespace Waypoint.Users.Tests;

public class GetMyProfileQueryHandlerTests
{
    private readonly IUsersRepository _repository = Substitute.For<IUsersRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();

    [Fact]
    public async Task Returns_profile_for_signed_in_user()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _repository.GetProfileAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Profile.CreateForNewUser(userId, "Alex Rivera"));

        var handler = new GetMyProfileQueryHandler(_repository, _currentUser);
        var result = await handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        result.DisplayName.Should().Be("Alex Rivera");
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Throws_when_not_signed_in()
    {
        _currentUser.UserId.Returns((Guid?)null);
        var handler = new GetMyProfileQueryHandler(_repository, _currentUser);

        var act = () => handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }

    [Fact]
    public async Task Throws_not_found_when_profile_missing()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _repository.GetProfileAsync(userId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        var handler = new GetMyProfileQueryHandler(_repository, _currentUser);
        var act = () => handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
