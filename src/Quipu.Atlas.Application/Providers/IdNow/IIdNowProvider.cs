using Quipu.Atlas.Application.Providers.IdNow;

public interface IIdNowProvider
{
    Task<ProviderCallResult<IdNowCheckResponse>> CheckAsync(
        IdNowCheckRequest request,
        CancellationToken cancellationToken);
}

