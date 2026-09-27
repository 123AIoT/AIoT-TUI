using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AIoT.Protocols.Abstractions;

namespace AIoT.PluginSamples.Echo;

public sealed class EchoProtocolClient : IProtocolClient
{
    private readonly ConcurrentDictionary<string, ProtocolValue> _store = new(StringComparer.OrdinalIgnoreCase);

    public string ProtocolName { get; private set; } = "Echo Plugin";
    public bool IsConnected { get; private set; }

    public Task<ProtocolResult> ConnectAsync(ProtocolConnectOptions options, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!options.Endpoint.StartsWith("echo://", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Extra?.GetValueOrDefault("protocol"), "echo", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ProtocolResult.Fail("Echo plugin only supports echo:// endpoints or --protocol echo."));
        }

        IsConnected = true;
        return Task.FromResult(ProtocolResult.Ok());
    }

    public Task<ProtocolResult> DisconnectAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IsConnected = false;
        return Task.FromResult(ProtocolResult.Ok());
    }

    public Task<ProtocolResult<ProtocolValue>> ReadAsync(string address, CancellationToken ct = default)
    {
        if (!IsConnected)
            return Task.FromResult(ProtocolResult<ProtocolValue>.Fail("Not connected"));

        if (_store.TryGetValue(address, out var value))
            return Task.FromResult(ProtocolResult<ProtocolValue>.Ok(value));

        return Task.FromResult(ProtocolResult<ProtocolValue>.Fail($"Address not found: {address}"));
    }

    public Task<ProtocolResult> WriteAsync(string address, string value, CancellationToken ct = default)
    {
        if (!IsConnected)
            return Task.FromResult(ProtocolResult.Fail("Not connected"));

        _store[address] = new ProtocolValue(address, value, DateTime.UtcNow, "Good");
        return Task.FromResult(ProtocolResult.Ok());
    }

    public Task<ProtocolResult<IReadOnlyList<ProtocolNode>>> BrowseAsync(string? parent = null, CancellationToken ct = default)
    {
        if (!IsConnected)
            return Task.FromResult(ProtocolResult<IReadOnlyList<ProtocolNode>>.Fail("Not connected"));

        IReadOnlyList<ProtocolNode> nodes = _store.Keys
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .Select(k => new ProtocolNode(k, k, false))
            .ToList();

        return Task.FromResult(ProtocolResult<IReadOnlyList<ProtocolNode>>.Ok(nodes));
    }

    public async IAsyncEnumerable<ProtocolValue> SubscribeAsync(
        IReadOnlyList<string> addresses,
        int intervalMs = 1000,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!IsConnected || addresses.Count == 0)
            yield break;

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(100, intervalMs)));
        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var address in addresses)
            {
                if (_store.TryGetValue(address, out var value))
                    yield return value;
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        IsConnected = false;
        return ValueTask.CompletedTask;
    }
}
