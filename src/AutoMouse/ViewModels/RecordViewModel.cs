using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoMouse.ViewModels;

public partial class RecordViewModel : ObservableObject
{
    private readonly IHookService _hookService;
    private readonly IScriptService _scriptService;
    private readonly MainViewModel _main;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private string _lastSavedMessage = string.Empty;

    public RecordViewModel(IHookService hookService, IScriptService scriptService, MainViewModel main)
    {
        _hookService = hookService;
        _scriptService = scriptService;
        _main = main;
    }

    [RelayCommand]
    private void StartRecord()
    {
        try
        {
            _hookService.Start();
            IsRecording = true;
            _main.SetRecordingState();
            LastSavedMessage = string.Empty;
        }
        catch (Exception ex)
        {
            LastSavedMessage = $"录制启动失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void StopRecord()
    {
        var events = _hookService.Stop();
        IsRecording = false;
        _main.SetIdleState();

        if (events.Count > 0)
        {
            var script = new Script
            {
                Name = $"录制_{DateTime.Now:yyyyMMdd_HHmmss}",
                CreatedAt = DateTime.Now,
                RecordDuration = events.Count > 0 ? events[^1].Timestamp : 0,
                Events = events
            };

            _scriptService.Save(script);
            LastSavedMessage = $"已保存: {script.Name} ({events.Count} 个事件)";
            
            // 刷新脚本列表
            _main.ScriptList.RefreshCommand.Execute(null);
            _main.Playback.RefreshScripts();
            
            // 自动加载到编辑器
            _main.Editor.LoadScript(script.Name);
        }
        else
        {
            LastSavedMessage = "未捕获到事件，未保存脚本";
        }
    }
}
