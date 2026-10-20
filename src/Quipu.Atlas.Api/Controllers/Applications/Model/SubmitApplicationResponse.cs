namespace Quipu.Atlas.Api.Endpoints.Applications.Model;

public sealed record SubmitApplicationResponse(
    Guid ApplicationId,
    string Status,
    string? ReasonCode);
