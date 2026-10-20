using FluentAssertions;
using Quipu.Atlas.Domain.Applications;
using Quipu.Atlas.Domain.Decisions;
using Xunit;

namespace Quipu.Atlas.UnitTests.Domain;

public sealed class OnboardingDecisionServiceTests
{
    private readonly OnboardingDecisionService _sut = new();

    [Theory]
    [InlineData(IdNowDocumentResult.Invalid, "IDNOW_DOCUMENT_INVALID")]
    [InlineData(IdNowDocumentResult.Inconclusive, "IDNOW_INCONCLUSIVE")]
    public void Decide_should_reject_invalid_or_inconclusive_identity_checks(
        IdNowDocumentResult documentResult,
        string expectedReason)
    {
        var decision = _sut.Decide(
            CreateIdNowAttempt(documentResult: documentResult),
            CreateWorldCheckAttempt());

        decision.Status.Should().Be(ApplicationStatus.Rejected);
        decision.ReasonCode.Should().Be(expectedReason);
    }

    [Fact]
    public void Decide_should_reject_when_face_match_fails()
    {
        var decision = _sut.Decide(
            CreateIdNowAttempt(faceMatch: false),
            CreateWorldCheckAttempt());

        decision.Status.Should().Be(ApplicationStatus.Rejected);
        decision.ReasonCode.Should().Be("IDNOW_FACE_MISMATCH");
    }

    [Fact]
    public void Decide_should_return_checks_failed_when_a_provider_does_not_respond()
    {
        var decision = _sut.Decide(
            CreateIdNowAttempt(status: ProviderAttemptStatus.TimedOut, failureCode: "IDNOW_TIMEOUT"),
            CreateWorldCheckAttempt());

        decision.Status.Should().Be(ApplicationStatus.ChecksFailed);
        decision.ReasonCode.Should().Be("IDNOW_TIMEOUT");
    }

    [Fact]
    public void Decide_should_use_world_check_failure_code_when_identity_check_has_no_failure_code()
    {
        var decision = _sut.Decide(
            CreateIdNowAttempt(status: ProviderAttemptStatus.Failed),
            CreateWorldCheckAttempt(status: ProviderAttemptStatus.Failed, failureCode: "WORLDCHECK_UNAVAILABLE"));

        decision.Status.Should().Be(ApplicationStatus.ChecksFailed);
        decision.ReasonCode.Should().Be("WORLDCHECK_UNAVAILABLE");
    }

    [Fact]
    public void Decide_should_use_default_failure_code_when_both_providers_have_no_failure_code()
    {
        var decision = _sut.Decide(
            CreateIdNowAttempt(status: ProviderAttemptStatus.Failed),
            CreateWorldCheckAttempt(status: ProviderAttemptStatus.Failed));

        decision.Status.Should().Be(ApplicationStatus.ChecksFailed);
        decision.ReasonCode.Should().Be("PROVIDER_CHECK_FAILED");
    }

    [Fact]
    public void Decide_should_send_possible_matches_to_manual_review()
    {
        var decision = _sut.Decide(
            CreateIdNowAttempt(),
            CreateWorldCheckAttempt(WorldCheckScreeningStatus.PossibleMatch));

        decision.Status.Should().Be(ApplicationStatus.PendingReview);
        decision.ReasonCode.Should().Be("WORLDCHECK_POSSIBLE_MATCH");
    }

    [Fact]
    public void Decide_should_approve_clear_checks()
    {
        var decision = _sut.Decide(
            CreateIdNowAttempt(),
            CreateWorldCheckAttempt(WorldCheckScreeningStatus.Clear));

        decision.Status.Should().Be(ApplicationStatus.Approved);
        decision.ReasonCode.Should().BeNull();
    }

    [Fact]
    public void Decide_should_fail_when_identity_response_is_missing()
    {
        var decision = _sut.Decide(
            new ProviderCheckAttempt { Status = ProviderAttemptStatus.ResponseReceived },
            CreateWorldCheckAttempt());

        decision.Status.Should().Be(ApplicationStatus.ChecksFailed);
        decision.ReasonCode.Should().Be("PROVIDER_CHECK_FAILED");
    }

    private static ProviderCheckAttempt CreateIdNowAttempt(
        ProviderAttemptStatus status = ProviderAttemptStatus.ResponseReceived,
        IdNowDocumentResult documentResult = IdNowDocumentResult.Valid,
        bool faceMatch = true,
        string? failureCode = null) => new()
        {
            Provider = ProviderType.IdNow,
            Status = status,
            FailureCode = failureCode,
            IdNowResult = new IdNowResult
            {
                DocumentResult = documentResult,
                FaceMatch = faceMatch
            }
        };

    private static ProviderCheckAttempt CreateWorldCheckAttempt(
        WorldCheckScreeningStatus screeningStatus = WorldCheckScreeningStatus.Clear,
        ProviderAttemptStatus status = ProviderAttemptStatus.ResponseReceived,
        string? failureCode = null) => new()
        {
            Provider = ProviderType.WorldCheck,
            Status = status,
            FailureCode = failureCode,
            WorldCheckResult = new WorldCheckResult { Status = screeningStatus }
        };
}
