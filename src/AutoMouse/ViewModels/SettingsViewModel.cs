using AutoMouse.Services;
using AutoMouse.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoMouse.ViewModels;

/// <summary>
/// 设置面板 ViewModel
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private string _startRecordKey = "F9";

    [ObservableProperty]
    private string _stopRecordKey = "F10";

    [ObservableProperty]
    private string _startPlaybackKey = "F7";

    [ObservableProperty]
    private string _stopPlaybackKey = "F8";

    [ObservableProperty]
    private bool _autoHideWindow = true;

    [ObservableProperty]
    private int _moveSampleInterval = 5;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _scriptDirectory = string.Empty;

    public SettingsViewModel(SettingsService settingsService, IScriptService scriptService)
    {
        _settingsService = settingsService;
        var s = settingsService.Current;
        StartRecordKey = s.Hotkeys.StartRecord;
        StopRecordKey = s.Hotkeys.StopRecord;
        StartPlaybackKey = s.Hotkeys.StartPlayback;
        StopPlaybackKey = s.Hotkeys.StopPlayback;
        AutoHideWindow = s.Recording.AutoHideWindow;
        MoveSampleInterval = s.Recording.MoveSampleInterval;
        ScriptDirectory = scriptService.ScriptDirectory;
    }

    [RelayCommand]
    private void Save()
    {
        var s = _settingsService.Current;
        s.Hotkeys.StartRecord = StartRecordKey;
        s.Hotkeys.StopRecord = StopRecordKey;
        s.Hotkeys.StartPlayback = StartPlaybackKey;
        s.Hotkeys.StopPlayback = StopPlaybackKey;
        s.Recording.AutoHideWindow = AutoHideWindow;
        s.Recording.MoveSampleInterval = MoveSampleInterval;

        _settingsService.Save();
        StatusMessage = "设置已保存（热键变更需重启应用生效）";
    }

    [RelayCommand]
    private void Reset()
    {
        StartRecordKey = "F9";
        StopRecordKey = "F10";
        StartPlaybackKey = "F7";
        StopPlaybackKey = "F8";
        AutoHideWindow = true;
        MoveSampleInterval = 5;
        StatusMessage = "已恢复默认设置（需点击保存生效）";
    }
}
