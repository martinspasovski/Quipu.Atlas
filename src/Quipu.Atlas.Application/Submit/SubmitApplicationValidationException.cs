namespace Quipu.Atlas.Application.Submit;

public sealed class SubmitApplicationValidationException(
    IReadOnlyDictionary<string, string[]> errors)
    : Exception("The submitted application is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
