using AIoT.Protocols.Abstractions;

namespace AIoT.PluginSamples.Echo;

public sealed class EchoProtocolFactory : IProtocolFactory
{
    public bool CanHandle(string endpointOrProtocol)
    {
        if (string.IsNullOrWhiteSpace(endpointOrProtocol))
            return false;

        var value = endpointOrProtocol.Trim().ToLowerInvariant();
        return value == "echo" || value.StartsWith("echo://");
    }

    public IProtocolClient Create() => new EchoProtocolClient();
}
