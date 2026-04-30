namespace AipmRegister.Core.Network;

public interface IInternetReachabilityProbe
{
    Task WaitUntilReachableAsync(
        string host,
        int port,
        TimeSpan timeout,
        CancellationToken ct = default);
}
