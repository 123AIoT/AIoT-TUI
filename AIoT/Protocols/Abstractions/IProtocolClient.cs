namespace AIoT.Protocols.Abstractions;

public interface IProtocolClient : IAsyncDisposable
{
    string ProtocolName { get; }
    bool IsConnected { get; }

    Task<ProtocolResult> ConnectAsync(ProtocolConnectOptions options, CancellationToken ct = default);
    Task<ProtocolResult> DisconnectAsync(CancellationToken ct = default);

    Task<ProtocolResult<ProtocolValue>> ReadAsync(ProtocolReadRequest request, CancellationToken ct = default);
    Task<ProtocolResult> WriteAsync(ProtocolWriteRequest request, CancellationToken ct = default);

    Task<ProtocolResult<IReadOnlyList<ProtocolNode>>> BrowseAsync(string? parent = null, CancellationToken ct = default);

    IAsyncEnumerable<ProtocolValue> SubscribeAsync(
        IReadOnlyList<ProtocolReadRequest> addresses,
        int intervalMs = 1000,
        CancellationToken ct = default);
}
