---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: 'e04c0560-f317-44a2-86b0-802d4e4ebd65'
  PropagateID: 'e04c0560-f317-44a2-86b0-802d4e4ebd65'
  ReservedCode1: '2a8b15cd-b5d4-4f31-94dd-6861731f2612'
  ReservedCode2: '2a8b15cd-b5d4-4f31-94dd-6861731f2612'
---

# AutoMouse 架构设计文档

**日期：** 2026-09-21

## 1. 设计原则

遵循"深度模块"原则：每个服务模块提供小接口、藏大实现，让调用方只需学习少量接口即可获得完整能力。

## 2. 模块设计

### 2.1 HookService（录制引擎）- 深度模块

**接口（小）：**
```csharp
interface IHookService
{
    void Start();                                  // 开始录制
    List<InputEvent> Stop();                        // 停止录制，返回事件列表
    bool IsRecording { get; }                      // 当前状态
    event EventHandler<InputEvent> EventCaptured;  // 事件捕获回调（可选）
}
```

**实现（大）：**
- 安装/卸载 WH_MOUSE_LL + WH_KEYBOARD_LL 钩子
- 钩子回调处理：解析 KBDLLHOOKSTRUCT / MSLLHOOKSTRUCT
- 事件类型转换：Win32 数据 → MouseEvent/KeyboardEvent
- 计时器管理：录制开始时启动 Stopwatch
- 移动事件采样降稠密：可配置最小采样间隔
- 线程安全：钩子回调在 UI 线程，事件写入需考虑并发

**设计理由：** 调用方只需 Start/Stop 两个方法，Win32 钩子的复杂性完全隐藏。测试时通过 mock IHookService 接口即可，不需要真实钩子。

### 2.2 PlaybackService（回放引擎）- 深度模块

**接口（小）：**
```csharp
interface IPlaybackService
{
    Task PlayAsync(List<InputEvent> events, int loopCount, CancellationToken ct);
    event EventHandler<PlaybackProgress> ProgressChanged;
    bool IsPlaying { get; }
}
```

**实现（大）：**
- SendInput API 调用：构造 INPUT 结构体数组
- 坐标转换：屏幕绝对坐标 → SendInput 绝对坐标（0~65535 映射）
- 按键映射：虚拟键码 → INPUT 结构体
- 时间调度：计算相邻事件 Timestamp 差值，Task.Delay 等待
- 循环控制：loopCount 次迭代，每轮重置时间基准
- 进度通知：通过 ProgressChanged 事件回调当前步骤/循环

**设计理由：** 调用方只需 PlayAsync + CancellationToken，所有 Win32 SendInput 细节隐藏。测试时可注入虚拟事件列表验证调度逻辑。

### 2.3 HotkeyService（热键服务）- 中等模块

**接口（小）：**
```csharp
interface IHotkeyService
{
    void Register(int id, ModifierKeys modifiers, Key key);
    void Unregister(int id);
    event EventHandler<HotkeyEventArgs> HotkeyPressed;
}
```

**实现（中）：**
- RegisterHotKey / UnregisterHotKey P/Invoke
- HwndSource.AddHook 拦截 WM_HOTKEY 消息
- 线程关联：必须在 UI 线程注册

### 2.4 ScriptService（脚本管理）- 深度模块

**接口（小）：**
```csharp
interface IScriptService
{
    void Save(Script script);
    Script Load(string name);
    void Delete(string name);
    void Rename(string oldName, string newName);
    List<ScriptInfo> ListAll();
}
```

**实现（大）：**
- JSON 序列化/反序列化（System.Text.Json + 自定义 InputEvent Converter）
- 文件 I/O 管理（存储目录创建、路径拼接）
- 多态类型处理：MouseEvent/KeyboardEvent 的 discriminated union
- 脚本元数据提取（ListAll 只返回摘要信息，不加载完整事件列表）

**设计理由：** 调用方只需 5 个 CRUD 方法，JSON 序列化和文件管理的复杂性完全隐藏。

### 2.5 ViewModel 层 - 薄适配器

ViewModel 是 Service 层的薄适配器，自身逻辑少：

```csharp
// RecordViewModel - 调用 IHookService
[RelayCommand] void StartRecord() => _hookService.Start();
[RelayCommand] void StopRecord()  { var events = _hookService.Stop(); _scriptService.Save(...); }

// PlaybackViewModel - 调用 IPlaybackService + IScriptService
[RelayCommand] async Task Play() => await _playbackService.PlayAsync(events, loopCount, _cts.Token);
[RelayCommand] void Stop() => _cts.Cancel();

// EditorViewModel - 操作 List<InputEvent>
```

**设计理由：** ViewModel 不应包含业务逻辑，只负责将 UI 交互转发给 Service，保持 UI 层可替换。

## 3. 依赖关系

```
View → ViewModel → Service (接口)
                      ├── HookService → Win32/Hooks
                      ├── PlaybackService → Win32/SendInput
                      ├── HotkeyService → Win32/RegisterHotKey
                      └── ScriptService → System.Text.Json + File IO
```

- ViewModel 通过接口依赖 Service，便于单元测试时 mock
- Service 之间无横向依赖，各自独立
- Win32 P/Invoke 声明集中在 Utils/Win32/，Service 通过调用使用

## 4. 测试策略

| 模块 | 测试方式 | 测试内容 |
|------|----------|----------|
| HookService | Mock Win32 API，验证事件转换逻辑 | KBDLLHOOKSTRUCT → KeyboardEvent 映射正确 |
| PlaybackService | 注入虚拟事件列表，验证调度顺序 | 事件按 Timestamp 排序、循环次数正确 |
| ScriptService | 临时目录 + 真实 JSON 文件 | 保存/加载/删除/重命名/列表 |
| HotkeyService | Mock RegisterHotKey | 注册/注销/事件触发 |
| ViewModel | Mock Service 接口 | 命令绑定、状态切换 |

## 5. 可测试性设计

- 所有 Service 通过接口暴露，ViewModel 通过构造函数注入
- 使用 .NET DI 容器注册：`services.AddSingleton<IHookService, HookService>()`
- Win32 P/Invoke 方法标记为 virtual 或通过接口包装，便于 mock
- InputEvent 和子类是纯数据模型，可直接构造用于测试
- PlaybackService 的时间调度逻辑与 SendInput 调用分离，可独立测试调度

## 6. 关键内部设计

### 6.1 InputEvent 多态 JSON 序列化

使用自定义 JsonConverter 处理 InputEvent 的多态序列化：
- Write 时写入 "type" 鉴别字段，按子类写入各自属性
- Read 时读取 "type" 字段判断子类，反序列化为 MouseEvent 或 KeyboardEvent

### 6.2 移动事件采样降稠密

鼠标移动事件可能非常密集（每秒上百个），需要采样策略：
- HookService 内部维护 _lastMoveTime
- 当 MouseMove 事件间隔小于配置的 _moveSampleInterval（默认 5ms）时丢弃
- 采样间隔可通过设置配置

### 6.3 回放线程模型

- PlayAsync 在独立 Task 中执行
- 每个事件按 Timestamp 差值 Task.Delay 等待
- CancellationToken 贯穿整个回放过程，随时可中断
- 循环外层 for 循环控制次数，内层 foreach 遍历事件
- 每个事件执行后触发 ProgressChanged 事件通知 UI

## 7. 项目结构

```
AutoMouse/
├── AutoMouse.sln
├── src/
│   └── AutoMouse/
│       ├── AutoMouse.csproj
│       ├── App.xaml / App.xaml.cs
│       ├── Models/
│       │   ├── InputEvent.cs
│       │   ├── MouseEvent.cs
│       │   ├── KeyboardEvent.cs
│       │   ├── Script.cs
│       │   └── Enums.cs
│       ├── Services/
│       │   ├── Interfaces/
│       │   │   ├── IHookService.cs
│       │   │   ├── IPlaybackService.cs
│       │   │   ├── IHotkeyService.cs
│       │   │   └── IScriptService.cs
│       │   ├── HookService.cs
│       │   ├── PlaybackService.cs
│       │   ├── HotkeyService.cs
│       │   └── ScriptService.cs
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   ├── RecordViewModel.cs
│       │   ├── PlaybackViewModel.cs
│       │   ├── EditorViewModel.cs
│       │   ├── ScriptListViewModel.cs
│       │   └── SettingsViewModel.cs
│       ├── Views/
│       │   ├── MainWindow.xaml / .cs
│       │   ├── RecordPanel.xaml / .cs
│       │   ├── PlaybackPanel.xaml / .cs
│       │   ├── EditorPanel.xaml / .cs
│       │   ├── ScriptListPanel.xaml / .cs
│       │   └── SettingsPanel.xaml / .cs
│       └── Utils/
│           ├── Win32/
│           │   ├── Win32Native.cs
│           │   ├── HookStructs.cs
│           │   └── InputStructs.cs
│           └── Json/
│               └── InputEventConverter.cs
├── tests/
│   └── AutoMouse.Tests/
│       ├── AutoMouse.Tests.csproj
│       ├── Services/
│       │   ├── HookServiceTests.cs
│       │   ├── PlaybackServiceTests.cs
│       │   ├── HotkeyServiceTests.cs
│       │   └── ScriptServiceTests.cs
│       └── ViewModels/
│           ├── RecordViewModelTests.cs
│           └── PlaybackViewModelTests.cs
└── docs/
```