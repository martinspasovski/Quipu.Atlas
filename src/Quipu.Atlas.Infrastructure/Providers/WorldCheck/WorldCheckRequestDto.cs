using System.Text.Json.Serialization;

namespace Quipu.Atlas.Infrastructure.Providers.WorldCheck
{
    internal sealed partial class WorldCheckHttpClient
    {
        private sealed record WorldCheckRequestDto(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("dateOfBirth")] DateOnly DateOfBirth,
            [property: JsonPropertyName("nationality")] string Nationality);
    }

}
