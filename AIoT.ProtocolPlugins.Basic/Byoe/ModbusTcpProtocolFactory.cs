using AIoT.Protocols.Abstractions;

namespace AIoT.ProtocolPlugins.Basic.Byoe;

public sealed class ModbusTcpProtocolFactory : IProtocolFactory
{
    public bool CanHandle(string endpointOrProtocol)
    {
        if (string.IsNullOrWhiteSpace(endpointOrProtocol))
            return false;

        var value = endpointOrProtocol.Trim().ToLowerInvariant();
        return value.StartsWith("modbus.tcp://")
               || value.StartsWith("modbus-tcp://")
               || value.StartsWith("mbtcp://")
               || value == "modbus.tcp";
    }

    public IProtocolClient Create() => new ModbusTcpProtocolClient();
}
