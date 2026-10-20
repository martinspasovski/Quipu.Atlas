using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Domain.Applications
{
    public sealed class OnboardingApplication
    {
        public Guid Id { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public byte[] RequestHash { get; set; } = [];
        public ApplicationStatus Status { get; set; } = ApplicationStatus.ChecksInProgress;
        public string? DecisionReasonCode { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public DateTimeOffset? ProcessingLeaseExpiresAtUtc { get; set; }
        public Guid? ProcessingToken { get; set; }
        public byte[] RowVersion { get; set; } = [];

        public ApplicantDetails Applicant { get; set; } = new();
        public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
        public ICollection<ProviderCheckAttempt> ProviderCheckAttempts { get; set; } = new List<ProviderCheckAttempt>();
    }

}

