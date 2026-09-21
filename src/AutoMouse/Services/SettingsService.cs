using System.IO;
using System.Text.Json;

namespace AutoMouse.Services;

/// <summary>
/// 应用配置模型
/// </summary>
public class AppSettings
{
    public HotkeySettings Hotkeys { get; set; } = new();
    public RecordingSettings Recording { get; set; } = new();
}

public class HotkeySettings
{
    public string StartRecord { get; set; } = "F9";
    public string StopRecord { get; set; } = "F10";
    public string StartPlayback { get; set; } = "F7";
    public string StopPlayback { get; set; } = "F8";
}

public class RecordingSettings
{
    public bool AutoHideWindow { get; set; } = true;
    public int MoveSampleInterval { get; set; } = 5;
}

/// <summary>
/// 配置管理服务 - 读写 settings.json
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;

    public AppSettings Current { get; private set; } = new();

    public SettingsService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoMouse");
        Directory.CreateDirectory(dir);
        _settingsPath = Path.Combine(dir, "settings.json");
        Load();
    }

    public void Load()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                Current = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
            catch
            {
                Current = new AppSettings();
            }
        }
        else
        {
            Current = new AppSettings();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Current, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
