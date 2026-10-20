using Quipu.Atlas.Application.Providers.IdNow;
using Quipu.Atlas.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Quipu.Atlas.Infrastructure.Providers.IdNow
{
    internal sealed partial class IdNowHttpClient(HttpClient httpClient) : IIdNowProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<ProviderCallResult<IdNowCheckResponse>> CheckAsync(
       IdNowCheckRequest request,
       CancellationToken cancellationToken)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    "v1/identifications",
                    new IdNowRequestDto(request.DocumentType, request.DocumentImage, request.SelfieImage),
                    JsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return ProviderCallResult<IdNowCheckResponse>
                        .Failed($"IDNOW_HTTP_{(int)response.StatusCode}");
                }

                var body = await response.Content.ReadFromJsonAsync<IdNowResponseDto>(
                    JsonOptions,
                    cancellationToken);

                if (body is null ||
                    string.IsNullOrWhiteSpace(body.IdentificationId) ||
                    body.FaceMatch is null ||
                    body.Confidence is null ||
                    !Enum.TryParse<IdNowDocumentResult>(body.DocumentResult, ignoreCase: true, out var documentResult) ||
                    body.Confidence is < 0 or > 1)
                {
                    return ProviderCallResult<IdNowCheckResponse>
                        .Failed("IDNOW_INVALID_RESPONSE");
                }

                return ProviderCallResult<IdNowCheckResponse>
                    .Received(new IdNowCheckResponse(
                        body.IdentificationId,
                        documentResult,
                        body.FaceMatch.Value,
                        body.Confidence.Value));
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return ProviderCallResult<IdNowCheckResponse>
                    .TimedOut("IDNOW_TIMEOUT");
            }
            catch (HttpRequestException)
            {
                return ProviderCallResult<IdNowCheckResponse>
                    .Failed("IDNOW_UNAVAILABLE");
            }
            catch (JsonException)
            {
                return ProviderCallResult<IdNowCheckResponse>
                    .Failed("IDNOW_INVALID_RESPONSE");
            }
        }
    }
}
