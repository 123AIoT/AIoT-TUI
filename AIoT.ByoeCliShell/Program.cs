using System.Text.Json;
using AIoT.ByoeCliShell.Runtime;

namespace AIoT.ByoeCliShell;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintUsage();
                return 0;
            }

            var command = args[0].ToLowerInvariant();
            var options = CliArgs.Parse(args.Skip(1).ToArray());

            return command switch
            {
                "list" => RunList(options),
                "client-connect" => RunClientConnect(options),
                "client-read" => RunClientRead(options),
                "client-browse" => RunClientBrowse(options),
                "client-write" => RunClientWrite(options),
                "sim-run" => RunSimRun(options),
                _ => Fail($"Unknown command '{command}'.")
            };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    private static int RunList(Dictionary<string, string> options)
    {
        var payload = new
        {
            protocols = ByoeReflectionRuntime.GetSupportedProtocols(),
            simulators = ByoeReflectionRuntime.GetSupportedSimulators()
        };

        WriteOutput(options, payload);
        return 0;
    }

    private static int RunClientConnect(Dictionary<string, string> options)
    {
        var protocol = Require(options, "protocol");
        var endpoint = Require(options, "endpoint");

        using var client = ByoeReflectionRuntime.CreateClient(protocol, endpoint, options);

        WriteOutput(options, new
        {
            ok = true,
            mode = "client-connect",
            protocol,
            endpoint,
            connected = true
        });

        return 0;
    }

    private static int RunClientRead(Dictionary<string, string> options)
    {
        var protocol = Require(options, "protocol");
        var endpoint = Require(options, "endpoint");
        var address = Require(options, "address");
        var readType = options.GetValueOrDefault("type", "int");

        using var client = ByoeReflectionRuntime.CreateClient(protocol, endpoint, options);
        var result = client.Read(address, readType);

        WriteOutput(options, new { ok = true, mode = "client-read", protocol, endpoint, address, type = readType, value = result });
        return 0;
    }

    private static int RunClientBrowse(Dictionary<string, string> options)
    {
        var protocol = Require(options, "protocol");
        var endpoint = Require(options, "endpoint");
        var readType = options.GetValueOrDefault("type", "int");
        var addresses = ResolveBrowseAddresses(protocol, options);

        using var client = ByoeReflectionRuntime.CreateClient(protocol, endpoint, options);

        var items = new List<object>();
        foreach (var address in addresses)
        {
            try
            {
                var value = client.Read(address, readType);
                items.Add(new { address, ok = true, value });
            }
            catch (Exception ex)
            {
                items.Add(new { address, ok = false, error = ex.Message });
            }
        }

        var payload = new
        {
            ok = true,
            mode = "client-browse",
            protocol,
            endpoint,
            type = readType,
            count = items.Count,
            items
        };

        WriteOutput(options, payload, items);
        return 0;
    }

    private static int RunClientWrite(Dictionary<string, string> options)
    {
        var protocol = Require(options, "protocol");
        var endpoint = Require(options, "endpoint");
        var address = Require(options, "address");
        var value = Require(options, "value");
        var writeType = options.GetValueOrDefault("type", "int");

        using var client = ByoeReflectionRuntime.CreateClient(protocol, endpoint, options);
        client.Write(address, value, writeType);

        WriteOutput(options, new { ok = true, mode = "client-write", protocol, endpoint, address, type = writeType, value });
        return 0;
    }

    private static int RunSimRun(Dictionary<string, string> options)
    {
        var protocol = Require(options, "protocol");
        var port = int.Parse(options.GetValueOrDefault("port", "502"));
        var preloadAddress = options.GetValueOrDefault("preloadAddress", "100");
        var preloadValue = options.GetValueOrDefault("preloadValue", "123");

        using var sim = ByoeReflectionRuntime.CreateSimulator(protocol, port);
        sim.Write(preloadAddress, preloadValue, options.GetValueOrDefault("type", "int"));

        WriteOutput(options, new
        {
            ok = true,
            mode = "sim-run",
            protocol,
            port,
            preloadAddress,
            preloadValue,
            message = "Simulator started. Press Enter to stop."
        });

        Console.ReadLine();
        return 0;
    }

    private static List<string> ResolveBrowseAddresses(string protocol, IReadOnlyDictionary<string, string> options)
    {
        if (options.TryGetValue("addresses", out var rawAddresses) && !string.IsNullOrWhiteSpace(rawAddresses))
        {
            var parsed = rawAddresses
                .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (parsed.Count > 0)
                return parsed;
        }

        if (protocol.StartsWith("modbus", StringComparison.OrdinalIgnoreCase))
        {
            var start = EndpointParser.ParseInt(options, "start", 100);
            var count = Math.Clamp(EndpointParser.ParseInt(options, "count", 10), 1, 200);
            return Enumerable.Range(start, count).Select(static n => n.ToString()).ToList();
        }

        throw new InvalidOperationException("Missing browse addresses. Use --addresses a1,a2,... or (for modbus) --start <n> --count <n>.");
    }

    private static void WriteOutput(IReadOnlyDictionary<string, string> options, object payload, IReadOnlyList<object>? ndjsonItems = null)
    {
        var format = options.TryGetValue("format", out var f) ? (f ?? "json").Trim().ToLowerInvariant() : "json";
        switch (format)
        {
            case "json":
                Console.WriteLine(JsonSerializer.Serialize(payload, JsonOptions.Default));
                break;
            case "ndjson":
                if (ndjsonItems is { Count: > 0 })
                {
                    foreach (var item in ndjsonItems)
                        Console.WriteLine(JsonSerializer.Serialize(item, JsonOptions.Default));
                }
                else
                {
                    Console.WriteLine(JsonSerializer.Serialize(payload, JsonOptions.Default));
                }
                break;
            case "text":
                WriteTextPayload(payload);
                break;
            default:
                throw new InvalidOperationException("Invalid --format, use json|ndjson|text");
        }
    }

    private static void WriteTextPayload(object payload)
    {
        var element = JsonSerializer.SerializeToElement(payload, JsonOptions.Default);
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        Console.WriteLine($"{property.Name}:");
                        foreach (var item in property.Value.EnumerateArray())
                            Console.WriteLine($"  - {item}");
                    }
                    else
                    {
                        Console.WriteLine($"{property.Name}: {property.Value}");
                    }
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    Console.WriteLine(item);
                break;
            default:
                Console.WriteLine(element);
                break;
        }
    }

    private static bool IsHelp(string arg)
        => arg.Equals("help", StringComparison.OrdinalIgnoreCase)
           || arg.Equals("--help", StringComparison.OrdinalIgnoreCase)
           || arg.Equals("-h", StringComparison.OrdinalIgnoreCase);

    private static string Require(IReadOnlyDictionary<string, string> options, string key)
    {
        if (options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        throw new InvalidOperationException($"Missing required option --{key}");
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(JsonSerializer.Serialize(new { ok = false, error = message }, JsonOptions.Default));
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("aiot-byoe-cli commands:");
        Console.WriteLine("  list");
        Console.WriteLine("  client-connect --protocol <...> --endpoint <...>");
        Console.WriteLine("  client-read  --protocol <modbus.tcp|modbus.rtu|siemens.s7|melsec|omron|delta|xinje> --endpoint <...> --address <...> [--type int|float|bool|string]");
        Console.WriteLine("  client-browse --protocol <...> --endpoint <...> [--addresses a1,a2,...] [--start 100 --count 10] [--type int|float|bool|string]");
        Console.WriteLine("  client-write --protocol <...> --endpoint <...> --address <...> --value <...> [--type int|float|bool|string]");
        Console.WriteLine("  sim-run      --protocol modbus.tcp --port 502 [--preloadAddress 100 --preloadValue 123 --type int]");
        Console.WriteLine();
        Console.WriteLine("Common options for endpoint/protocol:");
        Console.WriteLine("  --station <id> --baud <rate> --plc s1200|s1500|s300|s400|s200|s200smart --rack <id> --slot <id>");
        Console.WriteLine("  --series <delta/xinje series>");
        Console.WriteLine("  --format json|ndjson|text");
    }
}
