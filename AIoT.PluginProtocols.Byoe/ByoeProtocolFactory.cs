using AIoT.Protocols.Abstractions;

namespace AIoT.PluginProtocols.Byoe;

public sealed class ByoeProtocolFactory : IProtocolFactory
    , IProtocolCatalogProvider
{
    public bool CanHandle(string endpointOrProtocol)
    {
        if (string.IsNullOrWhiteSpace(endpointOrProtocol))
            return false;

        var value = endpointOrProtocol.Trim().ToLowerInvariant();
        return value.StartsWith("modbus.tcp://")
               || value.StartsWith("modbus-tcp://")
               || value.StartsWith("mbtcp://")
               || value.StartsWith("modbus.rtu://")
               || value.StartsWith("modbus-rtu://")
               || value.StartsWith("mbrtu://")
               || value.StartsWith("s7://")
               || value.StartsWith("siemens.s7://")
               || value.StartsWith("siemens://")
               || value.StartsWith("melsec://")
               || value.StartsWith("melsec.mc://")
               || value.StartsWith("mc://")
               || value.StartsWith("omron://")
               || value.StartsWith("omron.fins://")
               || value.StartsWith("fins://")
               || value.StartsWith("delta://")
               || value.StartsWith("xinje://")
               || value == "siemens.s7"
               || value == "s7"
               || value == "melsec"
               || value == "omron"
               || value == "delta"
               || value == "xinje"
               || value.StartsWith("com");
    }

    public IProtocolClient Create() => new ByoeProtocolClient();

    public IEnumerable<string> GetSupportedProtocolKeys()
    {
        yield return "modbus.tcp";
        yield return "modbus.rtu";
        yield return "siemens.s7";
        yield return "melsec";
        yield return "omron";
        yield return "delta";
        yield return "xinje";
    }
}
