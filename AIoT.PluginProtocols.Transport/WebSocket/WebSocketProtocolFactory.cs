using AIoT.Protocols.Abstractions;

namespace AIoT.PluginProtocols.Transport.WebSocket;

public sealed class WebSocketProtocolFactory : IProtocolFactory
    , IProtocolCatalogProvider
{
    public bool CanHandle(string endpointOrProtocol)
    {
        if (string.IsNullOrWhiteSpace(endpointOrProtocol))
            return false;

        var value = endpointOrProtocol.Trim().ToLowerInvariant();
        return value.StartsWith("ws://")
               || value.StartsWith("wss://")
               || value == "websocket"
               || value == "ws";
    }

    public IProtocolClient Create() => new WebSocketProtocolClient();

    public IEnumerable<string> GetSupportedProtocolKeys()
    {
        yield return "websocket";
    }
}
