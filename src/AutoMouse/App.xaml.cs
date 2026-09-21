using System.IO;
using System.Windows;
using AutoMouse.Services;
using AutoMouse.Services.Interfaces;
using AutoMouse.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMouse;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // 注册服务
        services.AddSingleton<IHookService, HookService>();
        services.AddSingleton<IPlaybackService, PlaybackService>();
        services.AddSingleton<IScriptService, ScriptService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();

        // 注册 ViewModel
        services.AddSingleton<MainViewModel>();

        Services = services.BuildServiceProvider();

        // 创建主窗口
        var hotkeyService = Services.GetRequiredService<IHotkeyService>();
        var mainWindow = new MainWindow(hotkeyService)
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}

