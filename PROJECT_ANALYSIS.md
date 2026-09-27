# AIoT-TUI 项目分析报告

> 2026-09-26 · 777 源文件（核心 ~130，Byoe 库 643）· net10.0 · MIT · Terminal.Gui 2.0
> 定位：**"lazygit 式的工业现场终端工作台"**——一手给工程师（TUI），一手给 AI Agent（NDJSON CLI）

## 产品定位

跨平台终端工业通信工具，双形态合一：
- **TUI 模式**：Terminal.Gui 交互式终端界面——浏览 OPC UA 地址空间树、订阅监控点位、实时示波器（Scope，最多 5 信号/30s 滑窗）、趋势图、CSV 录制
- **CLI 模式**：`read|write|browse|monitor <endpoint>` 机器可读模式（`--format json|ndjson|text`），面向 AI Agent 与自动化脚本

## 核心事实

- **协议覆盖（已暴露 7 种 + Byoe 库 64+ 族）**：OPC UA（原生订阅）、Modbus TCP/RTU、西门子 S7 全系、三菱 MC、欧姆龙 FINS、台达 DVP、信捷 XC、MQTT、WebSocket；Byoe 库（=HslCommunication 改名）含 KUKA/ABB/FANUC 机器人、Fanuc CNC、SECS/GEM 半导体、电表/温控/秤等 64+ 协议族——**插件只暴露了冰山一角**
- **插件系统**：IProtocolFactory + plugins/ 目录扫描 + 插件优先路由 + PLUGINS_ONLY 模式——干净的模式，可照此为 ILESP 私有协议写插件
- **部署**：单文件自包含发布（win-x64/linux-x64/osx-x64），~40MB 内存，无许可费
- **测试**：xUnit 21 文件，含进程内 OPC UA 测试服务器 + CLI 冒烟脚本
- **Byoe 授权风险**：Byoe 库携带 HslCommunication 商业授权代码路径（`Authorization.nzugaydgwadawdibbas()`），商用嵌入前需核实许可

## 对智能制造生态的补充价值

ILESP/MES/CPS 都是 GUI/Web 形态的"楼上"系统，缺一个不装环境、SSH 即用、键盘驱动的"井下"工具：

| # | 角色 | 说明 |
|---|---|---|
| 1 | **现场工程师随身工具** | 连上就浏览/监控/写值/录 CSV，示波器视图调试验证（伺服波形/传感器毛刺） |
| 2 | **AI Agent 接入设备的工具调用后端** | `aiot read/write/browse/monitor --format ndjson` 可封装为 LLM Skill——ILESP Agent 直接触达 PLC/OPC UA 数据（`docs/中文使用说明-Skills开发版.md` 已给完整蓝图） |
| 3 | **设备连通性 CI 冒烟组件** | 内置 OPC UA 测试服务器 + Modbus 模拟器 + 冒烟脚本——可移植做产线设备连通性回归 |
| 4 | **插件模式参考** | IProtocolFactory + plugins/ 目录 + 插件优先路由——干净的模式 |

**采纳前需确认**：Byoe 库的 HslCommunication 商业授权合规；net10.0 较激进；ProtocolSdk 旧版契约分叉。

**一句话**：设备边缘的轻量验证与排障层，不是 SCADA/MES 替代品——最佳角色是现场工具 + Agent 工具后端 + CI 冒烟组件。
