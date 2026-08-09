using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Mentorship.Application;
using Waypoint.Mentorship.Application.UpdateMentorVerification;
using Waypoint.Mentorship.Domain;
using Xunit;

namespace Waypoint.Mentorship.Tests;

public class UpdateMentorVerificationCommandHandlerTests
{
    private readonly IMentorshipRepository _repository = Substitute.For<IMentorshipRepository>();
    private readonly IProfileSummaryProvider _profileSummaryProvider = Substitute.For<IProfileSummaryProvider>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _adminId = Guid.NewGuid();

    private UpdateMentorVerificationCommandHandler CreateHandler() =>
        new(_repository, _profileSummaryProvider, _auditSink, _currentUser);

    [Fact]
    public async Task Moves_a_pending_profile_to_verified_and_records_an_audit_entry()
    {
        _currentUser.UserId.Returns(_adminId);
        var mentorUserId = Guid.NewGuid();
        var profile = MentorProfile.Create(mentorUserId, ["marketing"], 5, "2 hours/week");
        profile.VerificationStatus = VerificationStatus.Pending;
        _repository.GetMentorProfileByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        _profileSummaryProvider
            .GetForUserAsync(mentorUserId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(mentorUserId, "Sam Rivera", null));

        var result = await CreateHandler().Handle(
            new UpdateMentorVerificationCommand(profile.Id, VerificationStatus.Verified), CancellationToken.None);

        profile.VerificationStatus.Should().Be(VerificationStatus.Verified);
        result.VerificationStatus.Should().Be(VerificationStatus.Verified);
        await _repository.Received(1).SaveMentorProfileAsync(profile, Arg.Any<CancellationToken>());
        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "VerificationStatusChangedToVerified" && e.ActorUserId == _adminId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Can_revoke_verification_back_to_unverified()
    {
        var mentorUserId = Guid.NewGuid();
        var profile = MentorProfile.Create(mentorUserId, ["design"], null, null);
        profile.VerificationStatus = VerificationStatus.Verified;
        _repository.GetMentorProfileByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        _profileSummaryProvider
            .GetForUserAsync(mentorUserId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(mentorUserId, "Someone", null));

        await CreateHandler().Handle(
            new UpdateMentorVerificationCommand(profile.Id, VerificationStatus.Unverified), CancellationToken.None);

        profile.VerificationStatus.Should().Be(VerificationStatus.Unverified);
    }

    [Fact]
    public async Task Throws_when_the_mentor_profile_does_not_exist()
    {
        _repository.GetMentorProfileByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MentorProfile?)null);

        var act = () => CreateHandler().Handle(
            new UpdateMentorVerificationCommand(Guid.NewGuid(), VerificationStatus.Verified), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
