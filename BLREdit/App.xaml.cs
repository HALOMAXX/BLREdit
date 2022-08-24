using Octokit;

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

            //TestREST().GetAwaiter().GetResult();

            LoggingSystem.LogInfo("BLREdit Starting!");
            VersionCheck();
            RuntimeCheck();
            ImportSystem.Initialize();
        }

        public static async Task TestREST()
        {
            MagiCowsProfile[] profiles = await MagiCowClient.GetAllPlayers();
            foreach (MagiCowsProfile profile in profiles)
            {
                LoggingSystem.LogInfo(profile.ToString());
            }
        }

        public const string CurrentVersion = "v0.6.1";
        const string CurrentVersionName = "BLREdit Bug fix & Performance";
        public const string CurrentOwner = "HALOMAXX";
        public const string CurrentRepo = "BLREdit";
        public static void VersionCheck()
        {
            try
            {
                GitHubClient client = new(new ProductHeaderValue("BLREdit"));
                var releases = client.Repository.Release.GetAll(CurrentOwner, CurrentRepo);
                releases.Wait();
                var latest = releases.Result[0];
                LoggingSystem.LogInfo($"Newest Version: {latest.TagName} of {latest.Name} vs Current: {CurrentVersion} of {CurrentVersionName}");

                string[] remoteVersionParts = latest.TagName.Split('v');
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
                            IsNewVersionAvailable = true;
                            return;
                        }
                        if (current > remote)
                        {
                            return;
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
