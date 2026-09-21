using AutoMouse.Models;
using AutoMouse.Services;
using AutoMouse.ViewModels;
using FluentAssertions;
using System.IO;

namespace AutoMouse.Tests.Services;

public class EditorViewModelTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ScriptService _scriptService;
    private readonly EditorViewModel _editor;

    public EditorViewModelTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AutoMouseEditor_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _scriptService = new ScriptService(_tempDir);
        _editor = new EditorViewModel(_scriptService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void LoadScript_ExistingScript_LoadsEventsIntoEditor()
    {
        var script = new Script
        {
            Name = "EditTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, Action = MouseAction.Move, X = 100, Y = 200 },
                new MouseEvent { Timestamp = 100, Action = MouseAction.Down, Button = MouseButton.Left, X = 100, Y = 200 },
                new KeyboardEvent { Timestamp = 200, Key = 0x41, Action = KeyAction.Down }
            }
        };
        _scriptService.Save(script);

        _editor.LoadScript("EditTest");

        _editor.Events.Should().HaveCount(3);
        _editor.HasEvents.Should().BeTrue();
        _editor.LoadedScriptName.Should().Be("EditTest");
    }

    [Fact]
    public void LoadScript_NonExistent_ShowsError()
    {
        _editor.LoadScript("Ghost");
        _editor.Events.Should().BeEmpty();
        _editor.StatusMessage.Should().Contain("加载失败");
    }

    [Fact]
    public void DeleteSelected_RemovesEventFromList()
    {
        var script = new Script
        {
            Name = "DeleteTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 },
                new MouseEvent { Timestamp = 200, X = 50, Y = 60 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("DeleteTest");

        _editor.SelectedEvent = _editor.Events[1];
        _editor.DeleteSelectedCommand.Execute(null);

        _editor.Events.Should().HaveCount(2);
        _editor.Events[0].Timestamp.Should().Be(0);
        _editor.Events[1].Timestamp.Should().Be(200);
    }

    [Fact]
    public void InsertBefore_InsertsNewEventBeforeSelected()
    {
        var script = new Script
        {
            Name = "InsertTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("InsertTest");

        _editor.SelectedEvent = _editor.Events[1]; // 第二个事件
        _editor.InsertBeforeCommand.Execute(null);

        _editor.Events.Should().HaveCount(3);
        _editor.Events[1].Should().BeOfType<MouseEvent>();
    }

    [Fact]
    public void InsertAfter_InsertsNewEventAfterSelected()
    {
        var script = new Script
        {
            Name = "InsertAfterTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("InsertAfterTest");

        _editor.SelectedEvent = _editor.Events[0]; // 第一个事件
        _editor.InsertAfterCommand.Execute(null);

        _editor.Events.Should().HaveCount(3);
    }

    [Fact]
    public void MoveUp_MovesEventOnePositionUp()
    {
        var script = new Script
        {
            Name = "MoveUpTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 },
                new MouseEvent { Timestamp = 200, X = 50, Y = 60 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("MoveUpTest");

        _editor.SelectedEvent = _editor.Events[2]; // 第三个
        _editor.MoveUpCommand.Execute(null);

        _editor.Events[1].Should().Be(_editor.SelectedEvent);
    }

    [Fact]
    public void MoveDown_MovesEventOnePositionDown()
    {
        var script = new Script
        {
            Name = "MoveDownTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 },
                new MouseEvent { Timestamp = 200, X = 50, Y = 60 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("MoveDownTest");

        _editor.SelectedEvent = _editor.Events[0]; // 第一个
        _editor.MoveDownCommand.Execute(null);

        _editor.Events[1].Should().Be(_editor.SelectedEvent);
    }

    [Fact]
    public void MoveUp_AtTop_DoesNotMove()
    {
        var script = new Script
        {
            Name = "MoveUpTopTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("MoveUpTopTest");

        var first = _editor.Events[0];
        _editor.SelectedEvent = first;
        _editor.MoveUpCommand.Execute(null);

        _editor.Events[0].Should().Be(first);
        _editor.StatusMessage.Should().Contain("最顶部");
    }

    [Fact]
    public void MoveDown_AtBottom_DoesNotMove()
    {
        var script = new Script
        {
            Name = "MoveDownBottomTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("MoveDownBottomTest");

        var last = _editor.Events[1];
        _editor.SelectedEvent = last;
        _editor.MoveDownCommand.Execute(null);

        _editor.Events[1].Should().Be(last);
        _editor.StatusMessage.Should().Contain("最底部");
    }

    [Fact]
    public void Save_PersistsModifiedEvents()
    {
        var script = new Script
        {
            Name = "SaveTest",
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, X = 10, Y = 20 },
                new MouseEvent { Timestamp = 100, X = 30, Y = 40 }
            }
        };
        _scriptService.Save(script);
        _editor.LoadScript("SaveTest");

        // 删除一个事件
        _editor.SelectedEvent = _editor.Events[1];
        _editor.DeleteSelectedCommand.Execute(null);

        // 保存
        _editor.SaveCommand.Execute(null);

        // 重新加载验证
        var reloaded = _scriptService.Load("SaveTest");
        reloaded!.Events.Should().HaveCount(1);
        reloaded.Events[0].Timestamp.Should().Be(0);
    }

    [Fact]
    public void Clear_RemovesAllEvents()
    {
        var script = new Script
        {
            Name = "ClearTest",
            Events = new List<InputEvent> { new MouseEvent { Timestamp = 0, X = 10, Y = 20 } }
        };
        _scriptService.Save(script);
        _editor.LoadScript("ClearTest");

        _editor.ClearCommand.Execute(null);

        _editor.Events.Should().BeEmpty();
        _editor.HasEvents.Should().BeFalse();
        _editor.LoadedScriptName.Should().BeNull();
    }

    [Fact]
    public void DeleteSelected_NoSelection_ShowsError()
    {
        _editor.Events.Add(new MouseEvent { Timestamp = 0, X = 10, Y = 20 });
        _editor.SelectedEvent = null;
        _editor.DeleteSelectedCommand.Execute(null);

        _editor.Events.Should().HaveCount(1);
        _editor.StatusMessage.Should().Contain("请先选中");
    }
}
