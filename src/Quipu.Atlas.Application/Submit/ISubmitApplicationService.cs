namespace Quipu.Atlas.Application.Submit;

public interface ISubmitApplicationService
{
    Task<SubmitApplicationResult> SubmitAsync(
        string? idempotencyKey,
        SubmitApplicationInput? input,
        CancellationToken cancellationToken);
}
