namespace AutoMouse.Models;

/// <summary>
/// 输入事件基类。所有鼠标和键盘事件的抽象基类。
/// Timestamp 为相对录制开始的毫秒数。
/// </summary>
public abstract class InputEvent
{
    /// <summary>相对录制开始的时间戳（毫秒）</summary>
    public long Timestamp { get; set; }

    /// <summary>事件类型</summary>
    public abstract InputEventType Type { get; }
}
