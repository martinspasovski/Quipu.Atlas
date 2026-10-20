using System.Net;
using System.Net.Http.Json;

namespace Quipu.Atlas.Infrastructure.Providers.WorldCheck;

internal sealed class WorldCheckStubHandler(string scenario, int delayMilliseconds) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await DelayAsync(cancellationToken);

        var body = scenario.ToUpperInvariant() switch
        {
            "CLEAR" => new { caseId = "wc_stub_001", status = "CLEAR", matches = Array.Empty<object>() },
            "POSSIBLE_PEP_MATCH" => new
            {
                caseId = "wc_stub_001",
                status = "POSSIBLE_MATCH",
                matches = new object[] { new { listType = "PEP", score = 0.82m, subject = "Example PEP match" } }
            },
            "POSSIBLE_SANCTIONS_MATCH" => new
            {
                caseId = "wc_stub_001",
                status = "POSSIBLE_MATCH",
                matches = new object[] { new { listType = "SANCTIONS", score = 0.91m, subject = "Example sanctions match" } }
            },
            "HTTP_ERROR" => null,
            _ => null
        };

        if (body is null)
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(body)
        };
    }

    private Task DelayAsync(CancellationToken cancellationToken) =>
        delayMilliseconds > 0
            ? Task.Delay(delayMilliseconds, cancellationToken)
            : Task.CompletedTask;
}
