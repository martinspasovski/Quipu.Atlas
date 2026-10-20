using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Api.Persistence
{
    public interface IOnboardingApplicationStore
    {
        Task<bool> TryCreateAsync(OnboardingApplication application, CancellationToken cancellationToken);

        Task<OnboardingApplication?> FindByIdempotencyKeyAsync(
            string idempotencyKey,
            CancellationToken cancellationToken);

        Task SaveCheckOutcomeAsync(
            Guid applicationId,
            IReadOnlyCollection<ProviderCheckAttempt> providerAttempts,
            ApplicationStatus status,
            string? reasonCode,
            DateTimeOffset updatedAtUtc,
            CancellationToken cancellationToken);
    }

}
