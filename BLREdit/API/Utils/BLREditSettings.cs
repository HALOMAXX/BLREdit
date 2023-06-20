using BLREdit.Export;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.UI;

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;

namespace BLREdit;

public sealed class BLREditSettings : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    private static BLREditSettings settings = LoadSettings();
    public static BLREditSettings Settings { get { return settings; } }

    //Saves The Default Client will get validatet after GameClients have been loaded to make sure it's still a valid client
    public int SelectedClient { get; set; } = 0;
    [JsonIgnore] public BLRClient DefaultClient { get { if (SelectedClient >= MainWindow.GameClients.Count || SelectedClient < 0) { return null; } else { return MainWindow.GameClients[SelectedClient]; } } set { SelectedClient = MainWindow.GameClients.IndexOf(value); OnPropertyChanged(); } }
    //Saves the Default Server (not in use anymore)
    public int SelectedServer { get; set; } = 0;
    [JsonIgnore] public BLRServer DefaultServer { get { if (SelectedServer >= MainWindow.ServerList.Count || SelectedServer < 0) { return null; } else { return MainWindow.ServerList[SelectedServer]; } } set { SelectedServer = MainWindow.ServerList.IndexOf(value); OnPropertyChanged(); } }

    //Allows for App-Web-Protocol needs Admin rights will be set to false if it fails to Start BLREdit as Admin
    public UIBool EnableAPI { get; set; } = new(true);

    //Toggles Advanced Modding
    public UIBool AdvancedModding { get; set; } = new(false);

    //Toggles Debugging (has no use currently)
    public UIBool Debugging { get; set; } = new(false);

    //Toggle to Show if Important Runtime is missing
    public UIBool DoRuntimeCheck { get; set; } = new(true);

    //Forces Display of needed Runtimes
    public UIBool ForceRuntimeCheck { get; set; } = new(false);

    //Might not do anything anymore.
    public UIBool ShowUpdateNotice { get; set; } = new(true);

    //Toggles Advanced Modding
    public UIBool ServerWatchDog { get; set; } = new(false);

    //Filters Modules that are not in the GitHub AvailableModule List
    public UIBool StrictModuleChecks { get; set; } = new(true);

    //Adds modules that are in the Module Folder of the Client to the installed Module list and adds them to Proxy Load order
    public UIBool AllowCustomModules { get; set; } = new(false);

    //Always Installs Required Modules
    public UIBool InstallRequiredModules { get; set; } = new(true);
    public string SelectedLanguage { get; set; } = null;
    [JsonIgnore] public CultureInfo SelectedCulture { get { if (string.IsNullOrEmpty(SelectedLanguage)) { return null; } else { return CultureInfo.CreateSpecificCulture(SelectedLanguage); } } set { SelectedLanguage = value.Name; OnPropertyChanged(); } }

    private string playerName = "BLREdit-Player";
    private string lastplayerName = "BLREdit-Player";
    public string PlayerName { get { return playerName; } set { if (playerName != value) { MainWindow.Profile.IsChanged = true; } playerName = value; OnPropertyChanged(); MainWindow.View.UpdateWindowTitle(); } }
    [JsonIgnore] public string LastPlayerName { get { return lastplayerName; } set { lastplayerName = value; OnPropertyChanged(); } }

    //BotCount for Server Start
    private int botCount = 8;
    public int BotCount { get { return botCount; } set { botCount = value; OnPropertyChanged(); } }
    //PlayerCount for Server Start
    private int playerCount = 16;
    public int PlayerCount { get { return playerCount; } set { playerCount = value; OnPropertyChanged(); } }



    #region Commands
    private ICommand resetConfigCommand;
    [JsonIgnore]
    public ICommand ResetConfigCommand
    {
        get
        {
            resetConfigCommand ??= new RelayCommand(
                    param => this.ResetSettings()
                );
            return resetConfigCommand;
        }
    }
    #endregion Commands

    public void ResetSettings()
    {
        if (MessageBox.Show("Are you sure you want to reset all BLREdit Config", "this is a caption", MessageBoxButton.YesNo) != MessageBoxResult.Yes) { return; }
        LoggingSystem.MessageLog("Now Returning to Defaults. this will close BLREdit!");
        settings = new BLREditSettings();

        MainWindow.GameClients.Clear();
        MainWindow.ServerList.Clear();
        ProxyModule.CachedModules.Clear();

        MainWindow.Self.Close();
    }

    public static LaunchOptions GetLaunchOptions()
    {
        return new LaunchOptions() { UserName = Settings.PlayerName, Server=Settings.DefaultServer };
    }

    public static void SyncDefaultClient()
    {
        foreach (var client in MainWindow.GameClients)
        {
            if (client.OriginalPath == Settings.DefaultClient.OriginalPath)
            {
                Settings.DefaultClient = client;
                return;
            }
        }
    }

    private static BLREditSettings LoadSettings()
    { 
        var settings = IOResources.DeserializeFile<BLREditSettings>($"{IOResources.SETTINGS_FILE}") ?? new();
        settings.AdvancedModding.PropertyChanged += settings.AdvancedModdingChanged;
        return settings;
    }

    private void AdvancedModdingChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Is")
        {
            MainWindow.Profile.CalculateStats();
            MainWindow.Self.SetItemList();
        }
    }

    public static void Save()
    {
        if (MainWindow.GameClients is not null && MainWindow.GameClients.Count > 0)
        {
            bool client = false;
            foreach (var c in MainWindow.GameClients)
            {
                if (c.Equals(Settings.DefaultClient))
                {
                    client = true;
                }
            }
            if (!client) { Settings.DefaultClient = null; }
        }
        IOResources.SerializeFile($"{IOResources.SETTINGS_FILE}", Settings);
    }
}