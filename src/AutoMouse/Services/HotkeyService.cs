using AutoMouse.Services.Interfaces;
using AutoMouse.Utils.Win32;

namespace AutoMouse.Services;

/// <summary>
/// 热键服务 - 通过 RegisterHotKey 注册全局热键。
/// 在 WPF 中通过 HwndSource.AddHook 拦截 WM_HOTKEY 消息。
/// </summary>
public class HotkeyService : IHotkeyService
{
    private IntPtr _windowHandle = IntPtr.Zero;
    private readonly HashSet<int> _registeredIds = new();

    public event EventHandler<HotkeyEventArgs>? HotkeyPressed;

    public void SetWindowHandle(IntPtr handle)
    {
        _windowHandle = handle;
    }

    public void Register(int id, uint modifiers, uint key)
    {
        // 先注销已注册的同 ID 热键
        if (_registeredIds.Contains(id))
            Unregister(id);

        if (Win32Native.RegisterHotKey(_windowHandle, id, modifiers, key))
        {
            _registeredIds.Add(id);
        }
        else
        {
            throw new InvalidOperationException(
                $"Failed to register hotkey (ID={id}). It may conflict with another application.");
        }
    }

    public void Unregister(int id)
    {
        if (_registeredIds.Contains(id))
        {
            Win32Native.UnregisterHotKey(_windowHandle, id);
            _registeredIds.Remove(id);
        }
    }

    /// <summary>处理 WM_HOTKEY 消息，由 WPF HwndSource 钩子调用</summary>
    public bool ProcessMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32Native.WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_registeredIds.Contains(id))
            {
                HotkeyPressed?.Invoke(this, new HotkeyEventArgs { Id = id });
                handled = true;
                return true;
            }
        }
        return false;
    }
}
