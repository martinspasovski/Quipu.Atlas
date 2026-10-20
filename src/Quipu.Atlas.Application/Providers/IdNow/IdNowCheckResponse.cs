using Quipu.Atlas.Domain.Applications;

public sealed record IdNowCheckResponse(
    string ProviderReference,
    IdNowDocumentResult DocumentResult,
    bool FaceMatch,
    decimal Confidence);

