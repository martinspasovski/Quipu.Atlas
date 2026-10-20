using Microsoft.AspNetCore.Mvc;
using Quipu.Atlas.Api.Endpoints.Applications.Model;
using Quipu.Atlas.Application.Submit;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Api.Endpoints.Applications;

[ApiController]
[Route("applications")]
public sealed class ApplicationsController(ISubmitApplicationService submitApplicationService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SubmitApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SubmitApplicationResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(SubmitApplicationResponse), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<SubmitApplicationResponse>> Submit(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] SubmitApplicationInput input,
        CancellationToken cancellationToken)
    {
        var result = await submitApplicationService.SubmitAsync(
            idempotencyKey,
            input,
            cancellationToken);

        var httpStatusCode = result.IsInProgress
            ? StatusCodes.Status409Conflict
            : result.Status == ApplicationStatus.ChecksFailed
                ? StatusCodes.Status502BadGateway
                : StatusCodes.Status201Created;

        return StatusCode(httpStatusCode, SubmitApplicationResponseMapper.From(result));
    }
}
