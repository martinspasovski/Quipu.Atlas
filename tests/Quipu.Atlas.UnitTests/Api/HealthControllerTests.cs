using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Quipu.Atlas.Api.Controllers;
using Xunit;

namespace Quipu.Atlas.UnitTests.Api;

public sealed class HealthControllerTests
{
    [Fact]
    public void Get_should_return_a_healthy_response()
    {
        var controller = new HealthController();

        var action = controller.Get();

        var result = action.Result.Should().BeOfType<OkObjectResult>().Subject;
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<HealthResponse>()
            .Which.Status.Should().Be("healthy");
    }
}
