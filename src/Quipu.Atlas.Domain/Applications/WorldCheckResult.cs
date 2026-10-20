namespace Quipu.Atlas.Domain.Applications
{
    public sealed class WorldCheckResult
    {
        public Guid ProviderCheckAttemptId { get; set; }
        public string CaseId { get; set; } = string.Empty;
        public WorldCheckScreeningStatus Status { get; set; }
        public ProviderCheckAttempt ProviderCheckAttempt { get; set; } = null!;
        public ICollection<WorldCheckMatch> Matches { get; set; } = new List<WorldCheckMatch>();
    }

}
