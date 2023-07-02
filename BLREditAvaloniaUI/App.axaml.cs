using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BLREdit.Core;
using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Models.BLR.Server;
using BLREdit.ViewModels;
using BLREdit.Views;

using System.Diagnostics;
using System.Globalization;

namespace BLREdit;

public partial class App : Application
{
    public const string CurrentVersion = "v1.0.0";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Startup += OnAppStartup;
            desktop.ShutdownRequested += OnAppShutdownRequested;
            desktop.Exit += OnAppExit;

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    void OnAppStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        //TODO do stuff on startup once we enter base.OnFrameworkInit after MainWindow ctor
        Debug.WriteLine("BLREdit Startup!");
        PropertyChangedNotificationInterceptor.InitializationFinished();
    }
    void OnAppShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        //TODO do stuff on shutdown request happens before Exit Event also triggers when getting killed from task manager
        Debug.WriteLine("BLREdit Shutdown Requested!");
    }
    void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        //TODO do stuff on app exit after shutdown request and also triggers even when getting killed from task manager
        
        SaveData();
        Debug.WriteLine("BLREdit Exit!");
    }

    static void SaveData()
    {
        if (Avalonia.Controls.Design.IsDesignMode) return;
        Debug.WriteLine("Saving Data!");
        BLRClient.Save();
        BLRServer.Save();
        //TODO Add all files for saving purposes here
    }
}