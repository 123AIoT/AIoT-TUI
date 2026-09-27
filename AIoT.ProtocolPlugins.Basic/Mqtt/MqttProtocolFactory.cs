using AIoT.Protocols.Abstractions;

namespace AIoT.ProtocolPlugins.Basic.Mqtt;

public sealed class MqttProtocolFactory : IProtocolFactory
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
}
