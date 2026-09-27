using System.Runtime.CompilerServices;
using System.Text;
using AIoT.Protocols.Abstractions;
using Byoe;
using Byoe.Core;
using Byoe.ModBus;
using Byoe.Profinet.Delta;
using Byoe.Profinet.Melsec;
using Byoe.Profinet.Omron;
using Byoe.Profinet.Siemens;
using Byoe.Profinet.XINJE;

namespace AIoT.PluginProtocols.Byoe;

public sealed class ByoeProtocolClient : IProtocolClient
{
    private IReadWriteNet? _readWriteNet;
    private ModbusTcpNet? _modbusTcp;
    private ModbusRtu? _modbusRtu;
    private SiemensS7Net? _siemensS7;
    private MelsecMcNet? _melsecMc;
    private OmronFinsNet? _omronFins;
    private DeltaTcpNet? _deltaTcp;
    private XinJETcpNet? _xinjeTcp;
    private string? _readTypeHint;
    private string? _activeProtocolKey;

    public string ProtocolName { get; private set; } = "Byoe";
    public bool IsConnected { get; private set; }

    public Task<ProtocolResult> ConnectAsync(ProtocolConnectOptions options, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var target = ParseTarget(options.Endpoint, options.Extra);
        if (!target.Success || target.Data is null)
            return Task.FromResult(ProtocolResult.Fail(target.Error ?? "Unsupported endpoint."));

        var disconnectResult = DisconnectInternal();
        if (!disconnectResult.Success)
            return Task.FromResult(disconnectResult);

        _readTypeHint = ParseTypeHint(options.Extra);
        _activeProtocolKey = ParseProtocolHint(options.Extra);

        if (target.Data.Transport == "tcp")
        {
            _modbusTcp = new ModbusTcpNet(target.Data.Host!, target.Data.Port, target.Data.Station);
            var connectResult = _modbusTcp.ConnectServer();
            if (!connectResult.IsSuccess)
                return Task.FromResult(ProtocolResult.Fail(connectResult.Message));

            _readWriteNet = _modbusTcp;
            ProtocolName = "Byoe Modbus TCP";
            _activeProtocolKey ??= "modbus.tcp";
            IsConnected = true;
            return Task.FromResult(ProtocolResult.Ok());
        }

        if (target.Data.Transport == "delta")
        {
            _deltaTcp = new DeltaTcpNet(target.Data.Host!, target.Data.Port, target.Data.Station)
            {
                Series = ParseDeltaSeries(target.Data.Extra, DeltaSeries.Dvp)
            };

            var connectResult = _deltaTcp.ConnectServer();
            if (!connectResult.IsSuccess)
                return Task.FromResult(ProtocolResult.Fail(connectResult.Message));

            _readWriteNet = _deltaTcp;
            ProtocolName = "Byoe Delta";
            _activeProtocolKey ??= "delta";
            IsConnected = true;
            return Task.FromResult(ProtocolResult.Ok());
        }

        if (target.Data.Transport == "xinje")
        {
            _xinjeTcp = new XinJETcpNet(target.Data.Host!, target.Data.Port, target.Data.Station)
            {
                Series = ParseXinjeSeries(target.Data.Extra, XinJESeries.XC)
            };

            var connectResult = _xinjeTcp.ConnectServer();
            if (!connectResult.IsSuccess)
                return Task.FromResult(ProtocolResult.Fail(connectResult.Message));

            _readWriteNet = _xinjeTcp;
            ProtocolName = "Byoe Xinje";
            _activeProtocolKey ??= "xinje";
            IsConnected = true;
            return Task.FromResult(ProtocolResult.Ok());
        }

        if (target.Data.Transport == "s7")
        {
            var plcType = ParseSiemensPlcType(target.Data.Extra, SiemensPLCS.S1200);
            _siemensS7 = new SiemensS7Net(plcType, target.Data.Host!);
            _siemensS7.Port = target.Data.Port;
            _siemensS7.Rack = ParseByte(target.Data.Extra, "rack", 0);
            _siemensS7.Slot = ParseByte(target.Data.Extra, "slot", 0);

            var connectResult = _siemensS7.ConnectServer();
            if (!connectResult.IsSuccess)
                return Task.FromResult(ProtocolResult.Fail(connectResult.Message));

            _readWriteNet = _siemensS7;
            ProtocolName = "Byoe Siemens S7";
            _activeProtocolKey ??= "siemens.s7";
            IsConnected = true;
            return Task.FromResult(ProtocolResult.Ok());
        }

        if (target.Data.Transport == "melsec")
        {
            _melsecMc = new MelsecMcNet(target.Data.Host!, target.Data.Port);
            var connectResult = _melsecMc.ConnectServer();
            if (!connectResult.IsSuccess)
                return Task.FromResult(ProtocolResult.Fail(connectResult.Message));

            _readWriteNet = _melsecMc;
            ProtocolName = "Byoe Mitsubishi MC";
            _activeProtocolKey ??= "melsec";
            IsConnected = true;
            return Task.FromResult(ProtocolResult.Ok());
        }

        if (target.Data.Transport == "omron")
        {
            _omronFins = new OmronFinsNet(target.Data.Host!, target.Data.Port);
            var connectResult = _omronFins.ConnectServer();
            if (!connectResult.IsSuccess)
                return Task.FromResult(ProtocolResult.Fail(connectResult.Message));

            _readWriteNet = _omronFins;
            ProtocolName = "Byoe Omron FINS";
            _activeProtocolKey ??= "omron";
            IsConnected = true;
            return Task.FromResult(ProtocolResult.Ok());
        }

        _modbusRtu = new ModbusRtu(target.Data.Station);
        _modbusRtu.SerialPortInni(target.Data.PortName!, target.Data.BaudRate);
        var openResult = _modbusRtu.Open();
        if (!openResult.IsSuccess)
            return Task.FromResult(ProtocolResult.Fail(openResult.Message));

        _readWriteNet = _modbusRtu;
        ProtocolName = "Byoe Modbus RTU";
        _activeProtocolKey ??= "modbus.rtu";
        IsConnected = true;
        return Task.FromResult(ProtocolResult.Ok());
    }

    public Task<ProtocolResult> DisconnectAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(DisconnectInternal());
    }

    public Task<ProtocolResult<ProtocolValue>> ReadAsync(ProtocolReadRequest request, CancellationToken ct = default)
    {
        if (!IsConnected || _readWriteNet is null)
            return Task.FromResult(ProtocolResult<ProtocolValue>.Fail("Not connected"));

        return Task.Run(() =>
        {
            var address = request.Address;
            var typeHint = request.DataType ?? _readTypeHint;
            var validationError = ValidateAddressTypeCompatibility(_activeProtocolKey, address, typeHint, request.Length);
            if (validationError is not null)
                return ProtocolResult<ProtocolValue>.Fail(validationError);

            var normalizedLength = NormalizeLength(_activeProtocolKey, typeHint, request.Length);

            if (typeHint is not null)
            {
                var typed = ReadByType(_readWriteNet, address, typeHint, normalizedLength);
                if (typed.Success)
                    return typed;

                return typed;
            }

            var boolResult = _readWriteNet.ReadBool(address);
            if (boolResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, boolResult.Content.ToString(), request.DataType, normalizedLength));

            var intResult = _readWriteNet.ReadInt32(address);
            if (intResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, intResult.Content.ToString(), request.DataType, normalizedLength));

            var floatResult = _readWriteNet.ReadFloat(address);
            if (floatResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, floatResult.Content.ToString(), request.DataType, normalizedLength));

            var stringResult = _readWriteNet.ReadString(address, (ushort)(normalizedLength ?? 64), Encoding.ASCII);
            if (stringResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, (stringResult.Content ?? string.Empty).TrimEnd('\0'), request.DataType ?? "string", normalizedLength));

            var bytesResult = _readWriteNet.Read(address, (ushort)(normalizedLength ?? 16));
            if (bytesResult.IsSuccess)
                return ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, Convert.ToHexString(bytesResult.Content), request.DataType, normalizedLength));

            var error = FirstError(boolResult, intResult, floatResult, stringResult, bytesResult);
            return ProtocolResult<ProtocolValue>.Fail(error);
        }, ct);
    }

    public Task<ProtocolResult> WriteAsync(ProtocolWriteRequest request, CancellationToken ct = default)
    {
        if (!IsConnected || _readWriteNet is null)
            return Task.FromResult(ProtocolResult.Fail("Not connected"));

        return Task.Run(() =>
        {
            OperateResult result;
            var address = request.Address;
            var value = request.Value;
            var typeHint = request.DataType?.Trim().ToLowerInvariant();
            var validationError = ValidateAddressTypeCompatibility(_activeProtocolKey, address, typeHint, request.Length);
            if (validationError is not null)
                return ProtocolResult.Fail(validationError);

            var normalizedLength = NormalizeLength(_activeProtocolKey, typeHint, request.Length);

            if (typeHint is "hex" && TryParseHexBytes(value, out var forcedHexBytes))
            {
                result = _readWriteNet.Write(address, forcedHexBytes);
            }
            else if (TryParseHexBytes(value, out var hexBytes))
            {
                result = _readWriteNet.Write(address, hexBytes);
            }
            else if (typeHint is "bool" or "boolean" && bool.TryParse(value, out var typedBoolValue))
            {
                result = _readWriteNet.Write(address, typedBoolValue);
            }
            else if (typeHint is "byte" && byte.TryParse(value, out var byteValue))
            {
                result = _readWriteNet.Write(address, new[] { byteValue });
            }
            else if (typeHint is "short" or "int16" && short.TryParse(value, out var int16Value))
            {
                result = _readWriteNet.Write(address, int16Value);
            }
            else if (typeHint is "ushort" or "uint16" or "word" && ushort.TryParse(value, out var uint16Value))
            {
                result = _readWriteNet.Write(address, uint16Value);
            }
            else if (typeHint is "int" or "int32" && int.TryParse(value, out var int32Value))
            {
                result = _readWriteNet.Write(address, int32Value);
            }
            else if (typeHint is "uint" or "uint32" or "dword" && uint.TryParse(value, out var uint32Value))
            {
                result = _readWriteNet.Write(address, uint32Value);
            }
            else if (typeHint is "long" or "int64" && long.TryParse(value, out var int64Value))
            {
                result = _readWriteNet.Write(address, int64Value);
            }
            else if (typeHint is "ulong" or "uint64" && ulong.TryParse(value, out var uint64Value))
            {
                result = _readWriteNet.Write(address, uint64Value);
            }
            else if (typeHint is "float" or "single" && float.TryParse(value, out var typedFloatValue))
            {
                result = _readWriteNet.Write(address, typedFloatValue);
            }
            else if (typeHint is "double" && double.TryParse(value, out var doubleValue))
            {
                result = _readWriteNet.Write(address, doubleValue);
            }
            else if (typeHint is "string")
            {
                var stringLength = (ushort)Math.Max(1, normalizedLength ?? value.Length);
                result = _readWriteNet.Write(address, value.PadRight(stringLength, '\0').Substring(0, stringLength));
            }
            else if (bool.TryParse(value, out var boolValue))
            {
                result = _readWriteNet.Write(address, boolValue);
            }
            else if (int.TryParse(value, out var intValue))
            {
                result = _readWriteNet.Write(address, intValue);
            }
            else if (float.TryParse(value, out var floatValue))
            {
                result = _readWriteNet.Write(address, floatValue);
            }
            else
            {
                result = _readWriteNet.Write(address, value);
            }

            return result.IsSuccess ? ProtocolResult.Ok() : ProtocolResult.Fail(result.Message);
        }, ct);
    }

    public Task<ProtocolResult<IReadOnlyList<ProtocolNode>>> BrowseAsync(string? parent = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(ProtocolResult<IReadOnlyList<ProtocolNode>>.Fail("Browse is not supported for Byoe Modbus endpoints."));
    }

    public async IAsyncEnumerable<ProtocolValue> SubscribeAsync(
        IReadOnlyList<ProtocolReadRequest> addresses,
        int intervalMs = 1000,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!IsConnected || _readWriteNet is null || addresses.Count == 0)
            yield break;

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(100, intervalMs)));

        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var request in addresses)
            {
                var result = await ReadAsync(request, ct);
                if (result.Success && result.Data is not null)
                    yield return result.Data;
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
            _modbusTcp?.ConnectClose();
            _modbusRtu?.Close();
            _siemensS7?.ConnectClose();
            _melsecMc?.ConnectClose();
            _omronFins?.ConnectClose();
            _deltaTcp?.ConnectClose();
            _xinjeTcp?.ConnectClose();
            return ProtocolResult.Ok();
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }
        finally
        {
            _readWriteNet = null;
            _modbusTcp = null;
            _modbusRtu = null;
            _siemensS7 = null;
            _melsecMc = null;
            _omronFins = null;
            _deltaTcp = null;
            _xinjeTcp = null;
            _readTypeHint = null;
            _activeProtocolKey = null;
            IsConnected = false;
            ProtocolName = "Byoe";
        }
    }

    private static ProtocolResult<ByoeConnectTarget> ParseTarget(string endpoint, Dictionary<string, string>? extra)
    {
        var protocolHint = ParseProtocolHint(extra);

        if (protocolHint is "modbus.tcp" or "modbus-tcp" or "mbtcp")
        {
            var station = ParseByte(extra, "station", 1);
            var endpointParts = endpoint.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var host = endpointParts.Length > 0 ? endpointParts[0] : null;
            var port = endpointParts.Length > 1 && int.TryParse(endpointParts[1], out var parsedPort) ? parsedPort : 502;

            if (!string.IsNullOrWhiteSpace(host))
                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("tcp", host, port, null, station, 9600, extra));
        }

        if (protocolHint is "modbus.rtu" or "modbus-rtu" or "mbrtu")
        {
            var station = ParseByte(extra, "station", 1);
            var baudRate = ParseInt(extra, "baud", ParseInt(extra, "baudrate", 9600));
            if (!string.IsNullOrWhiteSpace(endpoint))
                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("rtu", null, 0, endpoint, station, baudRate, extra));
        }

        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            var scheme = uri.Scheme.ToLowerInvariant();
            if (scheme is "modbus.tcp" or "modbus-tcp" or "mbtcp")
            {
                var station = GetByte(uri, extra, "station", 1);
                var port = uri.Port > 0 ? uri.Port : 502;
                var host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
                if (string.IsNullOrWhiteSpace(host))
                    return ProtocolResult<ByoeConnectTarget>.Fail("Missing host for Modbus TCP endpoint.");

                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("tcp", host, port, null, station, 9600, extra));
            }

            if (scheme is "modbus.rtu" or "modbus-rtu" or "mbrtu")
            {
                var portName = string.IsNullOrWhiteSpace(uri.Host)
                    ? uri.AbsolutePath.Trim('/')
                    : uri.Host;

                if (string.IsNullOrWhiteSpace(portName))
                    return ProtocolResult<ByoeConnectTarget>.Fail("Missing serial port name for Modbus RTU endpoint.");

                var station = GetByte(uri, extra, "station", 1);
                var baudRate = GetInt(uri, extra, "baud", GetInt(uri, extra, "baudrate", 9600));

                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("rtu", null, 0, portName, station, baudRate, extra));
            }

            if (scheme is "s7" or "siemens.s7" or "siemens")
            {
                var host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
                if (string.IsNullOrWhiteSpace(host))
                    return ProtocolResult<ByoeConnectTarget>.Fail("Missing host for Siemens S7 endpoint.");

                var port = uri.Port > 0 ? uri.Port : 102;
                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("s7", host, port, null, 1, 9600, MergeQuery(extra, uri)));
            }

            if (scheme is "melsec" or "melsec.mc" or "mc")
            {
                var host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
                if (string.IsNullOrWhiteSpace(host))
                    return ProtocolResult<ByoeConnectTarget>.Fail("Missing host for Mitsubishi MC endpoint.");

                var port = uri.Port > 0 ? uri.Port : 6000;
                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("melsec", host, port, null, 1, 9600, MergeQuery(extra, uri)));
            }

            if (scheme is "omron" or "omron.fins" or "fins")
            {
                var host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
                if (string.IsNullOrWhiteSpace(host))
                    return ProtocolResult<ByoeConnectTarget>.Fail("Missing host for Omron FINS endpoint.");

                var port = uri.Port > 0 ? uri.Port : 9600;
                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("omron", host, port, null, 1, 9600, MergeQuery(extra, uri)));
            }

            if (scheme is "delta")
            {
                var host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
                if (string.IsNullOrWhiteSpace(host))
                    return ProtocolResult<ByoeConnectTarget>.Fail("Missing host for Delta endpoint.");

                var port = uri.Port > 0 ? uri.Port : 502;
                var station = GetByte(uri, extra, "station", 1);
                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("delta", host, port, null, station, 9600, MergeQuery(extra, uri)));
            }

            if (scheme is "xinje")
            {
                var host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
                if (string.IsNullOrWhiteSpace(host))
                    return ProtocolResult<ByoeConnectTarget>.Fail("Missing host for Xinje endpoint.");

                var port = uri.Port > 0 ? uri.Port : 502;
                var station = GetByte(uri, extra, "station", 1);
                return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("xinje", host, port, null, station, 9600, MergeQuery(extra, uri)));
            }
        }

        if (endpoint.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
        {
            var station = ParseByte(extra, "station", 1);
            var baudRate = ParseInt(extra, "baud", ParseInt(extra, "baudrate", 9600));
            return ProtocolResult<ByoeConnectTarget>.Ok(new ByoeConnectTarget("rtu", null, 0, endpoint, station, baudRate, extra));
        }

        return ProtocolResult<ByoeConnectTarget>.Fail("Unsupported Byoe endpoint. Use modbus.tcp://, modbus.rtu://, s7://, melsec://, omron://, delta:// or xinje:// endpoint formats.");
    }

    private static Dictionary<string, string>? MergeQuery(Dictionary<string, string>? extra, Uri uri)
    {
        Dictionary<string, string>? merged = null;
        if (extra is not null)
        {
            merged = new Dictionary<string, string>(extra, StringComparer.OrdinalIgnoreCase);
        }

        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in query)
        {
            var index = pair.IndexOf('=');
            if (index <= 0)
                continue;

            var key = Uri.UnescapeDataString(pair[..index]);
            var value = Uri.UnescapeDataString(pair[(index + 1)..]);
            merged ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            merged[key] = value;
        }

        return merged;
    }

    private static DeltaSeries ParseDeltaSeries(Dictionary<string, string>? extra, DeltaSeries fallback)
    {
        if (extra is null)
            return fallback;

        if (!extra.TryGetValue("series", out var value) || string.IsNullOrWhiteSpace(value))
            return fallback;

        return value.Trim().ToLowerInvariant() switch
        {
            "as" or "as300" => DeltaSeries.AS,
            "dvp" => DeltaSeries.Dvp,
            _ => fallback
        };
    }

    private static XinJESeries ParseXinjeSeries(Dictionary<string, string>? extra, XinJESeries fallback)
    {
        if (extra is null)
            return fallback;

        if (!extra.TryGetValue("series", out var value) || string.IsNullOrWhiteSpace(value))
            return fallback;

        return value.Trim().ToLowerInvariant() switch
        {
            "xd" => XinJESeries.XD,
            "xl" => XinJESeries.XL,
            "xc" => XinJESeries.XC,
            _ => fallback
        };
    }

    private static SiemensPLCS ParseSiemensPlcType(Dictionary<string, string>? extra, SiemensPLCS fallback)
    {
        if (extra is null)
            return fallback;

        if (!extra.TryGetValue("plc", out var value) || string.IsNullOrWhiteSpace(value))
            return fallback;

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "s1200" => SiemensPLCS.S1200,
            "s1500" => SiemensPLCS.S1500,
            "s300" => SiemensPLCS.S300,
            "s400" => SiemensPLCS.S400,
            "s200" => SiemensPLCS.S200,
            "s200smart" => SiemensPLCS.S200Smart,
            _ => fallback
        };
    }

    private static byte GetByte(Uri uri, Dictionary<string, string>? extra, string key, byte fallback)
    {
        var queryValue = GetQueryValue(uri, key);
        if (!string.IsNullOrWhiteSpace(queryValue) && byte.TryParse(queryValue, out var parsed))
            return parsed;

        return ParseByte(extra, key, fallback);
    }

    private static int GetInt(Uri uri, Dictionary<string, string>? extra, string key, int fallback)
    {
        var queryValue = GetQueryValue(uri, key);
        if (!string.IsNullOrWhiteSpace(queryValue) && int.TryParse(queryValue, out var parsed))
            return parsed;

        return ParseInt(extra, key, fallback);
    }

    private static byte ParseByte(Dictionary<string, string>? extra, string key, byte fallback)
    {
        if (extra is not null && extra.TryGetValue(key, out var value) && byte.TryParse(value, out var parsed))
            return parsed;

        return fallback;
    }

    private static int ParseInt(Dictionary<string, string>? extra, string key, int fallback)
    {
        if (extra is not null && extra.TryGetValue(key, out var value) && int.TryParse(value, out var parsed))
            return parsed;

        return fallback;
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

    private static ProtocolValue ToProtocolValue(string address, string? value, string? dataType = null, int? length = null)
        => new(address, value, DateTime.UtcNow, "Good", dataType, length);

    private static string? ParseProtocolHint(Dictionary<string, string>? extra)
    {
        if (extra is null)
            return null;

        return extra.TryGetValue("protocol", out var value)
            ? value.Trim().ToLowerInvariant()
            : null;
    }

    private static string? ParseTypeHint(Dictionary<string, string>? extra)
    {
        if (extra is null)
            return null;

        if (!extra.TryGetValue("type", out var value) || string.IsNullOrWhiteSpace(value))
            return null;

        var type = value.Trim().ToLowerInvariant();
        return type switch
        {
            "int32" => "int",
            "single" => "float",
            "text" => "string",
            _ => type
        };
    }

    private static ProtocolResult<ProtocolValue> ReadByType(IReadWriteNet net, string address, string type, int? length)
    {
        switch (type)
        {
            case "bool":
                {
                    var result = net.ReadBool(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "byte":
                {
                    var result = net.Read(address, 1);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.Length > 0 ? result.Content[0].ToString() : "0", type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "short":
            case "int16":
                {
                    var result = net.ReadInt16(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "ushort":
            case "uint16":
            case "word":
                {
                    var result = net.ReadUInt16(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "int":
            case "int32":
                {
                    var result = net.ReadInt32(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "uint":
            case "uint32":
            case "dword":
                {
                    var result = net.ReadUInt32(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "long":
            case "int64":
                {
                    var result = net.ReadInt64(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "ulong":
            case "uint64":
                {
                    var result = net.ReadUInt64(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "float":
            case "single":
                {
                    var result = net.ReadFloat(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "double":
                {
                    var result = net.ReadDouble(address);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, result.Content.ToString(), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "string":
                {
                    var result = net.ReadString(address, (ushort)(length ?? 64), Encoding.ASCII);
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, (result.Content ?? string.Empty).TrimEnd('\0'), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            case "hex":
                {
                    var result = net.Read(address, (ushort)(length ?? 16));
                    return result.IsSuccess
                        ? ProtocolResult<ProtocolValue>.Ok(ToProtocolValue(address, Convert.ToHexString(result.Content), type, length))
                        : ProtocolResult<ProtocolValue>.Fail(result.Message);
                }
            default:
                return ProtocolResult<ProtocolValue>.Fail($"Unsupported read type hint: {type}");
        }
    }

    private static int? NormalizeLength(string? protocolKey, string? typeHint, int? requestedLength)
    {
        if (requestedLength.HasValue && requestedLength.Value > 0)
            return requestedLength.Value;

        if (string.IsNullOrWhiteSpace(typeHint))
            return requestedLength;

        var normalizedType = typeHint.Trim().ToLowerInvariant();
        return normalizedType switch
        {
            "string" => 64,
            "double" => IsModbusProtocol(protocolKey) ? 4 : null,
            "float" or "single" => IsModbusProtocol(protocolKey) ? 2 : null,
            "long" or "int64" or "ulong" or "uint64" => IsModbusProtocol(protocolKey) ? 4 : null,
            "int" or "int32" or "uint" or "uint32" or "dword" => IsModbusProtocol(protocolKey) ? 2 : null,
            "short" or "int16" or "ushort" or "uint16" or "word" => IsModbusProtocol(protocolKey) ? 1 : null,
            _ => requestedLength
        };
    }

    private static string? ValidateAddressTypeCompatibility(string? protocolKey, string address, string? typeHint, int? length)
    {
        if (string.IsNullOrWhiteSpace(typeHint))
            return null;

        var normalizedType = typeHint.Trim().ToLowerInvariant();
        if (protocolKey is "siemens.s7" && !IsSupportedS7Type(address, normalizedType, length))
            return $"S7 address '{address}' is not compatible with data type '{normalizedType}'.";

        if (IsModbusProtocol(protocolKey) && !IsSupportedModbusType(normalizedType, length))
            return $"Modbus address '{address}' is not compatible with data type '{normalizedType}'.";

        return null;
    }

    private static bool IsSupportedS7Type(string address, string typeHint, int? length)
    {
        var normalizedAddress = address.Trim().ToUpperInvariant();
        var isBitAddress = normalizedAddress.Contains("DBX", StringComparison.OrdinalIgnoreCase)
            || normalizedAddress.StartsWith("I", StringComparison.OrdinalIgnoreCase)
            || normalizedAddress.StartsWith("Q", StringComparison.OrdinalIgnoreCase)
            || normalizedAddress.StartsWith("M", StringComparison.OrdinalIgnoreCase) && normalizedAddress.Contains('.');
        var isByteAddress = normalizedAddress.Contains("DBB", StringComparison.OrdinalIgnoreCase);
        var isWordAddress = normalizedAddress.Contains("DBW", StringComparison.OrdinalIgnoreCase);
        var isDWordAddress = normalizedAddress.Contains("DBD", StringComparison.OrdinalIgnoreCase);

        return typeHint switch
        {
            "bool" or "boolean" => isBitAddress || normalizedAddress.Contains('.'),
            "byte" => isByteAddress || (!isWordAddress && !isDWordAddress && !isBitAddress),
            "short" or "int16" or "ushort" or "uint16" or "word" => !isBitAddress,
            "int" or "int32" or "uint" or "uint32" or "dword" or "float" or "single" or "double" => isDWordAddress || !isBitAddress,
            "string" => (length ?? 0) > 0 && (isByteAddress || normalizedAddress.Contains("DB", StringComparison.OrdinalIgnoreCase)),
            "hex" => true,
            _ => true
        };
    }

    private static bool IsSupportedModbusType(string typeHint, int? length)
    {
        return typeHint switch
        {
            "bool" or "boolean" => true,
            "byte" or "short" or "int16" or "ushort" or "uint16" or "word" => true,
            "int" or "int32" or "uint" or "uint32" or "dword" or "float" or "single" => true,
            "long" or "int64" or "ulong" or "uint64" or "double" => true,
            "string" => (length ?? 0) > 0,
            "hex" => true,
            _ => false
        };
    }

    private static bool IsModbusProtocol(string? protocolKey)
        => protocolKey is "modbus.tcp" or "modbus.rtu";

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

    private sealed record ByoeConnectTarget(string Transport, string? Host, int Port, string? PortName, byte Station, int BaudRate, Dictionary<string, string>? Extra);
}
