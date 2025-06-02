using BLREdit.Export;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Shapes;

namespace BLREdit;

public static class LoggingSystem
{
    private static ThreadLocal<Stopwatch> ThreadLocalStopwatch { get; } = new();
    private static readonly double Frequency = Stopwatch.Frequency * 0.001d;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(string info)
    {
        Trace.Write($"[{DateTime.Now}]: {info}\n");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetWatch()
    {
        if (ThreadLocalStopwatch.IsValueCreated) { ThreadLocalStopwatch.Value.Restart(); } else { ThreadLocalStopwatch.Value = Stopwatch.StartNew(); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrintElapsedTime(string message = "{0}ms")
    {
        if (ThreadLocalStopwatch.IsValueCreated)
        {
            Log(string.Format(message, ThreadLocalStopwatch.Value.ElapsedTicks / Frequency));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MessageLog(string message, string title, MessageBoxButton buttonType = MessageBoxButton.OK)
    {
        var result = MessageBox.Show(message, title, buttonType);
        Log($"[MessageBox]({title})({result}): {message}");

        return result switch
        {
            MessageBoxResult.OK or MessageBoxResult.Yes => true,
            MessageBoxResult.None or MessageBoxResult.Cancel or MessageBoxResult.No => false,
            _ => false,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MessageLogClipboard(string message, string title)
    {
        Log($"[MessageBox]({title})(OK): {message}");
        Trace.Flush();

        IOResources.FilesToClipboard.Add(App.CurrentLogFile.FullName);
        Thread.Sleep(100);

        MessageBox.Show(message, title, MessageBoxButton.OK);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FatalLog(string message, [CallerFilePath] string? path = null, [CallerLineNumber] int line = 0)
    {
        Log($"[FatalError]({path}:{line}): {message}\n\nStacktrace:\n{Environment.StackTrace}");
        Trace.Flush();

        IOResources.FilesToClipboard.Add(App.CurrentLogFile.FullName);
        Thread.Sleep(100);

        MessageBox.Show("A Fatal error has occured. Please DM the latest logfile to @HKN1 or post it on the BLRevive discord!\n\nThe Latest log file has been copied to your Clipboard!\nIt can also be found in the logs->BLREdit folder next the BLREdit Executable\nThe latest ", "FatalError", MessageBoxButton.OK);
        Environment.Exit(69);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogNull([CallerFilePath] string? path = null, [CallerLineNumber] int line = 0)
    {
        Log($"[NullError]: {path}:{line}\n\tStacktrace:\n{Environment.StackTrace}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ObjectToTextWall<T>(T obj)
    {
        return IOResources.Serialize(obj);
    }

    public static string GetSourceLocation([CallerFilePath] string? path = null, [CallerLineNumber] int line = 0)
    {
        return $"{path}:{line}";
    }
}
