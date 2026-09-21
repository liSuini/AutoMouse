using System.Runtime.InteropServices;

namespace AutoMouse.Utils.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}

/// <summary>
/// 低级鼠标钩子结构体（WH_MOUSE_LL 回调参数）
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}

/// <summary>
/// 低级键盘钩子结构体（WH_KEYBOARD_LL 回调参数）
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct KBDLLHOOKSTRUCT
{
    public uint vkCode;
    public uint scanCode;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}
