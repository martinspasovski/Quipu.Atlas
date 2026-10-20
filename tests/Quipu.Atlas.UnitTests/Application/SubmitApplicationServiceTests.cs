using FluentAssertions;
using Moq;
using Quipu.Atlas.Api.Persistence;
using Quipu.Atlas.Application.Providers;
using Quipu.Atlas.Application.Providers.IdNow;
using Quipu.Atlas.Application.Providers.WorldCheck;
using Quipu.Atlas.Application.Submit;
using Quipu.Atlas.Domain.Applications;
using Quipu.Atlas.Domain.Decisions;
using Quipu.Atlas.UnitTests.TestData;
using Xunit;

namespace Quipu.Atlas.UnitTests.Application;

public sealed class SubmitApplicationServiceTests
{
    [Fact]
    public async Task SubmitAsync_should_call_both_providers_and_save_an_approved_outcome()
    {
        var store = CreateStore();
        var idNow = new Mock<IIdNowProvider>();
        var worldCheck = new Mock<IWorldCheckProvider>();
        ConfigureSuccessfulProviders(idNow, worldCheck);
        var sut = CreateService(store, idNow, worldCheck);

        var result = await sut.SubmitAsync("request-1", SubmitApplicationInputFactory.Create(), CancellationToken.None);

        result.Status.Should().Be(ApplicationStatus.Approved);
        result.ReasonCode.Should().BeNull();
        idNow.Verify(provider => provider.CheckAsync(
            It.Is<IdNowCheckRequest>(request => request.DocumentType == "PASSPORT"),
            It.IsAny<CancellationToken>()), Times.Once);
        worldCheck.Verify(provider => provider.CheckAsync(
            It.Is<WorldCheckCheckRequest>(request => request.Name == "Ada Lovelace" && request.Nationality == "GBR"),
            It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(repository => repository.SaveCheckOutcomeAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.Is<IReadOnlyCollection<ProviderCheckAttempt>>(attempts => attempts.Count == 2),
            ApplicationStatus.Approved,
            null,
            It.IsAny<DateTimeOffset>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_should_return_pending_review_for_a_possible_match()
    {
        var store = CreateStore();
        var idNow = new Mock<IIdNowProvider>();
        var worldCheck = new Mock<IWorldCheckProvider>();
        idNow.Setup(provider => provider.CheckAsync(It.IsAny<IdNowCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderCallResult<IdNowCheckResponse>.Received(
                new("id-1", IdNowDocumentResult.Valid, true, 0.99m)));
        worldCheck.Setup(provider => provider.CheckAsync(It.IsAny<WorldCheckCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderCallResult<WorldCheckCheckResponse>.Received(
                new("case-1", WorldCheckScreeningStatus.PossibleMatch, [])));
        var sut = CreateService(store, idNow, worldCheck);

        var result = await sut.SubmitAsync("request-1", SubmitApplicationInputFactory.Create(), CancellationToken.None);

        result.Status.Should().Be(ApplicationStatus.PendingReview);
        result.ReasonCode.Should().Be("WORLDCHECK_POSSIBLE_MATCH");
    }

    [Fact]
    public async Task SubmitAsync_should_save_a_failed_outcome_when_a_provider_times_out()
    {
        var store = CreateStore();
        var idNow = new Mock<IIdNowProvider>();
        var worldCheck = new Mock<IWorldCheckProvider>();
        idNow.Setup(provider => provider.CheckAsync(It.IsAny<IdNowCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderCallResult<IdNowCheckResponse>.TimedOut("IDNOW_TIMEOUT"));
        worldCheck.Setup(provider => provider.CheckAsync(It.IsAny<WorldCheckCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderCallResult<WorldCheckCheckResponse>.Received(
                new("case-1", WorldCheckScreeningStatus.Clear, [])));
        var sut = CreateService(store, idNow, worldCheck);

        var result = await sut.SubmitAsync("request-1", SubmitApplicationInputFactory.Create(), CancellationToken.None);

        result.Status.Should().Be(ApplicationStatus.ChecksFailed);
        result.ReasonCode.Should().Be("IDNOW_TIMEOUT");
    }

    [Fact]
    public async Task SubmitAsync_should_replay_a_completed_duplicate_without_calling_providers()
    {
        var store = CreateStore(createResult: false);
        var idNow = new Mock<IIdNowProvider>();
        var worldCheck = new Mock<IWorldCheckProvider>();
        OnDuplicate(store, ApplicationStatus.Approved, requestHashFromCreatedApplication: true);
        var sut = CreateService(store, idNow, worldCheck);

        var result = await sut.SubmitAsync("request-1", SubmitApplicationInputFactory.Create(), CancellationToken.None);

        result.IsReplay.Should().BeTrue();
        result.Status.Should().Be(ApplicationStatus.Approved);
        idNow.VerifyNoOtherCalls();
        worldCheck.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SubmitAsync_should_reject_a_duplicate_key_with_a_different_request()
    {
        var store = CreateStore(createResult: false);
        OnDuplicate(store, ApplicationStatus.Approved, requestHashFromCreatedApplication: false);
        var sut = CreateService(store, new Mock<IIdNowProvider>(), new Mock<IWorldCheckProvider>());

        var action = () => sut.SubmitAsync("request-1", SubmitApplicationInputFactory.Create(), CancellationToken.None);

        await action.Should().ThrowAsync<IdempotencyKeyConflictException>();
    }

    [Fact]
    public async Task SubmitAsync_should_report_an_existing_application_as_in_progress_when_lease_cannot_be_acquired()
    {
        var store = CreateStore(createResult: false);
        OnDuplicate(store, ApplicationStatus.ChecksInProgress, requestHashFromCreatedApplication: true);
        store.Setup(repository => repository.TryAcquireProcessingLeaseAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var sut = CreateService(store, new Mock<IIdNowProvider>(), new Mock<IWorldCheckProvider>());

        var result = await sut.SubmitAsync("request-1", SubmitApplicationInputFactory.Create(), CancellationToken.None);

        result.IsReplay.Should().BeTrue();
        result.IsInProgress.Should().BeTrue();
        result.Status.Should().Be(ApplicationStatus.ChecksInProgress);
    }

    private static SubmitApplicationService CreateService(
        Mock<IOnboardingApplicationStore> store,
        Mock<IIdNowProvider> idNow,
        Mock<IWorldCheckProvider> worldCheck) =>
        new(store.Object, idNow.Object, worldCheck.Object, new OnboardingDecisionService());

    private static Mock<IOnboardingApplicationStore> CreateStore(bool createResult = true)
    {
        var store = new Mock<IOnboardingApplicationStore>();
        store.Setup(repository => repository.TryCreateAsync(It.IsAny<OnboardingApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createResult);
        return store;
    }

    private static void OnDuplicate(
        Mock<IOnboardingApplicationStore> store,
        ApplicationStatus status,
        bool requestHashFromCreatedApplication)
    {
        OnboardingApplication? created = null;
        store.Setup(repository => repository.TryCreateAsync(It.IsAny<OnboardingApplication>(), It.IsAny<CancellationToken>()))
            .Callback<OnboardingApplication, CancellationToken>((application, _) => created = application)
            .ReturnsAsync(false);
        store.Setup(repository => repository.FindByIdempotencyKeyAsync("request-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new OnboardingApplication
            {
                Id = Guid.NewGuid(),
                RequestHash = requestHashFromCreatedApplication ? created?.RequestHash ?? [] : [1, 2, 3],
                Status = status
            });
    }

    private static void ConfigureSuccessfulProviders(Mock<IIdNowProvider> idNow, Mock<IWorldCheckProvider> worldCheck)
    {
        idNow.Setup(provider => provider.CheckAsync(It.IsAny<IdNowCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderCallResult<IdNowCheckResponse>.Received(
                new("id-1", IdNowDocumentResult.Valid, true, 0.99m)));
        worldCheck.Setup(provider => provider.CheckAsync(It.IsAny<WorldCheckCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderCallResult<WorldCheckCheckResponse>.Received(
                new("case-1", WorldCheckScreeningStatus.Clear, [])));
    }

}
