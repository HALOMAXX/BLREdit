using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.API.Utils;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.Import;
using BLREdit.UI.Views;

using PeNet.Asn1;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace BLREdit;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public const string CurrentVersion = "v0.7.3";
    public const string CurrentVersionTitle = "BLREdit Removed UDP Ping";
    public const string CurrentOwner = "HALOMAXX";
    public const string CurrentRepo = "BLREdit";

    public static bool IsNewVersionAvailable { get; private set; } = false;
    public static bool IsBaseRuntimeMissing { get; private set; } = true;
    public static bool IsUpdateRuntimeMissing { get; private set; } = true;
    public static GitHubRelease BLREditLatestRelease { get; private set; } = null;
    public static VisualProxyModule[] AvailableProxyModules { get; private set; }

    private const string LogFile = "log.txt";
    public static readonly string BLREditLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

    public App()
    {
        Directory.SetCurrentDirectory(BLREditLocation);

        File.Delete(LogFile);
        Trace.Listeners.Add(new TextWriterTraceListener(LogFile, "loggingListener"));
        Trace.AutoFlush = true;
        LoggingSystem.Log($"BLREdit Starting! @{BLREditLocation} or {Directory.GetCurrentDirectory()}");

        if (Environment.GetCommandLineArgs().Contains("-package"))
        {
            PackageAssets();
            Application.Current.Shutdown();
        }

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
        UI.MainWindow.GameClients = IOResources.DeserializeFile<ObservableCollection<BLRClient>>($"GameClients.json") ?? new();
        UI.MainWindow.ServerList = IOResources.DeserializeFile<ObservableCollection<BLRServer>>($"ServerList.json") ?? new();

        LoggingSystem.Log($"Validating Client List {UI.MainWindow.GameClients.Count}");
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

    public static void PackageAssets()
    {
        Directory.CreateDirectory(IOResources.UPDATE_DIR);
        currentExe.Info.CopyTo(newExe.Info.FullName);

        ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}", assetZip.Info.FullName);
        ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.JSON_DIR}", jsonZip.Info.FullName);
        ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.DLL_DIR}", dllsZip.Info.FullName);
        ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.TEXTURE_DIR}", texturesZip.Info.FullName);
        ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.PREVIEW_DIR}", crosshairsZip.Info.FullName);
        ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.PATCH_DIR}", patchesZip.Info.FullName);
    }

    public static async Task<RepositoryProxyModule[]> Initialize()
    {
        IsNewVersionAvailable = await VersionCheck();
        return await GetAvailableProxyModules();
    }

    private static Dictionary<FileInfoExtension, string> DownloadLinks = new();

    private readonly static FileInfoExtension currentExe = new($"BLREdit.exe");
    private readonly static FileInfoExtension backupExe = new($"{IOResources.UPDATE_DIR}BLREdit.exe.bak");
    //Need to download 
    private readonly static FileInfoExtension newExe = new($"{IOResources.UPDATE_DIR}BLREdit.exe");
    private readonly static FileInfoExtension assetZip = new($"{IOResources.UPDATE_DIR}Assets.zip");
    private readonly static FileInfoExtension jsonZip = new($"{IOResources.UPDATE_DIR}json.zip");
    private readonly static FileInfoExtension dllsZip = new($"{IOResources.UPDATE_DIR}dlls.zip");
    private readonly static FileInfoExtension texturesZip = new($"{IOResources.UPDATE_DIR}textures.zip");
    private readonly static FileInfoExtension crosshairsZip = new($"{IOResources.UPDATE_DIR}crosshairs.zip");
    private readonly static FileInfoExtension patchesZip = new($"{IOResources.UPDATE_DIR}patches.zip");

    public static async Task<bool> VersionCheck()
    {
        Directory.CreateDirectory(IOResources.UPDATE_DIR);

        try
        {
            BLREditLatestRelease = await GitHubClient.GetLatestRelease(CurrentOwner, CurrentRepo);
            if (BLREditLatestRelease is null) { LoggingSystem.Log("Can't connect to github to check for new Version"); return false; }
            LoggingSystem.Log($"Newest Version: {BLREditLatestRelease.tag_name} of {BLREditLatestRelease.name} vs Current: {CurrentVersion} of {CurrentVersionTitle}");

            var remoteVersion = CreateVersion(BLREditLatestRelease.tag_name);
            var localVersion = CreateVersion(CurrentVersion);

            bool newVersionAvailable = remoteVersion > localVersion;
            bool assetFolderMissing = !Directory.Exists(IOResources.ASSET_DIR);

            LoggingSystem.Log($"New Version Available:{newVersionAvailable}\nAssetFolderMissing:{assetFolderMissing}");

            //TODO Add function to Package Assets for upload

            if (BLREditLatestRelease is not null)
            {
                foreach (var asset in BLREditLatestRelease.assets)
                {
                    if (asset.name.StartsWith(newExe.Name) && asset.name.EndsWith(newExe.Info.Extension))
                    { DownloadLinks.Add(newExe, asset.browser_download_url); }

                    if (asset.name.StartsWith(assetZip.Name) && asset.name.EndsWith(assetZip.Info.Extension))
                    { DownloadLinks.Add(assetZip, asset.browser_download_url); }
                    
                    if (asset.name.StartsWith(jsonZip.Name) && asset.name.EndsWith(jsonZip.Info.Extension))
                    { DownloadLinks.Add(jsonZip, asset.browser_download_url); }
                    
                    if (asset.name.StartsWith(dllsZip.Name) && asset.name.EndsWith(dllsZip.Info.Extension))
                    { DownloadLinks.Add(dllsZip, asset.browser_download_url); }
                    
                    if (asset.name.StartsWith(texturesZip.Name) && asset.name.EndsWith(texturesZip.Info.Extension))
                    { DownloadLinks.Add(texturesZip, asset.browser_download_url); }
                    
                    if (asset.name.StartsWith(crosshairsZip.Name) && asset.name.EndsWith(crosshairsZip.Info.Extension))
                    { DownloadLinks.Add(crosshairsZip, asset.browser_download_url); }

                    if (asset.name.StartsWith(patchesZip.Name) && asset.name.EndsWith(patchesZip.Info.Extension))
                    { DownloadLinks.Add(patchesZip, asset.browser_download_url); }
                }

                if (newVersionAvailable && assetFolderMissing)
                {
                    DownloadAssetFolder();

                    UpdateEXE();
                }
                else if (newVersionAvailable && !assetFolderMissing)
                {
                    UpdateAssetPack(jsonZip, $"{IOResources.ASSET_DIR}{IOResources.JSON_DIR}");
                    UpdateAssetPack(dllsZip, $"{IOResources.ASSET_DIR}{IOResources.DLL_DIR}");
                    UpdateAssetPack(texturesZip, $"{IOResources.ASSET_DIR}{IOResources.TEXTURE_DIR}");
                    UpdateAssetPack(crosshairsZip, $"{IOResources.ASSET_DIR}{IOResources.PREVIEW_DIR}");
                    UpdateAssetPack(patchesZip, $"{IOResources.ASSET_DIR}{IOResources.PATCH_DIR}");

                    UpdateEXE();
                }
                else if (!newVersionAvailable && assetFolderMissing)
                {
                    DownloadAssetFolder();
                }
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Failed to Update to Newest Version\n{error}"); }
        return false;
    }

    private static void UpdateEXE()
    {
        if (DownloadLinks.TryGetValue(newExe, out string exeDL))
        {
            if (newExe.Info.Exists) { newExe.Info.Delete(); }
            IOResources.WebClient.DownloadFile(exeDL, newExe.Info.FullName);
            if (backupExe.Info.Exists) { backupExe.Info.Delete(); }
            currentExe.Info.MoveTo(backupExe.Info.FullName);
            newExe.Info.CopyTo(currentExe.Info.FullName);

            Process.Start("BLREdit.exe");
            Current.Shutdown();
        }
        else
        { LoggingSystem.Log("No new EXE Available!"); }
    }

    private static void DownloadAssetFolder()
    {
        if (DownloadLinks.TryGetValue(assetZip, out string assetDL))
        {
            if (assetZip.Info.Exists) { assetZip.Info.Delete(); }
            IOResources.WebClient.DownloadFile(assetDL, assetZip.Info.FullName);
            ZipFile.ExtractToDirectory(assetZip.Info.FullName, IOResources.ASSET_DIR);
        }
        else
        { LoggingSystem.MessageLog("No Asset folder for download available!"); }
    }

    private static void UpdateAssetPack(FileInfoExtension pack, string targetFolder)
    {
        if (DownloadLinks.TryGetValue(pack, out string dl))
        {
            if (pack.Info.Exists) { pack.Info.Delete(); }
            IOResources.WebClient.DownloadFile(dl, pack.Info.FullName);
            if (Directory.Exists(targetFolder)) { Directory.Delete(targetFolder, true); }
            ZipFile.ExtractToDirectory(pack.Info.FullName, targetFolder);
        }
        else
        { LoggingSystem.Log($"No {pack.Info.Name} for download available!"); }
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
        catch (Exception error)
        { LoggingSystem.MessageLog($"Can't get ProxyModule list from Github\n{error}"); }
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
