using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BLREdit
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static bool IsNewVersionAvailable { get; private set; } = false;
        public static bool IsBaseRuntimeMissing { get; private set; } = true;
        public static bool IsUpdateRuntimeMissing { get; private set; } = true;

        public App()
        {
            File.Delete("log.txt");
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt", "loggingListener"));
            Trace.AutoFlush = true;
            LoggingSystem.LogInfo("BLREdit Starting!");

            Initialize().Wait();
            //var task = GitlabClient.GetReleases("modules/loadout-manager", "blrevive");
            var task = GitHubClient.GetObjectFromFile<GitHubFile>(CurrentRepo, CurrentOwner, "master", "README.md");
            task.Wait();
            var release = task.Result;
            RuntimeCheck();
            ImportSystem.Initialize();
        }

        public async Task Initialize()
        {
            IsNewVersionAvailable = await VersionCheck();
        }

        public const string CurrentVersion = "v0.6.1";
        const string CurrentVersionName = "BLREdit Bug fix & Performance";
        public const string CurrentOwner = "HALOMAXX";
        public const string CurrentRepo = "BLREdit";
        public static async Task<bool> VersionCheck()
        {
            try
            {
                var release = await GitHubClient.GetLatestRelease(CurrentRepo, CurrentOwner);
                LoggingSystem.LogInfo($"Newest Version: {release.tag_name} of {release.name} vs Current: {CurrentVersion} of {CurrentVersionName}");

                string[] remoteVersionParts = release.tag_name.Split('v');
                remoteVersionParts = remoteVersionParts[remoteVersionParts.Length - 1].Split('.');

                string[] currentVersionParts = CurrentVersion.Split('v');
                currentVersionParts = currentVersionParts[currentVersionParts.Length - 1].Split('.');

                for (int i = 0; i < currentVersionParts.Length && i < remoteVersionParts.Length; i++)
                {
                    int? remote = null;
                    int? current = null;
                    try
                    {
                        remote = int.Parse(remoteVersionParts[i]);
                        current = int.Parse(currentVersionParts[i]);
                    }
                    catch
                    {
                        LoggingSystem.LogWarning("Can't determine version differences!");
                    }
                    if (remote != null && current != null)
                    {
                        if (remote > current)
                        {
                            return true;
                        }
                        if (current > remote)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        LoggingSystem.LogWarning("Can't determine version differences!");
                    }
                }

            }
            catch 
            { LoggingSystem.LogWarning("Can't connect to github to check for new Version"); }
            return false;
        }

        public static void RuntimeCheck()
        {
            var x86 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Installer\Dependencies\{33d1fd90-4274-48a1-9bc1-97e33d9c2d6f}", "Version", "-1");
            var x86Update = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Installer\Dependencies\Microsoft.VS.VC_RuntimeAdditional_x86,v11", "Version", "-1");
            if (x86 is string VC32Bit && x86Update is string VC32BitUpdate4)
            {
                IsBaseRuntimeMissing = (VC32Bit != "11.0.61030.0");
                IsUpdateRuntimeMissing = (VC32BitUpdate4 != "11.0.61030");

                if (!IsBaseRuntimeMissing && !IsUpdateRuntimeMissing)
                {
                    LoggingSystem.LogInfo("Both VC++ 2012 Runtimes are installed for BLRevive!");
                }
            }
        }
    }
}
