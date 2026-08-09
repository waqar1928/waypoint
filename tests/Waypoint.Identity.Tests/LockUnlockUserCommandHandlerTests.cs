using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Identity.Application;
using Waypoint.Identity.Application.Admin.LockUser;
using Waypoint.Identity.Application.Admin.UnlockUser;
using Xunit;

namespace Waypoint.Identity.Tests;

public class LockUserCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _targetUserId = Guid.NewGuid();

    private LockUserCommandHandler CreateHandler() => new(_identityService, _auditSink, _currentUser);

    [Fact]
    public async Task Records_an_audit_entry_attributed_to_the_acting_admin_on_success()
    {
        _currentUser.UserId.Returns(_adminId);
        _identityService.LockUserAsync(_targetUserId, Arg.Any<CancellationToken>())
            .Returns(IdentityOperationResult.Success());

        await CreateHandler().Handle(new LockUserCommand(_targetUserId), CancellationToken.None);

        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "LockedByAdmin" && e.EntityId == _targetUserId && e.ActorUserId == _adminId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_when_the_target_user_does_not_exist_and_records_no_audit_entry()
    {
        _identityService.LockUserAsync(_targetUserId, Arg.Any<CancellationToken>())
            .Returns(IdentityOperationResult.Failure(["User not found."]));

        var act = () => CreateHandler().Handle(new LockUserCommand(_targetUserId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _auditSink.DidNotReceive().RecordAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }
}

public class UnlockUserCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _targetUserId = Guid.NewGuid();

    private UnlockUserCommandHandler CreateHandler() => new(_identityService, _auditSink, _currentUser);

    [Fact]
    public async Task Records_an_audit_entry_attributed_to_the_acting_admin_on_success()
    {
        _currentUser.UserId.Returns(_adminId);
        _identityService.UnlockUserAsync(_targetUserId, Arg.Any<CancellationToken>())
            .Returns(IdentityOperationResult.Success());

        await CreateHandler().Handle(new UnlockUserCommand(_targetUserId), CancellationToken.None);

        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "UnlockedByAdmin" && e.EntityId == _targetUserId && e.ActorUserId == _adminId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_when_the_target_user_does_not_exist_and_records_no_audit_entry()
    {
        _identityService.UnlockUserAsync(_targetUserId, Arg.Any<CancellationToken>())
            .Returns(IdentityOperationResult.Failure(["User not found."]));

        var act = () => CreateHandler().Handle(new UnlockUserCommand(_targetUserId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _auditSink.DidNotReceive().RecordAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }
}
