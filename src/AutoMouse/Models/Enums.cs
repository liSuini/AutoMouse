namespace AutoMouse.Models;

/// <summary>输入事件类型</summary>
public enum InputEventType
{
    Mouse,
    Keyboard
}

/// <summary>鼠标动作</summary>
public enum MouseAction
{
    Move,
    Down,
    Up,
    Scroll
}

/// <summary>鼠标按钮</summary>
public enum MouseButton
{
    None,
    Left,
    Right,
    Middle
}

/// <summary>键盘动作</summary>
public enum KeyAction
{
    Down,
    Up
}

/// <summary>工具运行状态</summary>
public enum AppState
{
    Idle,
    Recording,
    Playing
}
