using Quipu.Atlas.Application.Submit;

namespace Quipu.Atlas.UnitTests.TestData;

internal static class SubmitApplicationInputFactory
{
    public static SubmitApplicationInput Create() => new()
    {
        FirstName = " Ada ",
        LastName = " Lovelace ",
        DateOfBirth = new DateOnly(1815, 12, 10),
        Country = "MA",
        Nationality = "GBR",
        NationalId = "123456",
        IssuingCountry = "GBR",
        Email = "ada@example.com",
        Phone = "+44123456789",
        TermsAccepted = true,
        Documents =
        [
            new SubmittedDocumentInput { Type = "PASSPORT", Image = "aW1hZ2U=" },
            new SubmittedDocumentInput { Type = "SELFIE", Image = "c2VsZmll" }
        ]
    };
}
