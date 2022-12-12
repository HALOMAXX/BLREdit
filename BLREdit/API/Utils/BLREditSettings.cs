using BLREdit.Export;
using BLREdit.Game;
using BLREdit.UI;

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;

namespace BLREdit;

public sealed class BLREditSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }


    private static readonly BLREditSettings settings = IOResources.DeserializeFile<BLREditSettings>($"{IOResources.SETTINGS_FILE}") ?? new();
    public static BLREditSettings Settings { get { return settings; } }

    private BLRClient client = null;
    public BLRClient DefaultClient { get { return client; } set { client = value; OnPropertyChanged(); } }
    private BLRServer server = null;
    public BLRServer DefaultServer { get { return server; } set { server = value; OnPropertyChanged(); } }

    private bool showUpdateNotice = true;
    public bool ShowUpdateNotice { get { return showUpdateNotice; } set { showUpdateNotice = value; OnPropertyChanged(); } }
    private bool doRuntimeCheck = true;
    public bool DoRuntimeCheck { get { return doRuntimeCheck; } set { doRuntimeCheck = value; OnPropertyChanged(); } }
    private bool forceRuntimeCheck = false;
    public bool ForceRuntimeCheck { get { return forceRuntimeCheck; } set { forceRuntimeCheck = value; OnPropertyChanged(); } }
    private UIBool debugging = new(false);
    public UIBool Debugging { get { return debugging; } set { debugging = value; OnPropertyChanged(); } }
    private UIBool modding = new(false);
    public UIBool AdvancedModding { get { return modding; } set { modding = value; OnPropertyChanged(); } }
    private int botCount = 8;
    public int BotCount { get { return botCount; } set { botCount = value; OnPropertyChanged(); } }
    private bool strictModuleChecks = true;
    public bool StrictModuleChecks { get { return strictModuleChecks; } set { strictModuleChecks = value; OnPropertyChanged(); } }

    private bool allowCustomModules = false;
    public bool AllowCustomModules { get { return allowCustomModules; } set { allowCustomModules = value; OnPropertyChanged(); } }

    private int playerCount = 16;
    public int PlayerCount { get { return playerCount; } set { playerCount = value; OnPropertyChanged(); } }

    public static LaunchOptions GetLaunchOptions()
    {
        return new LaunchOptions() { UserName = ExportSystem.ActiveProfile.PlayerName, Server=Settings.DefaultServer };
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

            IOResources.SerializeFile($"{IOResources.SETTINGS_FILE}", Settings);
        }
    }
}