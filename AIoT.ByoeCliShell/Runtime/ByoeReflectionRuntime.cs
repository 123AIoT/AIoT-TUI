using Byoe.BasicFramework;

namespace AIoT.ByoeCliShell.Runtime;

internal static class ByoeReflectionRuntime
{
    private static readonly Dictionary<string, ClientSpec> ClientSpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["modbus.tcp"] = new("Byoe.ModBus.ModbusTcpNet", "Byoe", static options =>
        {
            var endpoint = EndpointParser.ParseTcp(options["endpoint"], 502);
            var station = EndpointParser.ParseByte(options, "station", 1);
            return [endpoint.Host, endpoint.Port, station];
        }),
        ["modbus.rtu"] = new("Byoe.ModBus.ModbusRtu", "Byoe", static options =>
        {
            var station = EndpointParser.ParseByte(options, "station", 1);
            return [station];
        }, static (instance, options) =>
        {
            var endpoint = EndpointParser.ParseSerialPortName(options["endpoint"]);
            var baud = EndpointParser.ParseInt(options, "baud", 9600);
            ReflectionHelper.InvokeRequired(instance, "SerialPortInni", endpoint, baud);
            ReflectionHelper.InvokeRequired(instance, "Open");
        }, static instance => ReflectionHelper.InvokeRequired(instance, "Close")),
        ["siemens.s7"] = new("Byoe.Profinet.Siemens.SiemensS7Net", "Byoe", static options =>
        {
            var endpoint = EndpointParser.ParseTcp(options["endpoint"], 102);
            var plcName = options.GetValueOrDefault("plc", "s1200");
            var plcType = ParseEnum("Byoe.Profinet.Siemens.SiemensPLCS, Byoe", plcName, "S1200");
            return [plcType, endpoint.Host];
        }, static (instance, options) =>
        {
            var endpoint = EndpointParser.ParseTcp(options["endpoint"], 102);
            ReflectionHelper.SetOptionalProperty(instance, "Port", endpoint.Port);
            ReflectionHelper.SetOptionalProperty(instance, "Rack", EndpointParser.ParseByte(options, "rack", 0));
            ReflectionHelper.SetOptionalProperty(instance, "Slot", EndpointParser.ParseByte(options, "slot", 0));
            ReflectionHelper.InvokeRequired(instance, "ConnectServer");
        }, static instance => ReflectionHelper.InvokeRequired(instance, "ConnectClose")),
        ["melsec"] = new("Byoe.Profinet.Melsec.MelsecMcNet", "Byoe", static options =>
        {
            var endpoint = EndpointParser.ParseTcp(options["endpoint"], 6000);
            return [endpoint.Host, endpoint.Port];
        }),
        ["omron"] = new("Byoe.Profinet.Omron.OmronFinsNet", "Byoe", static options =>
        {
            var endpoint = EndpointParser.ParseTcp(options["endpoint"], 9600);
            return [endpoint.Host, endpoint.Port];
        }),
        ["delta"] = new("Byoe.Profinet.Delta.DeltaTcpNet", "Byoe", static options =>
        {
            var endpoint = EndpointParser.ParseTcp(options["endpoint"], 502);
            var station = EndpointParser.ParseByte(options, "station", 1);
            return [endpoint.Host, endpoint.Port, station];
        }, static (instance, options) =>
        {
            var series = ParseEnum("Byoe.Profinet.Delta.DeltaSeries, Byoe", options.GetValueOrDefault("series", "dvp"), "Dvp");
            ReflectionHelper.SetOptionalProperty(instance, "Series", series);
            ReflectionHelper.InvokeRequired(instance, "ConnectServer");
        }, static instance => ReflectionHelper.InvokeRequired(instance, "ConnectClose")),
        ["xinje"] = new("Byoe.Profinet.XINJE.XinJETcpNet", "Byoe", static options =>
        {
            var endpoint = EndpointParser.ParseTcp(options["endpoint"], 502);
            var station = EndpointParser.ParseByte(options, "station", 1);
            return [endpoint.Host, endpoint.Port, station];
        }, static (instance, options) =>
        {
            var series = ParseEnum("Byoe.Profinet.XINJE.XinJESeries, Byoe", options.GetValueOrDefault("series", "xc"), "XC");
            ReflectionHelper.SetOptionalProperty(instance, "Series", series);
            ReflectionHelper.InvokeRequired(instance, "ConnectServer");
        }, static instance => ReflectionHelper.InvokeRequired(instance, "ConnectClose"))
    };

    private static readonly Dictionary<string, SimulatorSpec> SimulatorSpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["modbus.tcp"] = new("Byoe.ModBus.ModbusTcpServer", "Byoe")
    };

    public static IReadOnlyList<string> GetSupportedProtocols() => ClientSpecs.Keys.OrderBy(static x => x).ToList();

    public static IReadOnlyList<string> GetSupportedSimulators() => SimulatorSpecs.Keys.OrderBy(static x => x).ToList();

    public static IDisposableClient CreateClient(string protocol, string endpoint, IReadOnlyDictionary<string, string> options)
    {
        if (!ClientSpecs.TryGetValue(protocol, out var spec))
            throw new InvalidOperationException($"Unsupported protocol '{protocol}'.");

        var merged = MergeOptions(options, endpoint);
        var instance = ReflectionHelper.CreateInstance(spec.TypeName, spec.AssemblyName, spec.BuildCtorArgs(merged));

        if (spec.OnConnect is not null)
            spec.OnConnect(instance, merged);
        else
            ReflectionHelper.InvokeRequired(instance, "ConnectServer");

        return new ReflectionClient(protocol, instance, spec.OnDispose ?? (o => ReflectionHelper.InvokeRequired(o, "ConnectClose")));
    }

    public static IDisposableSimulator CreateSimulator(string protocol, int port)
    {
        if (!SimulatorSpecs.TryGetValue(protocol, out var spec))
            throw new InvalidOperationException($"Unsupported simulator protocol '{protocol}'.");

        var instance = ReflectionHelper.CreateInstance(spec.TypeName, spec.AssemblyName, []);
        ReflectionHelper.InvokeRequired(instance, "ServerStart", port, true);

        return new ReflectionSimulator(protocol, instance, o => ReflectionHelper.InvokeRequired(o, "ServerClose"));
    }

    private static Dictionary<string, string> MergeOptions(IReadOnlyDictionary<string, string> options, string endpoint)
    {
        var merged = EndpointParser.ExtractQueryOptions(endpoint);

        foreach (var pair in options)
            merged[pair.Key] = pair.Value;

        merged["endpoint"] = endpoint;

        return merged;
    }

    private static object ParseEnum(string assemblyQualifiedName, string rawValue, string fallback)
    {
        var type = Type.GetType(assemblyQualifiedName, throwOnError: true)!;
        var normalized = rawValue.Replace(".", string.Empty, StringComparison.OrdinalIgnoreCase);

        foreach (var name in Enum.GetNames(type))
        {
            if (string.Equals(name, normalized, StringComparison.OrdinalIgnoreCase))
                return Enum.Parse(type, name, ignoreCase: true);
        }

        return Enum.Parse(type, fallback, ignoreCase: true);
    }

    private sealed record ClientSpec(
        string TypeName,
        string AssemblyName,
        Func<IReadOnlyDictionary<string, string>, object?[]> BuildCtorArgs,
        Action<object, IReadOnlyDictionary<string, string>>? OnConnect = null,
        Action<object>? OnDispose = null);

    private sealed record SimulatorSpec(string TypeName, string AssemblyName);
}
