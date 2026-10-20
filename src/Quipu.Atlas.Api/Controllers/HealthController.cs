using Microsoft.AspNetCore.Mvc;

namespace Quipu.Atlas.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse("healthy"));
    }
}

public sealed record HealthResponse(string Status);