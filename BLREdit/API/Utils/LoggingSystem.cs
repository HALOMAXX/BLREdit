using System;
using System.Diagnostics;
using System.Windows;

namespace BLREdit;

public static class LoggingSystem
{
    public static void Log(string info)
    {
        Trace.Write($"[{DateTime.Now}]: {info}\n");
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
