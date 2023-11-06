using BLREdit.API.InterProcess;
using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.API.Utils;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.Import;
using BLREdit.Properties;
using BLREdit.UI;
using BLREdit.UI.Views;
using BLREdit.UI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
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
public partial class App : System.Windows.Application
{
    public const string CurrentVersion = "v0.11.7";
    public const string CurrentVersionTitle = "Loadout Managment Preview";
    public const string CurrentOwner = "HALOMAXX";
    public const string CurrentRepo = "BLREdit";

    public static bool IsNewVersionAvailable { get; private set; } = false;
    public static bool IsBaseRuntimeMissing { get; private set; } = true;
    public static bool IsUpdateRuntimeMissing { get; private set; } = true;
    public static GitHubRelease? BLREditLatestRelease { get; private set; } = null;
    public static ObservableCollection<VisualProxyModule> AvailableProxyModules { get; } = new();
    public static Dictionary<string, string> AvailableLocalizations { get; set; } = new();

    public static List<BLRServer> DefaultServers { get; set; } = new();

    public static readonly string BLREditLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

    public static CultureInfo DefaultCulture { get; } = CultureInfo.CreateSpecificCulture("en-US");

    public static bool IsRunning { get; private set; } = true;

    public static List<Thread> AppThreads { get; private set; } = new();

    public static bool ForceStart { get; private set; }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        string[] argList = e.Args;
        Dictionary<string, string> argDict = new();

        for (var i = 0; i < argList.Length; i++)
        {
            string name = argList[i];
            string value = "";
            if (!name.StartsWith("-")) continue;
            if (i + 1 < argList.Length && !argList[i + 1].StartsWith("-")) value = argList[i + 1];
            argDict.Add(name, value);
        }

        if (argDict.TryGetValue("-forceStart", out var _))
        {
            ForceStart = true;
        }

        if (argDict.TryGetValue("-exportItemList", out var _))
        {
            ImportSystem.Initialize();

            IOResources.SerializeFile("itemList.json", ImportSystem.ItemLists);

            Application.Current.Shutdown();
            return;
        }

        if (argDict.TryGetValue("-package", out string _))
        {
            try
            {
                LoggingSystem.Log($"Started Packaging BLREdit Release");
                PackageAssets();
                LoggingSystem.Log($"Finished Packaging");
            }
            catch (Exception error) { LoggingSystem.MessageLog(string.Format(BLREdit.Properties.Resources.msg_PackagingFailed, error), BLREdit.Properties.Resources.msgT_Error); }
            if (LoggingSystem.MessageLog(BLREdit.Properties.Resources.msg_OpenPackagingFolder, BLREdit.Properties.Resources.msgT_Info, MessageBoxButton.YesNo))
            {
                Process.Start("explorer.exe", $"{Directory.GetCurrentDirectory()}\\packaged");
            }
            Application.Current.Shutdown();
            return;
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
                LoggingSystem.MessageLog(string.Format(BLREdit.Properties.Resources.msg_PackagingFailed, error), BLREdit.Properties.Resources.msgT_Error);
            }

            BLRProcess.KillAll();

            Application.Current.Shutdown();
            return;
        }

#if DEBUG

        if (argDict.TryGetValue("-localize", out string _))
        {
            ImportSystem.Initialize();

            var NameList = new Dictionary<BLRItem, string>();
            var TooltipList = new Dictionary<BLRItem, string>();

            foreach (var category in ImportSystem.ItemLists)
            {
                foreach (var item in category.Value)
                { 
                    if (!string.IsNullOrEmpty(item.DisplayName))
                    {
                        NameList.Add(item, item.DisplayName);
                    }
                    else
                    {
                        NameList.Add(item, item.Name ?? string.Empty);
                    }

                    if (!string.IsNullOrEmpty(item.DisplayTooltip))
                    {
                        TooltipList.Add(item, item.DisplayTooltip);
                    }
                    else
                    {
                        TooltipList.Add(item, string.Empty);
                    }
                }
            }

            GenerateNameIDs();

            //TestNameDuplication();

            using (ResXResourceWriter resx = new("newItemNames.resx"))
            {
                foreach (var item in NameList)
                {
                    var node = new ResXDataNode(item.Key.NameID.ToString("000000"), item.Value) { Comment = item.Key.Category };
                    resx.AddResource(node);
                }
            }
            using (ResXResourceWriter resx = new("newItemTooltips.resx"))
            {
                foreach (var item in TooltipList)
                {
                    var node = new ResXDataNode(item.Key.NameID.ToString("000000"), item.Value) { Comment = item.Key.Category };
                    resx.AddResource(node);
                }
            }

            IOResources.SerializeFile("localizedItemList.json", ImportSystem.ItemLists);

            LoggingSystem.Log("Done testing ItemList's");
            Application.Current.Shutdown();
            return;
        }
        #endif

        if (BLREditPipe.ForwardLaunchArgs(argList))
        {
            Application.Current.Shutdown();
            return;
        }

        new MainWindow(argList).Show();
    }

    private static void GenerateNameIDs(int categoryOffset = 1000)
    {
        int categoryIndex = categoryOffset;

        foreach (var category in ImportSystem.ItemLists)
        {
            int itemIndex = 0;
            foreach (var item in category.Value)
            {
                item.NameID = categoryIndex + itemIndex;
                itemIndex++;
            }
            categoryIndex += categoryOffset;
        }
    }

    private static void TestNameDuplication()
    {
        LoggingSystem.Log("Preparing Name Duplication Lists");
        Dictionary<int, List<BLRItem>> NameIDList = new();
        Dictionary<string?, List<BLRItem>> NameList = new();

        foreach (var cat in ImportSystem.ItemLists)
        {
            foreach (var item in cat.Value)
            {
                if (!NameIDList.ContainsKey(item.NameID))
                {
                    NameIDList.Add(item.NameID, new() { item });
                }
                else
                {
                    NameIDList[item.NameID].Add(item);
                }

                if (!NameList.ContainsKey(item.Name))
                {
                    NameList.Add(item.Name, new() { item });
                }
                else
                {
                    NameList[item.Name].Add(item);
                }

            }
        }

        LoggingSystem.Log("Testing NameID Duplicates");
        foreach (var itemList in NameIDList)
        {
            if (itemList.Value.Count > 1)
            {
                string items = $"[{itemList.Key}]:";
                foreach (var item in itemList.Value)
                {
                    items += $" {item.Name},";
                }
                LoggingSystem.Log(items);
            }
        }

        LoggingSystem.Log("Testing Name Duplicates");
        foreach (var itemList in NameList)
        {
            if (itemList.Value.Count > 1)
            {
                string items = $"[{itemList.Key}]:";
                foreach (var item in itemList.Value)
                {
                    items += $" {item.NameID},";
                    item.NameID = itemList.Value[0].NameID;
                }
                LoggingSystem.Log(items);
            }
        }
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
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

    static App()
    {
        Directory.SetCurrentDirectory(BLREditLocation);

        Trace.Listeners.Add(new TextWriterTraceListener($"logs\\BLREdit\\{DateTime.Now:MM.dd.yyyy(HHmmss)}.log", "loggingListener"));

        Trace.AutoFlush = true;

        LoggingSystem.Log($"BLREdit {CurrentVersion} {CultureInfo.CurrentCulture.Name} @{BLREditLocation} or {Directory.GetCurrentDirectory()}");

        if (DataStorage.Settings.SelectedCulture is null)
        {
            DataStorage.Settings.SelectedCulture = CultureInfo.CurrentCulture;
        }

        Thread.CurrentThread.CurrentUICulture = DataStorage.Settings.SelectedCulture;
    }

    public App()
    {
        LoggingSystem.Log("App Constructor Start");
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        SetUpdateFilePath();

        CreateAllDirectories();

        foreach (var file in Directory.EnumerateFiles("logs\\BLREdit"))
        {
            var fileInfo = new FileInfo(file);
            var creationDelta = DateTime.Now - fileInfo.CreationTime;
            if (creationDelta.Days >= 1)
            {
                try {
                    fileInfo.Delete();
                } catch(Exception e) {
                    LoggingSystem.Log($"[ERROR] Failed to delete old log file {fileInfo.Name}:\n {e}");
                }
            }
        }
    }

    void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LoggingSystem.Log($"[Unhandled]: {e.ExceptionObject}");
        Environment.Exit(666);
    }

    private static void SetUpdateFilePath()
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

    private static void SetPackageFilePath()
    {
        currentExe = new($"BLREdit.exe");
        backupExe = new($"{IOResources.PACKAGE_DIR}BLREdit.exe.bak");
        //Need to download 
        exeZip = new($"{IOResources.PACKAGE_DIR}BLREdit.zip");
        assetZip = new($"{IOResources.PACKAGE_DIR}Assets.zip");
        jsonZip = new($"{IOResources.PACKAGE_DIR}json.zip");
        dllsZip = new($"{IOResources.PACKAGE_DIR}dlls.zip");
        texturesZip = new($"{IOResources.PACKAGE_DIR}textures.zip");
        crosshairsZip = new($"{IOResources.PACKAGE_DIR}crosshairs.zip");
        patchesZip = new($"{IOResources.PACKAGE_DIR}patches.zip");
    }

    private static void CleanPackageOrUpdateDirectory()
    {
        if (exeZip is not null && exeZip.Info.Exists) { exeZip.Info.Delete(); }
        if (assetZip is not null && assetZip.Info.Exists) { assetZip.Info.Delete(); }
        if (jsonZip is not null && jsonZip.Info.Exists) { jsonZip.Info.Delete(); }
        if (dllsZip is not null && dllsZip.Info.Exists) { dllsZip.Info.Delete(); }
        if (texturesZip is not null && texturesZip.Info.Exists) { texturesZip.Info.Delete(); }
        if (crosshairsZip is not null && crosshairsZip.Info.Exists) { crosshairsZip.Info.Delete(); }
        if (patchesZip is not null && patchesZip.Info.Exists) { patchesZip.Info.Delete(); }
    }

    public static void PackageAssets()
    {
        Directory.CreateDirectory(IOResources.PACKAGE_DIR);
        Directory.CreateDirectory($"{IOResources.PACKAGE_DIR}\\locale\\Localizations");

        SetPackageFilePath();

        CleanPackageOrUpdateDirectory();

        if (exeZip is null) { LoggingSystem.Log("[PackageAssets]: exeZip was null"); return; }
        if (assetZip is null) { LoggingSystem.Log("[PackageAssets]: assetZip was null"); return; }
        if (jsonZip is null) { LoggingSystem.Log("[PackageAssets]: jsonZip was null"); return; }
        if (dllsZip is null) { LoggingSystem.Log("[PackageAssets]: dllsZip was null"); return; }
        if (texturesZip is null) { LoggingSystem.Log("[PackageAssets]: texturesZip was null"); return; }
        if (crosshairsZip is null) { LoggingSystem.Log("[PackageAssets]: crosshairsZip was null"); return; }
        if (patchesZip is null) { LoggingSystem.Log("[PackageAssets]: patchesZip was null"); return; }

        var taskLocalize = Task.Run(() => {
            Dictionary<string, string?> LocalePairs = new();
            var dirs = Directory.EnumerateDirectories(BLREditLocation);
            foreach (var dir in dirs)
            {
                string resourceFile = $"{dir}\\BLREdit.resources.dll";
                if (File.Exists(resourceFile))
                { 
                    var hash = IOResources.CreateFileHash(resourceFile);
                    string locale = dir.Substring(dir.Length - 5, 5);
                    string targetZip = $"{IOResources.PACKAGE_DIR}\\locale\\Localizations\\{locale}.zip";
                    LocalePairs.Add(locale, hash);
                    File.WriteAllText($"{dir}\\manifest.hash", hash);
                    if (File.Exists(targetZip)) { File.Delete(targetZip); }
                    ZipFile.CreateFromDirectory(dir, targetZip);
                }
            }

            IOResources.SerializeFile($"{IOResources.PACKAGE_DIR}\\locale\\Localizations.json", LocalePairs);
        });


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

        Task.WhenAll(taskExe, taskAsset, taskJson, taskDlls, taskTexture, taskPreview, taskPatches, taskLocalize).Wait();

        SetUpdateFilePath();
    }

    private readonly static Dictionary<FileInfoExtension?, string> DownloadLinks = new();

    private static FileInfoExtension? currentExe;
    private static FileInfoExtension? backupExe;
    //Need to download 
    private static FileInfoExtension? exeZip;
    private static FileInfoExtension? assetZip;
    private static FileInfoExtension? jsonZip;
    private static FileInfoExtension? dllsZip;
    private static FileInfoExtension? texturesZip;
    private static FileInfoExtension? crosshairsZip;
    private static FileInfoExtension? patchesZip;

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

            var remoteVersion = CreateVersion(BLREditLatestRelease?.TagName ?? string.Empty);
            var localVersion = CreateVersion(CurrentVersion);

            bool newVersionAvailable = remoteVersion > localVersion;
            bool assetFolderMissing = !Directory.Exists(IOResources.ASSET_DIR);

            LoggingSystem.Log($"New Version Available:{newVersionAvailable} AssetFolderMissing:{assetFolderMissing}");

            if (BLREditLatestRelease is not null && BLREditLatestRelease.Assets is not null)
            {
                foreach (var asset in BLREditLatestRelease.Assets)
                {
                    if (exeZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.StartsWith(exeZip.Name) && asset.Name.EndsWith(exeZip.Info.Extension))
                    { DownloadLinks.Add(exeZip, asset.BrowserDownloadURL); }

                    if (assetZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.StartsWith(assetZip.Name) && asset.Name.EndsWith(assetZip.Info.Extension))
                    { DownloadLinks.Add(assetZip, asset.BrowserDownloadURL); }

                    if (jsonZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.StartsWith(jsonZip.Name) && asset.Name.EndsWith(jsonZip.Info.Extension))
                    { DownloadLinks.Add(jsonZip, asset.BrowserDownloadURL); }

                    if (dllsZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.StartsWith(dllsZip.Name) && asset.Name.EndsWith(dllsZip.Info.Extension))
                    { DownloadLinks.Add(dllsZip, asset.BrowserDownloadURL); }

                    if (texturesZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.StartsWith(texturesZip.Name) && asset.Name.EndsWith(texturesZip.Info.Extension))
                    { DownloadLinks.Add(texturesZip, asset.BrowserDownloadURL); }

                    if (crosshairsZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.StartsWith(crosshairsZip.Name) && asset.Name.EndsWith(crosshairsZip.Info.Extension))
                    { DownloadLinks.Add(crosshairsZip, asset.BrowserDownloadURL); }

                    if (patchesZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.StartsWith(patchesZip.Name) && asset.Name.EndsWith(patchesZip.Info.Extension))
                    { DownloadLinks.Add(patchesZip, asset.BrowserDownloadURL); }
                }

                if (newVersionAvailable && assetFolderMissing)
                {
                    LoggingSystem.MessageLog(BLREdit.Properties.Resources.msg_UpdateMissingFiles, BLREdit.Properties.Resources.msgT_Info);
                    DownloadAssetFolder();

                    UpdateEXE();
                    return true;
                }
                else if (newVersionAvailable && !assetFolderMissing)
                {
                    LoggingSystem.MessageLog(BLREdit.Properties.Resources.msg_Update, BLREdit.Properties.Resources.msgT_Info);
                    UpdateAllAssetPacks();

                    UpdateEXE();
                    return true;
                }
                else if (!newVersionAvailable && assetFolderMissing)
                {
                    LoggingSystem.MessageLog(BLREdit.Properties.Resources.msg_MisingFiles, BLREdit.Properties.Resources.msgT_Info);
                    DownloadAssetFolder();
                    return true;
                }
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Failed to Update to Newest Version\n{error}", BLREdit.Properties.Resources.msgT_Error); return false; } //TODO: Add Localization
        return false;
    }

    private static void UpdateEXE()
    {
        if (exeZip is null) { LoggingSystem.Log("[UpdateEXE]: exeZip was null"); return; }
        if (backupExe is null) { LoggingSystem.Log("[UpdateEXE]: backupExe was null"); return; }
        if (currentExe is null) { LoggingSystem.Log("[UpdateEXE]: currentExe was null"); return; }

        if (DownloadLinks.TryGetValue(exeZip, out string exeDL))
        {
            if (exeZip.Info.Exists) { LoggingSystem.Log($"[Update]: Deleting {exeZip.Info.FullName}"); exeZip.Info.Delete(); }
            LoggingSystem.Log($"[Update]: Downloading {exeDL}");
            IOResources.DownloadFileMessageBox(exeDL, exeZip.Info.FullName);
            if (backupExe.Info.Exists) { LoggingSystem.Log($"[Update]: Deleting {backupExe.Info.FullName}"); backupExe.Info.Delete(); }
            LoggingSystem.Log($"[Update]: Moving {currentExe.Info.FullName} to {backupExe.Info.FullName}");
            currentExe.Info.MoveTo(backupExe.Info.FullName);
            currentExe = new("BLREdit.exe");
            LoggingSystem.Log($"[Update]: Unpacking {exeZip.Info.FullName} to BLREdit.exe");
            ZipFile.ExtractToDirectory(exeZip.Info.FullName, ".\\");
        }
        else
        { LoggingSystem.Log("No new EXE Available!"); }
    }

    public static void Restart()
    {
        if (IsWindowOpen<MainWindow>(UI.MainWindow.Instance?.Name ?? "")) 
        {
            UI.MainWindow.Instance?.Close();
        }
        LoggingSystem.Log($"Restarting BLREdit!");

        Process newApp = new()
        {
            StartInfo = new()
            {
                FileName = "BLREdit.exe",
                Arguments = "-forceStart"
            }
        };

        newApp.Start();

        Environment.Exit(0);
    }

    public static bool IsWindowOpen<T>(string name = "") where T : Window
    {
        return string.IsNullOrEmpty(name)
           ? Application.Current.Windows.OfType<T>().Any()
           : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
    }

    private static void DownloadAssetFolder()
    {
        if (assetZip is null) { LoggingSystem.Log("[DownloadAssetFolder] failed assetZip was null!"); return; }
        if (DownloadLinks.TryGetValue(assetZip, out string assetDL))
        {
            if (assetZip.Info.Exists) { assetZip.Info.Delete(); }
            IOResources.DownloadFileMessageBox(assetDL, assetZip.Info.FullName);
            ZipFile.ExtractToDirectory(assetZip.Info.FullName, IOResources.ASSET_DIR);
        }
        else
        { LoggingSystem.MessageLog("No Asset folder for download available!", "Error"); } //TODO: Add Localization
    }

    private static void UpdateAllAssetPacks()
    {
        if (exeZip is null) { LoggingSystem.Log("[UpdateAllAssetPacks]: exeZip was null"); return; }
        if (assetZip is null) { LoggingSystem.Log("[UpdateAllAssetPacks]: assetZip was null"); return; }
        if (jsonZip is null) { LoggingSystem.Log("[UpdateAllAssetPacks]: jsonZip was null"); return; }
        if (dllsZip is null) { LoggingSystem.Log("[UpdateAllAssetPacks]: dllsZip was null"); return; }
        if (texturesZip is null) { LoggingSystem.Log("[UpdateAllAssetPacks]: texturesZip was null"); return; }
        if (crosshairsZip is null) { LoggingSystem.Log("[UpdateAllAssetPacks]: crosshairsZip was null"); return; }
        if (patchesZip is null) { LoggingSystem.Log("[UpdateAllAssetPacks]: patchesZip was null"); return; }
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

        if (UpdatePanic) 
        {
            LoggingSystem.Log("Update failed cleaning BLREdit folder and restarting!");
            var dirs = Directory.EnumerateDirectories(BLREditLocation);
            foreach (var dir in dirs)
            {
                if (!dir.EndsWith("Profile") && !dir.EndsWith("logs") && !dir.EndsWith("ServerConfigs"))
                { 
                    Directory.Delete(dir, true);
                }
            }

            var files = Directory.EnumerateFiles(BLREditLocation);
            foreach (var file in files)
            {
                if (!file.EndsWith("BLREdit.exe") && !file.EndsWith("settings.json") && !file.EndsWith("GameClients.json") && !file.EndsWith("ServerList.json"))
                {
                    File.Delete(file);
                }
            }

            Restart();
        }
    }
    private static void DownloadAssetPack(FileInfoExtension? pack)
    {
        if (pack is null) { LoggingSystem.Log("[DownloadAssetPack] failed pack was null!"); return; }
        if (DownloadLinks.TryGetValue(pack, out string dl))
        {
            if (pack.Info.Exists) { LoggingSystem.Log($"[Update]: Deleting {pack.Info.FullName}"); pack.Info.Delete(); }
            LoggingSystem.Log($"[Update]: Downloading {dl}");
            IOResources.DownloadFileMessageBox(dl, pack.Info.FullName);
        }
        else
        { LoggingSystem.Log($"No {pack.Info.Name} for download available!"); }
    }

    static bool UpdatePanic = false;

    private static void UpdateAssetPack(FileInfoExtension pack, string targetFolder)
    {
        if (DownloadLinks.TryGetValue(pack, out string dl))
        {
            try
            {
                if (Directory.Exists(targetFolder)) { LoggingSystem.Log($"[Update]: Deleting {targetFolder}"); Directory.Delete(targetFolder, true); }
                LoggingSystem.Log($"[Update]: Extracting {pack.Info.FullName} to {targetFolder}");
                ZipFile.ExtractToDirectory(pack.Info.FullName, targetFolder);
            }
            catch (Exception error) 
            { 
                LoggingSystem.Log($"Failed to Unpack {pack.Name} reason:{error}");
                bool tryagain = true;
                int tries = 0;
                while (tryagain)
                {
                    if (tries >= 3) { UpdatePanic = true; return; }
                    tries++;
                    try
                    { 
                        if (pack.Info.Exists) { LoggingSystem.Log($"[Update]: Deleting {pack.Info.FullName}"); pack.Info.Delete(); }
                        LoggingSystem.Log($"[Update]: Downloading {dl}");
                        WebResources.DownloadFile(dl, pack.Info.FullName);

                        if (Directory.Exists(targetFolder)) { LoggingSystem.Log($"[Update]: Deleting {targetFolder}"); Directory.Delete(targetFolder, true); }
                        LoggingSystem.Log($"[Update]: Extracting {pack.Info.FullName} to {targetFolder}");
                        ZipFile.ExtractToDirectory(pack.Info.FullName, targetFolder);

                        tryagain = false;
                    }
                    catch { }
                }
            }
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

    private static async Task<RepositoryProxyModule[]?> GetAvailableProxyModules()
    {
        LoggingSystem.Log("Downloading AvailableProxyModule List!");
        try
        {
            if (await GitHubClient.GetFile(CurrentOwner, CurrentRepo, "master", "Resources/ProxyModules.json") is GitHubFile file)
            {
                var moduleList = IOResources.Deserialize<RepositoryProxyModule[]>(file.DecodedContent);
                LoggingSystem.Log("Finished Downloading AvailableProxyModule List!");
                return moduleList;
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Can't get ProxyModule list from Github\n{error}", "Error"); } //TODO: Add Localization
        return Array.Empty<RepositoryProxyModule>();
    }

    private static async Task<Dictionary<string, string>?> GetAvailableLocalizations()
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
        { LoggingSystem.MessageLog($"Can't get Localization list from Github\n{error}", "Error"); } //TODO: Add Localization
        return new();
    }

    private static async Task<List<BLRServer>?> GetDefaultServers()
    {
        LoggingSystem.Log($"Downloading Default Server List!");
        try
        {
            if (await GitHubClient.GetFile(CurrentOwner, CurrentRepo, "master", "Resources/ServerList.json") is GitHubFile file)
            {
                var serverList = IOResources.Deserialize<List<BLRServer>>(file.DecodedContent);
                LoggingSystem.Log("Finished Downloading Server List!");
                return serverList;
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Can't get server list from Github\n{error}", "Error"); } //TODO: Add Localization
        return new() 
        { //Only localhost is needed as most likely we are offline so there is no need to add anyother default servers
            new() { ServerAddress = "localhost", Port = 7777 } //Local User Server
        };
    }

    private static Task<T> StartSTATask<T>(Func<T> action)
    {
        var tcs = new TaskCompletionSource<T>();
        Thread thread = new(() =>
        {
            try
            {
                tcs.SetResult(action());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task;
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
        
        var serverList = defaultServers.Result;
        var localizationList = availableLocalizations.Result;
        var modules = availableModuleCheck.Result;

        if (serverList is not null) { DefaultServers = serverList; }
        else
        { LoggingSystem.Log("[AvailableProxyModuleCheck] returned Server List was null!"); }

        if (localizationList is not null) { AvailableLocalizations = localizationList; }
        else
        { LoggingSystem.Log("[AvailableProxyModuleCheck] returned Localization List was null!"); }
        
        if (modules is null) { LoggingSystem.Log("[AvailableProxyModuleCheck] returned Module List was null!"); return; }
        
        for (int i = 0; i < modules.Length; i++)
        {
            AvailableProxyModules.Add(new VisualProxyModule(modules[i]));
        }
    }

    public static void DownloadLocalization()
    {
        AvailableProxyModuleCheck();
        var current = CultureInfo.CurrentCulture;
        if (!AvailableLocalizations.TryGetValue(current.Name, out var _))
        {
            DataStorage.Settings.SelectedCulture = DefaultCulture;
            return;
        }
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
        if (WebResources.DownloadFile($"https://github.com/{CurrentOwner}/{CurrentRepo}/raw/master/Resources/Localizations/{locale}.zip", targetZip))
        {
            if (Directory.Exists(locale)) { Directory.Delete(locale, true); Directory.CreateDirectory(locale); }
            ZipFile.ExtractToDirectory(targetZip, $"{locale}");
            Restart();
        }
    }

    public static void CheckAppUpdate()
    {
        var versionCheck = StartSTATask(VersionCheck);
        versionCheck.Wait(); //wait for Version Check if it needed to download stuff it has to finish before we initialize the ImportSystem.
        if (versionCheck.Result)
        {
            Restart();
        }
    }
}
