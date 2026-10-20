using System.ComponentModel.DataAnnotations;

namespace Quipu.Atlas.Application.Submit;

public static class SubmitApplicationInputValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(
        string? idempotencyKey,
        SubmitApplicationInput? input)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        void Add(string field, string message)
        {
            if (!errors.TryGetValue(field, out var fieldErrors))
            {
                fieldErrors = [];
                errors.Add(field, fieldErrors);
            }

            fieldErrors.Add(message);
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 128)
        {
            Add("Idempotency-Key", "The Idempotency-Key header is required and must not exceed 128 characters.");
        }

        if (input is null)
        {
            Add("request", "An application request body is required.");
            return errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
        }

        if (string.IsNullOrWhiteSpace(input.FirstName) || input.FirstName.Trim().Length > 100)
            Add(nameof(input.FirstName), "FirstName is required and must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(input.LastName) || input.LastName.Trim().Length > 100)
            Add(nameof(input.LastName), "LastName is required and must not exceed 100 characters.");

        if (input.DateOfBirth == default || input.DateOfBirth >= DateOnly.FromDateTime(DateTime.UtcNow))
            Add(nameof(input.DateOfBirth), "DateOfBirth must be a valid date in the past.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(input.Country ?? string.Empty, "^M[A-F]$"))
            Add(nameof(input.Country), "Country must be one of the configured market codes MA through MF.");

        if (string.IsNullOrWhiteSpace(input.Nationality) || input.Nationality.Trim().Length is < 2 or > 3)
            Add(nameof(input.Nationality), "Nationality is required and must contain two or three characters.");

        ValidateIdentifier(input, Add);

        if (string.IsNullOrWhiteSpace(input.IssuingCountry) || input.IssuingCountry.Trim().Length != 3)
            Add(nameof(input.IssuingCountry), "IssuingCountry is required and must contain three characters.");

        if (string.IsNullOrWhiteSpace(input.Email) ||
            input.Email.Trim().Length > 254 ||
            !new EmailAddressAttribute().IsValid(input.Email.Trim()))
            Add(nameof(input.Email), "Email must be valid and must not exceed 254 characters.");

        if (string.IsNullOrWhiteSpace(input.Phone) || input.Phone.Trim().Length > 32)
            Add(nameof(input.Phone), "Phone is required and must not exceed 32 characters.");

        if (!input.TermsAccepted)
            Add(nameof(input.TermsAccepted), "Terms must be accepted before submitting an application.");

        ValidateDocuments(input.Documents, Add);

        return errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
    }

    private static void ValidateIdentifier(
        SubmitApplicationInput input,
        Action<string, string> addError)
    {
        var hasNationalId = !string.IsNullOrWhiteSpace(input.NationalId);
        var hasIdentifierType = !string.IsNullOrWhiteSpace(input.IdentifierType);
        var hasIdentifierValue = !string.IsNullOrWhiteSpace(input.IdentifierValue);

        if (hasNationalId && !hasIdentifierType && !hasIdentifierValue)
        {
            if (input.NationalId!.Trim().Length > 32)
                addError(nameof(input.NationalId), "NationalId must not exceed 32 characters.");
            return;
        }

        if (!hasNationalId && hasIdentifierType && hasIdentifierValue)
        {
            if (!string.Equals(input.IdentifierType, "NATIONAL_ID", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(input.IdentifierType, "PASSPORT", StringComparison.OrdinalIgnoreCase))
            {
                addError(nameof(input.IdentifierType), "IdentifierType must be NATIONAL_ID or PASSPORT.");
            }

            if (input.IdentifierValue!.Trim().Length > 32)
                addError(nameof(input.IdentifierValue), "IdentifierValue must not exceed 32 characters.");

            return;
        }

        addError("Identifier", "Provide either NationalId or both IdentifierType and IdentifierValue.");
    }

    private static void ValidateDocuments(
        IReadOnlyCollection<SubmittedDocumentInput>? documents,
        Action<string, string> addError)
    {
        if (documents is null)
        {
            addError(nameof(SubmitApplicationInput.Documents), "Documents are required.");
            return;
        }

        var identityDocuments = documents.Where(document =>
            IsType(document?.Type, "PASSPORT") || IsType(document?.Type, "ID_CARD")).ToList();
        var selfies = documents.Where(document => IsType(document?.Type, "SELFIE")).ToList();

        if (identityDocuments.Count != 1 || selfies.Count != 1 || documents.Count != 2)
        {
            addError(nameof(SubmitApplicationInput.Documents),
                "Submit exactly one PASSPORT or ID_CARD document and one SELFIE.");
        }

        var index = 0;
        foreach (var document in documents)
        {
            if (document is null)
            {
                addError(nameof(SubmitApplicationInput.Documents), $"Documents[{index}] cannot be null.");
                index++;
                continue;
            }

            if (!IsType(document.Type, "PASSPORT") &&
                !IsType(document.Type, "ID_CARD") &&
                !IsType(document.Type, "SELFIE"))
            {
                addError($"Documents[{index}].Type", "Type must be PASSPORT, ID_CARD, or SELFIE.");
            }

            if (string.IsNullOrWhiteSpace(document.Image))
            {
                addError($"Documents[{index}].Image", "Image is required.");
            }
            else
            {
                try
                {
                    _ = Convert.FromBase64String(document.Image);
                }
                catch (FormatException)
                {
                    addError($"Documents[{index}].Image", "Image must be valid Base64.");
                }
            }

            index++;
        }
    }

    private static bool IsType(string? value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
}
