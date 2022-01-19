using System.Diagnostics;
using System.Windows;

namespace BLREdit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt", "loggingListener"));
            Trace.AutoFlush = true;
            LoggingSystem.LogInfo("BLREdit Starting!");
        }
    }
}
