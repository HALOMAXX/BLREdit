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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BLREdit;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public const string CurrentVersion = "v0.7.9";
    public const string CurrentVersionTitle = "More Scrollbars";
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

        InitFiles();

        File.Delete(LogFile);
        Trace.Listeners.Add(new TextWriterTraceListener(LogFile, "loggingListener"));
        Trace.AutoFlush = true;
        LoggingSystem.Log($"BLREdit Starting! @{BLREditLocation} or {Directory.GetCurrentDirectory()}");

        if (Environment.GetCommandLineArgs().Contains("-package"))
        {
            try
            {
                PackageAssets();
            }
            catch (Exception error) { LoggingSystem.MessageLog($"failed to package release:\n{error}"); }
            var result = MessageBox.Show("Open Package folder?", "", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Process.Start("explorer.exe", exeZip.Info.Directory.FullName);
            }
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
        VersionCheck();
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

    private static void InitFiles()
    {
        currentExe = new($"BLREdit.exe");
        backupExe = new($"{IOResources.UPDATE_DIR}BLREdit.exe.bak");
        //Need to download 
        exeZip = new($"{IOResources.UPDATE_DIR}BLREdit.zip");
        assetZip = new($"{IOResources.UPDATE_DIR}Assets.zip");
        jsonZip = new($"{IOResources.UPDATE_DIR}json.zip");
        dllsZip = new($"{IOResources.UPDATE_DIR}dlls.zip");
        texturesZip = new($"{IOResources.UPDATE_DIR}textures.zip");
        crosshairsZip = new($"{IOResources.UPDATE_DIR}crosshairs.zip");
        patchesZip = new($"{IOResources.UPDATE_DIR}patches.zip");
    }

    public static void PackageAssets()
    {
        Directory.CreateDirectory(IOResources.UPDATE_DIR);

        if (exeZip.Info.Exists) { exeZip.Info.Delete(); }
        if (assetZip.Info.Exists) { assetZip.Info.Delete(); }
        if (jsonZip.Info.Exists) { jsonZip.Info.Delete(); }
        if (dllsZip.Info.Exists) { dllsZip.Info.Delete(); }
        if (texturesZip.Info.Exists) { texturesZip.Info.Delete(); }
        if (crosshairsZip.Info.Exists) { crosshairsZip.Info.Delete(); }
        if (patchesZip.Info.Exists) { patchesZip.Info.Delete(); }

        var taskExe = Task.Run(() => 
        {
            using var archive = ZipFile.Open(exeZip.Info.FullName, ZipArchiveMode.Create);
            var entry = archive.CreateEntryFromFile("BLREdit.exe", "BLREdit.exe");
        });
        var taskAsset = Task.Run(() => { ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}", assetZip.Info.FullName); });
        var taskJson = Task.Run(() => { ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.JSON_DIR}", jsonZip.Info.FullName); });
        var taskDlls = Task.Run(() => { ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.DLL_DIR}", dllsZip.Info.FullName); });
        var taskTexture = Task.Run(() => { ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.TEXTURE_DIR}", texturesZip.Info.FullName); });
        var taskPreview = Task.Run(() => { ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.PREVIEW_DIR}", crosshairsZip.Info.FullName); });
        var taskPatches = Task.Run(() => { ZipFile.CreateFromDirectory($"{IOResources.ASSET_DIR}{IOResources.PATCH_DIR}", patchesZip.Info.FullName); });

        Task.WhenAll(taskExe, taskAsset, taskJson, taskDlls, taskTexture, taskPreview, taskPatches).Wait();
    }

    public static async Task<RepositoryProxyModule[]> Initialize()
    {
        return await GetAvailableProxyModules();
    }

    private readonly static Dictionary<FileInfoExtension, string> DownloadLinks = new();

    private static FileInfoExtension currentExe;
    private static FileInfoExtension backupExe;
    //Need to download 
    private static FileInfoExtension exeZip;
    private static FileInfoExtension assetZip;
    private static FileInfoExtension jsonZip;
    private static FileInfoExtension dllsZip;
    private static FileInfoExtension texturesZip;
    private static FileInfoExtension crosshairsZip;
    private static FileInfoExtension patchesZip;

    public static void VersionCheck()
    {
        Directory.CreateDirectory(IOResources.UPDATE_DIR);

        try
        {
            var task = GitHubClient.GetLatestRelease(CurrentOwner, CurrentRepo);
            task.Wait();
            BLREditLatestRelease = task.Result;
            if (BLREditLatestRelease is null) { LoggingSystem.Log("Can't connect to github to check for new Version"); return; }
            LoggingSystem.Log($"Newest Version: {BLREditLatestRelease.tag_name} of {BLREditLatestRelease.name} vs Current: {CurrentVersion} of {CurrentVersionTitle}");

            var remoteVersion = CreateVersion(BLREditLatestRelease.tag_name);
            var localVersion = CreateVersion(CurrentVersion);

            bool newVersionAvailable = remoteVersion > localVersion;
            bool assetFolderMissing = !Directory.Exists(IOResources.ASSET_DIR);

            LoggingSystem.Log($"New Version Available:{newVersionAvailable} AssetFolderMissing:{assetFolderMissing}");

            if (BLREditLatestRelease is not null)
            {
                foreach (var asset in BLREditLatestRelease.assets)
                {
                    if (asset.name.StartsWith(exeZip.Name) && asset.name.EndsWith(exeZip.Info.Extension))
                    { DownloadLinks.Add(exeZip, asset.browser_download_url); }

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
                    UpdateAllAssetPacks();

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
    }

    private static void UpdateEXE()
    {
        if (DownloadLinks.TryGetValue(exeZip, out string exeDL))
        {
            if (exeZip.Info.Exists) { LoggingSystem.Log($"[Update]: Deleting {exeZip.Info.FullName}"); exeZip.Info.Delete(); }
            LoggingSystem.Log($"[Update]: Downloading {exeDL}");
            IOResources.WebClient.DownloadFile(exeDL, exeZip.Info.FullName);
            if (backupExe.Info.Exists) { LoggingSystem.Log($"[Update]: Deleting {backupExe.Info.FullName}"); backupExe.Info.Delete(); }
            LoggingSystem.Log($"[Update]: Moving {currentExe.Info.FullName} to {backupExe.Info.FullName}");
            currentExe.Info.MoveTo(backupExe.Info.FullName);
            currentExe = new("BLREdit.exe");
            LoggingSystem.Log($"[Update]: Unpacking {exeZip.Info.FullName} to BLREdit.exe");
            ZipFile.ExtractToDirectory(exeZip.Info.FullName, ".\\");

            LoggingSystem.Log($"[Update]: Launching New EXE !!!");

            File.WriteAllText("launch.bat", "@echo off\nBLREdit.exe\nexit");

            ProcessStartInfo psi = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "launch.bat",
            };
            Process newApp = new()
            {
                StartInfo = psi
            };
            newApp.Start();
            Current.Shutdown(-69);
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

    private static void UpdateAllAssetPacks()
    {
        //Sync DL
        DownloadAssetPack(jsonZip);
        DownloadAssetPack(dllsZip);
        DownloadAssetPack(texturesZip);
        DownloadAssetPack(crosshairsZip);
        DownloadAssetPack(patchesZip);
        //Extract them all
        var jsonTask = Task.Run(() => { UpdateAssetPack(jsonZip, $"{IOResources.ASSET_DIR}{IOResources.JSON_DIR}"); });
        var dllsTask = Task.Run(() => { UpdateAssetPack(dllsZip, $"{IOResources.ASSET_DIR}{IOResources.DLL_DIR}"); });
        var textureTask = Task.Run(() => { UpdateAssetPack(texturesZip, $"{IOResources.ASSET_DIR}{IOResources.TEXTURE_DIR}"); });
        var crosshairTask = Task.Run(() => { UpdateAssetPack(crosshairsZip, $"{IOResources.ASSET_DIR}{IOResources.PREVIEW_DIR}"); });
        var patchesTask = Task.Run(() => { UpdateAssetPack(patchesZip, $"{IOResources.ASSET_DIR}{IOResources.PATCH_DIR}"); });

        Task.WhenAll(jsonTask, dllsTask, textureTask, crosshairTask, patchesTask).Wait();
    }
    private static void DownloadAssetPack(FileInfoExtension pack)
    {
        if (DownloadLinks.TryGetValue(pack, out string dl))
        {
            if (pack.Info.Exists) { LoggingSystem.Log($"[Update]: Deleting {pack.Info.FullName}"); pack.Info.Delete(); }
            LoggingSystem.Log($"[Update]: Downloading {dl}");
            IOResources.WebClient.DownloadFile(dl, pack.Info.FullName);
        }
        else
        { LoggingSystem.Log($"No {pack.Info.Name} for download available!"); }
    }
    private static void UpdateAssetPack(FileInfoExtension pack, string targetFolder)
    {
        if (DownloadLinks.TryGetValue(pack, out _))
        {
            if (Directory.Exists(targetFolder)) { LoggingSystem.Log($"[Update]: Deleting {targetFolder}"); Directory.Delete(targetFolder, true); }
            LoggingSystem.Log($"[Update]: Extracting {pack.Info.FullName} to {targetFolder}");
            ZipFile.ExtractToDirectory(pack.Info.FullName, targetFolder);
        }
        else
        { LoggingSystem.Log($"No {pack.Info.Name} to Unpack!"); }
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
