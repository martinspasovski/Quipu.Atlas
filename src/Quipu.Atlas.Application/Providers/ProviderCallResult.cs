using Quipu.Atlas.Domain.Applications;

public sealed record ProviderCallResult<T>(
    ProviderAttemptStatus Status,
    T? Value,
    string? FailureCode)
    where T : class
{
    public static ProviderCallResult<T> Received(T value) =>
        new(ProviderAttemptStatus.ResponseReceived, value, null);

    public static ProviderCallResult<T> TimedOut(string failureCode) =>
        new(ProviderAttemptStatus.TimedOut, null, failureCode);

    public static ProviderCallResult<T> Failed(string failureCode) =>
        new(ProviderAttemptStatus.Failed, null, failureCode);
}

