using System.Collections.ObjectModel;
using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoMouse.ViewModels;

/// <summary>
/// 编辑面板 ViewModel - 用于查看、删除、修改、插入、排序事件步骤
/// </summary>
public partial class EditorViewModel : ObservableObject
{
    private readonly IScriptService _scriptService;

    /// <summary>可编辑的事件列表（ObservableCollection 通知 UI 变化）</summary>
    public ObservableCollection<InputEvent> Events { get; } = new();

    [ObservableProperty]
    private InputEvent? _selectedEvent;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string? _loadedScriptName;

    [ObservableProperty]
    private bool _hasEvents;

    public EditorViewModel(IScriptService scriptService)
    {
        _scriptService = scriptService;
    }

    /// <summary>加载脚本到编辑器</summary>
    public void LoadScript(string scriptName)
    {
        var script = _scriptService.Load(scriptName);
        if (script == null)
        {
            StatusMessage = $"脚本 '{scriptName}' 加载失败";
            return;
        }

        Events.Clear();
        foreach (var evt in script.Events)
            Events.Add(evt);

        LoadedScriptName = scriptName;
        HasEvents = Events.Count > 0;
        StatusMessage = $"已加载: {scriptName} ({Events.Count} 个事件)";
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedEvent == null)
        {
            StatusMessage = "请先选中一行";
            return;
        }

        Events.Remove(SelectedEvent);
        HasEvents = Events.Count > 0;
        StatusMessage = $"已删除，剩余 {Events.Count} 个事件";
    }

    [RelayCommand]
    private void InsertBefore()
    {
        if (SelectedEvent == null)
        {
            StatusMessage = "请先选中要插入的位置（在其前面插入）";
            return;
        }

        int index = Events.IndexOf(SelectedEvent);
        var newEvent = new MouseEvent
        {
            Timestamp = SelectedEvent.Timestamp,
            Action = MouseAction.Move,
            X = 0,
            Y = 0
        };
        Events.Insert(index, newEvent);
        HasEvents = Events.Count > 0;
        StatusMessage = $"已在第 {index + 1} 步前插入新事件";
    }

    [RelayCommand]
    private void InsertAfter()
    {
        if (SelectedEvent == null)
        {
            StatusMessage = "请先选中要插入的位置（在其后面插入）";
            return;
        }

        int index = Events.IndexOf(SelectedEvent);
        var newEvent = new MouseEvent
        {
            Timestamp = SelectedEvent.Timestamp,
            Action = MouseAction.Move,
            X = 0,
            Y = 0
        };
        Events.Insert(index + 1, newEvent);
        HasEvents = Events.Count > 0;
        StatusMessage = $"已在第 {index + 2} 步后插入新事件";
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (SelectedEvent == null)
        {
            StatusMessage = "请先选中要上移的步骤";
            return;
        }

        int index = Events.IndexOf(SelectedEvent);
        if (index <= 0)
        {
            StatusMessage = "已在最顶部";
            return;
        }

        Events.Move(index, index - 1);
        StatusMessage = $"已上移到第 {index} 步";
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (SelectedEvent == null)
        {
            StatusMessage = "请先选中要下移的步骤";
            return;
        }

        int index = Events.IndexOf(SelectedEvent);
        if (index >= Events.Count - 1)
        {
            StatusMessage = "已在最底部";
            return;
        }

        Events.Move(index, index + 1);
        StatusMessage = $"已下移到第 {index + 2} 步";
    }

    [RelayCommand]
    private void Save()
    {
        if (LoadedScriptName == null)
        {
            StatusMessage = "没有已加载的脚本";
            return;
        }

        var script = new Script
        {
            Name = LoadedScriptName,
            CreatedAt = DateTime.Now,
            RecordDuration = Events.Count > 0 ? Events[^1].Timestamp : 0,
            Events = new List<InputEvent>(Events)
        };

        _scriptService.Save(script);
        StatusMessage = $"已保存: {LoadedScriptName} ({Events.Count} 个事件)";
    }

    [RelayCommand]
    private void Clear()
    {
        Events.Clear();
        HasEvents = false;
        LoadedScriptName = null;
        StatusMessage = "已清空编辑器";
    }
}
