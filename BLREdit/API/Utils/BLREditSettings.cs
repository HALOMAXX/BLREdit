using BLREdit.API.Utils;
using BLREdit.Export;
using BLREdit.Game;
using BLREdit.UI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;

namespace BLREdit;

public sealed class BLREditSettings : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    public static ObservableCollection<string?> AvailableBLREditVersions { get; } = ["Release", "Beta"];
    public static ObservableCollection<string?> AvailableBLReviveVersions { get; } = ["Release", "Beta"];
    public static ObservableCollection<string?> AvailableSDKTypes { get; } = ["BLRevive", "Proxy"];
    #region Settings
    public string? LastRunVersion { get; set; }
    public int AppliedLoadout { get; set; }
    [JsonIgnore] public int CurrentlyAppliedLoadout { get { return AppliedLoadout; } set { AppliedLoadout = value; OnPropertyChanged(); } }
    public int SelectedClient { get; set; } = -1;
    [JsonIgnore] public BLRClient? DefaultClient { 
        get { if (SelectedClient >= 0 && SelectedClient < DataStorage.GameClients.Count) { return DataStorage.GameClients[SelectedClient]; } return null; } 
        set { SelectedClient = DataStorage.FindIn(DataStorage.GameClients, value); OnPropertyChanged(); } }
    public int SelectedServer { get; set; } = -1;
    [JsonIgnore] public BLRServer? DefaultServer { 
        get { if (SelectedServer >= 0 && SelectedServer < DataStorage.ServerList.Count) { return DataStorage.ServerList[SelectedServer]; } return null; } 
        set { SelectedServer = DataStorage.FindIn(DataStorage.ServerList, value); OnPropertyChanged(); } }

    public int SelectedLoadout { get; set; } = -1;
    [JsonIgnore]
    public BLRLoadoutStorage? DefaultLoadout
    {
        get { if (SelectedLoadout >= 0 && SelectedLoadout < DataStorage.Loadouts.Count) { return DataStorage.Loadouts[SelectedLoadout]; } return null; }
        set { SelectedLoadout = DataStorage.FindIn(DataStorage.Loadouts, value); OnPropertyChanged(); }
    }

    public UIBool EnableAPI { get; set; } = new(true);
    public UIBool EnableFramerateSmoothing { get; set; } = new(true);
    public UIBool FramerateSmoothingToDisplayRefreshrate { get; set; } = new(false); //TODO: Change to false before release
    public UIBool Debugging { get; set; } = new(false);
    public UIBool DoRuntimeCheck { get; set; } = new(true);
    public UIBool ForceRuntimeCheck { get; set; } = new(false);
    public UIBool ShowUpdateNotice { get; set; } = new(true);
    public UIBool ServerWatchDog { get; set; } = new(false);
    public UIBool StrictModuleChecks { get; set; } = new(true);
    public UIBool AllowCustomModules { get; set; } = new(false);
    public UIBool InstallRequiredModules { get; set; } = new(true);
    public UIBool PingHiddenServers { get; set; } = new(true);
    public UIBool ShowHiddenServers { get; set; } = new(false);
    public UIBool ApplyMergedProfiles { get; set; } = new(true);
    public UIBool SteamAwareToggle { get; set; } = new(true);
    public DateTime? SDKVersionDate { get; set; }

    [JsonPropertyName("SelectedSDKType"), JsonInclude] private string? _selectedSDKType;
    [JsonIgnore] public string? SelectedSDKType { get { _selectedSDKType ??= AvailableSDKTypes.First(); return _selectedSDKType; } set { if (AvailableSDKTypes.Contains(value)) { _selectedSDKType = value; } } }

    [JsonPropertyName("SelectedBLReviveVersion"), JsonInclude] private string? _selectedBLRevive;
    [JsonIgnore] public string? SelectedBLReviveVersion { get { _selectedBLRevive ??= AvailableBLReviveVersions.First(); return _selectedBLRevive; } set { if (AvailableBLReviveVersions.Contains(value)) { _selectedBLRevive = value; } } }

    [JsonPropertyName("SelectedBLREditVersion"), JsonInclude] private string? _selectedBLREditVersion;
    [JsonIgnore] public string? SelectedBLREditVersion { get { _selectedBLREditVersion ??= AvailableBLREditVersions.First(); return _selectedBLREditVersion; } set { if (AvailableBLREditVersions.Contains(value)) { _selectedBLREditVersion = value; } } }

    [JsonPropertyName("SelectedLanguage"), JsonInclude] private string? SelectedLanguage = null;
    [JsonIgnore] public CultureInfo SelectedCulture { get { if (string.IsNullOrEmpty(SelectedLanguage)) { return CultureInfo.InvariantCulture; } else { return CultureInfo.CreateSpecificCulture(SelectedLanguage); } } set { if (value is not null) { SelectedLanguage = value.Name; OnPropertyChanged(); } } }
    [JsonPropertyName("PlayerName"), JsonInclude] private string jsonPlayerName = "BLREdit-Player";
    [JsonIgnore] public string PlayerName { get { return jsonPlayerName; } set { if (jsonPlayerName != value) { MainWindow.MainView.Profile.BLR.IsChanged = true; } jsonPlayerName = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProfileSettings)); MainWindow.MainView.UpdateWindowTitle(); } }
    [JsonPropertyName("Region"), JsonInclude] private string jsonRegion = string.Empty;
    [JsonIgnore] public string Region { get { return jsonRegion; } set { jsonRegion = value; OnPropertyChanged(); MainWindow.MainView.UpdateWindowTitle(); } }
    [JsonIgnore] private string lastplayerName = "BLREdit-Player";
    [JsonIgnore] public string LastPlayerName { get { return lastplayerName; } set { lastplayerName = value; OnPropertyChanged(); } }
    [JsonIgnore] public BLRProfileSettingsWrapper ProfileSettings => ExportSystem.GetOrAddProfileSettings(PlayerName);

    [JsonPropertyName("BotCount"), JsonInclude] private int jsonBotCount = 8;
    [JsonIgnore] public int BotCount { get { return jsonBotCount; } set { jsonBotCount = value; OnPropertyChanged(); } }
    [JsonPropertyName("PlayerCount"), JsonInclude] private int jsonPlayerCount = 16;
    [JsonIgnore] public int PlayerCount { get { return jsonPlayerCount; } set { jsonPlayerCount = value; OnPropertyChanged(); } }
    #endregion Settings

    static BLREditSettings()
    {
        ApplyEvent();
    }

    #region Commands
    private ICommand? resetConfigCommand;
    [JsonIgnore]
    public ICommand ResetConfigCommand
    {
        get
        {
            resetConfigCommand ??= new RelayCommand(
                    param => ResetSettings()
                );
            return resetConfigCommand;
        }
    }
    private ICommand? fixClientsCommand;
    [JsonIgnore]
    public ICommand FixClientsCommand
    {
        get
        {
            fixClientsCommand ??= new RelayCommand(
                    param => FixClients()
                );
            return fixClientsCommand;
        }
    }
    private ICommand? fixServersCommand;
    [JsonIgnore]
    public ICommand FixServersCommand
    {
        get
        {
            fixServersCommand ??= new RelayCommand(
                    param => FixServerList()
                );
            return fixServersCommand;
        }
    }
    #endregion Commands

    private static void ApplyEvent()
    {
        DataStorage.Settings.ShowHiddenServers.PropertyChanged += DataStorage.Settings.ShowHiddenServersChanged;
    }

    public static void FixClients()
    {
        if (!LoggingSystem.MessageLog("Are you sure you want to Fix the Clients", "Fix Game Clients", MessageBoxButton.YesNo)) { return; }
        int clientCount = DataStorage.GameClients.Count;
        List<BLRClient> repairedClients = [];
        foreach (var client in DataStorage.GameClients)
        {
            bool contains = false;
            foreach (var reClient in repairedClients)
            {
                if (reClient.OriginalPath?.Equals(client.OriginalPath, StringComparison.OrdinalIgnoreCase) ?? false) { contains = true; break; }
            }
            if (contains) { continue; } //skip entry as we already have it repaired
            var path = client.OriginalPath;
            if (File.Exists(client.PatchedPath)) { File.Delete(client.PatchedPath); }

            #region BinariesDirectoryCleanup
            if (client.OriginalFileInfo is not null)
            {
                foreach (var file in client.OriginalFileInfo.Info.Directory.EnumerateFiles())
                {
                    try
                    {
                        if (file.Name.Contains("BLREdit") || file.Name.Contains("DINPUT8") || file.Name.Contains("Proxy") || file.Name.Contains("BLRevive")) { file.Delete(); }
                    }
                    catch { }
                }

                foreach (var modulesFolder in client.OriginalFileInfo.Info.Directory.EnumerateDirectories("Modules"))
                {
                    try
                    {
                        modulesFolder.Delete(true);
                    }
                    catch { }
                }
            }
            #endregion BinariesDirectoryCleanup

            #region BLReviveConfigDirectoryCleanup
            if (client.BLReviveConfigsDirectoryInfo is not null)
            {
                foreach (var file in client.BLReviveConfigsDirectoryInfo.EnumerateFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
                foreach (var dir in client.BLReviveConfigsDirectoryInfo.EnumerateDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch { }
                }
            }
            #endregion BLReviveConfigDirectoryCleanup

            #region LogDirectoryBackupAndCleanup
            if (client.LogsDirectoryInfo is not null)
            {
                foreach (var file in client.LogsDirectoryInfo.EnumerateFiles())
                {
                    try
                    {
                        var filePath = new FileInfoExtension($"{App.BLREditLocation}\\logs\\BackupAndClean\\{file.Name}");
                        if (!filePath.Info.Directory.Exists) { filePath.Info.Directory.Create(); }
                        file.MoveTo(filePath.Info.FullName);
                    }
                    catch { }
                }
                foreach (var dir in client.LogsDirectoryInfo.EnumerateDirectories())
                {
                    try
                    {
                        var dirPath = new DirectoryInfo($"{App.BLREditLocation}\\logs\\BackupAndClean\\{dir.Name}");
                        if (dirPath.Exists) { dirPath.Delete(); }
                        IOResources.CopyDirectory(dir.FullName, dirPath.FullName, true);
                        dir.Delete();
                    }
                    catch { }
                }
            }
            #endregion LogDirectoryBackupAndCleanup
            repairedClients.Add(new BLRClient() { OriginalPath = path });
        }

        DataStorage.GameClients.Clear();
        foreach (var reClient in repairedClients)
        {
            DataStorage.GameClients.Add(reClient);
        }
        LoggingSystem.MessageLog(
            $"Reset Clients:{repairedClients.Count}\n" +
            $"Removed Duplicates:{(clientCount-repairedClients.Count)}", "Client Repair Result");
    }

    public static void FixServerList()
    { 
        DataStorage.ServerList.Clear();
        foreach (BLRServer defaultServer in App.DefaultServers)
        {
            MainWindow.AddOrUpdateDefaultServer(defaultServer);
        }
    }

    public static void ResetSettings()
    {
        if (!LoggingSystem.MessageLog(Properties.Resources.msg_ResetConfig, Properties.Resources.msgT_ResetConfig, MessageBoxButton.YesNo)) { return; }

        LoggingSystem.MessageLog(Properties.Resources.msg_ResetConfigInfo, Properties.Resources.msgT_ResetConfig);

        DataStorage.Settings = new BLREditSettings
        {
            LastRunVersion = App.CurrentVersion.ToString()
        };

        foreach (var client in DataStorage.GameClients)
        {
            #region BLReviveConfigDirectoryCleanup
            if (client.BLReviveConfigsDirectoryInfo is not null)
            {
                foreach (var file in client.BLReviveConfigsDirectoryInfo.EnumerateFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
                foreach (var dir in client.BLReviveConfigsDirectoryInfo.EnumerateDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch { }
                }
            }
            #endregion BLReviveConfigDirectoryCleanup
        }

        DataStorage.GameClients.Clear();
        DataStorage.ServerList.Clear();
        DataStorage.CachedModules.Clear();

        App.Restart();
    }

    public static LaunchOptions DefaultLaunchOptions => new() { UserName = DataStorage.Settings.PlayerName, Server = DataStorage.Settings.DefaultServer ?? new() };

    private void AdvancedModdingChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Is")
        {
            MainWindow.MainView.Profile.BLR.CalculateStats();
            MainWindow.Instance?.SetItemList();
        }
    }

    private void ShowHiddenServersChanged(object sender, PropertyChangedEventArgs e)
    {
        MainWindow.Instance?.RefreshServerList();
    }

    public static void Save()
    {
        IOResources.SerializeFile($"{IOResources.SETTINGS_FILE}", DataStorage.Settings);
    }
}