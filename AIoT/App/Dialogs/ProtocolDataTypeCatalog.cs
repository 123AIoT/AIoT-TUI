namespace AIoT.App.Dialogs;

internal static class ProtocolDataTypeCatalog
{
    private static readonly string[] NumericDataTypes =
    [
        "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
        "float", "double", "word", "dword", "string", "hex"
    ];

    private static readonly string[] MessagingDataTypes =
    [
        "string", "bool", "int", "uint", "long", "ulong", "float", "double", "hex"
    ];

    public static string NormalizeProtocolKey(string? protocol)
    {
        var key = protocol?.Trim().ToLowerInvariant() ?? string.Empty;
        return key switch
        {
            "modbus-tcp" => "modbus.tcp",
            "modbus-rtu" => "modbus.rtu",
            "s7" => "siemens.s7",
            _ => key
        };
    }

    public static IReadOnlyList<string> GetSupportedDataTypes(string? protocol)
    {
        var key = NormalizeProtocolKey(protocol);
        return key switch
        {
            "mqtt" or "websocket" => MessagingDataTypes,
            "opcua" or "modbus.tcp" or "modbus.rtu" or "siemens.s7" or "melsec" or "omron" or "delta" or "xinje" => NumericDataTypes,
            _ => NumericDataTypes
        };
    }

    public static string GetDefaultDataType(string? protocol)
    {
        var key = NormalizeProtocolKey(protocol);
        return key switch
        {
            "mqtt" or "websocket" => "string",
            _ => "bool"
        };
    }
}