using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Domain.Applications
{
    public sealed class ApplicantDetails
    {
        public Guid ApplicationId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string MarketCode { get; set; } = string.Empty;
        public string? NationalityCode { get; set; }
        public IdentifierType IdentifierType { get; set; }
        public string IdentifierValue { get; set; } = string.Empty;
        public string IssuingCountryCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTimeOffset TermsAcceptedAtUtc { get; set; }

        public OnboardingApplication Application { get; set; } = null!;

    }
}
