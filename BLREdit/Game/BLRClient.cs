using BLREdit.API.Export;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.API.Utils;
using BLREdit.Export;
using BLREdit.Game.BLRevive;
using BLREdit.Game.Proxy;
using BLREdit.UI;
using BLREdit.UI.Views;
using BLREdit.UI.Windows;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BLREdit.Game;

public sealed class BLRClient : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events
    private bool hasBeenValidated;
    public UIBool Validate { get; set; } = new UIBool(true);
    public string ConfigName { get; set; } = "default";

    [JsonIgnore] public static readonly BLRServer LocalHost = new();
    [JsonIgnore] public UIBool Patched { get; private set; } = new UIBool(false);
    [JsonIgnore] public UIBool CurrentClient { get; private set; } = new UIBool(false);
    [JsonIgnore] public string ClientVersion { get { if (VersionHashes.TryGetValue(OriginalHash, out string version)) { return version; } else { return "Unknown"; } } }
    [JsonIgnore] public ObservableCollection<Process> RunningClients { get; } = [];
    [JsonIgnore] private Dictionary<string?, BLRProfileSettingsWrapper>? profileSettings;
    [JsonIgnore] public Dictionary<string?, BLRProfileSettingsWrapper> ProfileSettings { get { profileSettings ??= LoadProfiles(); return profileSettings; } }
    [JsonIgnore] public static BitmapImage ClientVersionPart0 { get { return new BitmapImage(new Uri(@"pack://application:,,,/UI/Resources/V.png", UriKind.Absolute)); } }
    [JsonIgnore] public BitmapImage? ClientVersionPart1 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 2) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[1])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage? ClientVersionPart2 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 3) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[2])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage? ClientVersionPart3 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 4) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[3])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage? ClientVersionPart4 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 5) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[4])}.png", UriKind.Absolute)); } return null; } }

    private string? _originalHash;
    public string? OriginalHash {
        get { return _originalHash?.ToUpperInvariant(); }
        set { if (_originalHash != value) { _originalHash = value; OnPropertyChanged(nameof(ClientVersion)); OnPropertyChanged(); } }
    }

    private string? _patchedHash;
    public string? PatchedHash {
        get { return _patchedHash; }
        set { if (_patchedHash != value) { _patchedHash = value; OnPropertyChanged(); } }
    }

    private string? _originalPath;
    public string? OriginalPath {
        get { return _originalPath; }
        set { if (File.Exists(value)) { _originalPath = value; OriginalHash ??= IOResources.CreateFileHash(value); OnPropertyChanged(); } else { LoggingSystem.Log($"[{this}]: not a valid Origin Client Path {value}"); } }
    }

    private string? _patchedPath;
    public string? PatchedPath {
        get { return _patchedPath; }
        set { if (File.Exists(value)) { _patchedPath = value; Patched.Set(true); OnPropertyChanged(); OnPropertyChanged(nameof(PatchedFileInfo)); } else { LoggingSystem.Log($"[{this}]: not a valid Patched Client Path {value}"); } }
    }

    [JsonIgnore]
    public FileInfoExtension? OriginalFileInfo
    {
        get { return OriginalPath != null ? new FileInfoExtension(OriginalPath) : null; }
    }

    [JsonIgnore] public FileInfoExtension? PatchedFileInfo
    {
        get { return PatchedPath != null ? new FileInfoExtension(PatchedPath) : null; }
    }

    [JsonIgnore]
    public DirectoryInfo? ModulesDirectoryInfo
    {
        get { return ModulesPath != null ? new DirectoryInfo(ModulesPath) : null; }
    }

    [JsonIgnore]
    public DirectoryInfo? BLReviveConfigsDirectoryInfo
    {
        get { return BLReviveConfigsPath != null ? new DirectoryInfo(BLReviveConfigsPath) : null; }
    }

    [JsonIgnore]
    public DirectoryInfo? LogsDirectoryInfo
    {
        get { return LogsPath != null ? new DirectoryInfo(LogsPath) : null; }
    }

    private string? _basePath;
    public string? BasePath { get { _basePath ??= GetBasePath(); return _basePath; } }

    [JsonIgnore] private FileInfo _D8INPUT;
    [JsonIgnore] public FileInfo D8INPUT { get { _D8INPUT ??= new($"{Path.GetDirectoryName(OriginalPath)}\\DINPUT8.dll"); return _D8INPUT; } }

    [JsonIgnore] private FileInfo _D8INPUTZCure;
    [JsonIgnore]public FileInfo D8INPUTZCure { get { _D8INPUTZCure ??= new($"{Path.GetDirectoryName(OriginalPath)}\\DINPUT8.dll"); return _D8INPUTZCure; } }

    //private string? _sdkType = "BLRevive";
    //public string? SDKType { get { return _sdkType; } set { _sdkType = value; OnPropertyChanged(); } }

    private string? _BLReviveVersion;
    public string? BLReviveVersion { get { return _BLReviveVersion; } set { _BLReviveVersion = value; OnPropertyChanged(); } }
    public DateTime? SDKVersionDate { get; set; }

    private string? _logsPath;
    public string LogsPath { get { _logsPath ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Logs\\").FullName; return _logsPath; } set { if (Directory.Exists(value)) _logsPath = value; } }
    private string? _configsPath;
    public string BLReviveConfigsPath { get { _configsPath ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\BLRevive\\").FullName; return _configsPath; } set { if (Directory.Exists(value)) _configsPath = value; } }
    private string? _modulesPath;
    public string ModulesPath { get { _modulesPath ??= Directory.CreateDirectory($"{BasePath}Binaries\\Win32\\Modules\\").FullName; return _modulesPath; } set { if (Directory.Exists(value)) _modulesPath = value; } }

    [JsonInclude][System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for Json Deserilization")]
    public ObservableCollection<BLRClientPatch> AppliedPatches { get; set; } = [];

    [JsonInclude][System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for Json Deserilization")]
    public ObservableCollection<ProxyModule> InstalledModules { get; set; } = [];

    [JsonInclude][System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for Json Deserilization")]
    public ObservableCollection<ProxyModule> CustomModules { get; set; } = [];

    [JsonIgnore] public static ObservableCollection<VisualProxyModule> AvailableModules { get { return App.AvailableProxyModules; } }

    public BLRClient()
    {
        LoggingSystem.Log($"[Client Event Handler]: has been invalidated!");
        InstalledModules.CollectionChanged += InstalledModules_CollectionChanged;
    }

    private void InstalledModules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Invalidate();
    }

    public void Invalidate()
    {
        hasBeenValidated = false;
        LoggingSystem.Log($"[{this}]: has been invalidated!");
    }

    public static Dictionary<string?, string> VersionHashes => new()
    {
        {"0F4A732484F566D928C580AFDAE6EF01C002198DD7158CB6DE29B9A4960064C7", "v302"},
        {"DE08147E419ED89D6DB050B4C23FA772338132587F6B533B6233733f9BCE46C3", "v301"},
        {"1742DF917761F9DC01B079AE2AAD78EF2FF17562AF1DAD6AD6EA7CF3622FE7f6", "v300"},
        {"4032ED1C45E717757A280E4CFE2408BB0C4E366676B785F0FFD177C3054C13A5", "v140"},
        {"01890318303354F588D9B89BB1A34C5C49FF881D2515388FCC292B54EB036B58", "v130"},
        {"D4F9CEC736A83F7930F04438344d35FF9F0E57212755974BD51F48FF89D303C4", "v120"},
        {"D0BC0AE14AB4DD9F407DE400DA4F333EE0B6DADF6D68B7504DB3FC46C4BAA59F", "v1100"},
        {"9200705DADDBBC10FEE56DB0586A20DF1ABF4C57A9384A630C578F772F1BD116", "v0993"}
    };

    public override bool Equals(object? obj)
    {
        if (obj is BLRClient client)
        {
            return (client.OriginalPath == OriginalPath);
        }
        else
        {
            return false;
        }
    }

    public override string ToString()
    {
        return $"({ClientVersion}){OriginalPath?.Substring(0, Math.Min(OriginalPath.Length, 24))}";
    }

    #region ProfileSettings
    private Dictionary<string?, BLRProfileSettingsWrapper> LoadProfiles()
    {
        var dict = new Dictionary<string?, BLRProfileSettingsWrapper>();
        var dirs = Directory.EnumerateDirectories($"{BLReviveConfigsPath}");
        foreach (var dir in dirs)
        {
            if (dir.Contains("settings_manager_"))
            {
                var data = dir.Split('\\');
                var name = data[data.Length - 1].Substring(17);

                var onlineProfile = IOResources.DeserializeFile<BLRProfileSettings[]>($"{dir}\\UE3_online_profile.json");
                //var keyBinds = IOResources.DeserializeFile<BLRKeyBindings>($"{dir}\\keybinding.json");

                var profile = new BLRProfileSettingsWrapper(name, onlineProfile);
                dict.Add(name, profile);
            }
        }
        return dict;
    }

    public void UpdateProfileSettings()
    {
        profileSettings = LoadProfiles();
        foreach (var profile in profileSettings)
        {
            ExportSystem.UpdateOrAddProfileSettings(profile.Value.ProfileName, profile.Value);
        }
    }

    public void ApplyProfileSetting(BLRProfileSettingsWrapper profileSettings)
    {
        if (profileSettings is null) return;
        if (ProfileSettings.TryGetValue(profileSettings.ProfileName, out var _))
        {
            ProfileSettings.Remove(profileSettings.ProfileName);
            ProfileSettings.Add(profileSettings.ProfileName, profileSettings);
        }
        else
        {
            Directory.CreateDirectory($"{BLReviveConfigsPath}settings_manager_{profileSettings.ProfileName}");
            ProfileSettings.Add(profileSettings.ProfileName, profileSettings);
        }
        IOResources.SerializeFile($"{BLReviveConfigsPath}settings_manager_{profileSettings.ProfileName}\\UE3_online_profile.json", profileSettings.ProfileSettings.Values.ToArray());
        //IOResources.SerializeFile($"{ConfigFolder}settings_manager_{profileSettings.ProfileName}\\keybinding.json", profileSettings.KeyBindings);
    }

    public void ApplyConfigs()
    {
        var info = new FileInfo($"{BasePath}FoxGame\\Config\\PCConsole\\Cooked\\PCConsole-FoxEngine.ini");
        var file = File.ReadAllText(info.FullName);
        string replaced;
        if (DataStorage.Settings.EnableFramerateSmoothing.Is)
        {
            replaced = file.Replace("MaxSmoothedFrameRate=62", "MaxSmoothedFrameRate=1000");
        }
        else
        {
            replaced = file.Replace("MaxSmoothedFrameRate=1000", "MaxSmoothedFrameRate=62");
        }
        File.WriteAllText(info.FullName, replaced);
    }
    #endregion ProfileSettings

    #region ClientValidation

    /// <summary>
    /// Validates the client
    /// </summary>
    /// <returns>Will return true if patching was succesful or is ready to use, false if patching failed or can't be used</returns>
    public bool ValidateClient()
    {
        if (!OriginalFileValidation())
        {
            LoggingSystem.MessageLog($"Client is not valid, original file is missing!\nMaybe client got moved or deleted\nClient can't be patched!", "Error"); //TODO: Add Localization
            return false;
        }

        var info = new FileInfo(new FileInfo(OriginalPath).Directory.FullName + "\\steam_appid.txt");
        if (DataStorage.Settings?.SteamAwareToggle.Is ?? false && !info.Exists)
        {
            using var file = info.CreateText();
            file.Write("209870");
            file.Close();
        }

        LoggingSystem.Log($"Client is in Good Health!");
        return true;
    }

    public bool OriginalFileValidation()
    {
        return !string.IsNullOrEmpty(OriginalPath) && File.Exists(OriginalPath);
    }

    public void RemoveModule(string moduleCacheName)
    {
        try
        {
            LoggingSystem.Log($"Removing {moduleCacheName}");

            foreach (var module in InstalledModules)
            {
                if (module.CacheName == moduleCacheName)
                {
                    InstalledModules.Remove(module);
                    Invalidate();
                    File.Delete($"{ModulesPath}\\{module.InstallName}.dll");
                    break;
                }
            }
        }
        catch (Exception error)
        {
            LoggingSystem.MessageLog($"Failed to remove {moduleCacheName} reason:\n{error.Message}", "Error"); //TODO: Add Localization
            LoggingSystem.Log(error.StackTrace);
        }
    }

    public void RemoveAllModules()
    {
        LoggingSystem.Log($"Removing All Modules from client:[{this}]");
        CustomModules.Clear();
        InstalledModules.Clear();
        Invalidate();
        LoggingSystem.Log("Finished Removing all modules and invalidating client");
    }

    private static void GetLatestBLRevivePackages()
    {
        var task = Task.Run(() => GitlabClient.GetGenericPackages("blrevive", "blrevive", "blrevive"));
        task.Wait();
        if (task.Result is null || task.Result.Length <= 0) { LoggingSystem.Log("Failed to get BLRevive packages"); return; }
        _latestBLRevivePackage = SelectLatestPackage(task.Result);
        var task2 = Task.Run(() => GitlabClient.GetLatestPackageFile(_latestBLRevivePackage, $"BLRevive.dll"));
        task2.Wait();
        if (task2.Result is null) { LoggingSystem.Log("Failed to get BLRevive package file"); return; }
        _latestBLRevivePackageFile = task2.Result;
    }

    private static GitlabPackage SelectLatestPackage(GitlabPackage[] packages)
    {
        if (DataStorage.Settings.SelectedBLReviveVersion == "Beta") { return packages[0]; }
        foreach (var package in packages)
        {
            if (package is null || package.Version is null || package.Version.IndexOf("beta", StringComparison.OrdinalIgnoreCase) >= 0) { continue; }
            else { return package; }
        }
        return packages[0];
    }

    static GitlabPackage? _latestBLRevivePackage;
    static GitlabPackage? LatestBLRevivePackage { get { if (_latestBLRevivePackage is null || _latestBLRevivePackageFile is null) { GetLatestBLRevivePackages(); } return _latestBLRevivePackage; } }

    static GitlabPackageFile? _latestBLRevivePackageFile;
    static GitlabPackageFile? LatestBLRevivePackageFile { get { if (_latestBLRevivePackage is null || _latestBLRevivePackageFile is null) { GetLatestBLRevivePackages(); } return _latestBLRevivePackageFile; } }
    private bool CheckBLReviveVersionIsUpToDateAndExists()
    {
        if (SDKVersionDate is null || string.IsNullOrEmpty(BLReviveVersion)) return false;
        var sdk = File.Exists($"{Path.GetDirectoryName(OriginalPath)}\\BLRevive.dll");

        if (!D8INPUT.Exists && D8INPUTZCure.Exists) { D8INPUTZCure.MoveTo(D8INPUT.FullName); }

        if (!sdk || !D8INPUT.Exists) return false;
        var d = LatestBLRevivePackageFile?.CreatedAt ?? DateTime.MinValue;
        return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second) <= SDKVersionDate;
    }
    
    public void ValidateBLReviveModuleSDK()
    {
        if (CheckBLReviveVersionIsUpToDateAndExists()) { LoggingSystem.Log($"BLRevive is Uptodate!"); return; } else { LoggingSystem.Log("BLRevive needs to Update!"); }
        MainWindow.BLREditAlert? SDKAnim = null;
        MainWindow.Instance?.Dispatcher.Invoke(new Action(() => { LoggingSystem.ResetWatch(); SDKAnim = MainWindow.ShowAlert("Updating BLRevive SDK!", 60, 600); }));
        RemoveAllModules();
        
        var proxySource = string.Empty;
        var proxyTarget = string.Empty;

        LoggingSystem.Log($"Downloading latest BLRevive release!");
        if (LatestBLRevivePackage is null) { LoggingSystem.Log("Can't update BLRevive no packages available!"); return; }
        var reiveResult = GitlabClient.DownloadPackage(LatestBLRevivePackage, "BLRevive.dll", "BLRevive");
        var dInputResult = GitlabClient.DownloadPackage(LatestBLRevivePackage, "DINPUT8.dll", "DINPUT8");
        LoggingSystem.Log($"Finished downloading latest BLRevive release!");
        if (reiveResult.Item1 && dInputResult.Item1)
        {
            LoggingSystem.Log($"Installing latest BLRevive version ({reiveResult.Item3}) before ({SDKVersionDate})");
            DataStorage.Settings.SDKVersionDate = reiveResult.Item3;
            SDKVersionDate = reiveResult.Item3;
            try
            {
                File.Copy($"{IOResources.BaseDirectory}{dInputResult.Item2}", $"{Path.GetDirectoryName(OriginalPath)}\\DINPUT8.dll", true);
                File.Copy($"{IOResources.BaseDirectory}{reiveResult.Item2}", $"{Path.GetDirectoryName(OriginalPath)}\\BLRevive.dll", true);
            }
            catch (Exception error)
            {
                LoggingSystem.MessageLogClipboard($"Message:\n{error.Message}\nStacktrace:{error.StackTrace}\n", "Failed to copy essential dll files to BLR client!");
            }
        }
        BLReviveVersion = DataStorage.Settings.SelectedBLReviveVersion;
        MainWindow.Instance?.Dispatcher.Invoke(new Action(() => { MainWindow.UpdateAlert(SDKAnim, $"Finished Updating BLRevive SDK! Took: {LoggingSystem.GetElapsedSeconds()}", 8); }));
    }

    private void InstallRequiredModules()
    {
        List<Task> moduleInstallTasks = [];
        foreach (var availableModule in App.AvailableProxyModules)
        {
            if (availableModule.RepositoryProxyModule.Required && availableModule.RepositoryProxyModule.ProxyVersion.Equals(DataStorage.Settings.SelectedSDKType, StringComparison.Ordinal))
            {
                moduleInstallTasks.Add(Task.Run(() => { availableModule.InstallModule(this); }));
            }
        }
        if (moduleInstallTasks.Count > 0) Task.WaitAll([.. moduleInstallTasks]);

        LoggingSystem.Log("Finalizing install of BLRevive Modules");

        foreach (var availableModule in App.AvailableProxyModules)
        {
            if (availableModule.RepositoryProxyModule.Required && availableModule.RepositoryProxyModule.ProxyVersion.Equals(DataStorage.Settings.SelectedSDKType, StringComparison.Ordinal))
            {
                availableModule.FinalizeInstall(this);
            }
        }
    }

    public void ValidateModules(List<ProxyModule>? enabledModules = null)
    {
        
        MainWindow.BLREditAlert? ModuleAnim = null;
        MainWindow.Instance?.Dispatcher.Invoke(new Action(() => { LoggingSystem.ResetWatch(); ModuleAnim = MainWindow.ShowAlert("Validating BLRevive Modules!", 60, 600); }));
        App.AvailableProxyModuleCheck(); // Get Available Modules just in case
        var count = InstalledModules.Count;
        var customCount = CustomModules.Count;
        LoggingSystem.Log($"Available Modules:{App.AvailableProxyModules.Count}, StrictModuleCheck:{DataStorage.Settings.StrictModuleChecks}, AllowCustomModules:{DataStorage.Settings.AllowCustomModules}, InstallRequiredModules:{DataStorage.Settings.InstallRequiredModules}");

        if (App.AvailableProxyModules.Count > 0 && DataStorage.Settings.InstallRequiredModules.Is)
        {
            LoggingSystem.Log($"Started Installing Required Modules");
            InstallRequiredModules();
            LoggingSystem.Log($"Finished Installing Required Modules!");
        }

        if (App.AvailableProxyModules.Count > 0 && DataStorage.Settings.StrictModuleChecks.Is)
        {
            LoggingSystem.Log($"Filtering Installed Modules");
            InstalledModules = new(InstalledModules.Where((module) => { bool isAvailable = false; foreach (var available in App.AvailableProxyModules) { if (available.RepositoryProxyModule.CacheName == module.CacheName) { module.Server = available.RepositoryProxyModule.Server; module.Client = available.RepositoryProxyModule.Client; isAvailable = true; } } return isAvailable; }));
        }

        foreach (var file in Directory.EnumerateFiles(ModulesPath))
        {
            var info = new FileInfo(file);
            if (info.Extension == ".dll")
            {
                var name = info.Name.Split('.')[0];
                bool isInstalled = false;
                foreach (var module in InstalledModules)
                {
                    if (name == module.InstallName)
                    { isInstalled = true; break; }
                }
                if (DataStorage.Settings.AllowCustomModules.Is && !isInstalled)
                {
                    bool isNew = true;
                    foreach (var module in CustomModules)
                    {
                        if (name == module.InstallName)
                        { isNew = false; module.FileAppearances++; break; }
                    }
                    if (isNew)
                    { CustomModules.Add(new(name, "")); }
                }
            }
        }

        List<ProxyModule> toRemove = [];

        foreach (var module in CustomModules) { if (module.FileAppearances <= 0) { toRemove.Add(module); } }

        foreach (var module in toRemove)
        { CustomModules.Remove(module); }

        LoggingSystem.Log($"Validating Modules Installed({count}/{InstalledModules.Count}) and Custom({customCount}/{CustomModules.Count}) of {this}");

        var configClient = IOResources.DeserializeFile<BLReviveConfig>($"{BLReviveConfigsPath}{ConfigName}-Client.json") ?? new();
        var configServer = IOResources.DeserializeFile<BLReviveConfig>($"{BLReviveConfigsPath}{ConfigName}-Server.json") ?? new();
        var config = IOResources.DeserializeFile<BLReviveConfig>($"{BLReviveConfigsPath}{ConfigName}.json") ?? new();

        var configClientCopy = configClient.Copy();
        var configServerCopy = configServer.Copy();
        var configCopy = config.Copy();
        
        configClient.Modules.Clear();
        configServer.Modules.Clear();
        config.Modules.Clear();
        LoggingSystem.Log($"Applying Installed Modules:");

        if (enabledModules is null)
        {
            enabledModules = [.. InstalledModules];
            if (DataStorage.Settings.AllowCustomModules.Is)
            {
                enabledModules.AddRange([.. CustomModules]);
            }
        }

        foreach (var module in enabledModules)
        {
            LoggingSystem.Log($"\t{module.CacheName}:");
            LoggingSystem.Log($"\t\tClient:{module.Client}");
            LoggingSystem.Log($"\t\tServer:{module.Server}");
            if (module.Client)
            {
                AddModuleConfigAndKeepSettings(configClient, configClientCopy, module.CacheName, module.InstallName);
            }
            if (module.Server)
            {
                AddModuleConfigAndKeepSettings(configServer, configServerCopy, module.CacheName, module.InstallName);
            }
            AddModuleConfigAndKeepSettings(config, configCopy, module.CacheName, module.InstallName);
        }
        try
        {
            IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}-Client.json", configClient);
            IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}-Server.json", configServer);
            IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}.json", config);
        }
        catch { LoggingSystem.Log("failed to write config files!"); }

        LoggingSystem.Log($"Finished Validating Modules of {this}");
        MainWindow.Instance?.Dispatcher.Invoke(new Action(() => { MainWindow.UpdateAlert(ModuleAnim, $"Finished Validating BLRevive Modules! Took: {LoggingSystem.GetElapsedSeconds()}", 8); }));
    }

    private static void AddModuleConfigAndKeepSettings(BLReviveConfig config, BLReviveConfig old, string moduleName, string installName)
    {
        RepositoryProxyModule? moduleMeta = null;
        foreach (var mod in App.AvailableProxyModules)
        {
            if (mod.RepositoryProxyModule.CacheName == moduleName) { moduleMeta = mod.RepositoryProxyModule; break; }
        }
        JsonObject? oldSettingsJsonObject = null;
        foreach (var mod in old.Modules)
        {
            if (mod.Key == installName) { oldSettingsJsonObject = mod.Value; break; }
        }

        Dictionary<string, JsonNode?> newSettingsDictonary = [];

        if (oldSettingsJsonObject != null)
        {
            foreach (var setting in oldSettingsJsonObject)
            {
                if (setting.Value != null) { newSettingsDictonary.Add(setting.Key, setting.Value.DeepClone()); }
            }
            
        }

        if (moduleMeta != null)
        {
            foreach (var moduleSetting in moduleMeta.ModuleSettings)
            {
                if (moduleSetting.CreateDefaultSetting() is KeyValuePair<string, JsonNode?> node)
                {
                    if (!newSettingsDictonary.ContainsKey(node.Key))
                    {
                        newSettingsDictonary.Add(node.Key, node.Value);
                    }
                }
            }
        }

        var newSettingsJsonObject = new JsonObject();
        foreach (var setting in newSettingsDictonary)
        {
            newSettingsJsonObject.Add(setting.Key, setting.Value);
        }
        if (config.Modules.ContainsKey(installName)) 
        { LoggingSystem.Log("Wow!"); }
        config.Modules.Add(installName, newSettingsJsonObject);
    }

    public static bool ValidateClientHash(string? currentHash, string? fileLocation, out string? newHash)
    {
        if (currentHash is null || string.IsNullOrEmpty(currentHash) || string.IsNullOrEmpty(fileLocation)) { newHash = null; return false; }
        newHash = IOResources.CreateFileHash(fileLocation);
        return currentHash.Equals(newHash, StringComparison.OrdinalIgnoreCase);
    }

    #endregion ClientValidation

    #region Commands

    private ICommand? launchZCure;
    [JsonIgnore]
    public ICommand LaunchZCure
    {
        get
        {
            launchZCure ??= new RelayCommand(
                    param => this.LaunchZCureClient()
                );
            return launchZCure;
        }
    }

    private ICommand? launchTrainingCommand;
    [JsonIgnore]
    public ICommand LaunchTrainingCommand
    {
        get
        {
            launchTrainingCommand ??= new RelayCommand((param) => {
                string launchArgs = $"server gunrange_persistent{(string.IsNullOrEmpty(ConfigName) ? "" : $"?config={ConfigName}-Server")}?Game=FoxGame.FoxGameMP_BO?ServerName=Training?Port=7777?NumBots=0?MaxPlayers=1?SingleMatch";
                var options = new LaunchOptions() { UserName = DataStorage.Settings.PlayerName, Server = LocalHost };
                StartProcess(launchArgs, true, DataStorage.Settings.ServerWatchDog.Is);
                PrepClientLaunch(options);
                LaunchClient(options);
            });
            return launchTrainingCommand;
        }
    }

    private ICommand? modifyClientCommand;
    [JsonIgnore]
    public ICommand ModifyClientCommand
    {
        get
        {
            modifyClientCommand ??= new RelayCommand(
                    param => this.ModifyClient()
                );
            return modifyClientCommand;
        }
    }

    private ICommand? currentClientCommand;
    [JsonIgnore]
    public ICommand CurrentClientCommand
    {
        get
        {
            currentClientCommand ??= new RelayCommand(
                    param => this.SetCurrentClient()
                );
            return currentClientCommand;
        }
    }
    #endregion Commands

    #region Launch/Exit
    public void LaunchClient()
    {
        LaunchClient(BLREditSettings.DefaultLaunchOptions);
    }

    public void LaunchZCureClient()
    {
        if (D8INPUT.Exists)
        {
            if (D8INPUTZCure.Exists) { D8INPUTZCure.Delete(); }
            D8INPUT.MoveTo(D8INPUTZCure.FullName);
        }
        BLRProcess.CreateProcess("-zcureurl=blrrevive.ddd-game.de -zcureport=80 -presenceurl=blrrevive.ddd-game.de -presenceport=9004", this, false);
    }

    public void PrepClientLaunch(LaunchOptions options) {
        if(options is null) { LoggingSystem.LogNull(); return; }
        MainWindow.ApplyBLReviveLoadouts(this);
        ApplyProfileSetting(ExportSystem.GetOrAddProfileSettings(options.UserName));
        ApplyConfigs();
    }

    public void LaunchClient(LaunchOptions options)
    {
        if (options is null) { LoggingSystem.LogNull(); return; }
        string launchArgs = options.Server.IPAddress + ':' + options.Server.Port;
        launchArgs += $"?Name={options.UserName}{(string.IsNullOrEmpty(ConfigName) ? "" : $"?config={ConfigName}-Client")}";
        StartProcess(launchArgs, false, false, null, options.Server);
    }

    public static bool ValidProfile(BLRProfile profile, BLRServer server, ref string message)
    {
        if (server is null || profile is null) { LoggingSystem.LogNull(); return false; }
        var isValid = true;
        if (!server.ValidatesLoadout)
        {
            if (!profile.Loadout1.ValidateLoadout(ref message)) isValid = false;
            if (!profile.Loadout2.ValidateLoadout(ref message)) isValid = false;
            if (!profile.Loadout3.ValidateLoadout(ref message)) isValid = false;
        }
        return isValid;
    }

    public void StartProcess(string launchArgs, bool isServer = false, bool watchDog = false, List<ProxyModule>? enabledModules = null, BLRServer? server = null)
    {
        Task.Run(() => { StartProcessAsync(launchArgs, isServer, watchDog, enabledModules, server); }).ConfigureAwait(false);
    }

    public async void StartProcessAsync(string launchArgs, bool isServer = false, bool watchDog = false, List<ProxyModule>? enabledModules = null, BLRServer? server = null)
    {
        if (!hasBeenValidated && Validate.Is)
        {
            if (!ValidateClient()) { return; }
            ValidateBLReviveModuleSDK();
            ValidateModules(enabledModules);
            DataStorage.Save();
            hasBeenValidated = true;
        }
        else
        {
            LoggingSystem.Log($"[{this}]: has already been validated!");
        }
        MainWindow.Instance?.Dispatcher.Invoke(new Action(() => { MainWindow.ShowAlert($"Starting BLR {(isServer ? "Server" : "Client")}!"); }));
        BLRProcess.CreateProcess(launchArgs, this, isServer, watchDog, server);
    }

    #endregion Launch/Exit

    private void ModifyClient()
    {
        App.AvailableProxyModuleCheck();
        MainWindow.ClientWindow.Client = this;
        MainWindow.ClientWindow.ShowDialog();
    }

    public void SetCurrentClient()
    {
        LoggingSystem.Log($"Setting Current Client:{this}");
        DataStorage.Settings.DefaultClient = this;
        foreach (BLRClient c in DataStorage.GameClients)
        {
            c.CurrentClient.Set(false);
        }
        this.CurrentClient.Set(true);
    }

    private string? GetBasePath()
    {
        if (OriginalPath is null || string.IsNullOrEmpty(OriginalPath)) { return null; }
        string[] pathParts = OriginalPath.Split('\\');
        string[] fileParts = pathParts[pathParts.Length - 1].Split('.');
        pathParts[pathParts.Length - 1] = fileParts[0] + "-BLREdit-Patched." + fileParts[1];
        string basePath = "";
        for (int i = pathParts.Length - 4; i >= 0; i--)
        {
            basePath = $"{pathParts[i]}\\{basePath}";
        }

        if (!basePath.EndsWith("\\", StringComparison.Ordinal)) { basePath += "\\"; }

        _patchedPath ??= $"{basePath}Binaries\\Win32\\{fileParts[0]}-BLREdit-Patched.{fileParts[1]}";

        return basePath;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    internal ObservableCollection<ProxyModuleSetting>? LoadModuleSettings(VisualProxyModule module)
    {
        var config = IOResources.DeserializeFile<BLReviveConfig>($"{BLReviveConfigsPath}{ConfigName}.json") ?? new();

        if (config.Modules.TryGetValue(module.RepositoryProxyModule.InstallName, out var value) && value is not null)
        {
            module.RepositoryProxyModule.ReadSettings(value);
        }

        return module.RepositoryProxyModule.ModuleSettings;
    }

    internal void SaveModuleSettings(VisualProxyModule module)
    {
        var configClient = IOResources.DeserializeFile<BLReviveConfig>($"{BLReviveConfigsPath}{ConfigName}-Client.json") ?? new();
        var configServer = IOResources.DeserializeFile<BLReviveConfig>($"{BLReviveConfigsPath}{ConfigName}-Server.json") ?? new();
        var config = IOResources.DeserializeFile<BLReviveConfig>($"{BLReviveConfigsPath}{ConfigName}.json") ?? new();

        if (module.RepositoryProxyModule.Client)
        {
            if (configClient.Modules.TryGetValue(module.RepositoryProxyModule.InstallName, out var client) && client is not null)
            {
                configClient.Modules.Remove(module.RepositoryProxyModule.InstallName);
                var settings = module.RepositoryProxyModule.GetCurrentSettings();
                module.RepositoryProxyModule.CombineSettings(settings, client);
                configClient.Modules.Add(module.RepositoryProxyModule.InstallName, settings);
            }
        }

        if (module.RepositoryProxyModule.Server)
        {
            if (configServer.Modules.TryGetValue(module.RepositoryProxyModule.InstallName, out var server) && server is not null)
            {
                configServer.Modules.Remove(module.RepositoryProxyModule.InstallName);
                var settings = module.RepositoryProxyModule.GetCurrentSettings();
                module.RepositoryProxyModule.CombineSettings(settings, server);
                configServer.Modules.Add(module.RepositoryProxyModule.InstallName, settings);
            }
        }

        if (config.Modules.TryGetValue(module.RepositoryProxyModule.InstallName, out var value) && value is not null)
        {
            config.Modules.Remove(module.RepositoryProxyModule.InstallName);
            var settings = module.RepositoryProxyModule.GetCurrentSettings();
            module.RepositoryProxyModule.CombineSettings(settings, value);
            config.Modules.Add(module.RepositoryProxyModule.InstallName, settings);
        }

        try
        {
            IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}-Client.json", configClient);
            IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}-Server.json", configServer);
            IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}.json", config);
        }
        catch (UnauthorizedAccessException notAuthorized) { LoggingSystem.MessageLog($"failed to write config file not Authorized!\nconfig file might be read only!\n{notAuthorized.Message}\n{notAuthorized.StackTrace}", "Error"); }
        catch (Exception error) { LoggingSystem.MessageLog($"failed to write config file!\n{error.Message}\n{error.StackTrace}", "Error"); }

    }
}