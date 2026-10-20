namespace Quipu.Atlas.Infrastructure.Providers.IdNow
{
    internal sealed partial class IdNowHttpClient
    {
        private sealed record IdNowResponseDto(
            string? IdentificationId,
            string? DocumentResult,
            bool? FaceMatch,
            decimal? Confidence);
    }
}
