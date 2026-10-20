using Quipu.Atlas.Application.Providers.IdNow;
using Quipu.Atlas.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Application.Submit
{
    public sealed record SubmitApplicationCommand(
     string IdempotencyKey,
     ApplicantDetails Applicant,
     IdNowCheckRequest IdNowCheck);

    public sealed record SubmitApplicationResult(
        Guid ApplicationId,
        ApplicationStatus Status,
        string? ReasonCode,
        bool IsReplay = false,
        bool IsInProgress = false);
}
