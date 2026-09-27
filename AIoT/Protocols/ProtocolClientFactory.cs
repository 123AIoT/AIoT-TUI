using AIoT.Protocols.Abstractions;
using AIoT.Protocols.OpcUa;
using AIoT.Protocols.Plugin;

namespace AIoT.Protocols;

public static class ProtocolClientFactory
{
    private static readonly object SyncRoot = new();
    private static readonly List<IProtocolFactory> BuiltinFactories =
    [
        new OpcUaProtocolFactory()
    ];
    private static List<IProtocolFactory> _pluginFactories = [];
    private static bool _includeBuiltins = true;

    public static void Initialize(string? pluginsDirectory, bool includeBuiltins = true, Action<string>? log = null)
    {
        lock (SyncRoot)
        {
            _includeBuiltins = includeBuiltins;
            _pluginFactories = string.IsNullOrWhiteSpace(pluginsDirectory)
                ? []
                : ProtocolPluginLoader.LoadFactories(pluginsDirectory, log).ToList();

            if (!string.IsNullOrWhiteSpace(pluginsDirectory))
                log?.Invoke($"Loaded {_pluginFactories.Count} protocol plugin factory(ies) from '{pluginsDirectory}'.");

            log?.Invoke(_includeBuiltins
                ? "Builtin protocol factories: enabled."
                : "Builtin protocol factories: disabled (plugins-only mode).");
        }
    }

    public static IProtocolClient Create(string endpointOrProtocol)
    {
        var factories = GetFactoriesSnapshot();
        foreach (var factory in factories)
        {
            if (factory.CanHandle(endpointOrProtocol))
                return factory.Create();
        }

        return new OpcUaProtocolClient();
    }

    public static IReadOnlyList<string> GetSupportedProtocolKeys()
    {
        var factories = GetFactoriesSnapshot();
        var protocols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var factory in factories)
        {
            if (factory is not IProtocolCatalogProvider catalogProvider)
                continue;

            foreach (var key in catalogProvider.GetSupportedProtocolKeys())
            {
                var normalized = key?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(normalized))
                    protocols.Add(normalized);
            }
        }

        if (protocols.Count == 0)
            protocols.Add("opcua");

        return protocols
            .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IProtocolFactory[] GetFactoriesSnapshot()
    {
        lock (SyncRoot)
        {
            if (_includeBuiltins)
            {
                if (_pluginFactories.Count == 0)
                    return BuiltinFactories.ToArray();

                return _pluginFactories.Concat(BuiltinFactories).ToArray();
            }

            if (_pluginFactories.Count == 0)
                return Array.Empty<IProtocolFactory>();

            return _pluginFactories.ToArray();
        }
    }
}
