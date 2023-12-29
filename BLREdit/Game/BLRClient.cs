using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;
using BLREdit.UI.Windows;

using PeNet;

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
    private bool hasBeenValidated = false;
    public UIBool Validate { get; set; } = new UIBool(true);
    public string ConfigName { get; set; } = "default";

    [JsonIgnore] private readonly BLRServer LocalHost = new() { ServerAddress = "localhost", Port = 7777, AllowAdvanced = true, AllowLMGR = true };
    [JsonIgnore] public UIBool Patched { get; private set; } = new UIBool(false);
    [JsonIgnore] public UIBool CurrentClient { get; private set; } = new UIBool(false);
    [JsonIgnore] public string ClientVersion { get { if (VersionHashes.TryGetValue(OriginalHash, out string version)) { return version; } else { return "Unknown"; } } }
    [JsonIgnore] public ObservableCollection<Process> RunningClients = new();
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
        set { if (File.Exists(value)) { _patchedPath = value; Patched.Set(true); OnPropertyChanged(); OnPropertyChanged(nameof(PatchedFile)); } else { LoggingSystem.Log($"[{this}]: not a valid Patched Client Path {value}"); } }
    }

    [JsonIgnore] public FileInfoExtension? PatchedFile
    {
        get { return new(PatchedPath); }
    }

    private string? _basePath;
    public string? BasePath { get { _basePath ??= GetBasePath(); return _basePath; } }

    private string? _sdkType = "v1.0.0-beta.2";
    public string? SDKType { get { return _sdkType; } set { _sdkType = value; OnPropertyChanged(); } }
    public DateTime? SDKVersionDate { get; set; }

    private string? _configFolder;
    public string ConfigFolder { get { _configFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\BLRevive\\").FullName; return _configFolder; } set { if (Directory.Exists(value)) _configFolder = value; } }
    private string? _modulesFolder;
    public string ModulesFolder { get { _modulesFolder ??= Directory.CreateDirectory($"{BasePath}Binaries\\Win32\\Modules\\").FullName; return _modulesFolder; } set { if (Directory.Exists(value)) _modulesFolder = value; } }

    public ObservableCollection<BLRClientPatch> AppliedPatches { get; set; } = new();

    public ObservableCollection<ProxyModule> InstalledModules { get; set; } = new();

    public ObservableCollection<ProxyModule> CustomModules { get; set; } = new();

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
        {"0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7", "v302"},
        {"de08147e419ed89d6db050b4c23fa772338132587f6b533b6233733f9bce46c3", "v301"},
        {"1742df917761f9dc01b079ae2aad78ef2ff17562af1dad6ad6ea7cf3622fe7f6", "v300"},
        {"4032ed1c45e717757a280e4cfe2408bb0c4e366676b785f0ffd177c3054c13a5", "v140"},
        {"01890318303354f588d9b89bb1a34c5c49ff881d2515388fcc292b54eb036b58", "v130"},
        {"d4f9cec736a83f7930f04438344d35ff9f0e57212755974bd51f48ff89d303c4", "v120"},
        {"d0bc0ae14ab4dd9f407de400da4f333ee0b6dadf6d68b7504db3fc46c4baa59f", "v1100"},
        {"9200705daddbbc10fee56db0586a20df1abf4c57a9384a630c578f772f1bd116", "v0993"}
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
        var dirs = Directory.EnumerateDirectories($"{ConfigFolder}");
        foreach (var dir in dirs)
        {
            if (dir.Contains("settings_manager_"))
            {
                var data = dir.Split('\\');
                var name = data[data.Length - 1].Substring(17);

                var onlineProfile = IOResources.DeserializeFile<BLRProfileSettings[]>($"{dir}\\UE3_online_profile.json");
                var keyBinds = IOResources.DeserializeFile<BLRKeyBindings>($"{dir}\\keybinding.json");

                var profile = new BLRProfileSettingsWrapper(name, onlineProfile, keyBinds);
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
        if (ProfileSettings.TryGetValue(profileSettings.ProfileName, out var _))
        {
            ProfileSettings.Remove(profileSettings.ProfileName);
            ProfileSettings.Add(profileSettings.ProfileName, profileSettings);
        }
        else
        {
            Directory.CreateDirectory($"{ConfigFolder}settings_manager_{profileSettings.ProfileName}");
            ProfileSettings.Add(profileSettings.ProfileName, profileSettings);
        }
        IOResources.SerializeFile($"{ConfigFolder}settings_manager_{profileSettings.ProfileName}\\UE3_online_profile.json", profileSettings.Settings.Values.ToArray());
        IOResources.SerializeFile($"{ConfigFolder}settings_manager_{profileSettings.ProfileName}\\keybinding.json", profileSettings.KeyBindings);
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

        if (!ValidateClientHash(OriginalHash, OriginalPath, out string? NewOriginalHash))
        {
            LoggingSystem.Log($"Original Client has changed was {OriginalHash} is now {NewOriginalHash}.");
            OriginalHash = NewOriginalHash;
            return PatchClient();
        }

        if (!PatchedFileValidation())
        {
            LoggingSystem.Log($"Client hasn't been patched yet!");
            return PatchClient();
        }

        if (!ValidateClientHash(PatchedHash, PatchedPath, out string? NewPatchedHash))
        {
            LoggingSystem.Log($"Client hasn't been patched yet or is corrupted/changed. [{PatchedHash}]/[{NewPatchedHash}]");
            return PatchClient();
        }

        if (ValidatePatches())
        {
            LoggingSystem.Log($"Available Patches has changed Repatching Client to update to new Patches!");
            return PatchClient();
        }

        LoggingSystem.Log($"Client is in Good Health!");
        return true;
    }

    public bool OriginalFileValidation()
    {
        return !string.IsNullOrEmpty(OriginalPath) && File.Exists(OriginalPath);
    }

    public bool PatchedFileValidation()
    {
        return !string.IsNullOrEmpty(PatchedPath) && File.Exists(PatchedPath);
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
                    File.Delete($"{ModulesFolder}\\{module.InstallName}.dll");
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

    private void GetLatestBLRevivePackages()
    {
        var task = Task.Run(() => GitlabClient.GetGenericPackages("blrevive", "blrevive", "blrevive"));
        task.Wait();
        _latestBLRevivePackage = task.Result[0];
        var task2 = Task.Run(() => GitlabClient.GetLatestPackageFile(_latestBLRevivePackage, $"BLRevive.dll"));
        task2.Wait();
        _latestBLRevivePackageFile = task2.Result;
    }

    GitlabPackage _latestBLRevivePackage;
    GitlabPackage? LatestBLRevivePackage { get { if (_latestBLRevivePackage is null || _latestBLRevivePackageFile is null) { GetLatestBLRevivePackages(); } return _latestBLRevivePackage; } }

    GitlabPackageFile _latestBLRevivePackageFile;
    GitlabPackageFile? LatestBLRevivePackageFile { get { if (_latestBLRevivePackage is null || _latestBLRevivePackageFile is null) { GetLatestBLRevivePackages(); } return _latestBLRevivePackageFile; } }
    private bool CheckProxyUpdate()
    {
        if (DataStorage.Settings.SelectedSDKType == "BLRevive")
        {
            if (SDKVersionDate is not null && SDKType is not null)
            {
                var d = LatestBLRevivePackageFile.CreatedAt;
                return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second) > SDKVersionDate;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return !File.Exists($"{Path.GetDirectoryName(PatchedPath)}\\Proxy.dll");
        }
    }

    public void ValidateProxy()
    {
        if (!CheckProxyUpdate()) return;
        RemoveAllModules();
        
        var proxySource = string.Empty;
        var proxyTarget = string.Empty;
        if (DataStorage.Settings.SelectedSDKType == "BLRevive")
        {
            LoggingSystem.Log($"Downloading latest BLRevive release!");
            var reiveResult = GitlabClient.DownloadPackage(LatestBLRevivePackage, "BLRevive.dll", "BLRevive");
            var dInputResult = GitlabClient.DownloadPackage(LatestBLRevivePackage, "DINPUT8.dll", "DINPUT8");
            LoggingSystem.Log($"Finished downloading latest BLRevive release!");
            if (reiveResult.Item1 && dInputResult.Item1)
            {
                LoggingSystem.Log($"Installing latest BLRevive version ({reiveResult.Item3}) before ({SDKVersionDate})");
                DataStorage.Settings.SDKVersionDate = reiveResult.Item3;
                SDKVersionDate = reiveResult.Item3;

                File.Copy($"{IOResources.BaseDirectory}{dInputResult.Item2}", $"{Path.GetDirectoryName(PatchedPath)}\\DINPUT8.dll", true);

                proxySource = $"{IOResources.BaseDirectory}{reiveResult.Item2}";
                proxyTarget = $"{Path.GetDirectoryName(PatchedPath)}\\BLRevive.dll";
            }
        }
        else
        {
            proxySource = $"{IOResources.BaseDirectory}{IOResources.ASSET_DIR}\\dlls\\Proxy.{DataStorage.Settings.SelectedSDKType}.dll";
            proxyTarget = $"{Path.GetDirectoryName(PatchedPath)}\\Proxy.dll";
        }
         
        if (File.Exists(proxySource))
        {
            File.Copy(proxySource, proxyTarget, true);
        }
        SDKType = DataStorage.Settings.SelectedSDKType;
    }

    public bool ValidatePatches()
    {
        if (DataStorage.Settings.SelectedSDKType == "BLRevive") return false;
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
        List<Task> moduleInstallTasks = new();
        foreach (var availableModule in App.AvailableProxyModules)
        {
            if (availableModule.RepositoryProxyModule.Required && availableModule.RepositoryProxyModule.ProxyVersion.Equals(DataStorage.Settings.SelectedSDKType))
            {
                moduleInstallTasks.Add(Task.Run(() => { availableModule.InstallModule(this); }));
            }
        }
        if (moduleInstallTasks.Count > 0) Task.WaitAll(moduleInstallTasks.ToArray());

        foreach (var availableModule in App.AvailableProxyModules)
        {
            if (availableModule.RepositoryProxyModule.Required && availableModule.RepositoryProxyModule.ProxyVersion.Equals(DataStorage.Settings.SelectedSDKType))
            {
                availableModule.FinalizeInstall(this);
            }
        }
        
    }

    public bool IsModuleInstalledAndUpToDate(VisualProxyModule module)
    {
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

        foreach (var file in Directory.EnumerateFiles(ModulesFolder))
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

        List<ProxyModule> toRemove = new();

        foreach (var module in CustomModules) { if (module.FileAppearances <= 0) { toRemove.Add(module); } }

        foreach (var module in toRemove)
        { CustomModules.Remove(module); }

        LoggingSystem.Log($"Validating Modules Installed({count}/{InstalledModules.Count}) and Custom({customCount}/{CustomModules.Count}) of {this}");

        if (SDKType != "BLRevive")
        {
            var config = IOResources.DeserializeFile<ProxyConfig>($"{ConfigFolder}default.json") ?? new();
            config.Proxy.Modules.Server.Clear();
            config.Proxy.Modules.Client.Clear();
            LoggingSystem.Log($"Applying Installed Modules:");

            if (enabledModules is null)
            {
                enabledModules = InstalledModules.ToList();
                if (DataStorage.Settings.AllowCustomModules.Is)
                {
                    enabledModules.AddRange(CustomModules.ToList());
                }
            }

            foreach (var module in enabledModules)
            {
                SetModuleInProxyConfig(config, module);
            }

            IOResources.SerializeFile($"{ConfigFolder}default.json", config);
        }
        else
        { 
            var configClient = IOResources.DeserializeFile<BLReviveConfig>($"{ConfigFolder}{ConfigName}-Client.json") ?? new();
            var configServer = IOResources.DeserializeFile<BLReviveConfig>($"{ConfigFolder}{ConfigName}-Server.json") ?? new();
            var config = IOResources.DeserializeFile<BLReviveConfig>($"{ConfigFolder}{ConfigName}.json") ?? new();
            configClient.Modules.Clear();
            configServer.Modules.Clear();
            config.Modules.Clear();
            LoggingSystem.Log($"Applying Installed Modules:");

            if (enabledModules is null)
            {
                enabledModules = InstalledModules.ToList();
                if (DataStorage.Settings.AllowCustomModules.Is)
                {
                    enabledModules.AddRange(CustomModules.ToList());
                }
            }

            foreach (var module in enabledModules)
            {
                LoggingSystem.Log($"\t{module.InstallName}:");
                LoggingSystem.Log($"\t\tClient:{module.Client}");
                LoggingSystem.Log($"\t\tServer:{module.Server}");
                if (module.Client)
                {
                    configClient.Modules.Add(module.InstallName, new());
                }
                if (module.Server)
                {
                    configServer.Modules.Add(module.InstallName, new());
                }
                config.Modules.Add(module.InstallName, new());
            }
            IOResources.SerializeFile($"{ConfigFolder}{ConfigName}-Client.json", configClient);
            IOResources.SerializeFile($"{ConfigFolder}{ConfigName}-Server.json", configServer);
            IOResources.SerializeFile($"{ConfigFolder}{ConfigName}.json", config);
        }

        LoggingSystem.Log($"Finished Validating Modules of {this}");
    }

    private static void SetModuleInProxyConfig(ProxyConfig config, ProxyModule module)
    {
        LoggingSystem.Log($"\t{module.InstallName}:");
        LoggingSystem.Log($"\t\tClient:{module.Client}");
        LoggingSystem.Log($"\t\tServer:{module.Server}");

        if (module.Client) config.Proxy.Modules.Client.Add(module.InstallName);
        if (module.Server) config.Proxy.Modules.Server.Add(module.InstallName);
    }

    public static bool ValidateClientHash(string? currentHash, string? fileLocation, out string? newHash)
    {
        if (string.IsNullOrEmpty(currentHash) || string.IsNullOrEmpty(fileLocation)) { newHash = null; return false; }
        newHash = IOResources.CreateFileHash(fileLocation);
        return currentHash?.Equals(newHash) ?? false;
    }

    #endregion ClientValidation

    #region Commands
    private ICommand? patchClientCommand;
    [JsonIgnore]
    public ICommand PatchClientCommand
    {
        get
        {
            patchClientCommand ??= new RelayCommand(
                    param => this.PatchClient()
                );
            return patchClientCommand;
        }
    }

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
    private void LaunchBotMatch()
    {
        (var mode, var map, var canceled) = MapModeSelect.SelectMapAndMode(this.ClientVersion);
        if (canceled) { LoggingSystem.Log($"Canceled Botmatch Launch"); return; }
        string launchArgs = $"server {map?.MapName ?? "helodeck"}{(string.IsNullOrEmpty(ConfigName) ? "" : $"?config={ConfigName}-Server")}?Game=FoxGame.FoxGameMP_{mode?.ModeName ?? "DM"}?ServerName=BLREdit-{mode?.ModeName ?? "DM"}-Server?Port=7777?NumBots={DataStorage.Settings.BotCount}?MaxPlayers={DataStorage.Settings.PlayerCount}";
        StartProcess(launchArgs, true, DataStorage.Settings.ServerWatchDog.Is);
        LaunchClient(new LaunchOptions() { UserName = DataStorage.Settings.PlayerName, Server = LocalHost });
    }

    private void LaunchServer()
    {
        (var mode, var map, var canceled) = MapModeSelect.SelectMapAndMode(this.ClientVersion);
        if (canceled) { LoggingSystem.Log($"Canceled Server Launch"); return; }
        string launchArgs = $"server {map?.MapName ?? "helodeck"}{(string.IsNullOrEmpty(ConfigName) ? "" : $"?config={ConfigName}-Server")}?Game=FoxGame.FoxGameMP_{mode?.ModeName ?? "DM"}?ServerName=BLREdit-{mode?.ModeName ?? "DM"}-Server?Port=7777?NumBots={DataStorage.Settings.BotCount}?MaxPlayers={DataStorage.Settings.PlayerCount}";
        StartProcess(launchArgs, true, DataStorage.Settings.ServerWatchDog.Is);
    }

    public void LaunchClient()
    {
        LaunchClient(BLREditSettings.GetLaunchOptions());
    }

    public void LaunchClient(LaunchOptions options)
    {
        var ProxyLoadout = SDKType != "BLRevive" ? IOResources.DeserializeFile<LoadoutManagerLoadout[]>($"{DataStorage.Settings.DefaultClient.ConfigFolder}profiles\\{DataStorage.Settings.PlayerName}.json") : null;
        var BLReviveLoadout = SDKType == "BLRevive" ? IOResources.DeserializeFile<LMLoadout[]>($"{DataStorage.Settings.DefaultClient.ConfigFolder}profiles\\{DataStorage.Settings.PlayerName}.json") : null;
        if (options.Server.AllowAdvanced && !options.Server.AllowLMGR)
        {
            bool hasLMGR = false;
            string message = "Please Remove the LMGR Mag from";
            if (ProxyLoadout is not null)
            {
                int i = 1;
                foreach (var loadout in ProxyLoadout)
                {
                    CheckLoadout(loadout, ref message, ref hasLMGR, i);
                    i++;
                }
            }
            if (BLReviveLoadout is not null)
            {
                int i = 1;
                foreach (var loadout in BLReviveLoadout)
                {
                    CheckLoadout(loadout, ref message, ref hasLMGR, i);
                    i++;
                }
            }

            if (hasLMGR)
            {
                hasLMGR = false;
                message = "Please Remove the LMGR Mag from";
                var currentlyAppliedLoadout = DataStorage.Loadouts[DataStorage.Settings.CurrentlyAppliedLoadout];
                if (currentlyAppliedLoadout.BLR.IsAdvanced.Is)
                {
                    CheckLoadout(currentlyAppliedLoadout.BLR.Loadout1, ref message, ref hasLMGR, 1);
                    CheckLoadout(currentlyAppliedLoadout.BLR.Loadout2, ref message, ref hasLMGR, 2);
                    CheckLoadout(currentlyAppliedLoadout.BLR.Loadout3, ref message, ref hasLMGR, 3);
                    if (hasLMGR)
                    {
                        LoggingSystem.MessageLog($"Current loadout is not supported on this server:\n{message}\nOr apply a non Advanced or modify current loadout!", "Warning");
                        return;
                    }
                    else
                    {
                        currentlyAppliedLoadout.ApplyLoadout(this);
                    }
                }
                else
                {
                    currentlyAppliedLoadout.ApplyLoadout(this);
                }
            }
        }
        else if (!options.Server.AllowAdvanced)
        {
            bool isAdvanced = false;

            if (ProxyLoadout is not null)
            {
                string message = "";
                foreach (var loadout in ProxyLoadout)
                {
                    if (IsAdvanced(loadout.GetLoadout(), ref message))
                    {
                        isAdvanced = true;
                    }
                }
            }

            if (BLReviveLoadout is not null)
            {
                string message = "";
                foreach (var loadout in BLReviveLoadout)
                {
                    if (IsAdvanced(loadout.GetLoadout(), ref message))
                    {
                        isAdvanced = true;
                    }
                }
            }

            if (isAdvanced)
            {
                var currentlyAppliedLoadout = DataStorage.Loadouts[DataStorage.Settings.CurrentlyAppliedLoadout];
                if (currentlyAppliedLoadout.BLR.IsAdvanced.Is)
                {
                    LoggingSystem.MessageLog($"Current loadout is not supported on this server\nOnly Vanilla loadouts are allowed!\nApply a non Advanced or modify this loadout!", "Warning");
                    return;
                }
                else
                {
                    isAdvanced = false;
                    string message = "Loadout 1:";
                    if (IsAdvanced(currentlyAppliedLoadout.BLR.Loadout1, ref message)) isAdvanced = true;
                    message += "\nLoadout 2:";
                    if (IsAdvanced(currentlyAppliedLoadout.BLR.Loadout2, ref message)) isAdvanced = true;
                    message += "\nLoadout 3:";
                    if (IsAdvanced(currentlyAppliedLoadout.BLR.Loadout3, ref message)) isAdvanced = true;

                    if (isAdvanced)
                    {
                        currentlyAppliedLoadout.BLR.IsAdvanced.Set(true);
                        LoggingSystem.MessageLog($"Current loadout is not supported on this server\nOnly Vanilla loadouts are allowed!\nApply a non Advanced or modify this loadout!\n{message}", "Warning");
                        return;
                    }

                    currentlyAppliedLoadout.ApplyLoadout(this);
                }
            }
        }
        ApplyProfileSetting(ExportSystem.GetOrAddProfileSettings(options.UserName));
        ApplyConfigs();
        string launchArgs = options.Server.IPAddress + ':' + options.Server.Port;
        launchArgs += $"?Name={options.UserName}{(string.IsNullOrEmpty(ConfigName) ? "" : $"?config={ConfigName}-Client")}";
        StartProcess(launchArgs, false, false, null, options.Server);
    }

    public static bool IsAdvanced(BLRLoadout? loadout, ref string message)
    {
        if (loadout is null) return false;
        bool advanced = false;
        message += $"\n\tPrimary:";
        if (IsAdvanced(loadout.Primary, ref message)) advanced = true;
        message += $"\n\tSecondary:";
        if (IsAdvanced(loadout.Secondary, ref message)) advanced = true;

        message += $"\n\tGear:";
        if (!(loadout.Helmet?.IsValidFor(null) ?? false)) { advanced = true; message += $"\n\t\tHelmet is invalid"; }
        if (!(loadout.UpperBody?.IsValidFor(null) ?? false)) { advanced = true; message += $"\n\t\tUpperBody is invalid"; }
        if (!(loadout.LowerBody?.IsValidFor(null) ?? false)) { advanced = true; message += $"\n\t\tLowerBody is invalid"; }

        if (!(loadout.Tactical?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTactical is invalid"; }
        if (!(loadout.Trophy?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTrophy is invalid"; }
        if (!(loadout.Avatar?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tAvatar is invalid"; }
        if (!(loadout.BodyCamo?.IsValidFor(null) ?? false)) { advanced = true; message += $"\n\t\tBodyCamo is invalid"; }

        if (!(loadout.Gear1?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tGear1 is invalid"; }
        if (!(loadout.Gear2?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tGear2 is invalid"; }
        if (!(loadout.Gear3?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tGear3 is invalid"; }
        if (!(loadout.Gear4?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tGear4 is invalid"; }

        if (loadout.HasDuplicatedGear) { advanced = true; message += $"\n\t\tGear duplicates are not allowed!"; }

        message += $"\n\tExtra:";
        if (!(loadout.Depot1?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tDepot1 is invalid"; }
        if (!(loadout.Depot2?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tDepot2 is invalid"; }
        if (!(loadout.Depot3?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tDepot3 is invalid"; }
        if (!(loadout.Depot4?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tDepot4 is invalid"; }
        if (!(loadout.Depot5?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tDepot5 is invalid"; }

        if (!(loadout.Taunt1?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt1 is invalid"; }
        if (!(loadout.Taunt2?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt2 is invalid"; }
        if (!(loadout.Taunt3?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt3 is invalid"; }
        if (!(loadout.Taunt4?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt4 is invalid"; }
        if (!(loadout.Taunt5?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt5 is invalid"; }
        if (!(loadout.Taunt6?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt6 is invalid"; }
        if (!(loadout.Taunt7?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt7 is invalid"; }
        if (!(loadout.Taunt8?.IsValidFor(null) ?? true)) { advanced = true; message += $"\n\t\tTaunt8 is invalid"; }
        return advanced;
    }

    public static bool IsAdvanced(BLRWeapon? weapon, ref string message)
    {
        bool advanced = false;
        if(weapon is null || weapon.Receiver is null) { message += $"\n\t\tNo Receiver Equipped"; return true; }
        if (!(weapon.Receiver?.IsValidFor(null) ?? false)) { advanced = true; message += $"\n\t\tReceiver is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Barrel, weapon.Receiver)) { advanced = true; message += $"\n\t\tBarrel is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Muzzle, weapon.Receiver)) { advanced = true; message += $"\n\t\tMuzzle is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Grip, weapon.Receiver)) { advanced = true; message += $"\n\t\tGrip is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Camo, weapon.Receiver)) { advanced = true; message += $"\n\t\tCamo is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Magazine, weapon.Receiver)) { advanced = true; message += $"\n\t\tMagazine is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Ammo, weapon.Receiver)) { advanced = true; message += $"\n\t\tAmmo is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Tag, weapon.Receiver)) { advanced = true; message += $"\n\t\tTag is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Stock, weapon.Receiver)) { advanced = true; message += $"\n\t\tStock is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Scope, weapon.Receiver)) { advanced = true; message += $"\n\t\tScope is invalid"; }
        if (!BLRItem.IsValidFor(weapon.Skin, weapon.Receiver)) { advanced = true; message += $"\n\t\tSkin is invalid"; }
        return advanced;
    }

    public static bool ValidLoadout(BLRProfile profile, BLRServer server, out string message)
    {
        var isValidLoadout = true;
        message = "";
        if (server.AllowAdvanced && !server.AllowLMGR)
        {
            message = "Please Remove the LMGR Magazine from:";
            if (profile.IsAdvanced.Is)
            {
                bool invalid = false;
                CheckLoadout(profile.Loadout1, ref message, ref invalid, 1);
                CheckLoadout(profile.Loadout2, ref message, ref invalid, 2);
                CheckLoadout(profile.Loadout3, ref message, ref invalid, 3);
                if (invalid) { isValidLoadout = false; }
                if (!isValidLoadout)
                {
                    message = $"Current loadout is not supported on this server!\n{message}\nOr apply a non Advanced loadout!";
                }
            }
        }
        else if (!server.AllowAdvanced)
        {
            if (profile.IsAdvanced.Is)
            {
                isValidLoadout = false;
                message = $"Current loadout is not supported on this server\nOnly Vanilla loadouts are allowed!\nApply a non Advanced loadout!";
            }
        }
        return isValidLoadout;
    }

    public static void CheckLoadout(BLRLoadout loadout, ref string message, ref bool invalid,int i)
    {
        message += $"\n\tLoadout{i}:";
        PrimaryHasLMGR(loadout, ref message, ref invalid);
        SecondaryHasLMGR(loadout, ref message, ref invalid);
        MissingArmor(loadout, ref message, ref invalid);
    }

    public static void CheckLoadout(LMLoadout loadout, ref string message, ref bool invalid, int i)
    {
        message += $"\n\tLoadout{i}:";
        PrimaryHasLMGR(loadout, ref message, ref invalid);
        SecondaryHasLMGR(loadout, ref message, ref invalid);
        MissingArmor(loadout, ref message, ref invalid);
    }

    public static void CheckLoadout(LoadoutManagerLoadout loadout, ref string message, ref bool invalid, int i)
    {
        message += $"\n\tLoadout{i}:";
        PrimaryHasLMGR(loadout, ref message, ref invalid);
        SecondaryHasLMGR(loadout, ref message, ref invalid);
        MissingArmor(loadout, ref message, ref invalid);
    }

    public static void CheckLoadout(ShareableLoadout loadout, ref string message, ref bool invalid, int i)
    {
        message += $"\n\tLoadout{i}:";
        PrimaryHasLMGR(loadout, ref message, ref invalid);
        SecondaryHasLMGR(loadout, ref message, ref invalid);
        MissingArmor(loadout, ref message, ref invalid);
    }

    public void StartProcess(string launchArgs, bool isServer = false, bool watchDog = false, List<ProxyModule>? enabledModules = null, BLRServer? server = null)
    {
        if (!hasBeenValidated && Validate.Is)
        {
            if (!ValidateClient()) { return; }
            ValidateProxy();
            ValidateModules(enabledModules);
            hasBeenValidated = true;
        }
        else
        {
            LoggingSystem.Log($"[{this}]: has already been validated!");
        }

        BLRProcess.CreateProcess(launchArgs, this, isServer, watchDog, server);
    }

    #endregion Launch/Exit

    static BLRItem? lmgrReceiver;
    static BLRItem? LMGReceiver { get{ lmgrReceiver ??= ImportSystem.GetItemByUIDAndType(ImportSystem.PRIMARY_CATEGORY, 40014); return lmgrReceiver; } }
    static BLRItem? lmgrMagazine;
    static BLRItem? LMGRMagazine { get { lmgrMagazine ??= ImportSystem.GetItemByUIDAndType(ImportSystem.MAGAZINES_CATEGORY, 44106); return lmgrMagazine; } }

    public static void PrimaryHasLMGR(ShareableLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Primary.Receiver != LMGReceiver.LMID && loadout.Primary.Magazine == ImportSystem.GetIDOfItem(LMGRMagazine)) { invalid = true; message += " Primary"; }
    }

    public static void SecondaryHasLMGR(ShareableLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Secondary.Magazine == ImportSystem.GetIDOfItem(LMGRMagazine)) { invalid = true; message += " Secondary"; }
    }

    public static void PrimaryHasLMGR(LMLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Primary.Receiver != LMGReceiver.UID && loadout.Primary.Magazine == LMGRMagazine.UID) { invalid = true; message += " Primary"; }
    }

    public static void SecondaryHasLMGR(LMLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Secondary.Magazine == LMGRMagazine.UID) { invalid = true; message += " Secondary"; }
    }

    public static void PrimaryHasLMGR(LoadoutManagerLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Primary.Receiver != LMGReceiver.LMID && loadout.Primary.Magazine == ImportSystem.GetIDOfItem(LMGRMagazine)) { invalid = true; message += " Primary"; }
    }

    public static void SecondaryHasLMGR(LoadoutManagerLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Secondary.Magazine == ImportSystem.GetIDOfItem(LMGRMagazine)) { invalid = true; message += " Secondary"; }
    }

    public static void PrimaryHasLMGR(BLRLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Primary.Receiver.LMID != LMGReceiver.LMID && loadout.Primary.Magazine.UID == LMGRMagazine.UID) { invalid = true; message += " Primary"; }
    }

    public static void SecondaryHasLMGR(BLRLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Secondary.Magazine.UID == LMGRMagazine.UID) { invalid = true; message += " Secondary"; }
    }

    public static void MissingArmor(BLRLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Helmet is null) { invalid = true; message += " Helmet"; }
        if (loadout.UpperBody is null) { invalid = true; message += " UpperBody"; }
        if (loadout.LowerBody is null) { invalid = true; message += " LowerBody"; }
    }
    public static void MissingArmor(LMLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Body.Helmet == -1) { invalid = true; message += " Helmet"; }
        if (loadout.Body.UpperBody == -1) { invalid = true; message += " UpperBody"; }
        if (loadout.Body.LowerBody == -1) { invalid = true; message += " LowerBody"; }
    }
    public static void MissingArmor(LoadoutManagerLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Gear.Helmet == -1) { invalid = true; message += " Helmet"; }
        if (loadout.Gear.UpperBody == -1) { invalid = true; message += " UpperBody"; }
        if (loadout.Gear.LowerBody == -1) { invalid = true; message += " LowerBody"; }
    }
    public static void MissingArmor(ShareableLoadout loadout, ref string message, ref bool invalid)
    {
        if (loadout.Helmet == -1) { invalid = true; message += " Helmet"; }
        if (loadout.UpperBody == -1) { invalid = true; message += " UpperBody"; }
        if (loadout.LowerBody == -1) { invalid = true; message += " LowerBody"; }
    }

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

        if (!basePath.EndsWith("\\")) { basePath += "\\"; }

        _patchedPath ??= $"{basePath}Binaries\\Win32\\{fileParts[0]}-BLREdit-Patched.{fileParts[1]}";

        return basePath;
    }

    /// <summary>
    /// Patch this GameClient
    /// </summary>
    public bool PatchClient()
    {
        try
        {
            if (string.IsNullOrEmpty(PatchedPath)) { _basePath = GetBasePath(); }
            if (DataStorage.Settings.SelectedSDKType != "BLRevive")
            {
                List<BLRClientPatch> toAppliedPatches = new();
                File.Copy(OriginalPath, PatchedPath, true);
                AppliedPatches.Clear();
                if (BLRClientPatch.AvailablePatches.TryGetValue(this.OriginalHash, out List<BLRClientPatch> patches))
                {
                    using (var stream = File.Open(PatchedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        List<byte> rawFile = new();
                        using var reader = new BinaryReader(stream);
                        rawFile.AddRange(reader.ReadBytes((int)stream.Length));

                        foreach (BLRClientPatch patch in patches)
                        {
                            LoggingSystem.Log($"Applying Patch:{patch.PatchName} to Client:{OriginalHash}");
                            foreach (var part in patch.PatchParts)
                            {
                                OverwriteBytes(rawFile, part.Key, part.Value.ToArray());
                            }
                            toAppliedPatches.Add(patch);
                        }

                        PeFile peFile = new(rawFile.ToArray());
                        peFile.AddImport("Proxy.dll", "InitializeThread");
                        stream.Position = 0;
                        stream.SetLength(peFile.RawFile.Length);
                        using var writer = new BinaryWriter(stream);

                        writer.Write(peFile.RawFile.ToArray());

                        reader.Close();
                        writer.Close();
                        stream.Close();
                        reader.Dispose();
                        writer.Dispose();
                        stream.Dispose();
                    }
                    foreach (var patch in toAppliedPatches)
                    {
                        AppliedPatches.Add(patch);
                    }
                }
                else
                {
                    LoggingSystem.Log($"No patches found for {OriginalHash}");
                }
                PatchedHash = IOResources.CreateFileHash(PatchedPath);
            }
        }
        catch (Exception error)
        {
            LoggingSystem.MessageLog($"[{this}]: Patching failed:\n{error.Message}", "Error"); //TODO: Add Localization
            return false;
        }
        SDKType = null;
        return true;
    }

    private static void OverwriteBytes(List<byte> bytes, int offsetFromBegining, byte[] bytesToWrite)
    {
        int i2 = 0;
        for (int i = offsetFromBegining; i < bytes.Count && i2 < bytesToWrite.Length; i++)
        {
            bytes[i] = bytesToWrite[i2];
            i2++;
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}