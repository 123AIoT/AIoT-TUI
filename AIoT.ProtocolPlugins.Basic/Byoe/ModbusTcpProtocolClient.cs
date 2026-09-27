using System.Runtime.CompilerServices;
using System.Text;
using AIoT.Protocols.Abstractions;
using Byoe.Core;
using Byoe.ModBus;

namespace AIoT.ProtocolPlugins.Basic.Byoe;

public sealed class ModbusTcpProtocolClient : IProtocolClient
{
    private ModbusTcpNet? _modbus;
    private string? _readTypeHint;

    public string ProtocolName { get; private set; } = "Byoe Modbus TCP";
    public bool IsConnected { get; private set; }

    public Task<ProtocolResult> ConnectAsync(ProtocolConnectOptions options, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!TryParseEndpoint(options.Endpoint, out var host, out var port, out var station, out var error))
            return Task.FromResult(ProtocolResult.Fail(error ?? "Invalid endpoint"));

        var disconnect = DisconnectInternal();
        if (!disconnect.Success)
            return Task.FromResult(disconnect);

        _readTypeHint = options.Extra is not null && options.Extra.TryGetValue("type", out var type) ? type?.Trim().ToLowerInvariant() : null;

        _modbus = new ModbusTcpNet(host!, port, station);
        var connectResult = _modbus.ConnectServer();
        if (!connectResult.IsSuccess)
            return Task.FromResult(ProtocolResult.Fail(connectResult.Message));

        IsConnected = true;
        return Task.FromResult(ProtocolResult.Ok());
    }

    public Task<ProtocolResult> DisconnectAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(DisconnectInternal());
    }

    public Task<ProtocolResult<ProtocolValue>> ReadAsync(string address, CancellationToken ct = default)
    {
        if (!IsConnected || _modbus is null)
            return Task.FromResult(ProtocolResult<ProtocolValue>.Fail("Not connected"));

        return Task.Run(() =>
        {
            if (_readTypeHint is not null)
                return ReadByType(_modbus, address, _readTypeHint);

            var boolResult = _modbus.ReadBool(address);
            if (boolResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, boolResult.Content.ToString(), DateTime.UtcNow, "Good"));

            var intResult = _modbus.ReadInt32(address);
            if (intResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, intResult.Content.ToString(), DateTime.UtcNow, "Good"));

            var floatResult = _modbus.ReadFloat(address);
            if (floatResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, floatResult.Content.ToString(), DateTime.UtcNow, "Good"));

            var strResult = _modbus.ReadString(address, 64, Encoding.ASCII);
            if (strResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, (strResult.Content ?? string.Empty).TrimEnd('\0'), DateTime.UtcNow, "Good"));

            var bytesResult = _modbus.Read(address, 16);
            if (bytesResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, Convert.ToHexString(bytesResult.Content), DateTime.UtcNow, "Good"));

            var error = FirstError(boolResult, intResult, floatResult, strResult, bytesResult);
            return ProtocolResult<ProtocolValue>.Fail(error);
        }, ct);
    }

    public Task<ProtocolResult> WriteAsync(string address, string value, CancellationToken ct = default)
    {
        if (!IsConnected || _modbus is null)
            return Task.FromResult(ProtocolResult.Fail("Not connected"));

        return Task.Run(() =>
        {
            OperateResult result;
            if (TryParseHexBytes(value, out var bytes))
                result = _modbus.Write(address, bytes);
            else if (bool.TryParse(value, out var b))
                result = _modbus.Write(address, b);
            else if (int.TryParse(value, out var i))
                result = _modbus.Write(address, i);
            else if (float.TryParse(value, out var f))
                result = _modbus.Write(address, f);
            else
                result = _modbus.Write(address, value);

            return result.IsSuccess ? ProtocolResult.Ok() : ProtocolResult.Fail(result.Message);
        }, ct);
    }

    public Task<ProtocolResult<IReadOnlyList<ProtocolNode>>> BrowseAsync(string? parent = null, CancellationToken ct = default)
        => Task.FromResult(ProtocolResult<IReadOnlyList<ProtocolNode>>.Fail("Browse is not supported for Modbus TCP endpoints."));

    public async IAsyncEnumerable<ProtocolValue> SubscribeAsync(
        IReadOnlyList<string> addresses,
        int intervalMs = 1000,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!IsConnected || _modbus is null || addresses.Count == 0)
            yield break;

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(100, intervalMs)));
        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var address in addresses)
            {
                var read = await ReadAsync(address, ct);
                if (read.Success && read.Data is not null)
                    yield return read.Data;
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        DisconnectInternal();
        return ValueTask.CompletedTask;
    }

    private ProtocolResult DisconnectInternal()
    {
        try
        {
            _modbus?.ConnectClose();
            return ProtocolResult.Ok();
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }
        finally
        {
            _modbus = null;
            _readTypeHint = null;
            IsConnected = false;
        }
    }

    private static bool TryParseEndpoint(string endpoint, out string? host, out int port, out byte station, out string? error)
    {
        host = null;
        port = 502;
        station = 1;
        error = null;

        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            var scheme = uri.Scheme.ToLowerInvariant();
            if (scheme is not "modbus.tcp" and not "modbus-tcp" and not "mbtcp")
            {
                error = "Unsupported Modbus TCP endpoint format.";
                return false;
            }

            host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
            if (string.IsNullOrWhiteSpace(host))
            {
                error = "Missing host for Modbus TCP endpoint.";
                return false;
            }

            port = uri.Port > 0 ? uri.Port : 502;
            var stationText = GetQueryValue(uri, "station");
            if (!string.IsNullOrWhiteSpace(stationText) && byte.TryParse(stationText, out var parsedStation))
                station = parsedStation;

            return true;
        }

        var parts = endpoint.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            error = "Invalid Modbus TCP endpoint.";
            return false;
        }

        host = parts[0];
        if (parts.Length > 1 && int.TryParse(parts[1], out var parsedPort))
            port = parsedPort;

        return true;
    }

    private static string? GetQueryValue(Uri uri, string key)
    {
        if (string.IsNullOrWhiteSpace(uri.Query))
            return null;

        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in query)
        {
            var index = pair.IndexOf('=');
            if (index <= 0)
                continue;

            var name = Uri.UnescapeDataString(pair[..index]);
            if (!name.Equals(key, StringComparison.OrdinalIgnoreCase))
                continue;

            return Uri.UnescapeDataString(pair[(index + 1)..]);
        }

        return null;
    }

    private static ProtocolResult<ProtocolValue> ReadByType(ModbusTcpNet net, string address, string type)
    {
        switch (type)
        {
            case "bool":
                {
                    var result = net.ReadBool(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, result.Content.ToString(), DateTime.UtcNow, "Good"))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "int":
            case "int32":
                {
                    var result = net.ReadInt32(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, result.Content.ToString(), DateTime.UtcNow, "Good"))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "float":
            case "single":
                {
                    var result = net.ReadFloat(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, result.Content.ToString(), DateTime.UtcNow, "Good"))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "string":
            case "text":
                {
                    var result = net.ReadString(address, 64, Encoding.ASCII);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, (result.Content ?? string.Empty).TrimEnd('\0'), DateTime.UtcNow, "Good"))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "hex":
                {
                    var result = net.Read(address, 16);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(new ProtocolValue(address, Convert.ToHexString(result.Content), DateTime.UtcNow, "Good"))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            default:
                return ProtocolResult<ProtocolValue>.Fail($"Unsupported read type hint: {type}");
        }
    }

    private static bool TryParseHexBytes(string input, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        var text = input.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? input[2..]
            : input;

        if (text.Length == 0 || text.Length % 2 != 0)
            return false;

        for (var i = 0; i < text.Length; i++)
        {
            if (!Uri.IsHexDigit(text[i]))
                return false;
        }

        bytes = Convert.FromHexString(text);
        return true;
    }

    private static string FirstError(params OperateResult[] results)
    {
        foreach (var result in results)
        {
            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(result.Message))
                return result.Message;
        }

        return "Operation failed";
    }
}
