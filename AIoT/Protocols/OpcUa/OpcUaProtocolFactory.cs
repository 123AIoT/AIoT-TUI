using AIoT.Protocols.Abstractions;

namespace AIoT.Protocols.OpcUa;

public sealed class OpcUaProtocolFactory : IProtocolFactory
    , IProtocolCatalogProvider
{
    public bool CanHandle(string endpointOrProtocol)
    {
        if (string.IsNullOrWhiteSpace(endpointOrProtocol))
            return false;

        var value = endpointOrProtocol.Trim().ToLowerInvariant();
        return value.StartsWith("opc.tcp://")
               || value.StartsWith("opcua://")
               || value == "opcua";
    }

    public IProtocolClient Create() => new OpcUaProtocolClient();

    public IEnumerable<string> GetSupportedProtocolKeys()
    {
        yield return "opcua";
    }
}
