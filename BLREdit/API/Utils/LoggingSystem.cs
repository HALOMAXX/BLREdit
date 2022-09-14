using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BLREdit;

public static class LoggingSystem
{
    public static void Log(string info)
    {
        var now = DateTime.Now;
        Trace.Write($"[{now}]: {info}\n");
    }

    public static string ObjectToTextWall<T>(T obj)
    {
        StringBuilder sb = new();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        var fields = obj.GetType().GetFields(flags);
        var props = obj.GetType().GetProperties(flags);
        sb.Append('{');
        foreach (FieldInfo field in fields)
        {
            sb.AppendFormat(" {0}:{1},", field.Name, field.GetValue(obj));
        }
        foreach (PropertyInfo prop in props)
        {
            sb.AppendFormat(" {0}:{1},", prop.Name, prop.GetValue(obj));
        }
        sb.Append(" }");
        return sb.ToString();
    }
}
