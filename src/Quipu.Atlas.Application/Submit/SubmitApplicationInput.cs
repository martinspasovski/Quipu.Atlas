namespace Quipu.Atlas.Application.Submit;

// Input to the application use case. The API binds its JSON body to this transport-neutral model.
public sealed class SubmitApplicationInput
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public string Country { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string? NationalId { get; init; }
    public string IssuingCountry { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public IReadOnlyCollection<SubmittedDocumentInput> Documents { get; init; } = [];
    public bool TermsAccepted { get; init; }
}

public sealed class SubmittedDocumentInput
{
    public string Type { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
}