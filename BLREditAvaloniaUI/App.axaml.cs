using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BLREdit.Models.BLR;
using BLREdit.ViewModels;
using BLREdit.Views;

using PropertyChanged;

using System.Diagnostics;

namespace BLREdit;

[DoNotNotify]
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
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
    }
    void OnAppShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        //TODO do stuff on shutdown request happens before Exit Event also triggers when getting killed from task manager
        Debug.WriteLine("BLREdit Shutdown Requested!");
    }
    void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        //TODO do stuff on app exit after shutdown request and also triggers even when getting killed from task manager
        Debug.WriteLine("BLREdit Exit!");
        SaveData();        
    }

    void SaveData()
    {
        if (Avalonia.Controls.Design.IsDesignMode) return;
        IOResources.SerializeFile("Data\\ClientList.json", BLRClient.Clients);
        //TODO Add all files for saving purposes here
    }
}