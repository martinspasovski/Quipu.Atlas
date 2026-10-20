namespace Quipu.Atlas.Domain.Applications
{
    public sealed class IdNowResult
    {
        public Guid ProviderCheckAttemptId { get; set; }
        public IdNowDocumentResult DocumentResult { get; set; }
        public bool FaceMatch { get; set; }
        public decimal Confidence { get; set; }
        public ProviderCheckAttempt ProviderCheckAttempt { get; set; } = null!;
    }

}
