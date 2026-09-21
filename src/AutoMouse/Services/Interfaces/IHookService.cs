using AutoMouse.Models;

namespace AutoMouse.Services.Interfaces;

/// <summary>
/// 录制引擎接口 - 深度模块，小接口藏大实现。
/// 通过 Win32 低级钩子全局捕获鼠标和键盘事件。
/// </summary>
public interface IHookService
{
    /// <summary>开始录制</summary>
    void Start();

    /// <summary>停止录制，返回捕获的事件列表</summary>
    List<InputEvent> Stop();

    /// <summary>当前是否正在录制</summary>
    bool IsRecording { get; }

    /// <summary>事件捕获时触发（可选，用于实时状态更新）</summary>
    event EventHandler<InputEvent>? EventCaptured;

    /// <summary>移动事件采样间隔（毫秒），默认 5ms</summary>
    int MoveSampleInterval { get; set; }
}
