using AutoMouse.Models;
using AutoMouse.Services;
using FluentAssertions;
using System.IO;

namespace AutoMouse.Tests.Services;

public class ScriptServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ScriptService _service;

    public ScriptServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AutoMouseTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _service = new ScriptService(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Save_NewScript_CreatesJsonFile()
    {
        var script = new Script
        {
            Name = "TestScript",
            CreatedAt = new DateTime(2026, 9, 21, 10, 0, 0),
            RecordDuration = 1000,
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, Action = MouseAction.Move, X = 100, Y = 200 },
                new KeyboardEvent { Timestamp = 500, Key = 0x41, Action = KeyAction.Down }
            }
        };

        _service.Save(script);

        var filePath = Path.Combine(_tempDir, "TestScript.json");
        File.Exists(filePath).Should().BeTrue();

        var json = File.ReadAllText(filePath);
        json.Should().Contain("\"name\": \"TestScript\"");
        json.Should().Contain("\"type\": \"mouse\"");
        json.Should().Contain("\"type\": \"keyboard\"");
    }

    [Fact]
    public void Load_ExistingScript_ReturnsCorrectData()
    {
        var script = new Script
        {
            Name = "LoadTest",
            RecordDuration = 500,
            Events = new List<InputEvent>
            {
                new MouseEvent { Timestamp = 0, Action = MouseAction.Down, Button = MouseButton.Left, X = 50, Y = 75 },
                new MouseEvent { Timestamp = 100, Action = MouseAction.Up, Button = MouseButton.Left, X = 50, Y = 75 },
                new KeyboardEvent { Timestamp = 200, Key = 0x43, Action = KeyAction.Down, Ctrl = true }
            }
        };

        _service.Save(script);
        var loaded = _service.Load("LoadTest");

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("LoadTest");
        loaded.RecordDuration.Should().Be(500);
        loaded.Events.Should().HaveCount(3);

        var mouseEvent = loaded.Events[0].Should().BeOfType<MouseEvent>().Subject;
        mouseEvent.Action.Should().Be(MouseAction.Down);
        mouseEvent.Button.Should().Be(MouseButton.Left);
        mouseEvent.X.Should().Be(50);
        mouseEvent.Y.Should().Be(75);

        var keyEvent = loaded.Events[2].Should().BeOfType<KeyboardEvent>().Subject;
        keyEvent.Key.Should().Be(0x43);
        keyEvent.Ctrl.Should().BeTrue();
    }

    [Fact]
    public void Load_NonExistent_ReturnsNull()
    {
        _service.Load("NonExistent").Should().BeNull();
    }

    [Fact]
    public void Delete_ExistingScript_RemovesFile()
    {
        var script = new Script { Name = "ToDelete", Events = new List<InputEvent>() };
        _service.Save(script);

        _service.Delete("ToDelete");

        File.Exists(Path.Combine(_tempDir, "ToDelete.json")).Should().BeFalse();
    }

    [Fact]
    public void Rename_ExistingScript_RenamesAndUpdatesInternalName()
    {
        var script = new Script { Name = "OldName", Events = new List<InputEvent>() };
        _service.Save(script);

        _service.Rename("OldName", "NewName");

        File.Exists(Path.Combine(_tempDir, "OldName.json")).Should().BeFalse();
        File.Exists(Path.Combine(_tempDir, "NewName.json")).Should().BeTrue();

        var loaded = _service.Load("NewName");
        loaded!.Name.Should().Be("NewName");
    }

    [Fact]
    public void Rename_NonExistent_ThrowsFileNotFoundException()
    {
        var act = () => _service.Rename("Ghost", "NewName");
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Rename_TargetExists_ThrowsInvalidOperationException()
    {
        _service.Save(new Script { Name = "A", Events = new List<InputEvent>() });
        _service.Save(new Script { Name = "B", Events = new List<InputEvent>() });

        var act = () => _service.Rename("A", "B");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ListAll_ReturnsAllScriptsSortedByDate()
    {
        _service.Save(new Script { Name = "Script1", CreatedAt = new DateTime(2026, 1, 1), RecordDuration = 100, Events = new List<InputEvent> { new MouseEvent() } });
        _service.Save(new Script { Name = "Script2", CreatedAt = new DateTime(2026, 9, 21), RecordDuration = 200, Events = new List<InputEvent> { new MouseEvent(), new KeyboardEvent() } });

        var list = _service.ListAll();

        list.Should().HaveCount(2);
        list[0].Name.Should().Be("Script2"); // 最新的在前
        list[0].EventCount.Should().Be(2);
        list[0].Duration.Should().Be(200);
        list[1].Name.Should().Be("Script1");
        list[1].EventCount.Should().Be(1);
    }

    [Fact]
    public void ListAll_EmptyDirectory_ReturnsEmptyList()
    {
        _service.ListAll().Should().BeEmpty();
    }

    [Fact]
    public void Save_EmptyName_ThrowsArgumentException()
    {
        var act = () => _service.Save(new Script { Name = "" });
        act.Should().Throw<ArgumentException>();
    }
}
