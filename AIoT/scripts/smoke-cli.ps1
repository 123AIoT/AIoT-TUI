param(
    [string]$AiotProject = "AIoT/AIoT.csproj",
    [string]$Configuration = "Debug",
    [string]$OpcEndpoint = "opc.tcp://localhost:4840",
    [string]$ModbusTcpEndpoint = "modbus.tcp://127.0.0.1:502",
    [string]$ModbusRtuEndpoint = "modbus.rtu://COM3?baud=9600",
    [string]$SiemensS7Endpoint = "s7://192.168.0.10:102?plc=s1200&rack=0&slot=1",
    [string]$MelsecEndpoint = "melsec://192.168.0.11:6000",
    [string]$OmronEndpoint = "omron://192.168.0.12:9600",
    [string]$DeltaEndpoint = "delta://192.168.0.13:502?series=dvp&station=1",
    [string]$XinjeEndpoint = "xinje://192.168.0.14:502?series=xc&station=1",
    [string]$MqttEndpoint = "mqtt://127.0.0.1:1883",
    [string]$WebSocketEndpoint = "ws://127.0.0.1:8080/ws",
    [switch]$NoBuild,
    [switch]$Strict,
    [switch]$SkipOpcUa,
    [switch]$SkipModbusTcp,
    [switch]$SkipModbusRtu,
    [switch]$SkipSiemens,
    [switch]$SkipMelsec,
    [switch]$SkipOmron,
    [switch]$SkipDelta,
    [switch]$SkipXinje,
    [switch]$SkipMqtt,
    [switch]$SkipWebSocket,
    [switch]$PluginsOnly
)

Write-Host "== AIoT CLI smoke tests ==" -ForegroundColor Cyan
Write-Host "Project: $AiotProject"
Write-Host "Configuration: $Configuration"

if ($PluginsOnly) {
    $env:AIOT_PROTOCOL_PLUGINS_ONLY = "1"
    Write-Host "Plugins-only mode: ON (AIOT_PROTOCOL_PLUGINS_ONLY=1)" -ForegroundColor Cyan
}

function Invoke-AiotCli {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$CliArgs
    )

    $dotnetArgs = @("run", "--project", $AiotProject, "--configuration", $Configuration)
    if ($NoBuild) {
        $dotnetArgs += "--no-build"
    }

    $dotnetArgs += "--"
    $dotnetArgs += $CliArgs

    & dotnet @dotnetArgs
}

$script:Failed = $false

function Invoke-SmokeStep {
    param(
        [string]$Title,
        [scriptblock]$Action
    )

    Write-Host "`n$Title" -ForegroundColor Yellow
    & $Action
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        Write-Host "FAILED (exit code $exitCode)" -ForegroundColor Red
        $script:Failed = $true
        if ($Strict) {
            exit $exitCode
        }
    }
    else {
        Write-Host "OK" -ForegroundColor Green
    }
}

if (-not $SkipOpcUa) {
    Invoke-SmokeStep "[1/6] OPC UA read (JSON)" {
        Invoke-AiotCli -CliArgs @("read", $OpcEndpoint, "ns=2;s=Counter", "--format", "json")
    }

    Invoke-SmokeStep "[2/6] OPC UA browse (JSON)" {
        Invoke-AiotCli -CliArgs @("browse", $OpcEndpoint, "--format", "json")
    }

    Invoke-SmokeStep "[3/6] OPC UA monitor (NDJSON, count=3)" {
        Invoke-AiotCli -CliArgs @("monitor", $OpcEndpoint, "ns=2;s=Counter", "--count", "3", "--format", "ndjson")
    }
}
else {
    Write-Host "`nSkipping OPC UA smoke tests (--SkipOpcUa)." -ForegroundColor DarkYellow
}

if (-not $SkipModbusTcp) {
    Invoke-SmokeStep "[4/6] Modbus TCP read (typed int)" {
        Invoke-AiotCli -CliArgs @("read", $ModbusTcpEndpoint, "100", "--type", "int", "--format", "json")
    }

    Invoke-SmokeStep "[5/6] Modbus TCP write (typed bool, station=1)" {
        Invoke-AiotCli -CliArgs @("write", $ModbusTcpEndpoint, "100", "true", "--type", "bool", "--station", "1", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping Modbus TCP smoke tests (--SkipModbusTcp)." -ForegroundColor DarkYellow
}

if (-not $SkipModbusRtu) {
    Invoke-SmokeStep "[6/6] Modbus RTU read (typed int, explicit protocol/baud/station)" {
        Invoke-AiotCli -CliArgs @("read", $ModbusRtuEndpoint, "100", "--protocol", "modbus.rtu", "--baud", "9600", "--station", "1", "--type", "int", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping Modbus RTU smoke tests (--SkipModbusRtu)." -ForegroundColor DarkYellow
}

if (-not $SkipSiemens) {
    Invoke-SmokeStep "[7/9] Siemens S7 read (typed int)" {
        Invoke-AiotCli -CliArgs @("read", "192.168.0.10:102", "DB1.0", "--protocol", "siemens.s7", "--plc", "s1200", "--rack", "0", "--slot", "1", "--type", "int", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping Siemens S7 smoke tests (--SkipSiemens)." -ForegroundColor DarkYellow
}

if (-not $SkipMelsec) {
    Invoke-SmokeStep "[8/9] Mitsubishi MC read (typed int)" {
        Invoke-AiotCli -CliArgs @("read", $MelsecEndpoint, "D100", "--protocol", "melsec", "--type", "int", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping Mitsubishi MC smoke tests (--SkipMelsec)." -ForegroundColor DarkYellow
}

if (-not $SkipOmron) {
    Invoke-SmokeStep "[9/9] Omron FINS read (typed int)" {
        Invoke-AiotCli -CliArgs @("read", $OmronEndpoint, "D100", "--protocol", "omron", "--type", "int", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping Omron FINS smoke tests (--SkipOmron)." -ForegroundColor DarkYellow
}

if (-not $SkipDelta) {
    Invoke-SmokeStep "[10/13] Delta read (typed int)" {
        Invoke-AiotCli -CliArgs @("read", $DeltaEndpoint, "D100", "--protocol", "delta", "--type", "int", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping Delta smoke tests (--SkipDelta)." -ForegroundColor DarkYellow
}

if (-not $SkipXinje) {
    Invoke-SmokeStep "[11/13] Xinje read (typed int)" {
        Invoke-AiotCli -CliArgs @("read", $XinjeEndpoint, "D100", "--protocol", "xinje", "--type", "int", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping Xinje smoke tests (--SkipXinje)." -ForegroundColor DarkYellow
}

if (-not $SkipMqtt) {
    Invoke-SmokeStep "[12/13] MQTT write (qos=1, retain)" {
        Invoke-AiotCli -CliArgs @("write", $MqttEndpoint, "aiot/smoke/value", "1", "--protocol", "mqtt", "--client-id", "aiot-smoke", "--qos", "1", "--retain", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping MQTT smoke tests (--SkipMqtt)." -ForegroundColor DarkYellow
}

if (-not $SkipWebSocket) {
    Invoke-SmokeStep "[13/13] WebSocket write" {
        Invoke-AiotCli -CliArgs @("write", $WebSocketEndpoint, "line1/status", "running", "--protocol", "websocket", "--format", "json")
    }
}
else {
    Write-Host "`nSkipping WebSocket smoke tests (--SkipWebSocket)." -ForegroundColor DarkYellow
}

Write-Host "`nSmoke commands finished." -ForegroundColor Green

if ($script:Failed) {
    Write-Host "One or more smoke steps failed." -ForegroundColor Red
    if ($PluginsOnly) {
        Remove-Item Env:AIOT_PROTOCOL_PLUGINS_ONLY -ErrorAction SilentlyContinue
    }
    exit 1
}

Write-Host "All executed smoke steps passed." -ForegroundColor Green

if ($PluginsOnly) {
    Remove-Item Env:AIOT_PROTOCOL_PLUGINS_ONLY -ErrorAction SilentlyContinue
}
