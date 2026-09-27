using System.Text.Json;

namespace AIoT.Cli;

public static class CliJsonWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions NdjsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static void WriteJson(object payload)
    {
        Console.WriteLine(JsonSerializer.Serialize(payload, JsonOptions));
    }

    public static void WriteNdjson(object payload)
    {
        Console.WriteLine(JsonSerializer.Serialize(payload, NdjsonOptions));
    }
}
