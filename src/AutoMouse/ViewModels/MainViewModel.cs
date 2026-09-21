using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoMouse.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IHookService _hookService;
    private readonly IPlaybackService _playbackService;
    private readonly IScriptService _scriptService;
    private readonly IHotkeyService _hotkeyService;

    [ObservableProperty]
    private AppState _state = AppState.Idle;

    [ObservableProperty]
    private string _statusText = "待机";

    [ObservableProperty]
    private int _eventCount;

    [ObservableProperty]
    private string _recordTime = "00:00.000";

    private System.Diagnostics.Stopwatch? _recordStopwatch;
    private System.Threading.Timer? _statusTimer;

    public RecordViewModel Record { get; }
    public PlaybackViewModel Playback { get; }
    public ScriptListViewModel ScriptList { get; }

    public MainViewModel(
        IHookService hookService,
        IPlaybackService playbackService,
        IScriptService scriptService,
        IHotkeyService hotkeyService)
    {
        _hookService = hookService;
        _playbackService = playbackService;
        _scriptService = scriptService;
        _hotkeyService = hotkeyService;

        Record = new RecordViewModel(hookService, scriptService, this);
        Playback = new PlaybackViewModel(playbackService, scriptService, this);
        ScriptList = new ScriptListViewModel(scriptService, Playback);

        _hookService.EventCaptured += (_, _) =>
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() => EventCount++);
        };

        _playbackService.ProgressChanged += (_, p) =>
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                StatusText = $"回放中 ({p.CurrentStep}/{p.TotalSteps} 循环 {p.CurrentLoop}/{(p.TotalLoops == 0 ? "∞" : p.TotalLoops)})";
            });
        };
    }

    public void SetRecordingState()
    {
        State = AppState.Recording;
        StatusText = "录制中";
        EventCount = 0;
        _recordStopwatch = System.Diagnostics.Stopwatch.StartNew();
        _statusTimer = new System.Threading.Timer(_ =>
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (_recordStopwatch != null)
                {
                    var elapsed = _recordStopwatch.Elapsed;
                    RecordTime = $"{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}";
                }
            });
        }, null, 0, 50);
    }

    public void SetIdleState()
    {
        _statusTimer?.Dispose();
        _statusTimer = null;
        _recordStopwatch?.Stop();
        State = AppState.Idle;
        StatusText = "待机";
    }

    public void SetPlayingState()
    {
        State = AppState.Playing;
        StatusText = "回放中";
    }
}
