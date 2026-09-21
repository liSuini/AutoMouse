using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using AutoMouse.Utils.Json;

namespace AutoMouse.Services;

/// <summary>
/// 脚本管理 - JSON 格式存储，支持多脚本管理。
/// 存储路径：%APPDATA%/AutoMouse/scripts/
/// </summary>
public class ScriptService : IScriptService
{
    private readonly string _scriptDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public string ScriptDirectory => _scriptDirectory;

    public ScriptService()
    {
        _scriptDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoMouse", "scripts");

        Directory.CreateDirectory(_scriptDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new InputEventConverter() }
        };
    }

    /// <summary>用于测试的自定义存储路径构造函数</summary>
    public ScriptService(string customDirectory)
    {
        _scriptDirectory = customDirectory;
        Directory.CreateDirectory(_scriptDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new InputEventConverter() }
        };
    }

    public void Save(Script script)
    {
        if (string.IsNullOrWhiteSpace(script.Name))
            throw new ArgumentException("Script name cannot be empty.", nameof(script));

        string filePath = GetFilePath(script.Name);
        string json = JsonSerializer.Serialize(script, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    public Script? Load(string name)
    {
        string filePath = GetFilePath(name);
        if (!File.Exists(filePath))
            return null;

        string json = File.ReadAllText(filePath);
        try
        {
            return JsonSerializer.Deserialize<Script>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void Delete(string name)
    {
        string filePath = GetFilePath(name);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public void Rename(string oldName, string newName)
    {
        string oldPath = GetFilePath(oldName);
        string newPath = GetFilePath(newName);

        if (!File.Exists(oldPath))
            throw new FileNotFoundException($"Script '{oldName}' not found.", oldPath);
        if (File.Exists(newPath))
            throw new InvalidOperationException($"Script '{newName}' already exists.");

        // 更新脚本内部的名称
        var script = Load(oldName);
        if (script != null)
        {
            script.Name = newName;
            Save(script);
            File.Delete(oldPath);
        }
    }

    public List<ScriptInfo> ListAll()
    {
        var result = new List<ScriptInfo>();

        foreach (var file in Directory.GetFiles(_scriptDirectory, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                var script = JsonSerializer.Deserialize<Script>(json, _jsonOptions);
                if (script != null)
                {
                    result.Add(new ScriptInfo
                    {
                        Name = script.Name,
                        EventCount = script.Events.Count,
                        Duration = script.RecordDuration,
                        CreatedAt = script.CreatedAt
                    });
                }
            }
            catch
            {
                // 跳过损坏的脚本文件
            }
        }

        return result.OrderByDescending(s => s.CreatedAt).ToList();
    }

    private string GetFilePath(string name)
    {
        // 清理文件名中的非法字符
        string safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_scriptDirectory, safeName + ".json");
    }
}
