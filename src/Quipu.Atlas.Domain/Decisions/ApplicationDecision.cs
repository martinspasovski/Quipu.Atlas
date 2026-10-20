using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Domain.Decisions
{
    public sealed record ApplicationDecision(ApplicationStatus Status, string? ReasonCode);

}
