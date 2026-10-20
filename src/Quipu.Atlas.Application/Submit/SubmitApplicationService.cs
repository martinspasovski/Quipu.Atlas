using Quipu.Atlas.Api.Persistence;
using Quipu.Atlas.Application.Providers.IdNow;
using Quipu.Atlas.Application.Providers.WorldCheck;
using Quipu.Atlas.Domain.Applications;
using Quipu.Atlas.Domain.Decisions;
using System.Security.Cryptography;
using System.Text.Json;

namespace Quipu.Atlas.Application.Submit;

public sealed class SubmitApplicationService(
    IOnboardingApplicationStore applicationStore,
    IIdNowProvider idNowProvider,
    IWorldCheckProvider worldCheckProvider,
    OnboardingDecisionService decisionService) : ISubmitApplicationService
{
    private static readonly TimeSpan ProcessingLeaseDuration = TimeSpan.FromMinutes(1);

    public async Task<SubmitApplicationResult> SubmitAsync(
        string? idempotencyKey,
        SubmitApplicationInput? input,
        CancellationToken cancellationToken)
    {
        var errors = SubmitApplicationInputValidator.Validate(idempotencyKey, input);
        if (errors.Count > 0)
        {
            throw new SubmitApplicationValidationException(errors);
        }

        var validInput = input!;
        var identityDocument = validInput.Documents.Single(document =>
            IsDocumentType(document.Type, "PASSPORT") || IsDocumentType(document.Type, "ID_CARD"));
        var selfie = validInput.Documents.Single(document => IsDocumentType(document.Type, "SELFIE"));

        var command = new SubmitApplicationCommand(
            idempotencyKey!,
            new SubmitApplicant(
                validInput.FirstName.Trim(),
                validInput.LastName.Trim(),
                validInput.DateOfBirth,
                validInput.Country.Trim().ToUpperInvariant(),
                validInput.Nationality.Trim().ToUpperInvariant(),
                IdentifierType.NationalId,
                validInput.NationalId!.Trim(),
                validInput.IssuingCountry.Trim().ToUpperInvariant(),
                validInput.Email.Trim(),
                validInput.Phone.Trim(),
                DateTimeOffset.UtcNow),
            new IdNowCheckRequest(
                identityDocument.Type.ToUpperInvariant(),
                identityDocument.Image,
                selfie.Image));

        return await ProcessCommandAsync(command, cancellationToken);
    }

    private async Task<SubmitApplicationResult> ProcessCommandAsync(
        SubmitApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var nationalityCode = command.Applicant.NationalityCode;

        var now = DateTimeOffset.UtcNow;
        var requestHash = CreateRequestHash(command);
        var processingToken = Guid.NewGuid();
        var application = new OnboardingApplication
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = command.IdempotencyKey,
            RequestHash = requestHash,
            Status = ApplicationStatus.ChecksInProgress,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            ProcessingToken = processingToken,
            ProcessingLeaseExpiresAtUtc = now.Add(ProcessingLeaseDuration),
            Applicant = new ApplicantDetails
            {
                FirstName = command.Applicant.FirstName,
                LastName = command.Applicant.LastName,
                DateOfBirth = command.Applicant.DateOfBirth,
                MarketCode = command.Applicant.MarketCode,
                NationalityCode = command.Applicant.NationalityCode,
                IdentifierType = command.Applicant.IdentifierType,
                IdentifierValue = command.Applicant.IdentifierValue,
                IssuingCountryCode = command.Applicant.IssuingCountryCode,
                Email = command.Applicant.Email,
                Phone = command.Applicant.Phone,
                TermsAcceptedAtUtc = command.Applicant.TermsAcceptedAtUtc
            }
        };
        application.Applicant.Application = application;

        var created = await applicationStore.TryCreateAsync(application, cancellationToken);
        if (!created)
        {
            var existing = await applicationStore.FindByIdempotencyKeyAsync(
                command.IdempotencyKey,
                cancellationToken);

            if (existing is null)
            {
                throw new InvalidOperationException(
                    "The idempotency key was reported as duplicate, but no application was found.");
            }

            if (!CryptographicOperations.FixedTimeEquals(existing.RequestHash, requestHash))
            {
                throw new IdempotencyKeyConflictException(command.IdempotencyKey);
            }

            if (existing.Status != ApplicationStatus.ChecksInProgress)
            {
                return new(existing.Id, existing.Status, existing.DecisionReasonCode, IsReplay: true);
            }

            now = DateTimeOffset.UtcNow;
            processingToken = Guid.NewGuid();
            var leaseAcquired = await applicationStore.TryAcquireProcessingLeaseAsync(
                existing.Id,
                processingToken,
                now,
                now.Add(ProcessingLeaseDuration),
                cancellationToken);

            if (!leaseAcquired)
            {
                return new(
                    existing.Id,
                    existing.Status,
                    existing.DecisionReasonCode,
                    IsReplay: true,
                    IsInProgress: true);
            }

            application = existing;
        }

        var applicant = command.Applicant;

        // Once the application is persisted, provider work is bounded by each HttpClient timeout
        // and continues if the mobile connection closes before the response is returned.
        var idNowTask = ExecuteTimedAsync(
            () => idNowProvider.CheckAsync(command.IdNowCheck, CancellationToken.None));

        var worldCheckTask = ExecuteTimedAsync(() => worldCheckProvider.CheckAsync(
            new WorldCheckCheckRequest(
                $"{applicant.FirstName} {applicant.LastName}".Trim(),
                applicant.DateOfBirth,
                nationalityCode),
            CancellationToken.None));

        await Task.WhenAll(new Task[] { idNowTask, worldCheckTask });

        var idNowOutcome = await idNowTask;
        var worldCheckOutcome = await worldCheckTask;
        var completedAt = DateTimeOffset.UtcNow;

        var idNowAttempt = CreateIdNowAttempt(application.Id, idNowOutcome);
        var worldCheckAttempt = CreateWorldCheckAttempt(application.Id, worldCheckOutcome);
        var decision = decisionService.Decide(idNowAttempt, worldCheckAttempt);

        await applicationStore.SaveCheckOutcomeAsync(
            application.Id,
            processingToken,
            [idNowAttempt, worldCheckAttempt],
            decision.Status,
            decision.ReasonCode,
            completedAt,
            CancellationToken.None);

        return new(application.Id, decision.Status, decision.ReasonCode);
    }

    private static bool IsDocumentType(string? value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static byte[] CreateRequestHash(SubmitApplicationCommand command)
    {
        var hashInput = JsonSerializer.SerializeToUtf8Bytes(new
        {
            command.Applicant.FirstName,
            command.Applicant.LastName,
            command.Applicant.DateOfBirth,
            command.Applicant.MarketCode,
            command.Applicant.NationalityCode,
            command.Applicant.IdentifierType,
            command.Applicant.IdentifierValue,
            command.Applicant.IssuingCountryCode,
            command.Applicant.Email,
            command.Applicant.Phone,
            command.IdNowCheck.DocumentType,
            command.IdNowCheck.DocumentImage,
            command.IdNowCheck.SelfieImage
        });

        return SHA256.HashData(hashInput);
    }

    private static ProviderCheckAttempt CreateIdNowAttempt(
        Guid applicationId,
        TimedProviderOutcome<IdNowCheckResponse> timedOutcome)
    {
        var outcome = timedOutcome.Result;
        var attempt = new ProviderCheckAttempt
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Provider = ProviderType.IdNow,
            AttemptNumber = 1,
            Status = outcome.Status,
            FailureCode = outcome.FailureCode,
            StartedAtUtc = timedOutcome.StartedAtUtc,
            CompletedAtUtc = timedOutcome.CompletedAtUtc
        };

        if (outcome.Value is { } value)
        {
            attempt.ProviderReference = value.ProviderReference;
            attempt.IdNowResult = new IdNowResult
            {
                ProviderCheckAttemptId = attempt.Id,
                DocumentResult = value.DocumentResult,
                FaceMatch = value.FaceMatch,
                Confidence = value.Confidence,
                ProviderCheckAttempt = attempt
            };
        }

        return attempt;
    }

    private static ProviderCheckAttempt CreateWorldCheckAttempt(
        Guid applicationId,
        TimedProviderOutcome<WorldCheckCheckResponse> timedOutcome)
    {
        var outcome = timedOutcome.Result;
        var attempt = new ProviderCheckAttempt
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Provider = ProviderType.WorldCheck,
            AttemptNumber = 1,
            Status = outcome.Status,
            FailureCode = outcome.FailureCode,
            StartedAtUtc = timedOutcome.StartedAtUtc,
            CompletedAtUtc = timedOutcome.CompletedAtUtc
        };

        if (outcome.Value is { } value)
        {
            attempt.ProviderReference = value.CaseId;

            var worldCheckResult = new WorldCheckResult
            {
                ProviderCheckAttemptId = attempt.Id,
                CaseId = value.CaseId,
                Status = value.Status,
                ProviderCheckAttempt = attempt,
                Matches = value.Matches.Select(match => new WorldCheckMatch
                {
                    Id = Guid.NewGuid(),
                    ListType = match.ListType,
                    Score = match.Score,
                    Subject = match.Subject
                }).ToList()
            };

            attempt.WorldCheckResult = worldCheckResult;

            foreach (var match in worldCheckResult.Matches)
            {
                match.WorldCheckResult = worldCheckResult;
            }
        }

        return attempt;
    }

    private static async Task<TimedProviderOutcome<T>> ExecuteTimedAsync<T>(
        Func<Task<ProviderCallResult<T>>> operation)
        where T : class
    {
        var startedAt = DateTimeOffset.UtcNow;
        var result = await operation();
        return new TimedProviderOutcome<T>(result, startedAt, DateTimeOffset.UtcNow);
    }

    private sealed record TimedProviderOutcome<T>(
        ProviderCallResult<T> Result,
        DateTimeOffset StartedAtUtc,
        DateTimeOffset CompletedAtUtc)
        where T : class;
}

public sealed class IdempotencyKeyConflictException(string idempotencyKey)
    : Exception($"Idempotency key '{idempotencyKey}' was already used for a different request.");