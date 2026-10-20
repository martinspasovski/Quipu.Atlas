namespace Quipu.Atlas.Domain.Applications
{
    public sealed class ProviderCheckAttempt
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public ProviderType Provider { get; set; }
        public int AttemptNumber { get; set; }
        public ProviderAttemptStatus Status { get; set; } = ProviderAttemptStatus.InProgress;
        public string? ProviderReference { get; set; }
        public string? FailureCode { get; set; }
        public DateTimeOffset StartedAtUtc { get; set; }
        public DateTimeOffset? CompletedAtUtc { get; set; }
        public IdNowResult? IdNowResult { get; set; }
        public WorldCheckResult? WorldCheckResult { get; set; }
        public OnboardingApplication Application { get; set; } = null!;
    }

}
