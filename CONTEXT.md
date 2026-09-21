---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: '27d29919-f8d3-4770-9b61-907835668467'
  PropagateID: '27d29919-f8d3-4770-9b61-907835668467'
  ReservedCode1: 'afb95f40-949d-470e-8ab4-8d4e102a48ab'
  ReservedCode2: 'afb95f40-949d-470e-8ab4-8d4e102a48ab'
---

# AutoMouse 领域模型

## 术语表

### 录制 (Recording)

录制是指捕获用户在系统全局范围内的鼠标和键盘输入操作，并将其转换为时间序列事件列表的过程。

**关键术语：**

| 术语 | 定义 |
|------|------|
| **输入事件 (InputEvent)** | 一次原子性的用户输入操作，包含操作类型、动作、参数和相对时间戳。是录制和回放的基本单位。 |
| **鼠标事件 (MouseEvent)** | 鼠标相关的输入事件，包括移动(Move)、按下(Down)、抬起(Up)、滚轮(Scroll)四种动作。 |
| **键盘事件 (KeyboardEvent)** | 键盘相关的输入事件，包括按下(Down)、抬起(Up)两种动作，附带修饰键状态。 |
| **时间戳 (Timestamp)** | 事件发生的相对时间，以毫秒为单位，从录制开始计时。用于回放时精确调度。 |
| **操作序列 (EventSequence)** | 录制产生的有序事件列表，是脚本的核心数据，贯穿录制→存储→回放全流程。 |
| **拖拽 (Drag)** | 鼠标拖拽操作，不是独立事件类型，由 MouseDown → MouseMove → MouseMove → MouseUp 序列自然组成。 |

### 回放 (Playback)

回放是指按照录制的事件序列，使用系统 API 模拟用户输入操作，精确重现原操作的过程。

**关键术语：**

| 术语 | 定义 |
|------|------|
| **回放循环 (PlaybackLoop)** | 将操作序列重复执行 N 次。循环次数为 0 表示无限循环，直到用户手动停止。 |
| **调度延迟 (ScheduleDelay)** | 回放时两个连续事件之间的等待时间，等于两事件 Timestamp 之差。 |
| **回放中断 (PlaybackCancellation)** | 用户通过热键或界面按钮终止正在进行的回放。 |

### 脚本 (Script)

脚本是持久化存储的操作序列及其元数据的载体。

**关键术语：**

| 术语 | 定义 |
|------|------|
| **脚本 (Script)** | 包含名称、创建时间、总时长和事件列表的完整录制记录，以 JSON 文件形式存储。 |
| **脚本名称 (ScriptName)** | 用户为脚本定义的可读名称，用于脚本列表展示和管理。 |
| **脚本时长 (RecordDuration)** | 录制的总时长（毫秒），等于最后一个事件的 Timestamp。 |

### 热键 (Hotkey)

| 术语 | 定义 |
|------|------|
| **全局热键 (GlobalHotkey)** | 在任何应用中均可触发的快捷键，通过系统 API 注册，用于控制录制和回放的启停。 |

### 系统状态

| 术语 | 定义 |
|------|------|
| **待机 (Idle)** | 工具未在录制或回放，等待用户操作。 |
| **录制中 (Recording)** | 钩子已安装，正在捕获输入事件。 |
| **回放中 (Playing)** | 正在通过 SendInput 模拟执行操作序列。 |

## 领域关系

```
Script 1──* InputEvent (一个脚本包含多个事件)
  Script ──1 RecordDuration (脚本包含时长元数据)
  Script ──1 ScriptName (脚本包含名称)
  
InputEvent (基类)
  ├── MouseEvent ── MouseButton, MouseAction, X, Y, Delta
  └── KeyboardEvent ── Key, KeyAction, ModifierKeys

Recording ──1 EventSequence (录制产生一个事件序列)
Playback ──1 EventSequence (回放消费一个事件序列)
Playback ──0..1 PlaybackLoop (回放可包含循环设置)
```

## 边界与约束

- **录制范围**：仅捕获鼠标和键盘事件，不涉及其他输入设备（触摸屏、手柄等）
- **回放范围**：仅通过 SendInput 模拟输入，不涉及窗口管理或进程控制
- **存储范围**：脚本存储在本地文件系统，不涉及云同步或网络传输
- **平台边界**：仅支持 Windows，依赖 Win32 API