using BLREdit.API.InterProcess;
using BLREdit.API.REST_API.GitHub;
using BLREdit.API.Utils;
using BLREdit.Export;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;
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
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace BLREdit;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public const string CurrentVersion = "v0.12.0";
    public const string CurrentVersionTitle = "Support for new BLRevive SDK";
    public const string CurrentOwner = "HALOMAXX";
    public const string CurrentRepo = "BLREdit";
    public static readonly int CurrentVersionNumber = CreateVersion(CurrentVersion);

    public static bool IsNewVersionAvailable { get; private set; } = false;
    public static bool IsBaseRuntimeMissing { get; private set; } = true;
    public static bool IsUpdateRuntimeMissing { get; private set; } = true;
    public static GitHubRelease? LatestRelease { get; private set; } = null;
    public static ObservableCollection<VisualProxyModule> AvailableProxyModules { get; } = new();
    public static Dictionary<string, string> AvailableLocalizations { get; set; } = new();

    public static List<BLRServer> DefaultServers { get; set; } = new();

    public static readonly string BLREditLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

    public static CultureInfo DefaultCulture { get; } = CultureInfo.CreateSpecificCulture("en-US");

    public static bool IsRunning { get; private set; } = true;

    public static List<Thread> AppThreads { get; private set; } = new();

    public static bool ForceStart { get; private set; }

    static void AddNewItemList(string name, KeyValuePair<string, JsonNode?> array)
    {
        var newList = new ObservableCollection<BLRItem>();
        foreach (var item in array.Value.AsObject())
        {
            newList.Add(new BLRItem() { UID = int.Parse(item.Key), Name = item.Value.ToString() });
        }
        ImportSystem.ItemLists.Add(name, newList);
    }

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

        if (argDict.TryGetValue("-importNewUIDs", out var _))
        {
            ImportSystem.Initialize();

            //var misc = JsonNode.Parse(File.ReadAllText("Assets\\json\\misc.json"));
            //var weapons = JsonNode.Parse(File.ReadAllText("Assets\\json\\weapons.json"));
            //var gear = JsonNode.Parse(File.ReadAllText("Assets\\json\\gear.json"));

            //foreach (var obj in misc.AsObject())
            //{
            //    switch (obj.Key)
            //    {
            //        case "dialogPackAnnouncers":
            //            AddNewItemList("dialogPackAnnouncers", obj);
            //            break;
            //        case "dialogPackPlayers":
            //            AddNewItemList("dialogPackPlayers", obj);
            //            break;
            //        case "emblem":
            //            foreach (var subObj in obj.Value.AsObject())
            //            {
            //                switch (subObj.Key)
            //                {
            //                    case "alpha":
            //                        AddNewItemList("emblem_alpha", subObj);
            //                        break;
            //                    case "background":
            //                        AddNewItemList("emblem_background", subObj);
            //                        break;
            //                    case "color":
            //                        AddNewItemList("emblem_color", subObj);
            //                        break;
            //                    case "icon":
            //                        AddNewItemList("emblem_icon", subObj);
            //                        break;
            //                    case "shape":
            //                        AddNewItemList("emblem_shape", subObj);
            //                        break;
            //                }
            //            }
            //            break;
            //        case "titles":
            //            AddNewItemList("titles", obj);
            //            break;
            //    }
            //}

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

            using (ResXResourceWriter resx = new("ItemNames.resx"))
            {
                foreach (var item in NameList)
                {
                    var node = new ResXDataNode(item.Key.UID.ToString(BLRItem.UID_FORMAT), item.Value) { Comment = item.Key.Category };
                    resx.AddResource(node);
                }
            }
            using (ResXResourceWriter resx = new("ItemTooltips.resx"))
            {
                foreach (var item in TooltipList)
                {
                    var node = new ResXDataNode(item.Key.UID.ToString(BLRItem.UID_FORMAT), item.Value) { Comment = item.Key.Category };
                    resx.AddResource(node);
                }
            }

            IOResources.SerializeFile("itemList.json", ImportSystem.ItemLists);
            Application.Current.Shutdown();
            return;
        }

        if (argDict.TryGetValue("-validateWeapons", out var _))
        {
            ImportSystem.Initialize();

            var weapons = ImportSystem.GetItemListOfType(ImportSystem.PRIMARY_CATEGORY).Concat(ImportSystem.GetItemListOfType(ImportSystem.SECONDARY_CATEGORY));

            var noBarrel = ImportSystem.GetItemByNameAndType(ImportSystem.BARRELS_CATEGORY, MagiCowsWeapon.NoBarrel);
            var noCamo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, MagiCowsWeapon.NoCamo);
            var noGrip = ImportSystem.GetItemByNameAndType(ImportSystem.GRIPS_CATEGORY, MagiCowsWeapon.NoGrip);
            var noMagazine = ImportSystem.GetItemByIDAndType(ImportSystem.MAGAZINES_CATEGORY, MagiCowsWeapon.NoMagazine);
            var noMuzzle = ImportSystem.GetItemByIDAndType(ImportSystem.MUZZELS_CATEGORY, MagiCowsWeapon.NoMuzzle);
            var noScope = ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, MagiCowsWeapon.NoScope);
            var noStock = ImportSystem.GetItemByNameAndType(ImportSystem.STOCKS_CATEGORY, MagiCowsWeapon.NoStock);
            var noTag = ImportSystem.GetItemByIDAndType(ImportSystem.HANGERS_CATEGORY, MagiCowsWeapon.NoTag);

            foreach (var primary in weapons)
            {
                LoggingSystem.Log($"{primary.Name}:{primary.UID}");
                if (!primary.SupportedMods.Contains("ammos"))
                {
                    LoggingSystem.Log($"doesn't support ammos");
                }
                if (!primary.SupportedMods.Contains("barrels") && (!noBarrel?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support barrels and\t\tHasValidEntry:{noBarrel?.IsValidFor(primary)}");
                }
                if (!primary.SupportedMods.Contains("camosWeapon") && (!noCamo?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support camos and\t\tHasValidEntry:{noCamo?.IsValidFor(primary)}");
                }
                if (!primary.SupportedMods.Contains("hangers") && (!noTag?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support hangers and\t\tHasValidEntry:{noTag?.IsValidFor(primary)}");
                }
                if (!primary.SupportedMods.Contains("magazines") && (!noMagazine?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support magazines and\tHasValidEntry:{noMagazine?.IsValidFor(primary)}");
                }
                if (!primary.SupportedMods.Contains("muzzles") && (!noMuzzle?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support muzzles and\t\tHasValidEntry:{noMuzzle?.IsValidFor(primary)}");
                }
                if (!primary.SupportedMods.Contains("scopes") && (!noScope?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support scopes and\t\tHasValidEntry:{noScope?.IsValidFor(primary)}");
                }
                if (!primary.SupportedMods.Contains("stocks") && (!noStock?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support stocks and\t\tHasValidEntry:{noStock?.IsValidFor(primary)}");
                }
                if (!primary.SupportedMods.Contains("grips") && (!noGrip?.IsValidFor(primary) ?? false))
                {
                    LoggingSystem.Log($"doesn't support grips and\t\tHasValidEntry:{noGrip?.IsValidFor(primary)}");
                }
                LoggingSystem.Log($"");
            }
            Application.Current.Shutdown();
            return;
        }

        if (argDict.TryGetValue("-exportItemList", out var _))
        {
            ImportSystem.Initialize();

            var magazines = ImportSystem.GetItemArrayOfType(ImportSystem.MAGAZINES_CATEGORY);
            var magnumRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Magnum Rounds");
            var APRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Armor Piercing Rounds");
            var standardRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Standard Rounds");
            var electroRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Electro Rounds");
            var exploRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Explosive Rounds");
            var HPRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Hollow Point Rounds");
            var incendiaryRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Incendiary Rounds");
            var toxicRounds = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Toxic Rounds");
            var incendiaryFlare = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Incendiary Flare");
            var explosiveFlare = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Explosive Flare");
            var canister = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Canister");
            var thumperFlare = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Thumper Flare");
            var explodingArrow = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Exploding Arrow");
            var stunArrow = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Stun Arrow");
            var poisonArrow = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Poison Arrow");
            var lightArrow= ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Light Arrow");
            var heavyArrow = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Heavy Arrow");
            var standardArrow = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Standard Arrow");
            var cupidArrow = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Cupid's Arrow");

            foreach (var mag in magazines)
            {
                if (mag.Name.Contains("Electro"))
                {
                    mag.AmmoType = electroRounds.UID;
                }
                else if (mag.Name.Contains("Explosive"))
                {
                    mag.AmmoType = exploRounds.UID;
                }
                else if (mag.Name.Contains("Toxic"))
                {
                    mag.AmmoType = toxicRounds.UID;
                }
                else if (mag.Name.Contains("Incendiary"))
                {
                    mag.AmmoType = incendiaryRounds.UID;
                }
                else if (mag.Name.Contains("Magnum"))
                {
                    mag.AmmoType = magnumRounds.UID;
                }
                else
                {
                    mag.AmmoType = -1;
                }
                if (mag.Name.Contains("Heavy Arrow"))
                {
                    mag.AmmoType = heavyArrow.UID;
                }
                else if (mag.Name.Contains("Light Arrow"))
                {
                    mag.AmmoType = lightArrow.UID;
                }
                else if (mag.Name.Contains("Poison Arrow"))
                {
                    mag.AmmoType = poisonArrow.UID;
                }
                else if (mag.Name.Contains("Stun Arrow"))
                {
                    mag.AmmoType = stunArrow.UID;
                }
                else if (mag.Name.Contains("Exploding Arrow"))
                {
                    mag.AmmoType = explodingArrow.UID;
                }
                else if (mag.Name.Contains("Standard Arrow"))
                {
                    mag.AmmoType = standardArrow.UID;
                }
                else if (mag.Name.Contains("Cupid's Arrow"))
                {
                    mag.AmmoType = cupidArrow.UID;
                }
                else if (mag.Name.Contains("High Explosive Round Bore"))
                {
                    mag.AmmoType = explosiveFlare.UID;
                }
                else if (mag.Name.Contains("Incendiary Round Bore"))
                {
                    mag.AmmoType = incendiaryFlare.UID;
                }
                else if (mag.Name.Contains("Flechette Chamber Boring"))
                {
                    mag.AmmoType = canister.UID;
                }
            }

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

            using (ResXResourceWriter resx = new("ItemNames.resx"))
            {
                foreach (var item in NameList)
                {
                    var node = new ResXDataNode(item.Key.UID.ToString(BLRItem.UID_FORMAT), item.Value) { Comment = item.Key.Category };
                    resx.AddResource(node);
                }
            }
            using (ResXResourceWriter resx = new("ItemTooltips.resx"))
            {
                foreach (var item in TooltipList)
                {
                    var node = new ResXDataNode(item.Key.UID.ToString(BLRItem.UID_FORMAT), item.Value) { Comment = item.Key.Category };
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

        var watch = Stopwatch.StartNew();
        App.CheckAppUpdate();
        LoggingSystem.Log($"[MainWindow]: Update Check took {watch.ElapsedMilliseconds}ms");


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
        LoggingSystem.Log("BLREdit Closed!");
    }

    private static void CreateAllDirectories()
    {
        Directory.CreateDirectory("logs");
        Directory.CreateDirectory("logs\\BLREdit");
        Directory.CreateDirectory("logs\\Client");
        Directory.CreateDirectory("logs\\Proxy");

        Directory.CreateDirectory("Profiles");
        Directory.CreateDirectory("Backup");

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

    static bool versionCheckDone = false;

    private static bool AddAssets(GitHubRelease release)
    {
        if (release.Assets is null || release.Assets.Length < 1) return false;

        bool ass = false, exe = false, jso = false, dll = false, tex = false, cro = false, pat = false;

        foreach (var asset in release.Assets)
        {
            if (exeZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.Equals(exeZip.Info.Name))
            { 
                if (DownloadLinks.ContainsKey(exeZip)) { exe = true; } else { DownloadLinks.Add(exeZip, asset.BrowserDownloadURL); exe = true; }
            }

            if (assetZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.Equals(assetZip.Info.Name))
            {
                if (DownloadLinks.ContainsKey(assetZip)) { ass = true; } else { DownloadLinks.Add(assetZip, asset.BrowserDownloadURL); ass = true; }
            }

            if (jsonZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.Equals(jsonZip.Info.Name))
            {
                if (DownloadLinks.ContainsKey(jsonZip)) { jso = true; } else { DownloadLinks.Add(jsonZip, asset.BrowserDownloadURL); jso = true; }
            }

            if (dllsZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.Equals(dllsZip.Info.Name))
            {
                if (DownloadLinks.ContainsKey(dllsZip)) { dll = true; } else { DownloadLinks.Add(dllsZip, asset.BrowserDownloadURL); dll = true; }
            }

            if (texturesZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.Equals(texturesZip.Info.Name))
            {
                if (DownloadLinks.ContainsKey(texturesZip)) { tex = true; } else { DownloadLinks.Add(texturesZip, asset.BrowserDownloadURL); tex = true; }
            }

            if (crosshairsZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.Equals(crosshairsZip.Info.Name))
            {
                if (DownloadLinks.ContainsKey(crosshairsZip)) { cro = true; } else { DownloadLinks.Add(crosshairsZip, asset.BrowserDownloadURL); cro = true; }
            }

            if (patchesZip is not null && asset.Name is not null && asset.BrowserDownloadURL is not null && asset.Name.Equals(patchesZip.Info.Name))
            {
                if (DownloadLinks.ContainsKey(patchesZip)) { pat = true; } else { DownloadLinks.Add(patchesZip, asset.BrowserDownloadURL); pat = true; }
            }
        }

        if (jso && dll && tex && cro && pat) 
            return true;
        else
            return false;
    }

    public static bool VersionCheck()
    {
        if (versionCheckDone)
        {
            LoggingSystem.Log("Version Check got run again");
            return false;
        }
        versionCheckDone = true;
        LoggingSystem.Log("Running Version Check!");

        try
        {
            using var task = GitHubClient.GetReleases(CurrentOwner, CurrentRepo);
            task.Wait();
            var releases = task.Result;
            if (releases is null) { LoggingSystem.Log("Can't connect to github to check for new Version"); return false; }

            LatestRelease = releases[0];
            
            LoggingSystem.Log($"Newest Version: {LatestRelease.TagName} of {LatestRelease.Name} vs Current: {CurrentVersion} of {CurrentVersionTitle}");

            bool newVersionAvailable = (LatestRelease?.Version ?? -1) > CurrentVersionNumber;
            bool assetFolderMissing = !Directory.Exists(IOResources.ASSET_DIR);
            if (DataStorage.Settings.LastRunVersion is null) { assetFolderMissing = true; }
            DataStorage.Settings.LastRunVersion = CurrentVersion;

            LoggingSystem.Log($"New Version Available:{newVersionAvailable} AssetFolderMissing:{assetFolderMissing}");

            foreach (var release in releases)
            {
                if (release.Version < CurrentVersionNumber) break;
                if (AddAssets(release)) { assetFolderMissing = true; break; }
            }

            if (newVersionAvailable && assetFolderMissing)
            {
                LoggingSystem.MessageLog(BLREdit.Properties.Resources.msg_UpdateMissingFiles, BLREdit.Properties.Resources.msgT_Info);
                bool dl = DownloadAssetFolder();

                UpdateEXE();
                return dl;
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
                return DownloadAssetFolder();
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

    private static bool DownloadAssetFolder()
    {
        if (assetZip is null) { LoggingSystem.Log("[DownloadAssetFolder] failed assetZip was null!"); return false; }
        if (DownloadLinks.TryGetValue(assetZip, out string assetDL))
        {
            if (assetZip.Info.Exists) { assetZip.Info.Delete(); }
            IOResources.DownloadFileMessageBox(assetDL, assetZip.Info.FullName);
            if (Directory.Exists(IOResources.ASSET_DIR)) { Directory.Delete(IOResources.ASSET_DIR, true); }
            ZipFile.ExtractToDirectory(assetZip.Info.FullName, IOResources.ASSET_DIR);
            return false;
        }
        else
        { LoggingSystem.MessageLog("No Asset folder for download available!", "Error"); return false; } //TODO: Add Localization
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
                LoggingSystem.MessageLog($"Failed to Unpack {pack.Name} reason:{error}", "Error");
            }
        }
        else
        { LoggingSystem.Log($"No {pack.Info.Name} to Unpack!"); }
    }

    public static int CreateVersion(string? versionTag)
    {
        if (versionTag is null) return -1;
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
#if DEBUG
        var moduleList = IOResources.Deserialize<RepositoryProxyModule[]>(File.ReadAllText("../../../../Resources/ProxyModules.json"));
        LoggingSystem.Log("Loaded AvailableProxyModule from local file!");
        return moduleList;
#else
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
#endif
    }

    private static async Task<Dictionary<string, string>?> GetAvailableLocalizations()
    {
        LoggingSystem.Log("Downloading AvailableLocalization List!");
#if DEBUG
        var localizations = IOResources.Deserialize<Dictionary<string, string>>(File.ReadAllText("../../../../Resources/Localizations.json"));
        LoggingSystem.Log("Loaded AvailableLocalizations from local file!");
        return localizations;
#else
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
#endif
    }

    private static async Task<List<BLRServer>?> GetDefaultServers()
    {
        LoggingSystem.Log($"Downloading Default Server List!");

#if DEBUG
        var servers = IOResources.Deserialize<List<BLRServer>>(File.ReadAllText("../../../../Resources/ServerList.json"));
        LoggingSystem.Log("Loaded Available Servers from local file!");
        return servers;
#else
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
#endif
    }

    public static Task<T> StartSTATask<T>(Func<T> action)
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
