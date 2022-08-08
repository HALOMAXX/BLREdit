using BLREdit.Game;
using BLREdit.UI;

using System;
using System.IO;
using System.Windows;

namespace BLREdit;

public class BLREditSettings
{
    public static BLREditSettings Settings { get; set; } = LoadSettings();

    public BLRClient DefaultClient { get; set; } = null;
    public BLRServer DefaultServer { get; set; } = null;
    public bool EnableDebugging { get; set; } = false;
    public bool ShowUpdateNotice { get; set; } = true;
    public bool DoRuntimeCheck { get; set; } = true;
    public bool ForceRuntimeCheck { get; set; } = false;
    public Visibility DebugVisibility { get; set; } = Visibility.Collapsed;
    public bool AdvancedModding { get; set; } = false;


    public void ApplySettings()
    {
        if (EnableDebugging)
        { LoggingSystem.IsDebuggingEnabled = true; }
    }

    public static LaunchOptions GetLaunchOptions()
    {
        return new LaunchOptions() { UserName = ExportSystem.ActiveProfile.PlayerName, Server=Settings.DefaultServer };
    }

    public static BLREditSettings LoadSettings()
    {
        if (File.Exists(IOResources.SETTINGS_FILE))
        {
            BLREditSettings settings = IOResources.DeserializeFile<BLREditSettings>(IOResources.SETTINGS_FILE); //Load settings file
            settings.ApplySettings();                                               //apply settings
            IOResources.SerializeFile(IOResources.SETTINGS_FILE, settings);                                     //write it back to disk to clean out settings that don't exist anymore from old builds/versions
            return settings;
        }
        else
        {
            var tmp = new BLREditSettings();
            tmp.ApplySettings();
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