using AutoMouse.Models;

namespace AutoMouse.Services.Interfaces;

/// <summary>
/// 脚本管理接口 - 深度模块。
/// JSON 格式存储，支持多脚本管理。
/// </summary>
public interface IScriptService
{
    /// <summary>保存脚本</summary>
    void Save(Script script);

    /// <summary>加载脚本</summary>
    Script? Load(string name);

    /// <summary>删除脚本</summary>
    void Delete(string name);

    /// <summary>重命名脚本</summary>
    void Rename(string oldName, string newName);

    /// <summary>列出所有脚本摘要</summary>
    List<ScriptInfo> ListAll();

    /// <summary>脚本存储目录</summary>
    string ScriptDirectory { get; }
}
