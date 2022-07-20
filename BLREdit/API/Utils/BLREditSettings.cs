using BLREdit.Game;
using BLREdit.UI;

using System.IO;
using System.Windows;

namespace BLREdit;

public class BLREditSettings
{
    public static BLREditSettings Settings { get; set; } = LoadSettings();

    public GameClient DefaultClient { get; set; } = null;
    public BLRServer DefaultServer { get; set; } = null;
    public bool EnableDebugging { get; set; } = false;
    public bool ShowUpdateNotice { get; set; } = true;
    public bool DoRuntimeCheck { get; set; } = true;
    public bool ForceRuntimeCheck { get; set; } = false;
    public Visibility DebugVisibility { get; set; } = Visibility.Collapsed;

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
}