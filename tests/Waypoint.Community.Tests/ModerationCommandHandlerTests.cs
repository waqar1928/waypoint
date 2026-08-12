using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Community.Application;
using Waypoint.Community.Application.DismissReport;
using Waypoint.Community.Application.RemoveReportedContent;
using Waypoint.Community.Application.ReportContent;
using Waypoint.Community.Application.ResolveReport;
using Waypoint.Community.Domain;
using Xunit;

namespace Waypoint.Community.Tests;

public class DismissReportCommandHandlerTests
{
    private readonly ICommunityRepository _repository = Substitute.For<ICommunityRepository>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _adminId = Guid.NewGuid();

    private DismissReportCommandHandler CreateHandler() => new(_repository, _auditSink, _currentUser);

    [Fact]
    public async Task Marks_the_report_dismissed_and_records_an_audit_entry()
    {
        _currentUser.UserId.Returns(_adminId);
        var report = ContentReportRecord.Create(ReportableEntityTypes.Post, Guid.NewGuid(), Guid.NewGuid(), "spam", null);
        _repository.GetReportByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        await CreateHandler().Handle(new DismissReportCommand(report.Id), CancellationToken.None);

        report.Status.Should().Be(ReportStatus.Dismissed);
        await _repository.Received(1).SaveReportAsync(report, Arg.Any<CancellationToken>());
        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "DismissedByAdmin" && e.ActorUserId == _adminId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_report_does_not_exist()
    {
        _repository.GetReportByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ContentReportRecord?)null);

        var act = () => CreateHandler().Handle(new DismissReportCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public class ResolveReportCommandHandlerTests
{
    private readonly ICommunityRepository _repository = Substitute.For<ICommunityRepository>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();

    private ResolveReportCommandHandler CreateHandler() => new(_repository, _auditSink, _currentUser);

    [Fact]
    public async Task Marks_the_report_resolved_without_touching_any_content()
    {
        var report = ContentReportRecord.Create(ReportableEntityTypes.HelpRequest, Guid.NewGuid(), Guid.NewGuid(), "off-topic", null);
        _repository.GetReportByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        await CreateHandler().Handle(new ResolveReportCommand(report.Id), CancellationToken.None);

        report.Status.Should().Be(ReportStatus.Resolved);
        await _repository.Received(1).SaveReportAsync(report, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().SavePostAsync(Arg.Any<CommunityPost>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().SaveCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }
}

public class RemoveReportedContentCommandHandlerTests
{
    private readonly ICommunityRepository _repository = Substitute.For<ICommunityRepository>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly INotificationSink _notificationSink = Substitute.For<INotificationSink>();

    private RemoveReportedContentCommandHandler CreateHandler() =>
        new(_repository, _auditSink, _currentUser, _notificationSink);

    [Fact]
    public async Task Soft_deletes_a_reported_post_and_marks_the_report_content_removed()
    {
        var authorId = Guid.NewGuid();
        var post = CommunityPost.Create(authorId, null, "Spam post", PostVisibility.Public);
        var report = ContentReportRecord.Create(ReportableEntityTypes.Post, post.Id, Guid.NewGuid(), "spam", null);
        _repository.GetReportByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);
        _repository.GetPostByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);

        await CreateHandler().Handle(new RemoveReportedContentCommand(report.Id), CancellationToken.None);

        post.DeletedAt.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.ContentRemoved);
        await _repository.Received(1).SavePostAsync(post, Arg.Any<CancellationToken>());
        await _notificationSink.Received(1).SendAsync(
            Arg.Is<NotificationToSend>(n => n.RecipientUserId == authorId && n.Category == NotificationCategories.Moderation),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Soft_deletes_a_reported_comment_and_marks_the_report_content_removed()
    {
        var authorId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), authorId, "Spam comment");
        var report = ContentReportRecord.Create(ReportableEntityTypes.Comment, comment.Id, Guid.NewGuid(), "spam", null);
        _repository.GetReportByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);
        _repository.GetCommentByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        await CreateHandler().Handle(new RemoveReportedContentCommand(report.Id), CancellationToken.None);

        comment.DeletedAt.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.ContentRemoved);
        await _repository.Received(1).SaveCommentAsync(comment, Arg.Any<CancellationToken>());
        await _notificationSink.Received(1).SendAsync(
            Arg.Is<NotificationToSend>(n => n.RecipientUserId == authorId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_conflict_for_entity_types_community_cannot_remove_directly()
    {
        var report = ContentReportRecord.Create(ReportableEntityTypes.HelpRequest, Guid.NewGuid(), Guid.NewGuid(), "off-topic", null);
        _repository.GetReportByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        var act = () => CreateHandler().Handle(new RemoveReportedContentCommand(report.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _repository.DidNotReceive().SaveReportAsync(Arg.Any<ContentReportRecord>(), Arg.Any<CancellationToken>());
    }
}
