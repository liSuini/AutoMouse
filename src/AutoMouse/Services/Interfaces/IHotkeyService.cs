namespace AutoMouse.Services.Interfaces;

/// <summary>
/// 热键事件参数
/// </summary>
public class HotkeyEventArgs : EventArgs
{
    public int Id { get; set; }
}

/// <summary>
/// 热键服务接口。
/// 通过 RegisterHotKey 注册全局热键，控制录制和回放启停。
/// </summary>
public interface IHotkeyService
{
    /// <summary>注册全局热键</summary>
    void Register(int id, uint modifiers, uint key);

    /// <summary>注销热键</summary>
    void Unregister(int id);

    /// <summary>热键按下时触发</summary>
    event EventHandler<HotkeyEventArgs>? HotkeyPressed;

    /// <summary>设置窗口句柄（WPF 中由 HwndSource 提供）</summary>
    void SetWindowHandle(IntPtr handle);
}
