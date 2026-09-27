namespace AIoT.Protocols.Abstractions;

public interface IProtocolFactory
{
    bool CanHandle(string endpointOrProtocol);
    IProtocolClient Create();
}
