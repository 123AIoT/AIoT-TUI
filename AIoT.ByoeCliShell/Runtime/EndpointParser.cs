namespace AIoT.ByoeCliShell.Runtime;

internal static class EndpointParser
{
    public static (string Host, int Port) ParseTcp(string endpoint, int defaultPort)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            return (uri.Host, uri.Port > 0 ? uri.Port : defaultPort);
        }

        var split = endpoint.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
            throw new InvalidOperationException("Invalid endpoint.");

        var host = split[0];
        var port = split.Length > 1 && int.TryParse(split[1], out var parsedPort) ? parsedPort : defaultPort;
        return (host, port);
    }

    public static string ParseSerialPortName(string endpoint)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            if (!string.IsNullOrWhiteSpace(uri.Host))
                return uri.Host;

            var localPath = uri.LocalPath?.Trim('/');
            if (!string.IsNullOrWhiteSpace(localPath))
                return localPath;
        }

        return endpoint;
    }

    public static Dictionary<string, string> ExtractQueryOptions(string endpoint)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            return result;

        var query = uri.Query;
        if (string.IsNullOrWhiteSpace(query))
            return result;

        foreach (var segment in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var split = segment.Split('=', 2, StringSplitOptions.TrimEntries);
            if (split.Length == 0 || string.IsNullOrWhiteSpace(split[0]))
                continue;

            var key = Uri.UnescapeDataString(split[0]);
            var value = split.Length > 1 ? Uri.UnescapeDataString(split[1]) : "true";
            result[key] = value;
        }

        return result;
    }

    public static int ParseInt(IReadOnlyDictionary<string, string> options, string key, int fallback)
        => options.TryGetValue(key, out var raw) && int.TryParse(raw, out var value) ? value : fallback;

    public static byte ParseByte(IReadOnlyDictionary<string, string> options, string key, byte fallback)
        => options.TryGetValue(key, out var raw) && byte.TryParse(raw, out var value) ? value : fallback;
}
