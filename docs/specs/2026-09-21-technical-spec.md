---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: '70abba69-4e49-4bc9-8d62-24f837605c5c'
  PropagateID: '70abba69-4e49-4bc9-8d62-24f837605c5c'
  ReservedCode1: 'c84ff6e6-ba77-40a7-b54f-bd3bae54ac26'
  ReservedCode2: 'c84ff6e6-ba77-40a7-b54f-bd3bae54ac26'
---

# AutoMouse 技术规格文档

**日期：** 2026-09-21

## 1. 技术栈规格

| 组件 | 版本/选择 | 说明 |
|------|-----------|------|
| .NET | 8.0 LTS | 桌面应用目标框架 |
| WPF | .NET 8 内置 | UI 框架 |
| MVVM | CommunityToolkit.Mvvm 8.x | ObservableObject, RelayCommand, Dependency Injection |
| JSON | System.Text.Json | 内置，无需第三方 |
| 测试 | xUnit + Moq | 单元测试框架 |
| CI/CD | GitHub Actions（可选） | 自动构建 |

## 2. NuGet 包依赖

```
CommunityToolkit.Mvvm        # MVVM 基础设施
xUnit                       # 测试框架（测试项目）
Moq                         # Mock 框架（测试项目）
FluentAssertions            # 断言增强（测试项目，可选）
```

## 3. 核心技术规格

### 3.1 全局钩子实现规格

- 钩子类型：WH_MOUSE_LL (14) + WH_KEYBOARD_LL (13)
- 安装：SetWindowsHookEx → 返回钩子句柄
- 回调签名：delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
- 卸载：UnhookWindowsHookEx(句柄)
- 回调处理时间限制：< 5ms（超过系统超时会被移除钩子）
- 消息循环：WPF Dispatcher 自动提供

### 3.2 SendInput 实现规格

- 函数签名：uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize)
- 鼠标输入：INPUT.type = INPUT_MOUSE，dwFlags 组合 MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE 等
- 坐标映射：屏幕坐标 → 绝对坐标公式 `value * 65535 / screenDimension`
- 键盘输入：INPUT.type = INPUT_KEYBOARD，wVk = 虚拟键码，dwFlags = 0(按下)/KEYEVENTF_KEYUP(抬起)
- 组合键：依次发送各修饰键按下 → 主键按下抬起 → 修饰键抬起

### 3.3 热键实现规格

- 注册：RegisterHotKey(hwnd, id, modifiers, vk)
- 消息拦截：HwndSource.AddHook → WM_HOTKEY (0x0312)
- 注销：UnregisterHotKey(hwnd, id)
- 修改键：先注销旧键再注册新键
- 线程要求：必须在创建窗口的线程（UI 线程）注册

### 3.4 JSON 序列化规格

- 序列化器：JsonSerializerOptions { PropertyNamingPolicy = CamelCase, WriteIndented = true }
- 多态处理：自定义 InputEventConverter 实现 JsonConverter<InputEvent>
- 鉴别字段："type" = "mouse" | "keyboard"
- 枚举序列化：JsonStringEnumConverter（枚举序列化为字符串而非数字）

## 4. 界面规格

### 4.1 窗口规格
- 默认尺寸：800 x 600
- 最小尺寸：640 x 480
- 窗口标题：AutoMouse - 鼠标键盘自动操作工具
- 支持：最大化/最小化/关闭 + 最小化到托盘

### 4.2 Tab 页规格
| Tab | 名称 | 内容 |
|-----|------|------|
| 1 | 录制 | 开始/停止按钮、状态显示、热键提示 |
| 2 | 回放 | 脚本选择、循环设置、开始/停止按钮、进度显示 |
| 3 | 编辑 | DataGrid 事件列表、增删改操作按钮 |
| 4 | 脚本管理 | 脚本列表、加载/删除/重命名 |
| 5 | 设置 | 热键配置、录制选项、存储路径 |

### 4.3 托盘规格
- 图标：使用应用图标
- 左键双击：显示/隐藏主窗口
- 右键菜单：开始录制、停止录制、开始回放、停止回放、显示窗口、退出
- 图标变化：待机(灰)、录制中(红)、回放中(绿)

## 5. 配置文件规格

路径：`%APPDATA%/AutoMouse/settings.json`

```json
{
  "hotkeys": {
    "startRecord": "F9",
    "stopRecord": "F10",
    "startPlayback": "F7",
    "stopPlayback": "F8"
  },
  "recording": {
    "autoHideWindow": true,
    "moveSampleInterval": 5
  },
  "storage": {
    "scriptPath": "%APPDATA%/AutoMouse/scripts"
  }
}
```

## 6. 错误处理规格

- 钩子安装失败：显示错误提示，禁用录制功能
- SendInput 失败：记录日志，继续执行下一事件
- 脚本文件损坏：捕获 JsonException，显示提示
- 热键冲突：注册失败时提示用户更换热键
- 回放被中断：正常退出，不视为错误