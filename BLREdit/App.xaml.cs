using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.Import;
using BLREdit.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BLREdit;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public const string CurrentVersion = "v0.7.0";
    public const string CurrentVersionTitle = "BLREdit Big Feature Update";
    public const string CurrentOwner = "HALOMAXX";
    public const string CurrentRepo = "BLREdit";

    public static bool IsNewVersionAvailable { get; private set; } = false;
    public static bool IsBaseRuntimeMissing { get; private set; } = true;
    public static bool IsUpdateRuntimeMissing { get; private set; } = true;

    public static VisualProxyModule[] AvailableProxyModules { get; private set; }

    public App()
    {
        
        File.Delete("log.txt");
        Trace.Listeners.Add(new TextWriterTraceListener("log.txt", "loggingListener"));
        Trace.AutoFlush = true;
        LoggingSystem.Log("BLREdit Starting!");

        var task = Initialize();
        task.Wait();
        var modules = task.Result;
        AvailableProxyModules = new VisualProxyModule[modules.Length];
        for (int i = 0; i < modules.Length; i++)
        {
            AvailableProxyModules[i] = new VisualProxyModule() { RepositoryProxyModule = modules[i] };
        }

        RuntimeCheck();
        ImportSystem.Initialize();

        LoggingSystem.Log("Loading Client List");
        UI.MainWindow.GameClients = IOResources.DeserializeFile<ObservableCollection<BLRClient>>("GameClients.json") ?? new();
        UI.MainWindow.ServerList = IOResources.DeserializeFile<ObservableCollection<BLRServer>>("ServerList.json") ?? new();

        LoggingSystem.Log("Validating Client List");
        for (int i = 0; i < UI.MainWindow.GameClients.Count; i++)
        {
            if (!UI.MainWindow.GameClients[i].OriginalFileValidation())
            { UI.MainWindow.GameClients.RemoveAt(i); i--; }
            else
            { 
                LoggingSystem.Log($"{UI.MainWindow.GameClients[i]} has {UI.MainWindow.GameClients[i].InstalledModules.Count} installed modules");
                if (UI.MainWindow.GameClients[i].InstalledModules.Count > 0)
                { 
                    UI.MainWindow.GameClients[i].InstalledModules = new System.Collections.ObjectModel.ObservableCollection<ProxyModule>(UI.MainWindow.GameClients[i].InstalledModules.Distinct(new ProxyModuleComparer())); 
                    LoggingSystem.Log($"{UI.MainWindow.GameClients[i]} has {UI.MainWindow.GameClients[i].InstalledModules.Count} installed modules"); 
                }
            }
        }
    }

    public static async Task<RepositoryProxyModule[]> Initialize()
    {
        IsNewVersionAvailable = await VersionCheck();
        return await GetAvailableProxyModules();
    }

    public static async Task<bool> VersionCheck()
    {
        try
        {
            var release = await GitHubClient.GetLatestRelease(CurrentOwner, CurrentRepo);
            if (release is null) { LoggingSystem.Log("Can't connect to github to check for new Version"); return false; }
            LoggingSystem.Log($"Newest Version: {release.tag_name} of {release.name} vs Current: {CurrentVersion} of {CurrentVersionTitle}");

            var remoteVersion = CreateVersion(release.tag_name);
            var localVersion = CreateVersion(CurrentVersion);

            if (localVersion >= remoteVersion)
            { return false; }
            else
            { return true; }

        }
        catch
        { LoggingSystem.Log("Can't connect to github to check for new Version"); }
        return false;
    }

    private static int CreateVersion(string versionTag)
    {
        var splitTag = versionTag.Split('v');
        var stringVersionParts = splitTag[splitTag.Length - 1].Split('.');
        int version = 0;
        int multiply = 1;
        for (int i = stringVersionParts.Length - 1; i > 0; i--)
        {
            if (int.TryParse(stringVersionParts[i], out int result))
            {
                version += result * (multiply);
            }
            multiply *= 10;
        }
        return version;
    }

    public static async Task<RepositoryProxyModule[]> GetAvailableProxyModules()
    {
        try
        {
            var file = await GitHubClient.GetFile(CurrentOwner, CurrentRepo, "master", "Resources/ProxyModules.json");
            if (file is null) return Array.Empty<RepositoryProxyModule>();
            return IOResources.Deserialize<RepositoryProxyModule[]>(file.decoded_content);
        }
        catch
        {
            LoggingSystem.Log("Can't connect to github");
        }
        return Array.Empty<RepositoryProxyModule>();
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
                LoggingSystem.Log("Both VC++ 2012 Runtimes are installed for BLRevive!");
            }
        }
    }
}
