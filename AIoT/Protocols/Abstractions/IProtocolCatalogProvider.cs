namespace AIoT.Protocols.Abstractions;

public interface IProtocolCatalogProvider
{
    IEnumerable<string> GetSupportedProtocolKeys();
}
