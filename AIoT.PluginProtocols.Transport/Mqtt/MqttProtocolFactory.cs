using AIoT.Protocols.Abstractions;

namespace AIoT.PluginProtocols.Transport.Mqtt;

public sealed class MqttProtocolFactory : IProtocolFactory
    , IProtocolCatalogProvider
{
    public bool CanHandle(string endpointOrProtocol)
    {
        if (string.IsNullOrWhiteSpace(endpointOrProtocol))
            return false;

        var value = endpointOrProtocol.Trim().ToLowerInvariant();
        return value.StartsWith("mqtt://")
               || value.StartsWith("mqtts://")
               || value == "mqtt";
    }

    public IProtocolClient Create() => new MqttProtocolClient();

    public IEnumerable<string> GetSupportedProtocolKeys()
    {
        yield return "mqtt";
    }
}
