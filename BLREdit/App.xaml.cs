using BLREdit.API.InterProcess;
using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.API.Utils;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.Import;
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
public partial class App : System.Windows.Application
{
    public const string CurrentVersion = "v0.9.4";
    public const string CurrentVersionTitle = "Oh boy big oopsie still!";
    public const string CurrentOwner = "HALOMAXX";
    public const string CurrentRepo = "BLREdit";

    public static bool IsNewVersionAvailable { get; private set; } = false;
    public static bool IsBaseRuntimeMissing { get; private set; } = true;
    public static bool IsUpdateRuntimeMissing { get; private set; } = true;
    public static GitHubRelease BLREditLatestRelease { get; private set; } = null;
    public static ObservableCollection<VisualProxyModule> AvailableProxyModules { get; } = new();

    public static readonly string BLREditLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

    public static CultureInfo DefaultCulture { get; } = CultureInfo.CreateSpecificCulture("en-US");

    public static bool IsRunning { get; private set; } = true;

    public static List<Thread> AppThreads { get; private set; } = new();

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

        if (argDict.TryGetValue("-package", out string _))
        {
            try
            {
                LoggingSystem.Log($"Started Packaging BLREdit Release");
                PackageAssets();
                LoggingSystem.Log($"Finished Packaging");
            }
            catch (Exception error) { LoggingSystem.MessageLog($"failed to package release:\n{error}"); }
            var result = MessageBox.Show("Open Package folder?", "", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
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

                var serverConfig = IOResources.DeserializeFile<ServerLaunchParameters>(configFile);

                BLRClient client;
                if(serverConfig.ClientId < 0)
                { client = BLREditSettings.Settings.DefaultClient; }
                else 
                { client = UI.MainWindow.GameClients[serverConfig.ClientId]; }

                foreach (var module in AvailableProxyModules)
                {
                    module.RepositoryProxyModule.Required= false;
                    foreach (var required in serverConfig.RequiredModules)
                    {
                        if (module.RepositoryProxyModule.InstallName == required)
                        {
                            module.InstallModule(client);
                        }
                    }
                }

                var serverName = serverConfig.ServerName;
                var port = serverConfig.Port;
                var botCount = serverConfig.BotCount;
                var maxPlayers = serverConfig.MaxPlayers;
                var playlist = serverConfig.Playlist;

                string launchArgs = $"server ?ServerName=\"{serverName}\"?Port={port}?NumBots={botCount}?MaxPlayers={maxPlayers}?Playlist={playlist}";
                client.StartProcess(launchArgs, serverConfig.WatchDog);
                Console.WriteLine("Press Q to Exit");
                while (Console.ReadKey().Key != ConsoleKey.Q) { }
            }
            catch (Exception error)
            { 
                LoggingSystem.MessageLog($"failed to start server:\n{error}"); 
            }
            Application.Current.Shutdown();
            return;
        }

        if (argDict.TryGetValue("-localize", out string _))
        {
            ImportSystem.Initialize();

            int categoryOffset = 1000;

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


            Dictionary<int, List<BLRItem>> NameIDList = new();
            Dictionary<string, List<BLRItem>> NameList = new();

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

            IOResources.SerializeFile("namedItemList.json", ImportSystem.ItemLists);

            LoggingSystem.Log("Done testing ItemList's");
            Application.Current.Shutdown();
            return;
        }

        if (BLREditPipe.ForwardLaunchArgs(argList))
        {
            Application.Current.Shutdown();
            return;
        }

        new MainWindow(argList).Show();
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
    }

    public App()
    {
        Directory.SetCurrentDirectory(BLREditLocation);

        SetUpdateFilePath();

        Directory.CreateDirectory("logs");
        Directory.CreateDirectory("logs\\BLREdit");
        Directory.CreateDirectory("logs\\Client");
        Directory.CreateDirectory("logs\\Proxy");

        foreach (var file in Directory.EnumerateFiles("logs\\BLREdit"))
        { 
            var fileInfo = new FileInfo(file);
            var creationDelta = DateTime.Now - fileInfo.CreationTime;
            if (creationDelta.Days >= 1)
            { 
                fileInfo.Delete();
            }
        }

        Trace.Listeners.Add(new TextWriterTraceListener($"logs\\BLREdit\\{DateTime.Now:MM.dd.yyyy(HHmmss)}.log", "loggingListener"));
        
        Trace.AutoFlush = true;

        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        // TODO: Check for Resource file if nonexistant try to download for language if not available go back to default other wise download new language pack. (Update mechanic with manifest)


        LoggingSystem.Log($"BLREdit Starting! {CultureInfo.CurrentCulture.Name} @{BLREditLocation} or {Directory.GetCurrentDirectory()}");
        LoggingSystem.Log("Loading Server and Client List");
        UI.MainWindow.GameClients = IOResources.DeserializeFile<ObservableCollection<BLRClient>>($"GameClients.json") ?? new();
        UI.MainWindow.ServerList = IOResources.DeserializeFile<ObservableCollection<BLRServer>>($"ServerList.json") ?? new();
        LoggingSystem.Log("Finished Loading Server and Client List");
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
        if (exeZip.Info.Exists) { exeZip.Info.Delete(); }
        if (assetZip.Info.Exists) { assetZip.Info.Delete(); }
        if (jsonZip.Info.Exists) { jsonZip.Info.Delete(); }
        if (dllsZip.Info.Exists) { dllsZip.Info.Delete(); }
        if (texturesZip.Info.Exists) { texturesZip.Info.Delete(); }
        if (crosshairsZip.Info.Exists) { crosshairsZip.Info.Delete(); }
        if (patchesZip.Info.Exists) { patchesZip.Info.Delete(); }
    }

    public static void PackageAssets()
    {
        Directory.CreateDirectory(IOResources.PACKAGE_DIR);

        SetPackageFilePath();

        CleanPackageOrUpdateDirectory();

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

        SetUpdateFilePath();
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

    public static bool VersionCheck()
    {
        LoggingSystem.Log("Running Version Check!");
        Directory.CreateDirectory(IOResources.UPDATE_DIR);

        try
        {
            var task = GitHubClient.GetLatestRelease(CurrentOwner, CurrentRepo);
            task.Wait();
            BLREditLatestRelease = task.Result;
            if (BLREditLatestRelease is null) { LoggingSystem.Log("Can't connect to github to check for new Version"); return false; }
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
                    MessageBox.Show("BLREdit will now Download Missing Files and Update!");
                    DownloadAssetFolder();

                    UpdateEXE();
                    return true;
                }
                else if (newVersionAvailable && !assetFolderMissing)
                {
                    MessageBox.Show("BLREdit will now Update!");
                    UpdateAllAssetPacks();

                    UpdateEXE();
                    return true;
                }
                else if (!newVersionAvailable && assetFolderMissing)
                {
                    MessageBox.Show("BLREdit will now Download Missing Files!");
                    DownloadAssetFolder();
                    return true;
                }
            }
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Failed to Update to Newest Version\n{error}"); return false; }
        return false;
    }

    private static void UpdateEXE()
    {
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
        File.WriteAllText("launch.bat", "@echo off\nTIMEOUT 1\nstart BLREdit.exe\nexit");

        ProcessStartInfo psi = new()
        {
            UseShellExecute= true,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = "launch.bat",
        };
        Process newApp = new()
        {
            StartInfo = psi
        };
        LoggingSystem.Log($"Restart: {newApp.Start()}");
    }

    private static void DownloadAssetFolder()
    {
        if (DownloadLinks.TryGetValue(assetZip, out string assetDL))
        {
            if (assetZip.Info.Exists) { assetZip.Info.Delete(); }
            IOResources.DownloadFileMessageBox(assetDL, assetZip.Info.FullName);
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
            IOResources.DownloadFileMessageBox(dl, pack.Info.FullName);
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

    private static async Task<RepositoryProxyModule[]> GetAvailableProxyModules()
    {
        LoggingSystem.Log("Downloading AvailableProxyModule List!");
        var moduleList = Array.Empty<RepositoryProxyModule>();
        try
        {
            var file = await GitHubClient.GetFile(CurrentOwner, CurrentRepo, "master", "Resources/ProxyModules.json");
            if (file is null) return Array.Empty<RepositoryProxyModule>();
            moduleList = IOResources.Deserialize<RepositoryProxyModule[]>(file.decoded_content);
            LoggingSystem.Log("Finished Downloading AvailableProxyModule List!");
        }
        catch (Exception error)
        { LoggingSystem.MessageLog($"Can't get ProxyModule list from Github\n{error}"); }
        return moduleList;
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
        availableModuleCheck.Wait();
        var modules = availableModuleCheck.Result;
        for (int i = 0; i < modules.Length; i++)
        {
            AvailableProxyModules.Add(new VisualProxyModule() { RepositoryProxyModule = modules[i] });
        }
    }

    public static void CheckAppUpdate()
    {
        var versionCheck = StartSTATask(VersionCheck);
        versionCheck.Wait(); //wait for Version Check if it needed to download stuff it has to finish before we initialize the ImportSystem.
        if (versionCheck.Result)
        {
            Restart();
            Environment.Exit(0);
        }
    }
}
