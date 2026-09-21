using System.Runtime.InteropServices;
using AutoMouse.Models;
using AutoMouse.Services.Interfaces;
using AutoMouse.Utils.Win32;

namespace AutoMouse.Services;

/// <summary>
/// 回放引擎 - 通过 SendInput API 按时间序列精确重放操作。
/// 实现 IPlaybackService 接口，隐藏所有 SendInput 细节。
/// </summary>
public class PlaybackService : IPlaybackService
{
    public bool IsPlaying { get; private set; }

    public event EventHandler<PlaybackProgress>? ProgressChanged;

    public async Task PlayAsync(List<InputEvent> events, int loopCount, CancellationToken ct)
    {
        if (events.Count == 0)
            return;

        IsPlaying = true;

        try
        {
            int maxLoop = loopCount == 0 ? int.MaxValue : loopCount;
            int screenWidth = Win32Native.GetSystemMetrics(Win32Native.SM_CXSCREEN);
            int screenHeight = Win32Native.GetSystemMetrics(Win32Native.SM_CYSCREEN);

            for (int i = 0; i < maxLoop; i++)
            {
                long baseTime = 0;

                for (int j = 0; j < events.Count; j++)
                {
                    ct.ThrowIfCancellationRequested();

                    var evt = events[j];
                    int delay = (int)(evt.Timestamp - baseTime);

                    if (delay > 0)
                        await Task.Delay(delay, ct);

                    if (evt is MouseEvent m)
                        SendMouseEvent(m, screenWidth, screenHeight);
                    else if (evt is KeyboardEvent k)
                        SendKeyboardEvent(k);

                    baseTime = evt.Timestamp;

                    ProgressChanged?.Invoke(this, new PlaybackProgress
                    {
                        CurrentStep = j + 1,
                        TotalSteps = events.Count,
                        CurrentLoop = i + 1,
                        TotalLoops = loopCount
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常中断，不视为错误
        }
        finally
        {
            IsPlaying = false;
        }
    }

    private static void SendMouseEvent(MouseEvent m, int screenWidth, int screenHeight)
    {
        uint flags = Win32Native.MOUSEEVENTF_ABSOLUTE;
        uint mouseData = 0;

        switch (m.Action)
        {
            case MouseAction.Move:
                flags |= Win32Native.MOUSEEVENTF_MOVE;
                break;
            case MouseAction.Down when m.Button == MouseButton.Left:
                flags |= Win32Native.MOUSEEVENTF_LEFTDOWN;
                break;
            case MouseAction.Up when m.Button == MouseButton.Left:
                flags |= Win32Native.MOUSEEVENTF_LEFTUP;
                break;
            case MouseAction.Down when m.Button == MouseButton.Right:
                flags |= Win32Native.MOUSEEVENTF_RIGHTDOWN;
                break;
            case MouseAction.Up when m.Button == MouseButton.Right:
                flags |= Win32Native.MOUSEEVENTF_RIGHTUP;
                break;
            case MouseAction.Down when m.Button == MouseButton.Middle:
                flags |= Win32Native.MOUSEEVENTF_MIDDLEDOWN;
                break;
            case MouseAction.Up when m.Button == MouseButton.Middle:
                flags |= Win32Native.MOUSEEVENTF_MIDDLEUP;
                break;
            case MouseAction.Scroll:
                flags |= Win32Native.MOUSEEVENTF_WHEEL;
                mouseData = (uint)(m.Delta << 16);
                break;
            default:
                return;
        }

        var input = new INPUT
        {
            type = Win32Native.INPUT_MOUSE,
            u = new INPUTUNION
            {
                mi = new MOUSEINPUT
                {
                    dx = m.X * 65535 / screenWidth,
                    dy = m.Y * 65535 / screenHeight,
                    mouseData = mouseData,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Win32Native.SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    private static void SendKeyboardEvent(KeyboardEvent k)
    {
        uint flags = k.Action == KeyAction.Up ? Win32Native.KEYEVENTF_KEYUP : 0;

        var input = new INPUT
        {
            type = Win32Native.INPUT_KEYBOARD,
            u = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)k.Key,
                    wScan = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Win32Native.SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }
}
