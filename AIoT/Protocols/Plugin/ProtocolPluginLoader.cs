using System.Reflection;
using AIoT.Protocols.Abstractions;

namespace AIoT.Protocols.Plugin;

public static class ProtocolPluginLoader
{
    public static IReadOnlyList<IProtocolFactory> LoadFactories(string pluginsDirectory, Action<string>? log = null)
    {
        var results = new List<IProtocolFactory>();

        if (string.IsNullOrWhiteSpace(pluginsDirectory) || !Directory.Exists(pluginsDirectory))
            return results;

        foreach (var dll in Directory.EnumerateFiles(pluginsDirectory, "AIoT.Plugin*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (!typeof(IProtocolFactory).IsAssignableFrom(type))
                        continue;

                    if (Activator.CreateInstance(type) is IProtocolFactory factory)
                        results.Add(factory);
                }
            }
            catch (Exception ex)
            {
                log?.Invoke($"Plugin load failed: {Path.GetFileName(dll)} ({ex.Message})");
            }
        }

        return results;
    }
}
