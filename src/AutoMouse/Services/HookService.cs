using System.Diagnostics;
using System.Runtime.InteropServices;
using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using AutoMouse.Utils.Win32;

namespace AutoMouse.Services;

/// <summary>
/// 录制引擎 - 通过 Win32 低级钩子全局捕获鼠标和键盘事件。
/// 实现 IHookService 接口，隐藏所有 Win32 钩子细节。
/// </summary>
public class HookService : IHookService
{
    private IntPtr _mouseHook = IntPtr.Zero;
    private IntPtr _keyboardHook = IntPtr.Zero;
    private Win32Native.HookProc? _mouseProc;
    private Win32Native.HookProc? _keyboardProc;
    private readonly Stopwatch _stopwatch = new();
    private readonly List<InputEvent> _events = new();
    private long _lastMoveTimestamp = 0;

    public bool IsRecording { get; private set; }

    public int MoveSampleInterval { get; set; } = 5;

    public event EventHandler<InputEvent>? EventCaptured;

    public void Start()
    {
        if (IsRecording)
            return;

        _events.Clear();
        _lastMoveTimestamp = 0;
        _stopwatch.Restart();

        _mouseProc = MouseHookCallback;
        _keyboardProc = KeyboardHookCallback;

        _mouseHook = Win32Native.SetWindowsHookEx(
            Win32Native.WH_MOUSE_LL, _mouseProc,
            Win32Native.GetModuleHandle("user32.dll"), 0);

        _keyboardHook = Win32Native.SetWindowsHookEx(
            Win32Native.WH_KEYBOARD_LL, _keyboardProc,
            Win32Native.GetModuleHandle("user32.dll"), 0);

        if (_mouseHook == IntPtr.Zero || _keyboardHook == IntPtr.Zero)
            throw new InvalidOperationException("Failed to install hooks. Error: " + Marshal.GetLastWin32Error());

        IsRecording = true;
    }

    public List<InputEvent> Stop()
    {
        if (!IsRecording)
            return new List<InputEvent>();

        Win32Native.UnhookWindowsHookEx(_mouseHook);
        Win32Native.UnhookWindowsHookEx(_keyboardHook);
        _mouseHook = IntPtr.Zero;
        _keyboardHook = IntPtr.Zero;

        _stopwatch.Stop();
        IsRecording = false;

        return new List<InputEvent>(_events);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && IsRecording)
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            int msg = wParam.ToInt32();
            long timestamp = _stopwatch.ElapsedMilliseconds;

            var evt = new MouseEvent { Timestamp = timestamp };

            bool captured = msg switch
            {
                Win32Native.WM_MOUSEMOVE => HandleMove(evt, hookStruct, timestamp),
                Win32Native.WM_LBUTTONDOWN => HandleButton(evt, MouseAction.Down, MouseButton.Left, hookStruct),
                Win32Native.WM_LBUTTONUP => HandleButton(evt, MouseAction.Up, MouseButton.Left, hookStruct),
                Win32Native.WM_RBUTTONDOWN => HandleButton(evt, MouseAction.Down, MouseButton.Right, hookStruct),
                Win32Native.WM_RBUTTONUP => HandleButton(evt, MouseAction.Up, MouseButton.Right, hookStruct),
                Win32Native.WM_MBUTTONDOWN => HandleButton(evt, MouseAction.Down, MouseButton.Middle, hookStruct),
                Win32Native.WM_MBUTTONUP => HandleButton(evt, MouseAction.Up, MouseButton.Middle, hookStruct),
                Win32Native.WM_MOUSEWHEEL => HandleScroll(evt, hookStruct),
                _ => false
            };

            if (captured)
                EventCaptured?.Invoke(this, evt);
        }

        return Win32Native.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    private bool HandleMove(MouseEvent evt, MSLLHOOKSTRUCT hookStruct, long timestamp)
    {
        // 采样降稠密：间隔太小的移动事件丢弃
        if (timestamp - _lastMoveTimestamp < MoveSampleInterval)
            return false;

        _lastMoveTimestamp = timestamp;
        evt.Action = MouseAction.Move;
        evt.Button = MouseButton.None;
        evt.X = hookStruct.pt.X;
        evt.Y = hookStruct.pt.Y;
        _events.Add(evt);
        return true;
    }

    private bool HandleButton(MouseEvent evt, MouseAction action, MouseButton button, MSLLHOOKSTRUCT hookStruct)
    {
        evt.Action = action;
        evt.Button = button;
        evt.X = hookStruct.pt.X;
        evt.Y = hookStruct.pt.Y;
        _events.Add(evt);
        return true;
    }

    private bool HandleScroll(MouseEvent evt, MSLLHOOKSTRUCT hookStruct)
    {
        evt.Action = MouseAction.Scroll;
        evt.Button = MouseButton.None;
        evt.X = hookStruct.pt.X;
        evt.Y = hookStruct.pt.Y;
        evt.Delta = (short)(hookStruct.mouseData >> 16);
        _events.Add(evt);
        return true;
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && IsRecording)
        {
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            int msg = wParam.ToInt32();

            KeyAction? action = msg switch
            {
                Win32Native.WM_KEYDOWN or Win32Native.WM_SYSKEYDOWN => KeyAction.Down,
                Win32Native.WM_KEYUP or Win32Native.WM_SYSKEYUP => KeyAction.Up,
                _ => null
            };

            if (action.HasValue)
            {
                var evt = new KeyboardEvent
                {
                    Timestamp = _stopwatch.ElapsedMilliseconds,
                    Key = hookStruct.vkCode,
                    Action = action.Value
                };

                // 修饰键状态由 vkCode 直接判断
                switch (hookStruct.vkCode)
                {
                    case Win32Native.VK_CONTROL:
                        // Ctrl 按下/抬起影响后续事件的 Ctrl 状态
                        break;
                    case Win32Native.VK_MENU:
                        break;
                    case Win32Native.VK_SHIFT:
                        break;
                }

                _events.Add(evt);
                EventCaptured?.Invoke(this, evt);
            }
        }

        return Win32Native.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }
}
