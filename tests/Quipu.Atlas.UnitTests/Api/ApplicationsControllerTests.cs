using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Quipu.Atlas.Api.Endpoints.Applications;
using Quipu.Atlas.Application.Submit;
using Quipu.Atlas.Domain.Applications;
using Xunit;

namespace Quipu.Atlas.UnitTests.Api;

public sealed class ApplicationsControllerTests
{
    [Theory]
    [InlineData(ApplicationStatus.Approved, false, 201)]
    [InlineData(ApplicationStatus.PendingReview, false, 201)]
    [InlineData(ApplicationStatus.ChecksFailed, false, 502)]
    [InlineData(ApplicationStatus.ChecksInProgress, true, 409)]
    public async Task Submit_should_map_application_result_to_the_expected_http_status(
        ApplicationStatus status,
        bool isInProgress,
        int expectedStatusCode)
    {
        var applicationId = Guid.NewGuid();
        var service = new Mock<ISubmitApplicationService>();
        service.Setup(applicationService => applicationService.SubmitAsync(
                "request-1", It.IsAny<SubmitApplicationInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmitApplicationResult(
                applicationId,
                status,
                status == ApplicationStatus.ChecksFailed ? "PROVIDER_CHECK_FAILED" : null,
                IsInProgress: isInProgress));
        var controller = new ApplicationsController(service.Object);

        var action = await controller.Submit("request-1", new SubmitApplicationInput(), CancellationToken.None);

        var result = action.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Submit_should_forward_the_idempotency_key_and_input_to_the_application_service()
    {
        var service = new Mock<ISubmitApplicationService>();
        service.Setup(applicationService => applicationService.SubmitAsync(
                It.IsAny<string?>(), It.IsAny<SubmitApplicationInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmitApplicationResult(Guid.NewGuid(), ApplicationStatus.Approved, null));
        var controller = new ApplicationsController(service.Object);
        var input = new SubmitApplicationInput { FirstName = "Ada" };

        await controller.Submit("request-1", input, CancellationToken.None);

        service.Verify(applicationService => applicationService.SubmitAsync(
            "request-1", input, It.IsAny<CancellationToken>()), Times.Once);
    }
}
