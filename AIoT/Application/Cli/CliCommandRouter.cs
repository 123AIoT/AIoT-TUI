using AIoT.Protocols.Abstractions;
using AIoT.Protocols;
using System.Globalization;

namespace AIoT.Cli;

public static class CliCommandRouter
{
    public static bool IsCliMode(string[] args)
    {
        if (args.Length == 0)
            return false;

        var first = args[0].ToLowerInvariant();
        return first is "read" or "write" or "browse" or "monitor";
    }

    public static CliCommand? Parse(string[] args, out string? error)
    {
        error = null;

        if (args.Length < 2)
        {
            error = "Missing required arguments.";
            return null;
        }

        var name = args[0].ToLowerInvariant();
        var endpoint = args[1];
        string? protocol = null;
        var addresses = new List<string>();
        string? value = null;
        string? dataType = null;
        byte? station = null;
        int? baudRate = null;
        string? plc = null;
        byte? rack = null;
        byte? slot = null;
        string? username = null;
        string? password = null;
        string? clientId = null;
        int? qos = null;
        var retain = false;
        string? topic = null;
        string? wsSubProtocol = null;
        string? wsHeader = null;
        string? wsAddress = null;
        var format = CliFormat.Text;
        var intervalMs = 1000;
        int? count = null;
        int? length = null;

        var positional = new List<string>();
        for (int i = 2; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                if (args[i] == "--format" && i + 1 < args.Length)
                {
                    i++;
                    format = args[i].ToLowerInvariant() switch
                    {
                        "json" => CliFormat.Json,
                        "ndjson" => CliFormat.Ndjson,
                        _ => CliFormat.Text
                    };
                }
                else if (args[i] == "--user" && i + 1 < args.Length)
                {
                    username = args[++i];
                }
                else if (args[i] == "--pass" && i + 1 < args.Length)
                {
                    password = args[++i];
                }
                else if (args[i] == "--client-id" && i + 1 < args.Length)
                {
                    clientId = args[++i];
                }
                else if (args[i] == "--qos" && i + 1 < args.Length && int.TryParse(args[++i], out var parsedQos))
                {
                    qos = parsedQos;
                }
                else if (args[i] == "--retain")
                {
                    retain = true;
                }
                else if (args[i] == "--topic" && i + 1 < args.Length)
                {
                    topic = args[++i];
                }
                else if (args[i] == "--ws-subprotocol" && i + 1 < args.Length)
                {
                    wsSubProtocol = args[++i];
                }
                else if (args[i] == "--ws-header" && i + 1 < args.Length)
                {
                    wsHeader = args[++i];
                }
                else if (args[i] == "--ws-address" && i + 1 < args.Length)
                {
                    wsAddress = args[++i];
                }
                else if (args[i] == "--protocol" && i + 1 < args.Length)
                {
                    protocol = args[++i];
                }
                else if (args[i] == "--type" && i + 1 < args.Length)
                {
                    dataType = args[++i];
                }
                else if (args[i] == "--station" && i + 1 < args.Length && byte.TryParse(args[++i], out var st))
                {
                    station = st;
                }
                else if (args[i] == "--baud" && i + 1 < args.Length && int.TryParse(args[++i], out var bd))
                {
                    baudRate = bd;
                }
                else if (args[i] == "--plc" && i + 1 < args.Length)
                {
                    plc = args[++i];
                }
                else if (args[i] == "--rack" && i + 1 < args.Length && byte.TryParse(args[++i], out var r))
                {
                    rack = r;
                }
                else if (args[i] == "--slot" && i + 1 < args.Length && byte.TryParse(args[++i], out var s))
                {
                    slot = s;
                }
                else if (args[i] == "--interval" && i + 1 < args.Length && int.TryParse(args[++i], out var interval))
                {
                    intervalMs = interval;
                }
                else if (args[i] == "--count" && i + 1 < args.Length && int.TryParse(args[++i], out var c))
                {
                    count = c;
                }
                else if (args[i] == "--length" && i + 1 < args.Length && int.TryParse(args[++i], out var len))
                {
                    length = len;
                }
            }
            else
            {
                positional.Add(args[i]);
            }
        }

        switch (name)
        {
            case "read":
            case "monitor":
                if (positional.Count < 1)
                {
                    var normalizedProtocol = (protocol ?? string.Empty).Trim().ToLowerInvariant();
                    var endpointLower = endpoint.Trim().ToLowerInvariant();
                    var isMqtt = normalizedProtocol == "mqtt" || endpointLower.StartsWith("mqtt://") || endpointLower.StartsWith("mqtts://");
                    var isWebSocket = normalizedProtocol is "websocket" or "ws" || endpointLower.StartsWith("ws://") || endpointLower.StartsWith("wss://");

                    if (!isMqtt && !isWebSocket)
                    {
                        error = $"{name} requires at least one address.";
                        return null;
                    }
                }
                addresses.AddRange(positional);
                break;
            case "browse":
                if (positional.Count > 0)
                    addresses.Add(positional[0]);
                break;
            case "write":
                if (positional.Count < 2)
                {
                    error = "write requires <address> and <value>.";
                    return null;
                }
                addresses.Add(positional[0]);
                value = positional[1];
                break;
            default:
                error = $"Unsupported command: {name}";
                return null;
        }

        return new CliCommand
        {
            Name = name,
            Endpoint = endpoint,
            Protocol = protocol,
            Addresses = addresses,
            Value = value,
            DataType = dataType,
            Length = length,
            Station = station,
            BaudRate = baudRate,
            Plc = plc,
            Rack = rack,
            Slot = slot,
            Username = username,
            Password = password,
            ClientId = clientId,
            Qos = qos,
            Retain = retain,
            Topic = topic,
            WebSocketSubProtocol = wsSubProtocol,
            WebSocketHeader = wsHeader,
            WebSocketAddress = wsAddress,
            Format = format,
            IntervalMs = intervalMs,
            Count = count
        };
    }

    public static async Task<int> RunAsync(CliCommand command, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(command.Protocol) && !IsSupportedProtocol(command.Protocol))
        {
            Console.Error.WriteLine($"Unsupported --protocol value: {command.Protocol}. Supported: {string.Join(", ", GetSupportedProtocols())}");
            return 2;
        }

        if (!string.IsNullOrWhiteSpace(command.DataType) && !IsSupportedDataType(command.DataType))
        {
            Console.Error.WriteLine($"Unsupported --type value: {command.DataType}");
            return 2;
        }

        if (command.Qos.HasValue && (command.Qos.Value < 0 || command.Qos.Value > 2))
        {
            Console.Error.WriteLine("Unsupported --qos value. Use 0, 1 or 2.");
            return 2;
        }

        var endpointOrProtocol = command.Protocol ?? command.Endpoint;
        await using IProtocolClient client = ProtocolClientFactory.Create(endpointOrProtocol);
        var connect = await client.ConnectAsync(
            new ProtocolConnectOptions(command.Endpoint, command.Username, command.Password, BuildExtra(command)),
            ct);
        if (!connect.Success)
        {
            Console.Error.WriteLine(connect.Error ?? "Connection failed.");
            return 1;
        }

        return command.Name switch
        {
            "read" => await RunReadAsync(client, command, ct),
            "write" => await RunWriteAsync(client, command, ct),
            "browse" => await RunBrowseAsync(client, command, ct),
            "monitor" => await RunMonitorAsync(client, command, ct),
            _ => 2
        };
    }

    private static async Task<int> RunReadAsync(IProtocolClient client, CliCommand command, CancellationToken ct)
    {
        foreach (var address in command.Addresses)
        {
            var result = await client.ReadAsync(new ProtocolReadRequest(address, command.DataType, command.Length, BuildExtra(command)), ct);
            if (!result.Success || result.Data == null)
            {
                Console.Error.WriteLine(result.Error ?? $"Read failed for {address}");
                return 3;
            }

            if (command.Format == CliFormat.Text)
            {
                Console.WriteLine($"{result.Data.Address} = {result.Data.Value} ({result.Data.Quality})");
            }
            else
            {
                CliJsonWriter.WriteJson(new
                {
                    command = "read",
                    endpoint = command.Endpoint,
                    address = result.Data.Address,
                    value = result.Data.Value,
                    dataType = result.Data.DataType,
                    length = result.Data.Length,
                    timestamp = result.Data.Timestamp,
                    quality = result.Data.Quality
                });
            }
        }

        return 0;
    }

    private static async Task<int> RunWriteAsync(IProtocolClient client, CliCommand command, CancellationToken ct)
    {
        var writeValue = ConvertWriteValue(command.Value ?? string.Empty, command.DataType);
        if (writeValue == null)
        {
            Console.Error.WriteLine($"Unsupported --type value: {command.DataType}");
            return 2;
        }

        var result = await client.WriteAsync(
            new ProtocolWriteRequest(command.Addresses[0], writeValue, command.DataType, command.Length, BuildExtra(command)),
            ct);

        if (command.Format == CliFormat.Text)
        {
            Console.WriteLine(result.Success ? "OK" : $"FAIL: {result.Error}");
        }
        else
        {
            CliJsonWriter.WriteJson(new
            {
                command = "write",
                endpoint = command.Endpoint,
                address = command.Addresses[0],
                value = command.Value,
                dataType = command.DataType,
                length = command.Length,
                success = result.Success,
                error = result.Error
            });
        }

        return result.Success ? 0 : 3;
    }

    private static Dictionary<string, string>? BuildExtra(CliCommand command)
    {
        Dictionary<string, string>? extra = null;

        if (!string.IsNullOrWhiteSpace(command.Protocol))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["protocol"] = command.Protocol;
        }

        if (!string.IsNullOrWhiteSpace(command.ClientId))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["clientId"] = command.ClientId;
        }

        if (command.Qos.HasValue)
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["qos"] = command.Qos.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (command.Retain)
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["retain"] = "true";
        }

        if (!string.IsNullOrWhiteSpace(command.Topic))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["topic"] = command.Topic;
        }

        if (!string.IsNullOrWhiteSpace(command.WebSocketSubProtocol))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["subprotocol"] = command.WebSocketSubProtocol;
        }

        if (!string.IsNullOrWhiteSpace(command.WebSocketHeader))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["header"] = command.WebSocketHeader;
        }

        if (!string.IsNullOrWhiteSpace(command.WebSocketAddress))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["address"] = command.WebSocketAddress;
        }

        if ((string.Equals(command.Protocol, "websocket", StringComparison.OrdinalIgnoreCase)
             || string.Equals(command.Protocol, "ws", StringComparison.OrdinalIgnoreCase))
            && command.Addresses.Count > 0)
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["address"] = command.Addresses[0];
        }

        if (!string.IsNullOrWhiteSpace(command.DataType))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["type"] = command.DataType;
        }

        if (command.Station.HasValue)
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["station"] = command.Station.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (command.BaudRate.HasValue)
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["baud"] = command.BaudRate.Value.ToString(CultureInfo.InvariantCulture);
            extra["baudrate"] = command.BaudRate.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(command.Plc))
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["plc"] = command.Plc;
        }

        if (command.Rack.HasValue)
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["rack"] = command.Rack.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (command.Slot.HasValue)
        {
            extra ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            extra["slot"] = command.Slot.Value.ToString(CultureInfo.InvariantCulture);
        }

        return extra;
    }

    private static string? ConvertWriteValue(string value, string? dataType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
            return value;

        var type = dataType.Trim().ToLowerInvariant();
        return type switch
        {
            "bool" => bool.TryParse(value, out var b) ? b.ToString() : null,
            "byte" => byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bt) ? bt.ToString(CultureInfo.InvariantCulture) : null,
            "sbyte" => sbyte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sb) ? sb.ToString(CultureInfo.InvariantCulture) : null,
            "short" or "int16" => short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sh) ? sh.ToString(CultureInfo.InvariantCulture) : null,
            "ushort" or "word" => ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ush) ? ush.ToString(CultureInfo.InvariantCulture) : null,
            "int" or "int32" => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i.ToString(CultureInfo.InvariantCulture) : null,
            "uint" or "dword" => uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ui) ? ui.ToString(CultureInfo.InvariantCulture) : null,
            "long" or "int64" => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? l.ToString(CultureInfo.InvariantCulture) : null,
            "ulong" or "uint64" => ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ul) ? ul.ToString(CultureInfo.InvariantCulture) : null,
            "float" or "single" => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f.ToString(CultureInfo.InvariantCulture) : null,
            "double" => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d.ToString(CultureInfo.InvariantCulture) : null,
            "string" or "text" => value,
            "hex" => IsHexText(value) ? value : null,
            _ => null
        };
    }

    private static bool IsSupportedDataType(string dataType)
    {
        var type = dataType.Trim().ToLowerInvariant();
        return type is "bool" or "byte" or "sbyte" or "short" or "int16" or "ushort" or "word"
            or "int" or "int32" or "uint" or "dword" or "long" or "int64" or "ulong" or "uint64"
            or "float" or "single" or "double" or "string" or "text" or "hex";
    }

    public static string[] GetSupportedProtocols() =>
    [
        "opcua",
        "mqtt",
        "websocket",
        "modbus.tcp",
        "modbus.rtu",
        "siemens.s7",
        "melsec",
        "omron",
        "delta",
        "xinje"
    ];

    public static bool IsSupportedProtocol(string? protocol)
    {
        if (string.IsNullOrWhiteSpace(protocol))
            return false;

        return GetSupportedProtocols().Any(p => p.Equals(protocol.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsHexText(string value)
    {
        var text = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;
        if (text.Length == 0 || text.Length % 2 != 0)
            return false;

        for (var i = 0; i < text.Length; i++)
        {
            if (!Uri.IsHexDigit(text[i]))
                return false;
        }

        return true;
    }

    private static async Task<int> RunBrowseAsync(IProtocolClient client, CliCommand command, CancellationToken ct)
    {
        var parent = command.Addresses.FirstOrDefault();
        var result = await client.BrowseAsync(parent, ct);
        if (!result.Success || result.Data == null)
        {
            Console.Error.WriteLine(result.Error ?? "Browse failed");
            return 3;
        }

        if (command.Format == CliFormat.Text)
        {
            foreach (var node in result.Data)
            {
                Console.WriteLine($"{node.Id} | {node.DisplayName} | children={node.HasChildren}");
            }
        }
        else
        {
            CliJsonWriter.WriteJson(new
            {
                command = "browse",
                endpoint = command.Endpoint,
                parent,
                nodes = result.Data
            });
        }

        return 0;
    }

    private static async Task<int> RunMonitorAsync(IProtocolClient client, CliCommand command, CancellationToken ct)
    {
        int received = 0;
        var requests = command.Addresses
            .Select(address => new ProtocolReadRequest(address, command.DataType, command.Length, BuildExtra(command)))
            .ToList();

        await foreach (var value in client.SubscribeAsync(requests, command.IntervalMs, ct))
        {
            if (command.Format == CliFormat.Text)
            {
                Console.WriteLine($"{value.Timestamp:O} {value.Address} = {value.Value} [{value.DataType ?? "unknown"}] ({value.Quality})");
            }
            else
            {
                CliJsonWriter.WriteNdjson(new
                {
                    address = value.Address,
                    value = value.Value,
                    dataType = value.DataType,
                    length = value.Length,
                    timestamp = value.Timestamp,
                    quality = value.Quality
                });
            }

            received++;
            if (command.Count.HasValue && received >= command.Count.Value)
                break;
        }

        return 0;
    }
}
