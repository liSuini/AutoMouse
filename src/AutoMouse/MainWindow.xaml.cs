using System.Windows;
using AutoMouse.Services;
using AutoMouse.Services.Interfaces;
using AutoMouse.Utils.Win32;

namespace AutoMouse;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IHotkeyService? _hotkeyService;

    // 热键 ID
    private const int HOTKEY_START_RECORD = 1;
    private const int HOTKEY_STOP_RECORD = 2;
    private const int HOTKEY_START_PLAYBACK = 3;
    private const int HOTKEY_STOP_PLAYBACK = 4;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(IHotkeyService hotkeyService) : this()
    {
        _hotkeyService = hotkeyService;
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hotkeyService == null)
            return;

        var hwndSource = System.Windows.Interop.HwndSource.FromHwnd(
            new System.Windows.Interop.WindowInteropHelper(this).Handle);

        if (hwndSource != null)
        {
            hwndSource.AddHook(WndProc);
            _hotkeyService.SetWindowHandle(hwndSource.Handle);

            // 注册热键
            try
            {
                _hotkeyService.Register(HOTKEY_START_RECORD, 0, 0x78);    // F9
                _hotkeyService.Register(HOTKEY_STOP_RECORD, 0, 0x79);     // F10
                _hotkeyService.Register(HOTKEY_START_PLAYBACK, 0, 0x76);  // F7
                _hotkeyService.Register(HOTKEY_STOP_PLAYBACK, 0, 0x77);  // F8
            }
            catch
            {
                // 热键注册失败不阻止应用启动
            }
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32Native.WM_HOTKEY && _hotkeyService is HotkeyService hs)
        {
            hs.ProcessMessage(hwnd, msg, wParam, lParam, ref handled);
        }
        return IntPtr.Zero;
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_hotkeyService != null)
        {
            _hotkeyService.Unregister(HOTKEY_START_RECORD);
            _hotkeyService.Unregister(HOTKEY_STOP_RECORD);
            _hotkeyService.Unregister(HOTKEY_START_PLAYBACK);
            _hotkeyService.Unregister(HOTKEY_STOP_PLAYBACK);
        }
        base.OnClosed(e);
    }
}
