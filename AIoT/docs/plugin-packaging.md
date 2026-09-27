# Protocol Plugin Packaging Guide

This document describes how to package protocol plugins for AIoT.

## Runtime discovery

At startup, AIoT scans this folder for plugin assemblies:

- `<app-base>/plugins/*.dll`

For each assembly, types implementing `AIoT.Protocols.Abstractions.IProtocolFactory` are instantiated and registered.

## Minimal plugin requirements

A plugin assembly should:

1. Reference AIoT abstractions (`IProtocolFactory`, `IProtocolClient`, models)
2. Provide one or more concrete `IProtocolFactory` implementations
3. Ensure all transitive dependencies are deployed alongside the plugin DLL

## Suggested output layout

```text
publish/
  AIoT.exe (or AIoT)
  ...
  plugins/
    MyCompany.AIoT.Protocols.Extras.dll
    dependency-a.dll
    dependency-b.dll
```

## Behavior notes

- Plugin factories are evaluated before built-in factories.
- If plugin loading fails, AIoT logs a warning and continues with built-in protocols.
- If no plugin is found, behavior is unchanged from non-plugin deployment.
- Optional plugins-only mode is available via environment variable:

```text
AIOT_PROTOCOL_PLUGINS_ONLY=1
```

In this mode, only plugin factories are used (built-in protocol factories are disabled).

## Validation checklist

1. Start AIoT and verify plugin load log contains factory count.
2. Use `--help` and a known plugin endpoint/protocol to validate routing.
3. Run smoke commands for the plugin protocol.
4. Remove the plugin DLL and verify built-in protocols still work.

## Sample plugin in this repository

A minimal sample plugin project is provided:

- `AIoT.PluginSamples.Echo`

Transport protocol migration plugin is also provided:

- `AIoT.PluginProtocols.Transport` (MQTT + WebSocket)

Byoe industrial protocol migration plugin is also provided:

- `AIoT.PluginProtocols.Byoe` (Modbus TCP/RTU, Siemens S7, Melsec, Omron, Delta, Xinje)

It adds support for `echo://` (or `--protocol echo`) and stores values in-memory.

### Build sample plugin

```powershell
dotnet build AIoT.PluginSamples.Echo/AIoT.PluginSamples.Echo.csproj -c Release
```

For local development in this repository, the sample project automatically copies its plugin DLL to:

```text
AIoT/bin/<Configuration>/net10.0/plugins/
```

So after build, source-run validation can typically be executed directly.

### Deploy sample plugin

Copy output DLL(s) into AIoT runtime plugin folder:

```text
<app-base>/plugins/
```

For source-run scenarios, `app-base` is typically:

```text
AIoT/bin/<Configuration>/net10.0/
```

### Quick verify

```powershell
dotnet run --project AIoT/AIoT.csproj -- write echo://local tag1 hello --protocol echo --format json
dotnet run --project AIoT/AIoT.csproj -- read echo://local tag1 --protocol echo --format json
```

### Build transport plugin (MQTT + WebSocket)

```powershell
dotnet build AIoT.PluginProtocols.Transport/AIoT.PluginProtocols.Transport.csproj -c Release
```

### Build Byoe plugin bundle

```powershell
dotnet build AIoT.PluginProtocols.Byoe/AIoT.PluginProtocols.Byoe.csproj -c Release
```

### Validate plugins-only mode

```powershell
$env:AIOT_PROTOCOL_PLUGINS_ONLY = "1"
dotnet run --project AIoT/AIoT.csproj -- write mqtt://127.0.0.1:1883 "" 23.5 --protocol mqtt --topic sensors/temp --qos 1 --retain --format json
dotnet run --project AIoT/AIoT.csproj -- monitor ws://127.0.0.1:8080/ws --protocol websocket --ws-address '$last' --count 3 --format ndjson
```

Or run smoke script in plugins-only mode:

```powershell
./AIoT/scripts/smoke-cli.ps1 -PluginsOnly -SkipOpcUa -SkipModbusTcp -SkipModbusRtu -SkipSiemens -SkipMelsec -SkipOmron
```
