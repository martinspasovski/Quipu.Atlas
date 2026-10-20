using Quipu.Atlas.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Domain.Decisions
{
    public sealed class OnboardingDecisionService
    {
        public ApplicationDecision Decide(
            ProviderCheckAttempt idNowAttempt,
            ProviderCheckAttempt worldCheckAttempt)
        {
            var idNowResult = idNowAttempt.IdNowResult;

            if (idNowAttempt.Status == ProviderAttemptStatus.ResponseReceived && idNowResult is not null)
            {
                if (idNowResult.DocumentResult == IdNowDocumentResult.Invalid)
                {
                    return new(ApplicationStatus.Rejected, "IDNOW_DOCUMENT_INVALID");
                }

                if (idNowResult.DocumentResult == IdNowDocumentResult.Inconclusive)
                {
                    return new(ApplicationStatus.Rejected, "IDNOW_INCONCLUSIVE");
                }

                if (!idNowResult.FaceMatch)
                {
                    return new(ApplicationStatus.Rejected, "IDNOW_FACE_MISMATCH");
                }
            }

            if (idNowAttempt.Status != ProviderAttemptStatus.ResponseReceived ||
                worldCheckAttempt.Status != ProviderAttemptStatus.ResponseReceived ||
                idNowResult is null ||
                worldCheckAttempt.WorldCheckResult is null)
            {
                var failureCode = idNowAttempt.FailureCode ?? worldCheckAttempt.FailureCode ?? "PROVIDER_CHECK_FAILED";
                return new(ApplicationStatus.ChecksFailed, failureCode);
            }

            if (worldCheckAttempt.WorldCheckResult.Status == WorldCheckScreeningStatus.PossibleMatch)
            {
                return new(ApplicationStatus.PendingReview, "WORLDCHECK_POSSIBLE_MATCH");
            }

            return new(ApplicationStatus.Approved, null);
        }
    }

}
