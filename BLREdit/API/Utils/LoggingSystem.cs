using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BLREdit
{
    public static class LoggingSystem
    {
#if DEBUG
        public static bool IsDebuggingEnabled = true;
#else
        public static bool IsDebuggingEnabled = false;
#endif
        public static Stopwatch LogInfo(string info, string newLine = "\n")
        {
            if (IsDebuggingEnabled)
            {
                var now = DateTime.Now;
                Trace.Write("[" + now + "]Info:" + info + newLine);
                return Stopwatch.StartNew();
            }
            return null;
        }

        public static void LogInfoAppend(Stopwatch watch, string finish = "")
        {
            if (IsDebuggingEnabled && watch != null)
            {
                Trace.WriteLine(finish + " Done! in " + watch.ElapsedMilliseconds + "ms");
            }
        }

        public static void LogWarning(string info)
        {
            if (IsDebuggingEnabled)
            {
                var now = DateTime.Now;
                Trace.WriteLine("[" + now + "]Warning:" + info);
            }
        }

        public static void LogError(string info)
        {
            if (IsDebuggingEnabled)
            {
                var now = DateTime.Now;
                Trace.WriteLine("[" + now + "]Error:" + info);
            }
        }

        public static string ObjectToTextWall<T>(T obj)
        {
            StringBuilder sb = new StringBuilder();
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
}
