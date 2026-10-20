using Quipu.Atlas.Application.Providers.WorldCheck;
using Quipu.Atlas.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Quipu.Atlas.Infrastructure.Providers.WorldCheck
{
    internal sealed partial class WorldCheckHttpClient(HttpClient httpClient) : IWorldCheckProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<ProviderCallResult<WorldCheckCheckResponse>> CheckAsync(
            WorldCheckCheckRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    "worldcheck/v2/screen",
                    new WorldCheckRequestDto(request.Name, request.DateOfBirth, request.Nationality),
                    JsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return ProviderCallResult<WorldCheckCheckResponse>.Failed(
                        $"WORLDCHECK_HTTP_{(int)response.StatusCode}");
                }

                var body = await response.Content.ReadFromJsonAsync<WorldCheckResponseDto>(
                    JsonOptions,
                    cancellationToken);

                if (body is null || string.IsNullOrWhiteSpace(body.CaseId) ||
                    !WorldCheckHttpClientHelpers.TryParseStatus(body.Status, out var status) || body.Matches is null)
                {
                    return ProviderCallResult<WorldCheckCheckResponse>
                        .Failed("WORLDCHECK_INVALID_RESPONSE");
                }

                var matches = new List<WorldCheckMatchResponse>();
                foreach (var match in body.Matches)
                {
                    if (!WorldCheckHttpClientHelpers.TryParseListType(match.ListType, out var listType) ||
                        string.IsNullOrWhiteSpace(match.Subject) || match.Score is < 0 or > 1)
                    {
                        return ProviderCallResult<WorldCheckCheckResponse>
                            .Failed("WORLDCHECK_INVALID_RESPONSE");
                    }

                    matches.Add(new WorldCheckMatchResponse(listType, match.Score, match.Subject));
                }

                return ProviderCallResult<WorldCheckCheckResponse>
                    .Received(new WorldCheckCheckResponse(
                        body.CaseId,
                        status,
                        matches));
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return ProviderCallResult<WorldCheckCheckResponse>
                    .TimedOut("WORLDCHECK_TIMEOUT");
            }
            catch (HttpRequestException)
            {
                return ProviderCallResult<WorldCheckCheckResponse>
                    .Failed("WORLDCHECK_UNAVAILABLE");
            }
            catch (JsonException)
            {
                return ProviderCallResult<WorldCheckCheckResponse>
                    .Failed("WORLDCHECK_INVALID_RESPONSE");
            }
        }
    }
}