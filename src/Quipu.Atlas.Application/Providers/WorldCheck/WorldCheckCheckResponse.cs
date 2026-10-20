using Quipu.Atlas.Domain.Applications;

public sealed record WorldCheckMatchResponse(
    WorldCheckListType ListType,
    decimal Score,
    string Subject);
public sealed record WorldCheckCheckResponse(
    string CaseId,
    WorldCheckScreeningStatus Status,
    IReadOnlyCollection<WorldCheckMatchResponse> Matches);
