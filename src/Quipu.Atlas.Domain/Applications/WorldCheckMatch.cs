namespace Quipu.Atlas.Domain.Applications
{
    public sealed class WorldCheckMatch
    {
        public Guid Id { get; set; }
        public Guid WorldCheckResultId { get; set; }
        public WorldCheckListType ListType { get; set; }
        public decimal Score { get; set; }
        public string Subject { get; set; } = string.Empty;
        public WorldCheckResult WorldCheckResult { get; set; } = null!;
    }

}
