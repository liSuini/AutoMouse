---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: '44232290-3157-4834-9226-9ad7f95ecdab'
  PropagateID: '44232290-3157-4834-9226-9ad7f95ecdab'
  ReservedCode1: 'ef912010-9d3c-466a-ad5e-1d1aa5e4028c'
  ReservedCode2: 'ef912010-9d3c-466a-ad5e-1d1aa5e4028c'
---

# AutoMouse - 鼠标键盘自动操作工具

一款 Windows 桌面端的鼠标、键盘操作录制与回放工具。

## 核心功能

- **录制**：全局捕获鼠标移动、点击、滚轮、拖拽及键盘按键（含组合键），记录时间戳与坐标
- **回放**：按录制时间序列精确重放，支持循环执行
- **编辑**：查看、删除、修改、插入、排序已录制的操作步骤
- **脚本管理**：JSON 格式存储，多脚本管理（加载/删除/重命名）
- **全局热键**：F9/F10/F7/F8 控制录制和回放，可在设置中自定义
- **系统托盘**：最小化到托盘，右键菜单快速操作

## 技术栈

- .NET 8 + WPF + CommunityToolkit.Mvvm
- Win32 低级钩子（WH_MOUSE_LL + WH_KEYBOARD_LL）录制
- SendInput API 精确回放
- System.Text.Json 多态序列化

## 运行

```bash
dotnet build
dotnet run --project src/AutoMouse
```

## 测试

```bash
dotnet test
```

37 个单元测试覆盖脚本管理、JSON 序列化、回放引擎、编辑器、配置管理。

## 项目文档

- [PRD](tasks/prd-automouse.md)
- [领域模型](CONTEXT.md)
- [架构设计](docs/specs/2026-09-21-architecture-design.md)
- [交接文档](docs/handoff.md)