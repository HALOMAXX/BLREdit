using Octokit;

using System.Diagnostics;
using System.Windows;

namespace BLREdit
{
    
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static bool IsNewVersionAvailable = false;
        public static bool IsBaseRuntimeMissing = false;
        public static bool IsUpdateRuntimeMissing = false;

        public App()
        {
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt", "loggingListener"));
            Trace.AutoFlush = true;
            LoggingSystem.LogInfo("BLREdit Starting!");
            VersionCheck();

        }
        const string CurrentVersion = "v0.0.7";
        const string CurrentVersionName = "BLREdit QoL Update 2";
        public const string CurrentOwner = "HALOMAXX";
        public const string CurrentRepo = "BLREdit";
        public static void VersionCheck()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("BLREdit"));
            var releases = client.Repository.Release.GetAll(CurrentOwner, CurrentRepo);
            releases.Wait();
            var latest = releases.Result[0];
            LoggingSystem.LogInfo("Newest Version: " + latest.TagName + " of " + latest.Name + " vs Current: " + CurrentVersion + " of " + CurrentVersionName);
                
            string[] remoteVersionParts = latest.TagName.Split('v');
            remoteVersionParts = remoteVersionParts[1].Split('.');

            string[] currentVersionParts = CurrentVersion.Split('v');
            currentVersionParts = currentVersionParts[1].Split('.');

            for (int i = 0; i < currentVersionParts.Length && i < remoteVersionParts.Length; i++)
            {
                int? remote = null;
                int? current = null;
                try
                {
                    remote = int.Parse(remoteVersionParts[i]);
                    current = int.Parse(currentVersionParts[i]);
                } catch
                {
                    LoggingSystem.LogWarning("Can't determine version differences!");
                }
                if (remote != null && current != null)
                {
                    if (remote > current)
                    {
                        IsNewVersionAvailable = true;
                        return;
                    }
                }
                else
                {
                    LoggingSystem.LogWarning("Can't determine version differences!");
                }
            }
        }

        public static void RuntimeCheck()
        {
            var x86 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Installer\Dependencies\{33d1fd90-4274-48a1-9bc1-97e33d9c2d6f}", "Version", "-1");
            var x86Update = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Installer\Dependencies\Microsoft.VS.VC_RuntimeAdditional_x86,v11", "Version", "-1");
            if (x86 is string VC32Bit && x86Update is string VC32BitUpdate4)
            {
                if (VC32Bit == "11.0.61030.0" && VC32BitUpdate4 == "11.0.61030")
                {
                    LoggingSystem.LogInfo("Both VC++ 2012 Runtimes are installed for BLRevive!");
                    return;
                }
                else
                { 
                    var info = new InfoPopups.DownloadRuntimes();
                    if (VC32Bit == "11.0.61030.0")
                    {
                        IsBaseRuntimeMissing = true;
                        info.Link2012.IsEnabled = false;
                        info.Link2012Content.Text = "Microsoft Visual C++ 2012(x86) is already installed!";
                    }
                    if (VC32BitUpdate4 == "11.0.61030")
                    {
                        IsUpdateRuntimeMissing = true;
                        info.Link2012Update4.IsEnabled = false;
                        info.Link2012Updatet4Content.Text = "Microsoft Visual C++ 2012 Update 4 is already installed!";
                    }
                    info.ShowDialog();
                }
            }
        }
    }
}
