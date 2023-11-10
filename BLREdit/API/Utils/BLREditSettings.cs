using BLREdit.Export;
using BLREdit.Game;
using BLREdit.UI;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

    public static ObservableCollection<string?> AvailableProxyVersions { get; } = new() { "v1.0.0-beta.2", "v1.0.0-beta.3" };

    #region Settings
    public int SelectedLoadout { get; set; } = 0;
    [JsonIgnore] public int CurrentlyAppliedLoadout { get { return SelectedLoadout; } set { SelectedLoadout = value; OnPropertyChanged(); } }
    public int SelectedClient { get; set; } = 0;
    [JsonIgnore] public BLRClient? DefaultClient { get { if (SelectedClient >= DataStorage.GameClients.Count || SelectedClient < 0) { return null; } else { return DataStorage.GameClients[SelectedClient]; } } set { if (value is not null) { SelectedClient = DataStorage.GameClients.IndexOf(value); } else { SelectedClient = 0; } OnPropertyChanged(); } }
    public int SelectedServer { get; set; } = 0;
    [JsonIgnore] public BLRServer? DefaultServer { get { if (SelectedServer >= DataStorage.ServerList.Count || SelectedServer < 0) { return null; } else { return DataStorage.ServerList[SelectedServer]; } } set { if (value is not null) { SelectedServer = DataStorage.ServerList.IndexOf(value); } else { SelectedServer = 0; } OnPropertyChanged(); } }
    public UIBool EnableAPI { get; set; } = new(true);
    //public UIBool AdvancedModding { get; set; } = new(false);
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
    [JsonPropertyName("SelectedProxyVersion")] public string? jsonSelectedProxyVersion;
    [JsonIgnore] public string? SelectedProxyVersion { get { jsonSelectedProxyVersion ??= AvailableProxyVersions.First(); return jsonSelectedProxyVersion; } set { if (AvailableProxyVersions.Contains(value)) { jsonSelectedProxyVersion = value; } } }
    [JsonPropertyName("SelectedLanguage")] public string? SelectedLanguage = null;
    [JsonIgnore] public CultureInfo SelectedCulture { get { if (string.IsNullOrEmpty(SelectedLanguage)) { return CultureInfo.InvariantCulture; } else { return CultureInfo.CreateSpecificCulture(SelectedLanguage); } } set { SelectedLanguage = value.Name; OnPropertyChanged(); } }
    [JsonPropertyName("PlayerName")] public string jsonPlayerName = "BLREdit-Player";
    [JsonIgnore] public string PlayerName { get { return jsonPlayerName; } set { if (jsonPlayerName != value) { MainWindow.View.Profile.BLR.IsChanged = true; } jsonPlayerName = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProfileSettings)); MainWindow.View.UpdateWindowTitle(); } }
    [JsonPropertyName("Region")] public string jsonRegion = string.Empty;
    [JsonIgnore] public string Region { get { return jsonRegion; } set { jsonRegion = value; OnPropertyChanged(); MainWindow.View.UpdateWindowTitle(); } }
    [JsonIgnore] private string lastplayerName = "BLREdit-Player";
    [JsonIgnore] public string LastPlayerName { get { return lastplayerName; } set { lastplayerName = value; OnPropertyChanged(); } }
    [JsonIgnore] public BLRProfileSettingsWrapper ProfileSettings => ExportSystem.GetOrAddProfileSettings(PlayerName);

    [JsonPropertyName("BotCount")] public int jsonBotCount = 8;
    [JsonIgnore] public int BotCount { get { return jsonBotCount; } set { jsonBotCount = value; OnPropertyChanged(); } }
    [JsonPropertyName("PlayerCount")] public int jsonPlayerCount = 16;
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
    #endregion Commands

    private static void ApplyEvent()
    {
        //DataStorage.Settings.AdvancedModding.PropertyChanged += DataStorage.Settings.AdvancedModdingChanged;
        DataStorage.Settings.ShowHiddenServers.PropertyChanged += DataStorage.Settings.ShowHiddenServersChanged;
    }

    public static void ResetSettings()
    {
        if (!LoggingSystem.MessageLog(Properties.Resources.msg_ResetConfig, Properties.Resources.msgT_ResetConfig, MessageBoxButton.YesNo)) { return; }

        LoggingSystem.MessageLog(Properties.Resources.msg_ResetConfigInfo, Properties.Resources.msgT_ResetConfig);

        DataStorage.Settings = new BLREditSettings();

        DataStorage.GameClients.Clear();
        DataStorage.ServerList.Clear();
        DataStorage.CachedModules.Clear();

        App.Restart();
    }

    public static LaunchOptions GetLaunchOptions()
    {
        return new LaunchOptions() { UserName = DataStorage.Settings.PlayerName, Server= DataStorage.Settings.DefaultServer ?? new() };
    }

    public static void SyncDefaultClient()
    {
        if(DataStorage.GameClients.Count <= 0 || DataStorage.Settings.DefaultClient is null) { return; }
        foreach (var client in DataStorage.GameClients)
        {
            if (client.OriginalPath == DataStorage.Settings.DefaultClient.OriginalPath)
            {
                DataStorage.Settings.DefaultClient = client;
                return;
            }
        }
    }

    private void AdvancedModdingChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Is")
        {
            MainWindow.View.Profile.BLR.CalculateStats();
            MainWindow.Instance?.SetItemList();
        }
    }

    private void ShowHiddenServersChanged(object sender, PropertyChangedEventArgs e)
    {
        MainWindow.Instance?.RefreshServerList();
    }

    public static void Save()
    {
        if (DataStorage.GameClients.Count > 0)
        {
            bool client = false;
            foreach (var c in DataStorage.GameClients)
            {
                if (c.Equals(DataStorage.Settings.DefaultClient))
                {
                    client = true;
                }
            }
            if (!client) { DataStorage.Settings.DefaultClient = null; }
        }
        IOResources.SerializeFile($"{IOResources.SETTINGS_FILE}", DataStorage.Settings);
    }
}