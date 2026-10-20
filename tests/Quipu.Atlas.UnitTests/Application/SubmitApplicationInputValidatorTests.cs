using FluentAssertions;
using Quipu.Atlas.Application.Submit;
using Quipu.Atlas.UnitTests.TestData;
using Xunit;

namespace Quipu.Atlas.UnitTests.Application;

public sealed class SubmitApplicationInputValidatorTests
{
    [Fact]
    public void Validate_should_accept_a_well_formed_application()
    {
        var errors = SubmitApplicationInputValidator.Validate("request-1", SubmitApplicationInputFactory.Create());

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_should_require_an_idempotency_key(string? idempotencyKey)
    {
        var errors = SubmitApplicationInputValidator.Validate(idempotencyKey, SubmitApplicationInputFactory.Create());

        errors.Should().ContainKey("Idempotency-Key");
    }

    [Fact]
    public void Validate_should_return_a_single_request_error_for_a_missing_body()
    {
        var errors = SubmitApplicationInputValidator.Validate("request-1", null);

        errors.Should().ContainKey("request");
        errors["request"].Should().ContainSingle("An application request body is required.");
    }

    [Fact]
    public void Validate_should_reject_invalid_market_and_document_data()
    {
        var input = SubmitApplicationInputFactory.Create();
        input = new SubmitApplicationInput
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            DateOfBirth = input.DateOfBirth,
            Country = "GB",
            Nationality = input.Nationality,
            NationalId = input.NationalId,
            IssuingCountry = input.IssuingCountry,
            Email = input.Email,
            Phone = input.Phone,
            TermsAccepted = input.TermsAccepted,
            Documents =
            [
                new SubmittedDocumentInput { Type = "PASSPORT", Image = "not-base64" },
                new SubmittedDocumentInput { Type = "DRIVERS_LICENSE", Image = "" }
            ]
        };

        var errors = SubmitApplicationInputValidator.Validate("request-1", input);

        errors.Should().ContainKeys("Country", "Documents[0].Image", "Documents[1].Type", "Documents[1].Image");
    }

    [Fact]
    public void Validate_should_require_terms_and_a_past_date_of_birth()
    {
        var input = SubmitApplicationInputFactory.Create();
        input = new SubmitApplicationInput
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow),
            Country = input.Country,
            Nationality = input.Nationality,
            NationalId = input.NationalId,
            IssuingCountry = input.IssuingCountry,
            Email = input.Email,
            Phone = input.Phone,
            TermsAccepted = false,
            Documents = input.Documents
        };

        var errors = SubmitApplicationInputValidator.Validate("request-1", input);

        errors.Should().ContainKeys(nameof(SubmitApplicationInput.DateOfBirth), nameof(SubmitApplicationInput.TermsAccepted));
    }

}
