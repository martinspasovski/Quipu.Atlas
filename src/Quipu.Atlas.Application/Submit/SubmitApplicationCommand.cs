using Quipu.Atlas.Application.Providers.IdNow;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Application.Submit;

public sealed record SubmitApplicationCommand(
    string IdempotencyKey,
    SubmitApplicant Applicant,
    IdNowCheckRequest IdNowCheck);

public sealed record SubmitApplicant(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string MarketCode,
    string NationalityCode,
    IdentifierType IdentifierType,
    string IdentifierValue,
    string IssuingCountryCode,
    string Email,
    string Phone,
    DateTimeOffset TermsAcceptedAtUtc);

public sealed record SubmitApplicationResult(
    Guid ApplicationId,
    ApplicationStatus Status,
    string? ReasonCode,
    bool IsReplay = false,
    bool IsInProgress = false);
