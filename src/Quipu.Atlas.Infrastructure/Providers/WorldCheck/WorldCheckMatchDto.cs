namespace Quipu.Atlas.Infrastructure.Providers.WorldCheck
{
    internal sealed partial class WorldCheckHttpClient
    {
        private sealed record WorldCheckMatchDto(string? ListType, decimal Score, string? Subject);
    }

}
