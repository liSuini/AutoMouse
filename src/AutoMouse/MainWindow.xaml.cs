using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using AutoMouse.Services;
using AutoMouse.Services.Interfaces;
using AutoMouse.Utils.Win32;
using AutoMouse.ViewModels;

namespace AutoMouse;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IHotkeyService? _hotkeyService;
    private MainViewModel? _viewModel;

    // 热键 ID
    private const int HOTKEY_START_RECORD = 1;
    private const int HOTKEY_STOP_RECORD = 2;
    private const int HOTKEY_START_PLAYBACK = 3;
    private const int HOTKEY_STOP_PLAYBACK = 4;

    // 托盘
    private NotifyIcon? _notifyIcon;
    private bool _isHiddenToTray;

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
        _viewModel = DataContext as MainViewModel;

        // 初始化托盘
        InitNotifyIcon();

        if (_hotkeyService == null)
            return;

        var hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);

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
                _hotkeyService.Register(HOTKEY_STOP_PLAYBACK, 0, 0x77);   // F8
            }
            catch
            {
                // 热键注册失败不阻止应用启动
            }

            // 热键事件处理
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        }
    }

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            switch (e.Id)
            {
                case HOTKEY_START_RECORD:
                    if (_viewModel != null && !_viewModel.Record.IsRecording)
                        _viewModel.Record.StartRecordCommand.Execute(null);
                    break;
                case HOTKEY_STOP_RECORD:
                    if (_viewModel != null && _viewModel.Record.IsRecording)
                        _viewModel.Record.StopRecordCommand.Execute(null);
                    break;
                case HOTKEY_START_PLAYBACK:
                    if (_viewModel != null && !_viewModel.Playback.IsPlaying)
                        _viewModel.Playback.PlayCommand.Execute(null);
                    break;
                case HOTKEY_STOP_PLAYBACK:
                    if (_viewModel != null && _viewModel.Playback.IsPlaying)
                        _viewModel.Playback.StopCommand.Execute(null);
                    break;
            }
        });
    }

    private void InitNotifyIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "AutoMouse - 鼠标键盘自动操作工具",
            Visible = true,
        };

        // 使用系统图标作为默认图标
        _notifyIcon.Icon = SystemIcons.Application;

        // 右键菜单
        var menu = new ContextMenuStrip();
        menu.Items.Add("开始录制", null, (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel?.Record.StartRecordCommand.Execute(null);
            });
        });
        menu.Items.Add("停止录制", null, (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel?.Record.StopRecordCommand.Execute(null);
            });
        });
        menu.Items.Add("-");
        menu.Items.Add("开始回放", null, (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel?.Playback.PlayCommand.Execute(null);
            });
        });
        menu.Items.Add("停止回放", null, (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel?.Playback.StopCommand.Execute(null);
            });
        });
        menu.Items.Add("-");
        menu.Items.Add("显示窗口", null, (s, e) =>
        {
            Dispatcher.Invoke(() => ShowFromTray());
        });
        menu.Items.Add("退出", null, (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                _notifyIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            });
        });

        _notifyIcon.ContextMenuStrip = menu;

        // 双击显示窗口
        _notifyIcon.DoubleClick += (s, e) =>
        {
            Dispatcher.Invoke(() => ShowFromTray());
        };
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _isHiddenToTray = false;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        // 最小化时隐藏到托盘
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            _isHiddenToTray = true;
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // 关闭时最小化到托盘而不是退出
        if (_notifyIcon != null && !_isHiddenToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
        base.OnClosing(e);
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
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        if (_hotkeyService != null)
        {
            _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
            _hotkeyService.Unregister(HOTKEY_START_RECORD);
            _hotkeyService.Unregister(HOTKEY_STOP_RECORD);
            _hotkeyService.Unregister(HOTKEY_START_PLAYBACK);
            _hotkeyService.Unregister(HOTKEY_STOP_PLAYBACK);
        }
        base.OnClosed(e);
    }
}
