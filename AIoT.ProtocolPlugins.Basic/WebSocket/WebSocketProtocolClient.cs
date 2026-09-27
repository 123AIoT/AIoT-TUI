using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using AIoT.Protocols.Abstractions;

namespace AIoT.ProtocolPlugins.Basic.WebSocket;

public sealed class WebSocketProtocolClient : IProtocolClient
{
    private const string DefaultAddress = "$last";

    private ClientWebSocket? _socket;
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveTask;
    private string? _lastAddress;
    private string _defaultAddress = DefaultAddress;

    private readonly ConcurrentDictionary<string, ProtocolValue> _latestValues =
        new(StringComparer.OrdinalIgnoreCase);

    public string ProtocolName { get; private set; } = "WebSocket";
    public bool IsConnected => _socket?.State == WebSocketState.Open;

    public async Task<ProtocolResult> ConnectAsync(ProtocolConnectOptions options, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var uri))
            return ProtocolResult.Fail("Invalid WebSocket endpoint.");

        var scheme = uri.Scheme.ToLowerInvariant();
        if (scheme is not "ws" and not "wss")
            return ProtocolResult.Fail("Unsupported WebSocket endpoint scheme. Use ws:// or wss://");

        var disconnect = await DisconnectAsync(ct);
        if (!disconnect.Success)
            return disconnect;

        _defaultAddress = string.IsNullOrWhiteSpace(GetExtra(options.Extra, "address"))
            ? DefaultAddress
            : GetExtra(options.Extra, "address")!;

        _socket = new ClientWebSocket();

        var subProtocol = GetExtra(options.Extra, "subprotocol");
        if (!string.IsNullOrWhiteSpace(subProtocol))
            _socket.Options.AddSubProtocol(subProtocol);

        var header = GetExtra(options.Extra, "header");
        foreach (var pair in ParseHeaders(header))
        {
            _socket.Options.SetRequestHeader(pair.Key, pair.Value);
        }

        try
        {
            await _socket.ConnectAsync(uri, ct);
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }

        _receiveCts = new CancellationTokenSource();
        _receiveTask = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), _receiveCts.Token);

        ProtocolName = "WebSocket";
        return ProtocolResult.Ok();
    }

    public async Task<ProtocolResult> DisconnectAsync(CancellationToken ct = default)
    {
        var socket = _socket;
        if (socket == null)
            return ProtocolResult.Ok();

        try
        {
            _receiveCts?.Cancel();

            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "disconnect", ct);

            if (_receiveTask != null)
                await _receiveTask;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }
        finally
        {
            _receiveCts?.Dispose();
            _receiveCts = null;
            _receiveTask = null;
            socket.Dispose();
            _socket = null;
            _latestValues.Clear();
            _lastAddress = null;
            _defaultAddress = DefaultAddress;
            ProtocolName = "WebSocket";
        }

        return ProtocolResult.Ok();
    }

    public Task<ProtocolResult<ProtocolValue>> ReadAsync(string address, CancellationToken ct = default)
    {
        if (!IsConnected)
            return Task.FromResult(ProtocolResult<ProtocolValue>.Fail("Not connected"));

        if (!string.IsNullOrWhiteSpace(address) && _latestValues.TryGetValue(address, out var found))
            return Task.FromResult(ProtocolResult<ProtocolValue>.Ok(found));

        if (_lastAddress is not null && _latestValues.TryGetValue(_lastAddress, out var latest))
            return Task.FromResult(ProtocolResult<ProtocolValue>.Ok(latest));

        return Task.FromResult(ProtocolResult<ProtocolValue>.Fail("No cached WebSocket message available yet."));
    }

    public async Task<ProtocolResult> WriteAsync(string address, string value, CancellationToken ct = default)
    {
        if (!IsConnected || _socket == null)
            return ProtocolResult.Fail("Not connected");

        var targetAddress = string.IsNullOrWhiteSpace(address) ? _defaultAddress : address;

        var payload = string.IsNullOrWhiteSpace(targetAddress)
            ? value ?? string.Empty
            : $"{targetAddress}={value}";

        var bytes = Encoding.UTF8.GetBytes(payload);

        try
        {
            await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
            return ProtocolResult.Ok();
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }
    }

    public Task<ProtocolResult<IReadOnlyList<ProtocolNode>>> BrowseAsync(string? parent = null, CancellationToken ct = default)
    {
        IReadOnlyList<ProtocolNode> nodes = _latestValues.Keys
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
        if (!IsConnected)
            yield break;

        var keys = addresses.Count > 0
            ? addresses.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : new List<string> { _defaultAddress };

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(100, intervalMs)));
        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var key in keys)
            {
                if (_latestValues.TryGetValue(key, out var value))
                    yield return value;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        if (_socket == null)
            return;

        var buffer = new byte[8192];

        try
        {
            while (!ct.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _socket.ReceiveAsync(buffer, ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                        return;

                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                var message = Encoding.UTF8.GetString(ms.ToArray());
                var (address, value) = ParseMessage(message);

                var protocolValue = new ProtocolValue(address, value, DateTime.UtcNow, "Good");
                _latestValues[address] = protocolValue;
                _latestValues[_defaultAddress] = protocolValue;
                _lastAddress = address;
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
    }

    private static string? GetExtra(Dictionary<string, string>? extra, string key)
    {
        if (extra is null)
            return null;

        return extra.TryGetValue(key, out var value) ? value : null;
    }

    private static IReadOnlyList<KeyValuePair<string, string>> ParseHeaders(string? headers)
    {
        if (string.IsNullOrWhiteSpace(headers))
            return Array.Empty<KeyValuePair<string, string>>();

        var values = new List<KeyValuePair<string, string>>();
        var segments = headers.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segment in segments)
        {
            var idx = segment.IndexOf(':');
            if (idx <= 0)
                continue;

            var key = segment[..idx].Trim();
            var value = segment[(idx + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(key))
                continue;

            values.Add(new KeyValuePair<string, string>(key, value));
        }

        return values;
    }

    private static (string Address, string Value) ParseMessage(string message)
    {
        var idx = message.IndexOf('=');
        if (idx <= 0)
            return (DefaultAddress, message);

        var address = message[..idx].Trim();
        var value = message[(idx + 1)..];

        if (string.IsNullOrWhiteSpace(address))
            address = DefaultAddress;

        return (address, value);
    }
}
