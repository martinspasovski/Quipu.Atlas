using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Domain.Applications
{
    public sealed class ApplicationDocument
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public ApplicationDocumentType Type { get; set; }
        public string StorageKey { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public byte[] Sha256 { get; set; } = [];
        public DateTimeOffset ReceivedAtUtc { get; set; }
        public OnboardingApplication Application { get; set; } = null!;
    }

}
