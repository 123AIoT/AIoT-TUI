using System.Runtime.CompilerServices;
using System.Threading.Channels;
using AIoT.OpcUa;
using AIoT.OpcUa.Models;
using AIoT.Protocols.Abstractions;
using AIoT.Utilities;
using Opc.Ua;

namespace AIoT.Protocols.OpcUa;

public sealed class OpcUaProtocolClient : IProtocolClient
{
    private readonly Logger _logger;
    private ConnectionManager? _connectionManager;

    public string ProtocolName => "OPC UA";
    public bool IsConnected => _connectionManager?.IsConnected ?? false;

    public OpcUaProtocolClient(Logger? logger = null)
    {
        _logger = logger ?? new Logger();
    }

    public async Task<ProtocolResult> ConnectAsync(ProtocolConnectOptions options, CancellationToken ct = default)
    {
        _connectionManager = new ConnectionManager(_logger);

        var credentials = string.IsNullOrWhiteSpace(options.Username)
            ? ConnectionCredentials.Anonymous
            : new ConnectionCredentials(AuthenticationType.UserName, options.Username, options.Password);

        var success = await _connectionManager.ConnectAsync(options.Endpoint, credentials: credentials);
        return success ? ProtocolResult.Ok() : ProtocolResult.Fail($"Failed to connect to {options.Endpoint}");
    }

    public Task<ProtocolResult> DisconnectAsync(CancellationToken ct = default)
    {
        _connectionManager?.Disconnect();
        return Task.FromResult(ProtocolResult.Ok());
    }

    public async Task<ProtocolResult<ProtocolValue>> ReadAsync(ProtocolReadRequest request, CancellationToken ct = default)
    {
        if (_connectionManager?.Client is null || !IsConnected)
            return ProtocolResult<ProtocolValue>.Fail("Not connected");

        try
        {
            var nodeId = NodeId.Parse(request.Address);
            var value = await _connectionManager.Client.ReadValueAsync(nodeId);
            if (value == null)
                return ProtocolResult<ProtocolValue>.Fail("Read returned null");

            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (request.Options != null)
            {
                foreach (var pair in request.Options)
                    metadata[pair.Key] = pair.Value;
            }

            var result = new ProtocolValue(
                request.Address,
                value.Value?.ToString(),
                value.SourceTimestamp == DateTime.MinValue ? DateTime.UtcNow : value.SourceTimestamp,
                StatusCode.IsGood(value.StatusCode) ? "Good" : "Bad",
                request.DataType,
                request.Length,
                metadata);

            return ProtocolResult<ProtocolValue>.Ok(result);
        }
        catch (Exception ex)
        {
            return ProtocolResult<ProtocolValue>.Fail(ex.Message);
        }
    }

    public async Task<ProtocolResult> WriteAsync(ProtocolWriteRequest request, CancellationToken ct = default)
    {
        if (_connectionManager?.Client is null || !IsConnected)
            return ProtocolResult.Fail("Not connected");

        try
        {
            var nodeId = NodeId.Parse(request.Address);
            var typedValue = ConvertToOpcValue(request.Value, request.DataType);
            var status = await _connectionManager.Client.WriteValueAsync(nodeId, typedValue);
            return StatusCode.IsGood(status)
                ? ProtocolResult.Ok()
                : ProtocolResult.Fail($"Write failed with status: {status}");
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }
    }

    public async Task<ProtocolResult<IReadOnlyList<ProtocolNode>>> BrowseAsync(string? parent = null, CancellationToken ct = default)
    {
        if (_connectionManager?.NodeBrowser is null || !IsConnected)
            return ProtocolResult<IReadOnlyList<ProtocolNode>>.Fail("Not connected");

        try
        {
            BrowsedNode parentNode;
            if (string.IsNullOrWhiteSpace(parent))
            {
                parentNode = _connectionManager.NodeBrowser.GetRootNode();
            }
            else
            {
                parentNode = new BrowsedNode
                {
                    NodeId = NodeId.Parse(parent),
                    BrowseName = parent,
                    DisplayName = parent,
                    NodeClass = NodeClass.Object,
                    HasChildren = true
                };
            }

            var children = await _connectionManager.NodeBrowser.GetChildrenAsync(parentNode);
            IReadOnlyList<ProtocolNode> nodes = children
                .Select(c => new ProtocolNode(c.NodeId.ToString(), c.DisplayName, c.HasChildren))
                .ToList();

            return ProtocolResult<IReadOnlyList<ProtocolNode>>.Ok(nodes);
        }
        catch (Exception ex)
        {
            return ProtocolResult<IReadOnlyList<ProtocolNode>>.Fail(ex.Message);
        }
    }

    public async IAsyncEnumerable<ProtocolValue> SubscribeAsync(
        IReadOnlyList<ProtocolReadRequest> addresses,
        int intervalMs = 1000,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (_connectionManager?.SubscriptionManager is null || !IsConnected || addresses.Count == 0)
            yield break;

        var channel = Channel.CreateUnbounded<ProtocolValue>();
        var tracked = new Dictionary<uint, ProtocolReadRequest>();

        Action<MonitoredNode> valueChanged = node =>
        {
            if (!tracked.TryGetValue(node.ClientHandle, out var request))
                return;

            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (request.Options != null)
            {
                foreach (var pair in request.Options)
                    metadata[pair.Key] = pair.Value;
            }

            var value = new ProtocolValue(
                request.Address,
                node.Value,
                node.Timestamp ?? DateTime.UtcNow,
                node.IsGood ? "Good" : node.IsUncertain ? "Uncertain" : "Bad",
                node.DataType,
                request.Length,
                metadata);

            channel.Writer.TryWrite(value);
        };

        _connectionManager.SubscriptionManager.ValueChanged += valueChanged;

        try
        {
            foreach (var request in addresses)
            {
                if (ct.IsCancellationRequested)
                    break;

                var nodeId = NodeId.Parse(request.Address);
                var added = await _connectionManager.SubscribeAsync(nodeId, request.Address, request.DataType);
                if (added != null)
                    tracked[added.ClientHandle] = request;
            }

            await foreach (var value in channel.Reader.ReadAllAsync(ct))
            {
                yield return value;
            }
        }
        finally
        {
            _connectionManager.SubscriptionManager.ValueChanged -= valueChanged;

            foreach (var handle in tracked.Keys)
            {
                await _connectionManager.UnsubscribeAsync(handle);
            }

            channel.Writer.TryComplete();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _connectionManager?.Dispose();
        _connectionManager = null;
    }

    private static object ConvertToOpcValue(string value, string? dataType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
            return value;

        return dataType.Trim().ToLowerInvariant() switch
        {
            "bool" or "boolean" => bool.Parse(value),
            "byte" => byte.Parse(value),
            "sbyte" => sbyte.Parse(value),
            "short" or "int16" => short.Parse(value),
            "ushort" or "uint16" or "word" => ushort.Parse(value),
            "int" or "int32" => int.Parse(value),
            "uint" or "uint32" or "dword" => uint.Parse(value),
            "long" or "int64" => long.Parse(value),
            "ulong" or "uint64" => ulong.Parse(value),
            "float" or "single" => float.Parse(value),
            "double" => double.Parse(value),
            _ => value
        };
    }
}
