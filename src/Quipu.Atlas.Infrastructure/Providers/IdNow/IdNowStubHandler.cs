using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Infrastructure.Providers.IdNow
{
    internal sealed class IdNowStubHandler(string scenario, int delayMilliseconds) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await DelayAsync(cancellationToken);

            var body = scenario.ToUpperInvariant() switch
            {
                "VALID" => new { identificationId = "idn_stub_001", documentResult = "VALID", faceMatch = true, confidence = 0.97m },
                "INVALID" => new { identificationId = "idn_stub_001", documentResult = "INVALID", faceMatch = true, confidence = 0.97m },
                "INCONCLUSIVE" => new { identificationId = "idn_stub_001", documentResult = "INCONCLUSIVE", faceMatch = true, confidence = 0.52m },
                "FACE_MISMATCH" => new { identificationId = "idn_stub_001", documentResult = "VALID", faceMatch = false, confidence = 0.21m },
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

}
