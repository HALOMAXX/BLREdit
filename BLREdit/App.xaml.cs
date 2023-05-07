using BLREdit.API.InterProcess;
using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.API.Utils;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.Import;
using BLREdit.Model.BLR;
using BLREdit.Model.Proxy;
using BLREdit.Properties;
using BLREdit.UI;
using BLREdit.UI.Views;
using BLREdit.UI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BLREdit;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public const string CurrentVersion = "v0.11.1";
    public const string CurrentVersionTitle = "Crash fix when patching";
    public const string CurrentOwner = "HALOMAXX";
    public const string CurrentRepo = "BLREdit";
    public static RangeObservableCollection<ProxyModuleModel> AvailableProxyModules { get; } = new();
    


    

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        string[] argList = e.Args;
        Dictionary<string, string> argDict = new();

        for (var i = 0; i < argList.Length; i++)
        {
            string name = argList[i];
            string value = "";
            if (!name.StartsWith("-")) continue;
            if (i+1 < argList.Length && !argList[i + 1].StartsWith("-")) value = argList[i+1];
            argDict.Add(name, value);
        }

        if (argDict.TryGetValue("-server", out string configFile))
        {
            try
            {
                IOResources.SpawnConsole();
                Console.Title = $"[Watchdog(BLREdit:{CurrentVersion})]: Starting!";

                App.AvailableProxyModuleCheck();

                string command = "blredit://start-server/" + Uri.EscapeDataString(File.ReadAllText(configFile));

                BLREditPipe.ProcessArgs(new string[] { command });

                Console.WriteLine("Press Q to Exit and Kill all Server Processes");
                while (Console.ReadKey().Key != ConsoleKey.Q) { }
            }
            catch (Exception error)
            { 
                LoggingSystem.MessageLog($"failed to start server:\n{error}"); 
            }

            BLRProcess.KillAll();

            Shutdown();
            return;
        }

        if (BLREditPipe.ForwardLaunchArgs(argList))
        {
            Shutdown();
            return;
        }

        new MainWindow(argList).Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        //TODO Save All data to disk

        IsRunning = false;
        foreach (var t in AppThreads)
        {
            try
            {
                if (t?.IsAlive ?? false)
                {
                    t?.Abort();
                }
            }
            catch { }
        }

        IOResources.SerializeFile("Clients.json", BLRClientModel.Clients);
        IOResources.SerializeFile("Servers.json", BLRServerModel.Servers);

        LoggingSystem.Log("BLREdit Closed!");
    }

    private static void CreateAllDirectories()
    {
        Directory.CreateDirectory("logs");
        Directory.CreateDirectory("logs\\BLREdit");
        Directory.CreateDirectory("logs\\Client");
        Directory.CreateDirectory("logs\\Proxy");

        Directory.CreateDirectory("Profiles");

        Directory.CreateDirectory(IOResources.UPDATE_DIR);
        Directory.CreateDirectory("downloads");
        Directory.CreateDirectory("downloads\\localizations");
    }

    public App()
    {
        Directory.SetCurrentDirectory(BLREditEnvironment.BLREditLocation);

        Trace.Listeners.Add(new TextWriterTraceListener($"logs\\BLREdit\\{DateTime.Now:MM.dd.yyyy(HHmmss)}.log", "loggingListener"));
        
        Trace.AutoFlush = true;

        LoggingSystem.Log($"BLREdit {CurrentVersion} {IOResources.WineVersion} {CultureInfo.CurrentCulture.Name} @{BLREditLocation} or {Directory.GetCurrentDirectory()}");

        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        CreateAllDirectories();

        foreach (var file in Directory.EnumerateFiles("logs\\BLREdit"))
        {
            var fileInfo = new FileInfo(file);
            var creationDelta = DateTime.Now - fileInfo.CreationTime;
            if (creationDelta.Days >= 1)
            {
                fileInfo.Delete();
            }
        }

        if (BLREditSettings.Settings.SelectedCulture is null)
        {
            BLREditSettings.Settings.SelectedCulture = CultureInfo.CurrentCulture;
        }

        System.Threading.Thread.CurrentThread.CurrentUICulture = BLREditSettings.Settings.SelectedCulture;
    }

    void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LoggingSystem.Log($"[Unhandled]: {e.ExceptionObject}");
        Environment.Exit(666);
    }

    public static bool VersionCheck()
    {
        LoggingSystem.Log("Running Version Check!");

        try
        {
            var task = GitHubClient.GetLatestRelease(CurrentOwner, CurrentRepo);
            task.Wait();
            BLREditLatestRelease = task.Result;
            if (BLREditLatestRelease is null) { LoggingSystem.Log("Can't connect to github to check for new Version"); return false; }
            LoggingSystem.Log($"Newest Version: {BLREditLatestRelease.TagName} of {BLREditLatestRelease.Name} vs Current: {CurrentVersion} of {CurrentVersionTitle}");

            var remoteVersion = new Version("".Substring(1));
            var localVersion = new Version(CurrentVersion.Substring(1));

            bool newVersionAvailable = remoteVersion > localVersion;
            bool assetFolderMissing = !Directory.Exists(IOResources.ASSET_DIR);

            LoggingSystem.Log($"New Version Available:{newVersionAvailable} AssetFolderMissing:{assetFolderMissing}");

            //TODO Run app updater
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Failed to Update to Newest Version\n{error}"); return false; }
        return false;
    }

    private static async Task<ProxyModuleModel[]> GetAvailableProxyModules()
    {
        LoggingSystem.Log("Downloading AvailableProxyModule List!");
        try
        {
            if (await GitHubClient.GetFile(CurrentOwner, CurrentRepo, "refactoring", "Resources/v2/ProxyModuleList.json") is GitHubFile file)
            {
                var moduleList = IOResources.Deserialize<ProxyModuleModel[]>(file.DecodedContent);
                LoggingSystem.Log("Finished Downloading AvailableProxyModule List!");
                return moduleList;
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Can't get ProxyModule list from Github\n{error}"); }
        return Array.Empty<ProxyModuleModel>();
    }

    private static async Task<Dictionary<string, string>> GetAvailableLocalizations()
    {
        LoggingSystem.Log("Downloading AvailableLocalization List!");
        try
        {
            if (await GitHubClient.GetFile(CurrentOwner, CurrentRepo, "master", "Resources/Localizations.json") is GitHubFile file)
            {
                var localizations = IOResources.Deserialize<Dictionary<string, string>>(file.DecodedContent);
                LoggingSystem.Log("Finished Downloading AvailableLocalization List!");
                return localizations;
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Can't get Localization list from Github\n{error}"); }
        return new();
    }

    private static async Task<List<BLRServerModel>> GetDefaultServers()
    {
        LoggingSystem.Log($"Downloading Default Server List!");
        try
        {
            if (await GitHubClient.GetFile(CurrentOwner, CurrentRepo, "master", "Resources/ServerList.json") is GitHubFile file)
            {
                var serverList = IOResources.Deserialize<List<BLRServerModel>>(file.DecodedContent);
                LoggingSystem.Log("Finished Downloading Server List!");
                if (serverList is not null)
                { return serverList; }
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Can't get server list from Github using internal defaults\n{error}"); }
        return new() 
        {
            //TODO: Here you can Add new Default servers which should be intigrated into the EXE
            new() { ServerAddress = "mooserver.ddns.net", ServerPort = 7777 }, //MagiCow | Moo Server
            new() { ServerAddress = "blrevive.northamp.fr", ServerPort = 7777, InfoPort = 80 }, //ALT Server
            new() { ServerAddress = "aegiworks.com", ServerPort = 7777 }, //VIVIGAR Server
            new() { ServerAddress = "blr.akoot.me", ServerPort = 7777 }, //Akoot Server
            new() { ServerAddress = "blr.753z.net", ServerPort = 7777 }, //IKE753Z Server (not active anymore)
            new() { ServerAddress = "localhost", ServerPort = 7777 } //Local User Server
        };
    }

    public static void RuntimeCheck()
    {
        LoggingSystem.Log("Checking for Runtime Libraries!");
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

    static bool checkedForModules = false;
    public static void AvailableProxyModuleCheck()
    {
        if (checkedForModules) return;
        checkedForModules = true;
        var availableModuleCheck = Task.Run(GetAvailableProxyModules);
        var availableLocalizations = Task.Run(GetAvailableLocalizations);
        var defaultServers = Task.Run(GetDefaultServers);
        Task.WaitAll(availableModuleCheck, availableLocalizations, defaultServers);

        List<BLRServerModel> servers = new();

        foreach (var server in defaultServers.Result) 
        {
            if (!BLRServerModel.Servers.Contains(server))
            { 
                servers.Add(server);
            }
        }
        BLRServerModel.Servers.AddRange(servers);

        AvailableLocalizations = availableLocalizations.Result;
        var modules = availableModuleCheck.Result;
        modules ??= Array.Empty<ProxyModuleModel>();

        AvailableProxyModules.AddRange(modules);

        bool isAvailableLocale = false;
        foreach (var locale in AvailableLocalizations)
        {
            if (locale.Key == BLREditSettings.Settings.SelectedCulture.Name) isAvailableLocale = true;
        }
        if (!isAvailableLocale)
        { 
            BLREditSettings.Settings.SelectedCulture = CultureInfo.GetCultureInfo("en-US");
        }
    }

    public static void DownloadLocalization()
    {
        AvailableProxyModuleCheck();
        var current = CultureInfo.CurrentCulture;
        if (current != DefaultCulture)
        {
            if (Directory.Exists(current.Name))
            {
                var manifestFileName = $"{current.Name}\\manifest.hash";
                if (File.Exists(manifestFileName))
                {
                    string hash = File.ReadAllText(manifestFileName);
                    if (AvailableLocalizations.TryGetValue(current.Name, out string availableHash))
                    {
                        if (!hash.Equals(availableHash))
                        {
                            DownloadLocale(current.Name);
                        }
                    }
                }
            }
            else
            {
                if (AvailableLocalizations.TryGetValue(current.Name, out string _))
                {
                    DownloadLocale(current.Name);
                }
            }
        }
    }

    private static void DownloadLocale(string locale)
    {
        string targetZip = $"downloads\\localizations\\{locale}.zip";
        IOResources.DownloadFile($"https://github.com/{CurrentOwner}/{CurrentRepo}/raw/master/Resources/Localizations/{locale}.zip", targetZip);
        if (Directory.Exists(locale)) { Directory.Delete(locale, true); Directory.CreateDirectory(locale); }
        ZipFile.ExtractToDirectory(targetZip, $"{locale}");
    }
}
