using System.Text.Json.Serialization;

namespace Quipu.Atlas.Infrastructure.Providers.IdNow
{
    internal sealed partial class IdNowHttpClient
    {
        private sealed record IdNowRequestDto(
            [property: JsonPropertyName("documentType")] string DocumentType,
            [property: JsonPropertyName("documentImage")] string DocumentImage,
            [property: JsonPropertyName("selfieImage")] string SelfieImage);
    }
}
