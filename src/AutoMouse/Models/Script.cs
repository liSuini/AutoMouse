namespace AutoMouse.Models;

/// <summary>
/// 脚本，包含录制的事件列表和元数据。
/// 以 JSON 文件形式持久化存储。
/// </summary>
public class Script
{
    /// <summary>脚本名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>录制总时长（毫秒）</summary>
    public long RecordDuration { get; set; }

    /// <summary>事件列表</summary>
    public List<InputEvent> Events { get; set; } = new();
}

/// <summary>
/// 脚本摘要信息，用于列表展示，不包含完整事件列表。
/// </summary>
public class ScriptInfo
{
    public string Name { get; set; } = string.Empty;
    public int EventCount { get; set; }
    public long Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}
