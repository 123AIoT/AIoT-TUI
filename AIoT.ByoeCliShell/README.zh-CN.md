# AIoT.ByoeCliShell 新手使用说明（中文）

`AIoT.ByoeCliShell` 是一个**纯命令行**工具，用于通过反射方式调用 Byoe 协议库。

它适合：
- 快速验证多协议连通性
- 做脚本化读写
- 做最小化协议模拟（目前支持 Modbus TCP Sim）

> 目标框架：`.NET 10`

---

## 1. 快速开始

在仓库根目录执行：

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- list
```

如果正常，会返回 JSON，包含：
- `protocols`：可用客户端协议
- `simulators`：可用模拟器协议

---

## 2. 命令总览

```text
list
client-connect
client-read
client-browse
client-write
sim-run
```

通用输出格式参数：
- `--format json`（默认）
- `--format ndjson`
- `--format text`

---

## 3. list（查看支持能力）

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- list --format text
```

用途：确认当前版本支持的客户端协议和模拟器协议。

---

## 4. client-connect（仅连通性测试）

### 4.1 Siemens S7

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- client-connect --protocol siemens.s7 --endpoint "s7://192.168.1.10:102?plc=s1200&rack=0&slot=1"
```

### 4.2 Modbus TCP

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- client-connect --protocol modbus.tcp --endpoint modbus.tcp://127.0.0.1:502 --station 1
```

---

## 5. client-read（读取）

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- client-read --protocol modbus.tcp --endpoint modbus.tcp://127.0.0.1:502 --address 100 --type int
```

`--type` 可选：
- `int`（默认）
- `float`
- `bool`
- `string`

---

## 6. client-write（写入）

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- client-write --protocol modbus.tcp --endpoint modbus.tcp://127.0.0.1:502 --address 100 --value 123 --type int
```

---

## 7. client-browse（批量探测读取）

`client-browse` 的作用是：
- 一次读取一组地址
- 返回每个地址的成功/失败结果

### 7.1 显式地址列表

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- client-browse --protocol siemens.s7 --endpoint "s7://192.168.1.10:102?plc=s1200&rack=0&slot=1" --addresses "DB1.0,DB1.2,M100" --type int --format ndjson
```

### 7.2 Modbus 区间方式

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- client-browse --protocol modbus.tcp --endpoint modbus.tcp://127.0.0.1:502 --start 100 --count 10 --type int
```

> 非 Modbus 协议建议使用 `--addresses`。

---

## 8. sim-run（模拟器）

目前默认支持：`modbus.tcp`

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- sim-run --protocol modbus.tcp --port 1502 --preloadAddress 100 --preloadValue 321 --type int
```

启动后会提示 `Press Enter to stop.`

你可以开第二个终端执行 `client-read` 连接这个模拟器：

```powershell
dotnet run --project AIoT.ByoeCliShell/AIoT.ByoeCliShell.csproj -- client-read --protocol modbus.tcp --endpoint modbus.tcp://127.0.0.1:1502 --address 100 --type int
```

---

## 9. 参数优先级与 endpoint 规则

本工具支持 endpoint Query 参数自动注入（再由 CLI 参数覆盖）：

例如：

```text
s7://192.168.1.10:102?plc=s1200&rack=0&slot=1
```

等价于传了：
- `--plc s1200`
- `--rack 0`
- `--slot 1`

如果命令行又传了同名参数，则命令行为准。

---

## 10. 常见问题

### Q1：报 `Unsupported protocol`？
先执行 `list` 看当前支持清单，确认 `--protocol` 拼写。

### Q2：连接超时？
优先检查：
- IP/端口
- PLC 协议参数（如 `plc/rack/slot`）
- 防火墙

### Q3：RTU 串口如何写 endpoint？
可直接写：
- `COM3`
- `modbus.rtu://COM3?baud=9600&station=1`

---

## 11. 建议的新手调试顺序

1. `list`（确认能力）
2. `sim-run modbus.tcp`（先在本机闭环）
3. `client-read/client-write`（验证读写流程）
4. 再切换到真实 PLC 协议参数

---

## 12. 当前能力边界

- 客户端协议覆盖较高（按当前映射表）
- 服务器模拟目前是最小实现（`modbus.tcp`）
- 其他协议模拟可后续按协议逐步扩展
