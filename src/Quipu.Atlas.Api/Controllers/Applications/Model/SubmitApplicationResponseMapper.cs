using Quipu.Atlas.Api.Endpoints.Applications.Model;
using Quipu.Atlas.Application.Submit;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Api.Endpoints.Applications.Model;

internal static class SubmitApplicationResponseMapper
{
    public static SubmitApplicationResponse From(SubmitApplicationResult result) =>
        new(result.ApplicationId, ToApiStatus(result.Status), result.ReasonCode);

    private static string ToApiStatus(ApplicationStatus status) => status switch
    {
        ApplicationStatus.ChecksInProgress => "CHECKS_IN_PROGRESS",
        ApplicationStatus.Approved => "APPROVED",
        ApplicationStatus.Rejected => "REJECTED",
        ApplicationStatus.PendingReview => "PENDING_REVIEW",
        ApplicationStatus.ChecksFailed => "CHECKS_FAILED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown application status.")
    };
}
