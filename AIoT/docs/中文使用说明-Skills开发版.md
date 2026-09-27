# AIoT 中文使用说明（Skills 开发版）

> 面向对象：平台开发、Agent 开发、自动化开发、测试开发。  
> 目标：将 AIoT CLI 稳定封装为可调用的技能（skills/tooling API）。

> 说明：完整总文档仍保留在 `docs/中文使用说明.md`。本文件是面向开发集成的独立版。

---

## 1. 集成定位

AIoT CLI 适合作为统一工业协议执行后端：

- 输入：结构化参数（action、endpoint、addresses...）
- 输出：JSON/NDJSON
- 场景：Agent 工具调用、脚本编排、CI 烟测、回归任务

---

## 2. CLI 执行模型

基础命令：

```text
aiot <read|write|browse|monitor> <endpoint> ... [options]
```

源码执行：

```powershell
dotnet run --project AIoT/AIoT.csproj -- <read|write|browse|monitor> <endpoint> ... [options]
```

推荐输出：

- 单次命令用 `--format json`
- 流式监控用 `--format ndjson`

---

## 3. 参数设计建议（Skill 入参）

核心参数：

- `action`: `read|write|browse|monitor`
- `endpoint`: string
- `addresses`: string[]
- `value`: string（write 必填）
- `protocol`: string（可选）
- `format`: `json|ndjson|text`（默认 `json`）

通用附加：

- `type`, `count`, `interval`
- `user`, `pass`

协议附加：

- Modbus/Siemens：`station`, `baud`, `plc`, `rack`, `slot`
- MQTT：`clientId`, `qos`, `retain`, `topic`
- WebSocket：`wsSubprotocol`, `wsHeader`, `wsAddress`

返回结构建议：

- `exitCode`
- `stdout`
- `stderr`
- `commandLine`
- `durationMs`

---

## 4. Skill 封装要点（强烈建议）

1. **白名单化 action 与 protocol**（禁止任意拼接）
2. **参数合法性校验**
   - `qos` 只允许 `0|1|2`
   - `wsHeader` 建议校验 `k:v[;k2:v2]`
3. **默认输出 json/ndjson**，减少自然语言解析
4. **完整保留 stderr 与退出码**，便于故障定位
5. **超时控制与取消机制**（尤其 monitor）
6. **日志脱敏**（不要记录明文密码）

---

## 5. PowerShell Skill 包装模板

```powershell
function Invoke-AIoTSkill {
    param(
        [Parameter(Mandatory = $true)] [ValidateSet('read','write','browse','monitor')] [string]$Action,
        [Parameter(Mandatory = $true)] [string]$Endpoint,
        [string[]]$Addresses = @(),
        [string]$Value,
        [string]$Protocol,
        [ValidateSet('text','json','ndjson')] [string]$Format = 'json',
        [string]$Type,
        [int]$Count,
        [int]$Interval,
        [string]$User,
        [string]$Pass,
        [string]$ClientId,
        [int]$Qos,
        [switch]$Retain,
        [string]$Topic,
        [string]$WsSubprotocol,
        [string]$WsHeader,
        [string]$WsAddress
    )

    if ($Qos -and ($Qos -lt 0 -or $Qos -gt 2)) {
        throw 'qos 仅允许 0/1/2'
    }

    if ($WsHeader -and ($WsHeader -notmatch ':')) {
        throw 'wsHeader 格式应为 k:v 或 k1:v1;k2:v2'
    }

    $args = @($Action, $Endpoint)

    if ($Action -eq 'write') {
        if ($Addresses.Count -lt 1) { throw 'write 需要至少一个地址' }
        if ([string]::IsNullOrWhiteSpace($Value)) { throw 'write 需要 value' }
        $args += $Addresses[0]
        $args += $Value
    }
    elseif ($Addresses.Count -gt 0) {
        $args += $Addresses
    }

    if ($Protocol) { $args += @('--protocol', $Protocol) }
    if ($Type) { $args += @('--type', $Type) }
    if ($Count) { $args += @('--count', "$Count") }
    if ($Interval) { $args += @('--interval', "$Interval") }

    if ($User) { $args += @('--user', $User) }
    if ($Pass) { $args += @('--pass', $Pass) }

    if ($ClientId) { $args += @('--client-id', $ClientId) }
    if ($Qos -ge 0 -and $Qos -le 2) { $args += @('--qos', "$Qos") }
    if ($Retain) { $args += '--retain' }
    if ($Topic) { $args += @('--topic', $Topic) }

    if ($WsSubprotocol) { $args += @('--ws-subprotocol', $WsSubprotocol) }
    if ($WsHeader) { $args += @('--ws-header', $WsHeader) }
    if ($WsAddress) { $args += @('--ws-address', $WsAddress) }

    $args += @('--format', $Format)

    $dotnetArgs = @('run', '--project', 'AIoT/AIoT.csproj', '--') + $args

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $stdout = & dotnet @dotnetArgs 2>&1
    $exitCode = $LASTEXITCODE
    $sw.Stop()

    [pscustomobject]@{
        exitCode = $exitCode
        output = $stdout
        commandLine = "dotnet " + ($dotnetArgs -join ' ')
        durationMs = $sw.ElapsedMilliseconds
    }
}
```

---

## 6. Agent 调用建议

## 6.1 读（单次）

- 用 `read + --format json`
- 失败直接看 `exitCode != 0` 和 stderr

## 6.2 监控（流式）

- 用 `monitor + --format ndjson + --count`
- 由上层按行消费 JSON

## 6.3 写入（可审计）

- 记录输入参数与 `commandLine`
- 保留执行结果与时间戳

---

## 7. 协议语义注意事项

- OPC UA：原生读写/订阅/浏览
- MQTT：read/browse 多为缓存语义；write=publish
- WebSocket：read/browse 多为缓存语义；write=发送文本
- PLC 协议：monitor 多为轮询

在设计 Agent 策略时，避免把缓存语义误当作设备实时采样语义。

---

## 8. 安全建议

1. 不要在日志里打印 `--pass` 明文
2. 建议通过环境变量注入敏感参数
3. 子进程要有超时和取消
4. 对 endpoint/address 做白名单约束（生产环境）

---

## 9. 回归建议

每次升级后，至少跑一遍：

```powershell
./AIoT/scripts/smoke-cli.ps1 -Strict
```

并结合 `docs/cli-smoke-tests.md` 做协议专项命令验证。
