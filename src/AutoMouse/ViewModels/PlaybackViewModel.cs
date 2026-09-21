using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoMouse.ViewModels;

public partial class PlaybackViewModel : ObservableObject
{
    private readonly IPlaybackService _playbackService;
    private readonly IScriptService _scriptService;
    private readonly MainViewModel _main;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private List<ScriptInfo> _scripts = new();

    [ObservableProperty]
    private ScriptInfo? _selectedScript;

    [ObservableProperty]
    private int _loopCount = 1;

    [ObservableProperty]
    private bool _infiniteLoop;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private string _playbackStatus = string.Empty;

    public PlaybackViewModel(IPlaybackService playbackService, IScriptService scriptService, MainViewModel main)
    {
        _playbackService = playbackService;
        _scriptService = scriptService;
        _main = main;
        RefreshScripts();
    }

    public void RefreshScripts()
    {
        Scripts = _scriptService.ListAll();
    }

    [RelayCommand]
    private async Task Play()
    {
        if (SelectedScript == null)
        {
            PlaybackStatus = "请先选择一个脚本";
            return;
        }

        var script = _scriptService.Load(SelectedScript.Name);
        if (script == null || script.Events.Count == 0)
        {
            PlaybackStatus = "脚本为空或加载失败";
            return;
        }

        _cts = new CancellationTokenSource();
        IsPlaying = true;
        _main.SetPlayingState();

        int loops = InfiniteLoop ? 0 : LoopCount;
        PlaybackStatus = $"开始回放: {SelectedScript.Name}";

        try
        {
            await _playbackService.PlayAsync(script.Events, loops, _cts.Token);
            PlaybackStatus = "回放完成";
        }
        catch (OperationCanceledException)
        {
            PlaybackStatus = "回放已停止";
        }
        finally
        {
            IsPlaying = false;
            _main.SetIdleState();
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _cts?.Cancel();
        PlaybackStatus = "正在停止...";
    }
}
