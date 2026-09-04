using FluentAssertions;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Exceptions;
using Xunit;

namespace SentinelQA.UnitTests.Domain;

public sealed class ChangeRequestTests
{
    private static ChangeRequest Opened() =>
        ChangeRequest.Open(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), "Open port 443 for the new API.");

    [Fact]
    public void Happy_path_follows_the_full_state_machine()
    {
        var approver = Guid.CreateVersion7();
        var requester = Guid.CreateVersion7();
        var cr = ChangeRequest.Open(Guid.CreateVersion7(), Guid.CreateVersion7(), requester, "reason");

        cr.Submit();
        cr.BeginValidation();
        cr.CompleteValidation(passed: true);
        cr.AttachTestRun(Guid.CreateVersion7());
        cr.CompleteTesting(passed: true);
        cr.Approve(approver);
        cr.BeginDeployment();
        cr.CompleteDeployment(success: true, "SIM-123");
        cr.CompleteVerification(passed: true);

        cr.State.Should().Be(ChangeRequestState.Completed);
        cr.DeploymentRef.Should().Be("SIM-123");
    }

    [Fact]
    public void Cannot_approve_before_testing_passes()
    {
        var cr = Opened();
        cr.Submit();

        var act = () => cr.Approve(Guid.CreateVersion7());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Requester_cannot_approve_own_change()
    {
        var requester = Guid.CreateVersion7();
        var cr = ChangeRequest.Open(Guid.CreateVersion7(), Guid.CreateVersion7(), requester, "reason");

        cr.Submit();
        cr.BeginValidation();
        cr.CompleteValidation(true);
        cr.CompleteTesting(true);

        var act = () => cr.Approve(requester);

        act.Should().Throw<DomainException>().WithMessage("*segregation of duties*");
    }

    [Fact]
    public void Failed_change_can_retry_from_testing()
    {
        var cr = Opened();
        cr.Submit();
        cr.BeginValidation();
        cr.CompleteValidation(true);
        cr.CompleteTesting(passed: false);

        cr.State.Should().Be(ChangeRequestState.Failed);

        cr.Retry();

        cr.State.Should().Be(ChangeRequestState.Testing);
    }
}