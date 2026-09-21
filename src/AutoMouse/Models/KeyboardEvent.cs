namespace AutoMouse.Models;

/// <summary>
/// 键盘事件，包括按键按下和抬起。
/// 修饰键状态（Ctrl/Alt/Shift/Win）独立记录。
/// </summary>
public class KeyboardEvent : InputEvent
{
    public override InputEventType Type => InputEventType.Keyboard;

    /// <summary>虚拟键码</summary>
    public uint Key { get; set; }

    /// <summary>按键动作</summary>
    public KeyAction Action { get; set; }

    /// <summary>Ctrl 键是否按下</summary>
    public bool Ctrl { get; set; }

    /// <summary>Alt 键是否按下</summary>
    public bool Alt { get; set; }

    /// <summary>Shift 键是否按下</summary>
    public bool Shift { get; set; }

    /// <summary>Win 键是否按下</summary>
    public bool Win { get; set; }
}
