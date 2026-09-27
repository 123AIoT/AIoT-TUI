namespace AIoT.Protocols.Abstractions;

public record ProtocolConnectOptions(
    string Endpoint,
    string? Username = null,
    string? Password = null,
    Dictionary<string, string>? Extra = null);

public record ProtocolNode(string Id, string DisplayName, bool HasChildren);

public record ProtocolReadRequest(
    string Address,
    string? DataType = null,
    int? Length = null,
    Dictionary<string, string>? Options = null);

public record ProtocolWriteRequest(
    string Address,
    string Value,
    string? DataType = null,
    int? Length = null,
    Dictionary<string, string>? Options = null);

public record ProtocolValue(
    string Address,
    string? Value,
    DateTime Timestamp,
    string Quality = "Good",
    string? DataType = null,
    int? Length = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public record ProtocolResult(bool Success, string? Error = null)
{
    public static ProtocolResult Ok() => new(true);
    public static ProtocolResult Fail(string error) => new(false, error);
}

public record ProtocolResult<T>(bool Success, T? Data = default, string? Error = null)
{
    public static ProtocolResult<T> Ok(T data) => new(true, data);
    public static ProtocolResult<T> Fail(string error) => new(false, default, error);
}
