# 使用 Byoe 库扩展 AIoT 插件（中文指南）

> 目标：指导你基于 **Byoe** 快速开发一个 AIoT 协议插件（而不是改宿主核心）。  
> 适用：AIoT 当前插件化架构（宿主内置 OPC UA，其他协议通过插件加载）。

---

## 1. 背景与架构

AIoT 启动时会从以下目录扫描插件：

- `<app-base>/plugins/*.dll`

插件程序集里只要有实现 `IProtocolFactory` 的类型，就会被发现并注册。  
协议路由顺序：**插件优先**，然后才是宿主内置工厂（可通过 `AIOT_PROTOCOL_PLUGINS_ONLY=1` 强制仅插件）。

Byoe 插件的核心思路：

1. 用 `IProtocolFactory` 识别协议（endpoint/protocol hint）
2. 用 `IProtocolClient` 封装连接、读写、监控
3. 在插件内部引用 Byoe 的具体协议类（如 `SiemensS7Net`、`OmronFinsNet` 等）

---

## 2. 最小开发流程

## 2.1 新建插件工程

建议新建类库工程（示例命名）：

- `AIoT.PluginProtocols.YourByoe`

目标框架与宿主保持一致（`net10.0`）。

## 2.2 添加引用

插件工程至少需要引用：

- `AIoT/AIoT.csproj`（拿到 `IProtocolFactory` / `IProtocolClient` / 模型）
- `Byoe/Byoe.csproj`（使用 Byoe 通信库）

> 说明：当前仓库中 `AIoT.PluginProtocols.Byoe` 就是完整参考实现。

## 2.3 实现工厂（Factory）

你需要实现：

- `bool CanHandle(string endpointOrProtocol)`
- `IProtocolClient Create()`

常见识别方式：

- endpoint scheme（如 `s7://`、`omron://`）
- `--protocol` hint（如 `siemens.s7`）

## 2.4 实现客户端（Client）

你需要实现接口：

- `ConnectAsync`
- `DisconnectAsync`
- `ReadAsync`
- `WriteAsync`
- `BrowseAsync`
- `SubscribeAsync`

推荐做法：

1. 在 `ConnectAsync` 里解析 endpoint + extra 参数
2. 创建 Byoe 具体客户端并连接（`ConnectServer`/`Open`）
3. `ReadAsync/WriteAsync` 做最小类型适配（bool/int/float/string/hex）
4. `SubscribeAsync` 用轮询包装（很多 PLC 协议无原生订阅）
5. `DisconnectAsync` 里统一关闭连接并清理状态

---

## 3. 参数映射建议（CLI/TUI 兼容）

AIoT CLI/TUI 会把协议参数放在 `ProtocolConnectOptions.Extra` 中。  
插件应优先从 `Extra` 读取：

- 通用：`protocol`、`type`
- Modbus：`station`、`baud`
- Siemens：`plc`、`rack`、`slot`
- 其他协议可自定义：如 `series`、`unit` 等

建议保留“endpoint query + extra”的双来源兼容：

- endpoint：`s7://192.168.0.10:102?plc=s1200&rack=0&slot=1`
- CLI extra：`--protocol siemens.s7 --plc s1200 --rack 0 --slot 1`

---

## 4. 构建与部署

## 4.1 构建插件

```powershell
dotnet build AIoT.PluginProtocols.Byoe/AIoT.PluginProtocols.Byoe.csproj -c Release
```

## 4.2 部署插件

将插件 DLL（以及依赖）放入：

- `<app-base>/plugins/`

当前仓库内多数插件工程已配置构建后自动复制到：

- `AIoT/bin/<Configuration>/net10.0/plugins/`

---

## 5. 验证方式

## 5.1 普通模式（插件优先 + 内置兜底）

```powershell
dotnet run --project AIoT/AIoT.csproj -- read s7://192.168.0.10:102?plc=s1200&rack=0&slot=1 DB1.0 --protocol siemens.s7 --type int --format json
```

## 5.2 插件专用模式（建议迁移验收使用）

```powershell
$env:AIOT_PROTOCOL_PLUGINS_ONLY = "1"
dotnet run --project AIoT/AIoT.csproj -- read omron://192.168.0.12:9600 D100 --protocol omron --type int --format json
```

若命令成功，说明该协议已可在“仅插件路由”下工作。

---

## 6. 推荐开发规范

1. **协议粒度清晰**：一个插件工程可含多个协议，但每协议至少独立 Factory + Client
2. **错误信息可诊断**：连接失败返回明确原因（host/port/auth/地址格式）
3. **最小语义一致**：`read/write/monitor` 行为要与 CLI 文档一致
4. **兼容 TUI 与 CLI**：不要只支持一种输入方式
5. **保留回退策略**：迁移初期建议先启用插件优先，再逐步开启 plugins-only 验收

---

## 7. 现成参考

可直接参考以下工程：

- `AIoT.PluginProtocols.Byoe`：Byoe 全协议插件实现（Modbus/S7/Melsec/Omron/Delta/Xinje）
- `AIoT.PluginProtocols.Transport`：MQTT/WebSocket 插件化实现
- `AIoT.PluginSamples.Echo`：最小协议插件样例
- `docs/plugin-packaging.md`：插件打包与运行机制

---

## 8. 常见问题

### Q1：插件已编译但没生效？

检查：

- DLL 是否在 `<app-base>/plugins/`
- 启动日志是否显示加载到工厂数量
- `CanHandle` 是否匹配你的 endpoint 或 `--protocol`

### Q2：为什么 CLI 成功但 TUI 失败？

通常是参数来源不一致：

- CLI 多通过 `--xxx` 写入 `Extra`
- TUI 多通过连接对话框字段写入 `Extra`

请确保插件两者都兼容。

### Q3：何时需要新建独立插件工程？

当协议依赖、发布节奏、责任团队不同，建议独立工程，便于版本管理和灰度发布。
