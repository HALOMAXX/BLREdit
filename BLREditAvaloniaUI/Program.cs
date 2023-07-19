using Avalonia;
using Avalonia.ReactiveUI;
using BLREdit.Core.Utils;
using System;
using System.Diagnostics;

namespace BLREdit;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) 
    {
        CreateAllDirectories();

        Trace.Listeners.Add(new TextWriterTraceListener(IOResources.MakePathCompatible($"logs\\BLREdit\\{DateTime.Now:yyyy.MM.dd(HH-mm-ss)}.log"), "logFileListener"));
        Trace.AutoFlush = true;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

    private static void CreateAllDirectories()
    {
        //TODO Create all folders

        IOResources.CreateDirectory("logs");
        IOResources.CreateDirectory("logs\\BLREdit");
        IOResources.CreateDirectory("logs\\Client");
        IOResources.CreateDirectory("logs\\Proxy");
    }


}