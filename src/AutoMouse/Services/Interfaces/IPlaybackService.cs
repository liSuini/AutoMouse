using AutoMouse.Models;

namespace AutoMouse.Services.Interfaces;

/// <summary>
/// 回放进度信息
/// </summary>
public class PlaybackProgress
{
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public int CurrentLoop { get; set; }
    public int TotalLoops { get; set; }
}

/// <summary>
/// 回放引擎接口 - 深度模块。
/// 通过 SendInput API 按时间序列精确重放操作。
/// </summary>
public interface IPlaybackService
{
    /// <summary>开始回放。loopCount=0 表示无限循环。</summary>
    Task PlayAsync(List<InputEvent> events, int loopCount, CancellationToken ct);

    /// <summary>当前是否正在回放</summary>
    bool IsPlaying { get; }

    /// <summary>回放进度变化时触发</summary>
    event EventHandler<PlaybackProgress>? ProgressChanged;
}
