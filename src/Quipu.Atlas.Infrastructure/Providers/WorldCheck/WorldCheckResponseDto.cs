namespace Quipu.Atlas.Infrastructure.Providers.WorldCheck
{
    internal sealed partial class WorldCheckHttpClient
    {
        private sealed record WorldCheckResponseDto(
            string? CaseId,
            string? Status,
            IReadOnlyCollection<WorldCheckMatchDto>? Matches);
    }

}
