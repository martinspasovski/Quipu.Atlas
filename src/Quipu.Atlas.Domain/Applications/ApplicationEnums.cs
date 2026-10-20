using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Domain.Applications
{

    public enum ApplicationStatus
    {
        ChecksInProgress, 
        Approved, 
        Rejected,
        PendingReview,
        ChecksFailed
    }

    public enum IdentifierType
    {
        NationalId, 
        Passport
    }

    public enum ApplicationDocumentType
    {
        Passport, 
        IdentityCard,
        Selfie
    }

    public enum ProviderType
    {
        IdNow,
        WorldCheck
    }

    public enum ProviderAttemptStatus
    {
        InProgress, 
        ResponseReceived, 
        TimedOut, 
        Failed
    }

    public enum IdNowDocumentResult
    {
        Valid, 
        Invalid, 
        Inconclusive
    }

    public enum WorldCheckScreeningStatus
    {
        Clear, 
        PossibleMatch
    }

    public enum WorldCheckListType
    {
        Sanctions, 
        Pep
    }
}
