using System.Text.Json.Serialization;
using AIoT.Configuration.Models;

namespace AIoT.Configuration;

/// <summary>
/// Source-generated JSON serialization context for AIoT configuration types.
/// This enables AOT/trimming-compatible JSON serialization without reflection.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AIoTConfig))]
[JsonSerializable(typeof(List<string>))]
internal partial class AIoTJsonContext : JsonSerializerContext
{
}
