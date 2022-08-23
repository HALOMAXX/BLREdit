using BLREdit.Game;
using BLREdit.UI;

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;

namespace BLREdit;

public class BLREditSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }


    private static BLREditSettings settings = LoadSettings();
    public static BLREditSettings Settings { get { return settings; } private set { settings = value; } }

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
    private UIBool debugging = new UIBool(false);
    public UIBool Debugging { get { return debugging; } set { debugging = value; OnPropertyChanged(); } }
    private UIBool modding = new UIBool(false);
    public UIBool AdvancedModding { get { return modding; } set { modding = value; OnPropertyChanged(); } }

    public static LaunchOptions GetLaunchOptions()
    {
        return new LaunchOptions() { UserName = ExportSystem.ActiveProfile.PlayerName, Server=Settings.DefaultServer };
    }

    public static BLREditSettings LoadSettings()
    {
        if (File.Exists(IOResources.SETTINGS_FILE))
        {
            BLREditSettings settings = IOResources.DeserializeFile<BLREditSettings>(IOResources.SETTINGS_FILE); //Load settings file
            IOResources.SerializeFile(IOResources.SETTINGS_FILE, settings);                                     //write it back to disk to clean out settings that don't exist anymore from old builds/versions
            return settings;
        }
        else
        {
            var tmp = new BLREditSettings();
            IOResources.SerializeFile(IOResources.SETTINGS_FILE, tmp);
            return tmp;
        }
    }

    public static void Save()
    {
        bool client = false;
        foreach (var c in MainWindow.Self.GameClients)
        {
            if (c.Equals(Settings.DefaultClient))
            {
                client = true;
            }
        }
        if (!client) { Settings.DefaultClient = null; }

        IOResources.SerializeFile(IOResources.SETTINGS_FILE, Settings);
    }
}