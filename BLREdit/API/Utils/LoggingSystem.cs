using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

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

        switch (result)
        {
            case MessageBoxResult.OK:
            case MessageBoxResult.Yes:
                return true;
            case MessageBoxResult.None:
            case MessageBoxResult.Cancel:
            case MessageBoxResult.No:
                return false;
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ObjectToTextWall<T>(T obj)
    {
        return IOResources.Serialize(obj);
    }
}
