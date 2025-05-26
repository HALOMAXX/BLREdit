using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using BLREdit.API.Export;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.API.Utils;
using BLREdit.Export;
using BLREdit.Game.BLRevive;
using BLREdit.Game.Proxy;
using BLREdit.UI;
using BLREdit.UI.Views;
using BLREdit.UI.Windows;

using PeNet;
using PeNet.Header.Resource;

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

    [JsonIgnore] private readonly BLRServer LocalHost = new();
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
        get { return _originalHash; }
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

    public void RemoveModule(string moduleInstallName)
    {
        try
        {
            LoggingSystem.Log($"Removing {moduleInstallName}");

            foreach (var module in InstalledModules)
            {
                if (module.InstallName == moduleInstallName)
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
            LoggingSystem.MessageLog($"Failed to remove {moduleInstallName} reason:\n{error.Message}", "Error"); //TODO: Add Localization
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
    private bool CheckBLReviveVersion()
    {
        if (SDKVersionDate is not null && BLReviveVersion is not null)
        {
            var d = LatestBLRevivePackageFile?.CreatedAt ?? DateTime.MinValue;
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second) > SDKVersionDate;
        }
        else
        {
            return true;
        }
    }

    public void ValidateProxy()
    {
        if (!CheckBLReviveVersion()) { LoggingSystem.Log($"BLRevive is Uptodate!"); return; } else { LoggingSystem.Log("BLRevive needs to Update!"); }
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
                File.Copy($"{IOResources.BaseDirectory}{dInputResult.Item2}", $"{Path.GetDirectoryName(PatchedPath)}\\DINPUT8.dll", true);
            }
            catch {}

            proxySource = $"{IOResources.BaseDirectory}{reiveResult.Item2}";
            proxyTarget = $"{Path.GetDirectoryName(PatchedPath)}\\BLRevive.dll";
        }
         
        if (File.Exists(proxySource))
        {
            try
            {
                File.Copy(proxySource, proxyTarget, true);
            }
            catch { }
        }
        BLReviveVersion = DataStorage.Settings.SelectedBLReviveVersion;
    }

    public bool ValidatePatches()
    {
        if (DataStorage.Settings.SelectedBLReviveVersion == "BLRevive") return false;
        bool needUpdatedPatches = false;

        if (BLRClientPatch.AvailablePatches.TryGetValue(this.OriginalHash, out List<BLRClientPatch> patches))
        {
            if (AppliedPatches.Count > 0)
            {
                foreach (var installedPatch in AppliedPatches)
                {
                    bool isValid = false;
                    foreach (var patch in patches)
                    {
                        if (installedPatch.Equals(patch)) isValid = true;
                    }
                    if (!isValid) { needUpdatedPatches = true; LoggingSystem.Log($"found old patch {installedPatch.PatchName}"); }
                }
            }
            else
            {
                LoggingSystem.Log($"no installed patches for {OriginalHash}");
                needUpdatedPatches = true;
            }
        }
        else
        {
            LoggingSystem.Log($"No patches found for {OriginalHash}");
            needUpdatedPatches = true;
        }
        return needUpdatedPatches;
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

    public bool IsModuleInstalledAndUpToDate(VisualProxyModule module)
    {
        if (module is null) return false;
        foreach (var installedModule in InstalledModules)
        {
            if (installedModule.InstallName == module.RepositoryProxyModule.InstallName && installedModule.Published >= module.ReleaseDate)
            {
                return true;
            }
        }
        return false;
    }

    public void ValidateModules(List<ProxyModule>? enabledModules = null)
    {
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
            InstalledModules = new(InstalledModules.Where((module) => { bool isAvailable = false; foreach (var available in App.AvailableProxyModules) { if (available.RepositoryProxyModule.InstallName == module.InstallName) { module.Server = available.RepositoryProxyModule.Server; module.Client = available.RepositoryProxyModule.Client; isAvailable = true; } } return isAvailable; }));
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
            LoggingSystem.Log($"\t{module.InstallName}:");
            LoggingSystem.Log($"\t\tClient:{module.Client}");
            LoggingSystem.Log($"\t\tServer:{module.Server}");
            if (module.Client)
            {
                AddModuleConfigAndKeepSettings(configClient, configClientCopy, module);
            }
            if (module.Server)
            {
                AddModuleConfigAndKeepSettings(configServer, configServerCopy, module);
            }
            AddModuleConfigAndKeepSettings(config, configCopy, module);
        }
        IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}-Client.json", configClient);
        IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}-Server.json", configServer);
        IOResources.SerializeFile($"{BLReviveConfigsPath}{ConfigName}.json", config);

        LoggingSystem.Log($"Finished Validating Modules of {this}");
    }

    private static void AddModuleConfigAndKeepSettings(BLReviveConfig config, BLReviveConfig old, ProxyModule module)
    {
        RepositoryProxyModule? moduleMeta = null;
        foreach (var mod in App.AvailableProxyModules)
        {
            if (mod.RepositoryProxyModule.InstallName == module.InstallName) { moduleMeta = mod.RepositoryProxyModule; break; }
        }
        JsonObject? settings = null;
        foreach (var mod in old.Modules)
        {
            if (mod.Key == module.InstallName) { settings = mod.Value; break; }
        }

        Dictionary<string, JsonNode?> newSettingsDictonary = [];

        if (settings != null)
        {
            foreach (var setting in settings)
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

        var Settings = new JsonObject();
        foreach (var setting in newSettingsDictonary)
        {
            Settings.Add(setting.Key, setting.Value);
        }

        config.Modules.Add(module.InstallName, Settings);
    }

    public static bool ValidateClientHash(string? currentHash, string? fileLocation, out string? newHash)
    {
        if (currentHash is null || string.IsNullOrEmpty(currentHash) || string.IsNullOrEmpty(fileLocation)) { newHash = null; return false; }
        newHash = IOResources.CreateFileHash(fileLocation);
        return currentHash.Equals(newHash, StringComparison.Ordinal);
    }

    #endregion ClientValidation

    #region Commands

    private ICommand? launchClientCommand;
    [JsonIgnore]
    public ICommand LaunchClientCommand
    {
        get
        {
            launchClientCommand ??= new RelayCommand(
                    param => this.LaunchClient()
                );
            return launchClientCommand;
        }
    }

    private ICommand? launchServerCommand;
    [JsonIgnore]
    public ICommand LaunchServerCommand
    {
        get
        {
            launchServerCommand ??= new RelayCommand(
                    param => this.LaunchServer()
                );
            return launchServerCommand;
        }
    }

    private ICommand? launchBotMatchCommand;
    [JsonIgnore]
    public ICommand LaunchBotMatchCommand
    {
        get
        {
            launchBotMatchCommand ??= new RelayCommand(
                    param => this.LaunchBotMatch()
                );
            return launchBotMatchCommand;
        }
    }

    private ICommand? launchSafeMatchCommand;
    [JsonIgnore]
    public ICommand LaunchSafeMatchCommand
    {
        get
        {
            launchSafeMatchCommand ??= new RelayCommand(
                    param => {
                        string launchArgs = $"server containment containment containment containment";
                        StartProcess(launchArgs, true, false);
                    }
                );
            return launchSafeMatchCommand;
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
                StartProcess(launchArgs, true, DataStorage.Settings.ServerWatchDog.Is);
                LaunchClient(new LaunchOptions() { UserName = DataStorage.Settings.PlayerName, Server = LocalHost });
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
    private bool SelectMapAndMode(out string launchArgs)
    {
        launchArgs = "";
        (var mode, var map, var canceled) = MapModeSelect.SelectMapAndMode(this.ClientVersion);
        if (canceled) { LoggingSystem.Log($"Canceled Server Launch"); return false; }
        //string launchArgs = $"server {map?.MapName ?? "helodeck"}{(string.IsNullOrEmpty(ConfigName) ? "" : $"?config={ConfigName}-Server")}?Game=FoxGame.FoxGameMP_{mode?.ModeName ?? "DM"}?ServerName=BLREdit-{mode?.ModeName ?? "DM"}-Server?Port=7777?NumBots={DataStorage.Settings.BotCount}?MaxPlayers={DataStorage.Settings.PlayerCount}";

        if (map is null || mode is null) { LoggingSystem.MessageLog($"Failed to launch server as no {((map is null && mode is null) ? "Map and Mode" : ((map is null) ? "Map" : ((mode is null) ? "Mode" : "")))} was selected!", "Failed to launch Server/Bot Match"); return false; }
        if (string.IsNullOrEmpty(mode.ModeName)) { LoggingSystem.MessageLog("Faile to launch server ModeName is missing in files please send a error report in Discord!", "Failed to launch Server/Bot Match"); return false; }

        launchArgs = $"server {(string.IsNullOrEmpty(ConfigName) ? "" : $"?config={ConfigName}-Server")}?ServerName=BLREdit-{mode.ModeName}-Server?Playlist=BLREditPlaylist?Port=7777?blre.server.authenticateusers=false";

        List<BLRPlaylistEntry> entries = [new() { Map = map.MapName, GameMode = mode.ModeName, Properties = new() { MaxPlayers = DataStorage.Settings.PlayerCount, MaxBotCount = DataStorage.Settings.BotCount, NumBots = DataStorage.Settings.BotCount } }];

        IOResources.SerializeFile($"{BLReviveConfigsPath}server_utils\\playlists\\BLREditPlaylist.json", entries);
        return true;
    }

    private void LaunchBotMatch()
    {
        if (SelectMapAndMode(out var launchArgs))
        {
            StartProcess(launchArgs, true, DataStorage.Settings.ServerWatchDog.Is);
            LaunchClient(new LaunchOptions() { UserName = DataStorage.Settings.PlayerName, Server = LocalHost });
        }        
    }

    private void LaunchServer()
    {
        if (SelectMapAndMode(out var launchArgs))
        {
            StartProcess(launchArgs, true, DataStorage.Settings.ServerWatchDog.Is);
        }
    }

    public void LaunchClient()
    {
        LaunchClient(BLREditSettings.DefaultLaunchOptions);
    }

    public void LaunchClient(LaunchOptions options)
    {
        if(options is null) { LoggingSystem.LogNull(); return; }
        MainWindow.ApplyBLReviveLoadouts(this);
        ApplyProfileSetting(ExportSystem.GetOrAddProfileSettings(options.UserName));
        ApplyConfigs();
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
        if (!hasBeenValidated && Validate.Is)
        {
            if (!ValidateClient()) { return; }
            ValidateProxy();
            ValidateModules(enabledModules);
            DataStorage.Save();
            hasBeenValidated = true;
        }
        else
        {
            LoggingSystem.Log($"[{this}]: has already been validated!");
        }
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
}