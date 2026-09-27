namespace AIoT.Cli;

public enum CliFormat
{
    Text,
    Json,
    Ndjson
}

public sealed class CliCommand
{
    public string Name { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string? Protocol { get; init; }
    public List<string> Addresses { get; init; } = new();
    public string? Value { get; init; }
    public string? DataType { get; init; }
    public int? Length { get; init; }
    public byte? Station { get; init; }
    public int? BaudRate { get; init; }
    public string? Plc { get; init; }
    public byte? Rack { get; init; }
    public byte? Slot { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? ClientId { get; init; }
    public int? Qos { get; init; }
    public bool Retain { get; init; }
    public string? Topic { get; init; }
    public string? WebSocketSubProtocol { get; init; }
    public string? WebSocketHeader { get; init; }
    public string? WebSocketAddress { get; init; }
    public CliFormat Format { get; init; } = CliFormat.Text;
    public int IntervalMs { get; init; } = 1000;
    public int? Count { get; init; }
}
