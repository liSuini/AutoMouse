namespace AutoMouse.Models;

/// <summary>
/// 鼠标事件，包括移动、按下、抬起、滚轮。
/// 拖拽操作由 Down → Move → Up 序列自然组成，无需独立类型。
/// </summary>
public class MouseEvent : InputEvent
{
    public override InputEventType Type => InputEventType.Mouse;

    /// <summary>鼠标动作</summary>
    public MouseAction Action { get; set; }

    /// <summary>按键（Move 和 Scroll 时为 None）</summary>
    public MouseButton Button { get; set; }

    /// <summary>屏幕 X 坐标</summary>
    public int X { get; set; }

    /// <summary>屏幕 Y 坐标</summary>
    public int Y { get; set; }

    /// <summary>滚轮增量（仅 Scroll 时有效，正值向上，负值向下）</summary>
    public int Delta { get; set; }
}
