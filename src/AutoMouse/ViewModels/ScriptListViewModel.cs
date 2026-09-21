using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoMouse.ViewModels;

public partial class ScriptListViewModel : ObservableObject
{
    private readonly IScriptService _scriptService;
    private readonly PlaybackViewModel _playbackViewModel;

    [ObservableProperty]
    private List<ScriptInfo> _scripts = new();

    [ObservableProperty]
    private ScriptInfo? _selectedScript;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ScriptListViewModel(IScriptService scriptService, PlaybackViewModel playbackViewModel)
    {
        _scriptService = scriptService;
        _playbackViewModel = playbackViewModel;
        Refresh();
    }

    [RelayCommand]
    private void Refresh()
    {
        Scripts = _scriptService.ListAll();
    }

    [RelayCommand]
    private void Load()
    {
        if (SelectedScript == null)
        {
            StatusMessage = "请先选择脚本";
            return;
        }

        _playbackViewModel.RefreshScripts();
        _playbackViewModel.SelectedScript = _playbackViewModel.Scripts
            .FirstOrDefault(s => s.Name == SelectedScript.Name);
        StatusMessage = $"已加载: {SelectedScript.Name}";
    }

    [RelayCommand]
    private void Delete()
    {
        if (SelectedScript == null)
        {
            StatusMessage = "请先选择脚本";
            return;
        }

        _scriptService.Delete(SelectedScript.Name);
        StatusMessage = $"已删除: {SelectedScript.Name}";
        Refresh();
    }

    [RelayCommand]
    private void Rename()
    {
        if (SelectedScript == null)
        {
            StatusMessage = "请先选择脚本";
            return;
        }

        string newName = $"重命名_{DateTime.Now:HHmmss}";
        try
        {
            _scriptService.Rename(SelectedScript.Name, newName);
            StatusMessage = $"已重命名为: {newName}";
            Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = $"重命名失败: {ex.Message}";
        }
    }
}
