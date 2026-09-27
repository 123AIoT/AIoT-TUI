# CLI Smoke Test Commands

These commands validate the agent-friendly CLI path for OPC UA, MQTT, WebSocket, and Byoe-backed common protocols (Modbus, Siemens S7, Mitsubishi MC, Omron FINS, Delta, Xinje).

## Prerequisites

- Build succeeds: `dotnet build`
- Optional OPC UA test server:
  - `dotnet run --project Tests/AIoT.TestServer`
  - endpoint: `opc.tcp://localhost:4840`
- Optional PLC/device reachable by your endpoint settings.

## Quick run (PowerShell)

```powershell
./scripts/smoke-cli.ps1
```

### CI-friendly mode

```powershell
./scripts/smoke-cli.ps1 -Strict
```

Useful switches:

- `-Strict` : stop on first failure and return that exit code.
- `-Configuration <Debug|Release>` : build/run configuration (default `Debug`).
- `-NoBuild` : skip build step during `dotnet run`.
- `-SkipOpcUa` : skip OPC UA command group.
- `-SkipModbusTcp` : skip Modbus TCP command group.
- `-SkipModbusRtu` : skip Modbus RTU command group.
- `-SkipSiemens` : skip Siemens S7 command group.
- `-SkipMelsec` : skip Mitsubishi MC command group.
- `-SkipOmron` : skip Omron FINS command group.
- `-SkipMqtt` : skip MQTT command group.
- `-SkipWebSocket` : skip WebSocket command group.

You can also override endpoints and project path:

```powershell
./scripts/smoke-cli.ps1 -AiotProject "AIoT/AIoT.csproj" -Configuration Release -NoBuild -OpcEndpoint "opc.tcp://localhost:4840"
```

Example (only OPC UA in CI):

```powershell
./scripts/smoke-cli.ps1 -Strict -SkipModbusTcp -SkipModbusRtu
```

## Individual commands

### OPC UA

```powershell
dotnet run --project AIoT/AIoT.csproj -- read opc.tcp://localhost:4840 "ns=2;s=Counter" --format json
```

```powershell
dotnet run --project AIoT/AIoT.csproj -- monitor opc.tcp://localhost:4840 "ns=2;s=Counter" --count 3 --format ndjson
```

### Modbus TCP (Byoe)

```powershell
dotnet run --project AIoT/AIoT.csproj -- read modbus.tcp://127.0.0.1:502 100 --type int --format json
```

```powershell
dotnet run --project AIoT/AIoT.csproj -- write modbus.tcp://127.0.0.1:502 100 true --type bool --station 1 --format json
```

```powershell
dotnet run --project AIoT/AIoT.csproj -- read modbus.tcp://127.0.0.1:502 100 --type word --length 1 --format json
```

### MQTT

```powershell
dotnet run --project AIoT/AIoT.csproj -- write mqtt://127.0.0.1:1883 sensors/temp 23.4 --protocol mqtt --client-id aiot-smoke --qos 1 --retain --format json
```

```powershell
dotnet run --project AIoT/AIoT.csproj -- write mqtt://127.0.0.1:1883 "" 23.4 --protocol mqtt --topic sensors/temp --qos 1 --retain --format json
```

```powershell
dotnet run --project AIoT/AIoT.csproj -- monitor mqtt://127.0.0.1:1883 sensors/temp --protocol mqtt --count 3 --format ndjson
```

### WebSocket

```powershell
dotnet run --project AIoT/AIoT.csproj -- write ws://127.0.0.1:8080/ws line1/status running --protocol websocket --format json
```

```powershell
dotnet run --project AIoT/AIoT.csproj -- monitor ws://127.0.0.1:8080/ws '$last' --protocol websocket --count 3 --format ndjson
```

```powershell
dotnet run --project AIoT/AIoT.csproj -- monitor ws://127.0.0.1:8080/ws --protocol websocket --ws-subprotocol telemetry --ws-header "X-Site:line1;X-Role:agent" --ws-address '$last' --count 3 --format ndjson
```

### Modbus RTU (Byoe)

```powershell
dotnet run --project AIoT/AIoT.csproj -- read COM3 100 --protocol modbus.rtu --baud 9600 --station 1 --type int --format json
```

### Siemens S7 (Byoe)

```powershell
dotnet run --project AIoT/AIoT.csproj -- read s7://192.168.0.10:102?plc=s1200&rack=0&slot=1 DB1.0 --protocol siemens.s7 --type int --format json
```

Equivalent explicit options form:

```powershell
dotnet run --project AIoT/AIoT.csproj -- read 192.168.0.10:102 DB1.0 --protocol siemens.s7 --plc s1200 --rack 0 --slot 1 --type int --format json
```

### Mitsubishi MC (Byoe)

```powershell
dotnet run --project AIoT/AIoT.csproj -- read melsec://192.168.0.11:6000 D100 --protocol melsec --type int --format json
```

### Omron FINS (Byoe)

```powershell
dotnet run --project AIoT/AIoT.csproj -- read omron://192.168.0.12:9600 D100 --protocol omron --type int --format json
```

### Delta (Byoe)

```powershell
dotnet run --project AIoT/AIoT.csproj -- read delta://192.168.0.13:502?series=dvp&station=1 D100 --protocol delta --type int --format json
```

### Xinje (Byoe)

```powershell
dotnet run --project AIoT/AIoT.csproj -- read xinje://192.168.0.14:502?series=xc&station=1 D100 --protocol xinje --type int --format json
```

## Notes

- `--type` supports: `bool|byte|sbyte|short|ushort|int|uint|long|ulong|float|double|string|word|dword|hex`
- `--length` can be used for `string` / `hex` and multi-register reads or writes
- `--protocol` supports: `opcua|mqtt|websocket|modbus.tcp|modbus.rtu|siemens.s7|melsec|omron|delta|xinje`
- Siemens options: `--plc`, `--rack`, `--slot`
- MQTT options: `--client-id`, `--qos`, `--retain`, `--user`, `--pass`
- MQTT options: `--client-id`, `--qos`, `--retain`, `--topic`, `--user`, `--pass`
- WebSocket options: `--ws-subprotocol`, `--ws-header`, `--ws-address`
- For RTU, you can use either:
  - `modbus.rtu://COM3?baud=9600`
  - `COM3 --protocol modbus.rtu --baud 9600`
