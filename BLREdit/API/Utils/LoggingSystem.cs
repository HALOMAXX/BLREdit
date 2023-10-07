using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace BLREdit;

public static class LoggingSystem
{
    private static ThreadLocal<Stopwatch> ThreadLocalStopwatch { get; } = new();
    private static readonly double Frequency = Stopwatch.Frequency * 0.001d;


    public static void Log(string info)
    {
        Trace.Write($"[{DateTime.Now}]: {info}\n");
    }

    public static void ResetWatch()
    {
        if (ThreadLocalStopwatch.IsValueCreated) { ThreadLocalStopwatch.Value.Restart(); } else { ThreadLocalStopwatch.Value = Stopwatch.StartNew(); }
    }

    public static void PrintElapsedTime(string message = "{0}ms")
    {
        if (ThreadLocalStopwatch.IsValueCreated)
        {
            Log(string.Format(message, ThreadLocalStopwatch.Value.ElapsedTicks / Frequency));
        }
    }

    public static void MessageLog(string info)
    {
        Log(info);
        MessageBox.Show(info);
    }

    public static string ObjectToTextWall<T>(T obj)
    {
        return IOResources.Serialize(obj);
    }
}
