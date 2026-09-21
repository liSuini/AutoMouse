using AutoMouse.Models;
using AutoMouse.Services;
using FluentAssertions;
using System.IO;

namespace AutoMouse.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _settingsPath;

    public SettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AutoMouseSettings_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        // SettingsService uses %APPDATA%/AutoMouse/, we'll test via the model directly
        _settingsPath = Path.Combine(_tempDir, "settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void AppSettings_Default_HasCorrectDefaults()
    {
        var settings = new AppSettings();
        settings.Hotkeys.StartRecord.Should().Be("F9");
        settings.Hotkeys.StopRecord.Should().Be("F10");
        settings.Hotkeys.StartPlayback.Should().Be("F7");
        settings.Hotkeys.StopPlayback.Should().Be("F8");
        settings.Recording.AutoHideWindow.Should().BeTrue();
        settings.Recording.MoveSampleInterval.Should().Be(5);
    }

    [Fact]
    public void AppSettings_CanSerializeAndDeserialize()
    {
        var settings = new AppSettings
        {
            Hotkeys = { StartRecord = "F1", StopRecord = "F2" },
            Recording = { AutoHideWindow = false, MoveSampleInterval = 10 }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(settings);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);

        deserialized!.Hotkeys.StartRecord.Should().Be("F1");
        deserialized.Hotkeys.StopRecord.Should().Be("F2");
        deserialized.Recording.AutoHideWindow.Should().BeFalse();
        deserialized.Recording.MoveSampleInterval.Should().Be(10);
    }
}
