---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: '39af3f9e-ba83-42c6-ab63-ddd7d813af97'
  PropagateID: '39af3f9e-ba83-42c6-ab63-ddd7d813af97'
  ReservedCode1: 'e8896a07-c905-4937-8332-ab4442120cf5'
  ReservedCode2: 'e8896a07-c905-4937-8332-ab4442120cf5'
---

# AutoMouse 头脑风暴记录

**日期：** 2026-09-21
**项目：** AutoMouse - 鼠标键盘自动操作工具

## 1. 需求收集

### 1.1 技术栈选择
- **决策：** C# WPF (.NET 8)
- **理由：** 原生 Windows 桌面应用，全局键鼠钩子性能最优，用户熟悉 C# 和 GUI 开发

### 1.2 录制范围
- 鼠标点击（左键/右键/中键的按下、抬起、单击、双击）
- 鼠标移动（记录坐标和时间戳）
- 滚轮滚动（上下滚动事件）
- 键盘按键（按下/抬起，含组合键 Ctrl+C 等）
- 鼠标拖拽（由 down → move → up 序列自然组成）

### 1.3 回放控制
- 原速回放（按录制时的时间间隔精确重放）
- 循环回放（可设循环次数或无限循环）
- 热键控制（全局热键 F7/F8/F9/F10 控制启停）
- 步骤编辑（录制后可查看、删除、修改、插入步骤）

### 1.4 脚本管理
- JSON 格式存储脚本文件
- 多脚本管理（列表查看、加载、删除、重命名）
- 存储路径：`%APPDATA%/AutoMouse/scripts/`

### 1.5 使用场景
- 办公自动化（批量填表、数据录入、格式化处理）
- 软件测试（UI 操作回放、回归测试）
- 游戏辅助（挂机、刷材料等重复操作）

## 2. 架构方案对比

### 方案 A：事件驱动录制架构（已选定）
- Windows 底层钩子（WH_MOUSE_LL + WH_KEYBOARD_LL）全局捕获
- 事件流 List<InputEvent> 贯穿录制→存储→回放
- SendInput Win32 API 精确回放
- WPF MVVM 四模块分离（Recorder/Player/ScriptManager/Editor）
- 优点：结构清晰、易测试、JSON 天然可读可编辑
- 缺点：拖拽需额外处理（down→move→up 序列）

### 方案 B：命令模式（未选）
- 每个操作抽象为 Command 对象，编辑能力强但开发量大

### 方案 C：单窗口直写（未选）
- 最快出原型但难以维护和扩展

## 3. 整体架构设计

```
AutoMouse (WPF MVVM)
├── Models/          # 数据模型
│   ├── InputEvent    # 输入事件基类
│   ├── MouseEvent    # 鼠标事件
│   ├── KeyboardEvent # 键盘事件
│   └── Script        # 脚本（事件列表+元数据）
├── Services/        # 核心服务
│   ├── HookService     # 全局钩子（录制）
│   ├── PlaybackService # SendInput 回放
│   ├── HotkeyService   # 全局热键注册
│   └── ScriptService   # 脚本 CRUD 管理
├── ViewModels/      # MVVM 视图模型
│   ├── MainViewModel
│   ├── RecordViewModel
│   ├── PlaybackViewModel
│   ├── EditorViewModel
│   └── ScriptListViewModel
├── Views/           # WPF 界面
│   ├── MainWindow
│   ├── RecordPanel
│   ├── PlaybackPanel
│   ├── EditorPanel
│   └── ScriptListPanel
└── Utils/           # 工具
    ├── Win32/        # P/Invoke 声明
    └── JsonConverter # JSON 序列化
```

## 4. 数据模型与 JSON 格式

### 事件类型体系
- InputEvent（基类）：Timestamp（相对毫秒）、Type（Mouse/Keyboard）
- MouseEvent：Action（Move/Down/Up/Scroll）、Button、X/Y 坐标、Delta（滚轮）
- KeyboardEvent：Key（虚拟键码）、Action（Down/Up）、修饰键状态

### JSON 脚本格式
```json
{
  "name": "脚本名称",
  "createdAt": "ISO-8601",
  "recordDuration": 15200,
  "events": [
    { "type": "mouse", "action": "move", "x": 500, "y": 300, "timestamp": 0 },
    { "type": "mouse", "action": "down", "button": "left", "x": 500, "y": 300, "timestamp": 120 },
    { "type": "keyboard", "action": "down", "key": "Ctrl+C", "timestamp": 500 }
  ]
}
```

## 5. 核心服务设计

### HookService（录制引擎）
- SetWindowsHookEx 安装 WH_MOUSE_LL + WH_KEYBOARD_LL
- 回调捕获事件，记录时间戳，存入 List<InputEvent>
- Start() / Stop() 接口

### PlaybackService（回放引擎）
- 读取事件列表，按 Timestamp 计算延迟
- SendInput API 模拟操作
- 支持 loopCount，独立线程 + CancellationToken
- 回放中监听停止热键

### HotkeyService（热键管理）
- RegisterHotKey / UnregisterHotKey
- 默认：F9 录制 | F10 停止录制 | F7 回放 | F8 停止回放
- 可自定义，通过 HwndSource.AddHook 接收 WM_HOTKEY

### ScriptService（脚本管理）
- 存储路径 %APPDATA%/AutoMouse/scripts/
- Save / Load / Delete / ListAll / Rename
- System.Text.Json 序列化

## 6. UI 界面设计

- 单窗口多 Tab 页（录制/回放/编辑/脚本管理/设置）
- 窗口可最小化到系统托盘
- 录制中窗口自动隐藏（可配置）
- 编辑面板用 DataGrid 展示事件列表
- 顶部状态栏实时显示录制时长和事件计数
- 系统托盘右键菜单快速操作