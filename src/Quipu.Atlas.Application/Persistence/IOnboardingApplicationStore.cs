using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Api.Persistence
{
    public interface IOnboardingApplicationStore
    {
        Task<bool> TryCreateAsync(OnboardingApplication application, CancellationToken cancellationToken);

        Task<OnboardingApplication?> FindByIdempotencyKeyAsync(
            string idempotencyKey,
            CancellationToken cancellationToken);

        Task<bool> TryAcquireProcessingLeaseAsync(
            Guid applicationId,
            Guid processingToken,
            DateTimeOffset nowUtc,
            DateTimeOffset leaseExpiresAtUtc,
            CancellationToken cancellationToken);


        Task SaveCheckOutcomeAsync(
            Guid applicationId,
            Guid processingToken,
            IReadOnlyCollection<ProviderCheckAttempt> providerAttempts,
            ApplicationStatus status,
            string? reasonCode,
            DateTimeOffset updatedAtUtc,
            CancellationToken cancellationToken);
    }

}
