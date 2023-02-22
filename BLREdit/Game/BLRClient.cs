using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;

using BLREdit.Export;
using BLREdit.Game.Proxy;
using BLREdit.UI;
using BLREdit.UI.Views;
using BLREdit.UI.Windows;

namespace BLREdit.Game;

public sealed class BLRClient : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events
    private bool hasBeenValidated = false;

    [JsonIgnore] private readonly BLRServer LocalHost = new() { ServerAddress = "localhost", Port = 7777 };
    [JsonIgnore] public UIBool Patched { get; private set; } = new UIBool(false);
    [JsonIgnore] public UIBool CurrentClient { get; private set; } = new UIBool(false);
    [JsonIgnore] public string ClientVersion { get { if (VersionHashes.TryGetValue(ClientHash, out string version)) { return version; } else { return "Unknown"; } } }
    [JsonIgnore] public ObservableCollection<Process> RunningClients = new();
    private Dictionary<string, BLRProfileSettingsWrapper> profileSettings;
    [JsonIgnore] public Dictionary<string, BLRProfileSettingsWrapper> ProfileSettings { get { profileSettings ??= LoadProfiles(); return profileSettings; } }
    [JsonIgnore] public static BitmapImage ClientVersionPart0 { get { return new BitmapImage(new Uri(@"pack://application:,,,/UI/Resources/V.png", UriKind.Absolute)); } }
    [JsonIgnore] public BitmapImage ClientVersionPart1 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 2) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[1])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage ClientVersionPart2 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 3) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[2])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage ClientVersionPart3 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 4) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[3])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage ClientVersionPart4 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 5) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[4])}.png", UriKind.Absolute)); } return null; } }

    private string clientHash;
    public string ClientHash {
        get { return clientHash; }
        set { if (clientHash != value && !string.IsNullOrEmpty(value)) { clientHash = value; OnPropertyChanged(nameof(ClientVersion)); OnPropertyChanged(); } }
    }

    private string patchedHash;
    public string PatchedHash {
        get { return patchedHash; }
        set { if (patchedHash != value && !string.IsNullOrEmpty(value)) { patchedHash = value; OnPropertyChanged(); } }
    }

    private string originalPath;
    public string OriginalPath {
        get { return originalPath; }
        set { if (File.Exists(value)) { originalPath = value; ClientHash ??= IOResources.CreateFileHash(value); OnPropertyChanged(); } else { LoggingSystem.Log($"[{this}]: not a valid Origin Client Path {value}"); } }
    }

    private string patchedPath;
    public string PatchedPath {
        get { return patchedPath; }
        set { if (File.Exists(value)) { patchedPath = value; Patched.Set(true); OnPropertyChanged(); } else { LoggingSystem.Log($"[{this}]: not a valid Patched Client Path {value}"); } }
    }

    public string ConfigFolder { get; set; }
    public string ModulesFolder { get; set; }

    public ObservableCollection<BLRClientPatch> AppliedPatches { get; set; } = new();

    public ObservableCollection<ProxyModule> InstalledModules { get; set; } = new();

    public ObservableCollection<ProxyModule> CustomModules { get; set; } = new();

    [JsonIgnore] public static ObservableCollection<VisualProxyModule> AvailabeModules { get { return App.AvailableProxyModules; } }

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

    public static Dictionary<string, string> VersionHashes => new()
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

    public override bool Equals(object obj)
    {
        if (obj is BLRClient client)
        {
            return (client.OriginalPath == OriginalPath && client.ClientVersion == ClientVersion && client.Patched.Is == Patched.Is);
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
    private Dictionary<string, BLRProfileSettingsWrapper> LoadProfiles()
    {
        var dict = new Dictionary<string, BLRProfileSettingsWrapper>();
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
        App.Current.Dispatcher.Invoke(() => { MainWindow.Self.SetProfileSettings(); }); 
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
    #endregion ProfileSettings

    #region ClientValidation
    public bool ValidateClient()
    {
        bool isValid = true;
        bool NeedsPatching = false;

        if (OriginalFileValidation())
        {
            //this is super overkill
            if (!ValidateClientHash(ClientHash, OriginalPath, out string NewHash))
            {
                LoggingSystem.Log($"Client has changed was {ClientHash} is now {NewHash} needs patching!");
                ClientHash = NewHash;
                NeedsPatching = true;
            }
            else
            { LoggingSystem.Log($"Client is still the Same! {OriginalPath}"); }
        }
        else
        {
            LoggingSystem.Log($"Client is not valid missing original file path or file is missing!");
            isValid = false;
        }

        if (PatchedFileValidation())
        {
            if (!ValidateClientHash(PatchedHash, PatchedPath, out string NewHash))
            {
                LoggingSystem.Log($"Patched client changed/corrupted was {PatchedHash} is now {NewHash} needs patching!");
                NeedsPatching = true;
            }
            else
            {
                LoggingSystem.Log($"Patched file is still the same!");
                if (ValidatePatches()) { NeedsPatching = true; }
            }
        }
        else
        {
            LoggingSystem.Log($"Client hasn't been patched yet!");
            NeedsPatching=true;
        }

        if (NeedsPatching)
        {
            PatchClient();
        }

        return isValid;
    }

    public bool OriginalFileValidation()
    {
        return !string.IsNullOrEmpty(OriginalPath) && File.Exists(OriginalPath);
    }

    public bool PatchedFileValidation()
    {
        return !string.IsNullOrEmpty(PatchedPath) && File.Exists(PatchedPath);
    }

    public bool ValidatePatches()
    {
        bool needUpdatedPatches = false;

        if (BLRClientPatch.AvailablePatches.TryGetValue(this.ClientHash, out List<BLRClientPatch> patches))
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

                var proxySource = $"{IOResources.BaseDirectory}{IOResources.ASSET_DIR}\\dlls\\Proxy.dll";
                var proxyTarget = $"{Path.GetDirectoryName(PatchedPath)}\\Proxy.dll";
                if (File.Exists(proxySource) && File.Exists(proxyTarget))
                {
                    var sourceHash = IOResources.CreateFileHash(proxySource);
                    var targetHash = IOResources.CreateFileHash(proxyTarget);
                    if (sourceHash != targetHash)
                    {
                        File.Copy(proxySource, proxyTarget, true);
                    }
                }
                else if (File.Exists(proxySource) && !File.Exists(proxyTarget))
                {
                    File.Copy(proxySource, proxyTarget);
                }
            }
            else
            {
                LoggingSystem.Log($"no installed patches for {ClientHash}");
                needUpdatedPatches = true;
            }
        }
        else
        {
            LoggingSystem.Log($"No patches found for {ClientHash}");
            needUpdatedPatches = true;
        }
        return needUpdatedPatches;
    }

    public void ValidateModules(List<ProxyModule> enabledModules = null)
    {
        App.AvailableProxyModuleCheck(); // Get Available Modules just in case

        var count = InstalledModules.Count;
        var customCount = CustomModules.Count;
        LoggingSystem.Log($"Available Modules:{App.AvailableProxyModules.Count}, StrictModuleCheck:{BLREditSettings.Settings.StrictModuleChecks}, AllowCustomModules:{BLREditSettings.Settings.AllowCustomModules}, InstallRequiredModules:{BLREditSettings.Settings.InstallRequiredModules}");

        if (App.AvailableProxyModules.Count > 0 && BLREditSettings.Settings.InstallRequiredModules.Is)
        {
            var oldClient = MainWindow.ClientWindow.Client ;
            MainWindow.ClientWindow.Client = this;
            foreach (var availableModule in App.AvailableProxyModules)
            {
                if ((availableModule.Installed.IsNot || availableModule.UpToDate.IsNot) && availableModule.RepositoryProxyModule.Required)
                { 
                    availableModule.InstallCommand.Execute(null);
                }
            }
            MainWindow.ClientWindow.Client = oldClient;
        }

        if (App.AvailableProxyModules.Count > 0 && BLREditSettings.Settings.StrictModuleChecks.Is)
        { InstalledModules = new(InstalledModules.Where((module) => { bool isAvailable = false; foreach (var available in App.AvailableProxyModules) { if (available.RepositoryProxyModule.InstallName == module.InstallName) { module.Server = available.RepositoryProxyModule.Server; module.Client = available.RepositoryProxyModule.Client; isAvailable = true; } } return isAvailable; })); }

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
                if (BLREditSettings.Settings.AllowCustomModules.Is && !isInstalled)
                {
                    bool isNew = true;
                    foreach (var module in CustomModules)
                    {
                        if (name == module.InstallName)
                        { isNew = false; module.FileAppearances++; break; }
                    }
                    if (isNew)
                    { CustomModules.Add(new(name)); }
                }
            }
        }

        List<ProxyModule> toRemove= new();

        foreach (var module in CustomModules) { if (module.FileAppearances <= 0) { toRemove.Add(module); } }

        foreach (var module in toRemove)
        { CustomModules.Remove(module); }

        LoggingSystem.Log($"Validating Modules Installed({count}/{InstalledModules.Count}) and Custom({customCount}/{CustomModules.Count}) of {this}");

        ProxyConfig config = IOResources.DeserializeFile<ProxyConfig>($"{ConfigFolder}default.json") ?? new();
        config.Proxy.Modules.Server.Clear();
        config.Proxy.Modules.Client.Clear();
        LoggingSystem.Log($"Applying Installed Modules:");

        if (enabledModules is null)
        {
            enabledModules = InstalledModules.ToList();
            if (BLREditSettings.Settings.AllowCustomModules.Is)
            {
                enabledModules.AddRange(CustomModules.ToList());
            }
        }

        foreach (var module in enabledModules)
        {
            SetModuleInProxyConfig(config, module);
        }

        IOResources.SerializeFile($"{ConfigFolder}default.json", config);
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

    public static bool ValidateClientHash(string currentHash, string fileLocation, out string newHash)
    {
        if (string.IsNullOrEmpty(currentHash) || string.IsNullOrEmpty(fileLocation)) { newHash = null; return false; }
        newHash = IOResources.CreateFileHash(fileLocation);
        return currentHash.Equals(newHash);
    }

    #endregion ClientValidation

    #region Commands
    private ICommand patchClientCommand;
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

    private ICommand launchClientCommand;
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

    private ICommand launchServerCommand;
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

    private ICommand launchBotMatchCommand;
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

    private ICommand modifyClientCommand;
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

    private ICommand currentClientCommand;
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
        string launchArgs = $"server {map.MapName}?Game=FoxGame.FoxGameMP_{mode.ModeName}?ServerName=BLREdit-{mode.ModeName}-Server?Port=7777?NumBots={BLREditSettings.Settings.BotCount}?MaxPlayers={BLREditSettings.Settings.PlayerCount}?SingleMatch";
        StartProcess(launchArgs, true, BLREditSettings.Settings.ServerWatchDog.Is);
        LaunchClient(new LaunchOptions() { UserName= BLREditSettings.Settings.PlayerName, Server=LocalHost });
    }

    private void LaunchServer()
    {
        (var mode, var map, var canceled) = MapModeSelect.SelectMapAndMode(this.ClientVersion);
        if (canceled) { LoggingSystem.Log($"Canceled Server Launch"); return; }
        string launchArgs = $"server {map.MapName}?Game=FoxGame.FoxGameMP_{mode?.ModeName ?? "DM"}?ServerName=BLREdit-{mode?.ModeName ?? "DM"}-Server?Port=7777?NumBots={BLREditSettings.Settings.BotCount}?MaxPlayers={BLREditSettings.Settings.PlayerCount}";
        StartProcess(launchArgs, true, BLREditSettings.Settings.ServerWatchDog.Is);
    }

    public void LaunchClient()
    {
        LaunchClient(BLREditSettings.GetLaunchOptions());
    }

    public void LaunchClient(LaunchOptions options)
    {
        ApplyProfileSetting(ExportSystem.GetOrAddProfileSettings(options.UserName));

        string launchArgs = options.Server.IPAddress + ':' + options.Server.Port;
        launchArgs += $"?Name={options.UserName}";
        StartProcess(launchArgs);
    }

    public void StartProcess(string launchArgs, bool isServer = false, bool watchDog = false, List<ProxyModule> enabledModules = null)
    {
        if (!hasBeenValidated)
        {
            var oldClient = MainWindow.ClientWindow.Client;
            MainWindow.ClientWindow.Client = this;
            ValidateClient();
            ValidateModules(enabledModules);
            hasBeenValidated = true;
            MainWindow.ClientWindow.Client = oldClient;
        }
        else
        {
            LoggingSystem.Log($"[{this}]: has already been validated!");
        }



        //TODO: Fix this, done??
        BLRProcess.CreateProcess(launchArgs, this, isServer, watchDog);
    }

    #endregion Launch/Exit

    private void ModifyClient()
    {
        App.AvailableProxyModuleCheck();
        MainWindow.ClientWindow.Client = this;
        MainWindow.ClientWindow.ShowDialog();
    }

    private void SetCurrentClient()
    {
        LoggingSystem.Log($"Setting Current Client:{this}");
        if (this.Patched.Is)
        {
            BLREditSettings.Settings.DefaultClient = this;
            foreach (BLRClient c in MainWindow.GameClients)
            {
                c.CurrentClient.Set(false);
            }
            this.CurrentClient.Set(true);
        }
    }

    private string CreateFolderStructure()
    {
        string[] pathParts = OriginalPath.Split('\\');
        string[] fileParts = pathParts[pathParts.Length - 1].Split('.');
        pathParts[pathParts.Length - 1] = fileParts[0] + "-BLREdit-Patched." + fileParts[1];
        string basePath = "";
        for (int i = pathParts.Length-4; i >= 0; i--)
        { 
            basePath = $"{pathParts[i]}\\{basePath}";
        }
        LoggingSystem.Log($"found root BLR Directory {basePath}");
        ConfigFolder = Directory.CreateDirectory($"{basePath}\\FoxGame\\Config\\BLRevive\\").FullName;
        ModulesFolder = Directory.CreateDirectory($"{basePath}\\Binaries\\Win32\\Modules\\").FullName;

        return $"{basePath}\\Binaries\\Win32\\{fileParts[0]}-BLREdit-Patched.{fileParts[1]}";
    }

    /// <summary>
    /// Patch this GameClient
    /// </summary>
    public void PatchClient()
    {
        string outFile = CreateFolderStructure();
        File.Copy(OriginalPath, outFile, true);

        AppliedPatches.Clear();

        if (BLRClientPatch.AvailablePatches.TryGetValue(this.ClientHash, out List<BLRClientPatch> patches))
        {
            try
            {
                using var PatchedFile = File.Open(outFile, FileMode.Open);
                using BinaryWriter binaryWriter = new((Stream)PatchedFile);
                foreach (BLRClientPatch patch in patches)
                {
                    LoggingSystem.Log($"Applying Patch:{patch.PatchName} to Client:{ClientHash}");
                    foreach (var part in patch.PatchParts)
                    {
                        binaryWriter.Seek(part.Key, SeekOrigin.Begin);
                        binaryWriter.Write(part.Value.ToArray());
                    }
                    this.AppliedPatches.Add(patch);
                }
                binaryWriter.Close();
                binaryWriter.Dispose();
                PatchedFile.Close();
                PatchedFile.Dispose();

                File.Copy($"{IOResources.ASSET_DIR}\\dlls\\Proxy.dll", $"{Path.GetDirectoryName(outFile)}\\Proxy.dll", true);

                var peFile = new PeNet.PeFile(outFile);
                peFile.AddImport("Proxy.dll", "InitializeThread");
                File.WriteAllBytes(outFile, peFile.RawFile.ToArray());
            }
            catch (Exception error)
            {
                LoggingSystem.MessageLog($"[{this}]Client Patch Failed: {error}");
            }
        }
        else
        {
            LoggingSystem.Log($"No patches found for {ClientHash}");
        }

        PatchedPath = outFile;
        PatchedHash = IOResources.CreateFileHash(outFile);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}