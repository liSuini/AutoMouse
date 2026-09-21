---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: '402bd3be-a127-40a1-a7ec-978ccc61cf9e'
  PropagateID: '402bd3be-a127-40a1-a7ec-978ccc61cf9e'
  ReservedCode1: 'abde997b-644d-4f59-b6c6-5b2ea210acc9'
  ReservedCode2: 'abde997b-644d-4f59-b6c6-5b2ea210acc9'
---

# AutoMouse 交接文档

## 项目概述

AutoMouse 是一款 Windows 桌面端的鼠标键盘操作录制与回放工具。支持全局捕获鼠标移动、点击、滚轮、拖拽及键盘按键操作，按录制时间序列精确重放，支持循环执行。适用于办公自动化、软件测试和游戏辅助等场景。

## 技术栈

| 组件 | 版本 |
|------|------|
| .NET | 8.0 |
| WPF | .NET 8 内置 |
| MVVM | CommunityToolkit.Mvvm 8.4.2 |
| DI | Microsoft.Extensions.DependencyInjection 10.0.12 |
| 测试 | xUnit 2.5.3 + Moq 4.20.72 + FluentAssertions 8.11.0 |
| JSON | System.Text.Json（内置） |

## 项目结构

```
AutoMouse/
├── AutoMouse.sln                    # 解决方案
├── src/AutoMouse/                    # 主项目
│   ├── Models/                       # 数据模型
│   │   ├── Enums.cs                  # 枚举（InputEventType, MouseAction 等）
│   │   ├── InputEvent.cs             # 输入事件基类
│   │   ├── MouseEvent.cs             # 鼠标事件
│   │   ├── KeyboardEvent.cs          # 键盘事件
│   │   └── Script.cs                 # 脚本 + ScriptInfo
│   ├── Services/                     # 核心服务
│   │   ├── Interfaces/               # 4 个服务接口
│   │   ├── HookService.cs            # 录制引擎（Win32 低级钩子）
│   │   ├── PlaybackService.cs        # 回放引擎（SendInput API）
│   │   ├── HotkeyService.cs          # 热键服务（RegisterHotKey）
│   │   ├── ScriptService.cs          # 脚本管理（JSON CRUD）
│   │   └── SettingsService.cs        # 配置管理
│   ├── ViewModels/                   # MVVM 视图模型
│   │   ├── MainViewModel.cs          # 主 VM + 全局状态
│   │   ├── RecordViewModel.cs        # 录制面板 VM
│   │   ├── PlaybackViewModel.cs      # 回放面板 VM
│   │   ├── EditorViewModel.cs        # 编辑面板 VM
│   │   ├── ScriptListViewModel.cs    # 脚本管理 VM
│   │   └── SettingsViewModel.cs       # 设置面板 VM
│   ├── Utils/
│   │   ├── Win32/                    # P/Invoke 声明
│   │   └── Json/                     # JSON 转换器
│   ├── App.xaml / App.xaml.cs        # DI 容器配置
│   ├── MainWindow.xaml / .cs         # 主界面（5 Tab 页 + 托盘）
│   └── InverseBoolConverter.cs       # 值转换器
├── tests/AutoMouse.Tests/            # 测试项目（37 个测试）
│   └── Services/
│       ├── ScriptServiceTests.cs     # 脚本管理测试
│       ├── InputEventConverterTests.cs  # JSON 序列化测试
│       ├── PlaybackServiceTests.cs   # 回放引擎测试
│       ├── EditorViewModelTests.cs   # 编辑器测试
│       └── SettingsServiceTests.cs   # 配置测试
├── docs/                             # 设计文档
│   ├── brainstorming/                # 头脑风暴记录
│   ├── requirements/                 # 需求拆分
│   ├── specs/                        # 架构设计 + 技术规格 + 原型验证
│   ├── plans/                        # 开发计划
│   └── adr/                          # 4 条架构决策记录
├── tasks/prd-automouse.md            # PRD
├── CONTEXT.md                        # 领域模型
└── README.md                         # 项目说明
```

## 运行方式

```bash
# 前提：已安装 .NET 8 SDK
cd D:\code\AICode\AutoMouse
dotnet build
dotnet run --project src/AutoMouse
```

## 功能使用说明

### 录制
1. 切换到「录制」Tab
2. 点击「开始录制」或按 F9
3. 执行需要录制的鼠标和键盘操作
4. 点击「停止录制」或按 F10
5. 脚本自动保存到 `%APPDATA%/AutoMouse/scripts/`，并加载到编辑器

### 回放
1. 切换到「回放」Tab
2. 在下拉框选择脚本
3. 设置循环次数或勾选「无限循环」
4. 点击「开始回放」或按 F7
5. 按 F8 或点击「停止回放」中断

### 编辑
1. 在「脚本管理」Tab 选中脚本，点击「加载到编辑器」
2. 切换到「编辑」Tab
3. 在 DataGrid 中选中事件行，可删除、前插、后插、上移、下移
4. 点击「保存」更新脚本

### 脚本管理
- 查看：脚本管理 Tab 展示所有已保存脚本
- 加载到回放：选中脚本 → 点击「加载到回放」
- 加载到编辑器：选中脚本 → 点击「加载到编辑器」
- 重命名：选中脚本 → 点击「重命名」
- 删除：选中脚本 → 点击「删除」

### 设置
1. 切换到「设置」Tab
2. 热键配置：修改后需重启应用生效
3. 录制选项：自动隐藏窗口、移动采样间隔
4. 点击「保存设置」

### 系统托盘
- 最小化窗口时自动隐藏到托盘
- 双击托盘图标恢复窗口
- 右键菜单：开始录制/停止录制/开始回放/停止回放/显示窗口/退出
- 关闭窗口时最小化到托盘（不退出）
- 通过托盘右键「退出」真正关闭应用

## 测试

```bash
dotnet test
```

37 个单元测试覆盖：
- ScriptService：保存/加载/删除/重命名/列表（8 个测试）
- InputEventConverter：序列化/反序列化/多态/往返（8 个测试）
- PlaybackService：空事件/循环/取消/状态（5 个测试）
- EditorViewModel：加载/删除/插入/移动/保存/清空（11 个测试）
- SettingsService：默认值/序列化（2 个测试）
- 其他：3 个模板测试

## 全局热键

| 热键 | 功能 |
|------|------|
| F9 | 开始录制 |
| F10 | 停止录制 |
| F7 | 开始回放 |
| F8 | 停止回放 |

热键可在设置面板自定义（重启后生效）。

## 数据存储

| 数据 | 路径 | 格式 |
|------|------|------|
| 脚本文件 | `%APPDATA%/AutoMouse/scripts/*.json` | JSON |
| 配置文件 | `%APPDATA%/AutoMouse/settings.json` | JSON |

## 架构要点

- 事件驱动架构：InputEvent 贯穿录制→存储→回放全流程
- 深度模块：4 个 Service 小接口藏大实现
- MVVM：ViewModel 通过 DI 构造函数注入 Service
- Win32 P/Invoke 集中在 Utils/Win32/，不散落在业务代码
- 测试通过接口 mock，所有 Service 可独立测试

## 已知限制

- 不支持变速回放（仅原速）
- 不支持脚本导入/导出（文件级分享）
- 不支持条件分支和循环逻辑（if/while）
- 仅支持 Windows
- 不支持图像识别和找图点击
- 热键修改需重启应用生效
- 编辑面板 DataGrid 对 MouseEvent/KeyboardEvent 子类属性的列编辑有限（当前仅展示 Timestamp 和 Type）

## 后续建议

1. 完善编辑面板 DataGrid 列：为 MouseEvent 展示 X/Y/Button/Action，为 KeyboardEvent 展示 Key/Action
2. 录制时自动隐藏窗口：实现设置面板的 AutoHideWindow 选项联动
3. 脚本导入/导出：支持文件选择对话框导入外部 JSON 脚本
4. 变速回放：在回放面板增加倍速选项（2x/5x/10x）
5. 脚本分组和标签：支持按场景分组管理脚本
6. 录制预览：录制时在独立窗口展示已捕获事件列表
7. 打包发布：使用 dotnet publish 生成单文件 exe 或 MSI 安装包