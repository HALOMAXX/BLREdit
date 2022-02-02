﻿using System.IO;
using System.Windows;

namespace BLREdit
{
    public class BLREditSettings
    {
        public static BLREditSettings Settings { get; set; } = LoadSettings();

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

        public static BLREditSettings LoadSettings()
        {
            if (File.Exists(IOResources.SETTINGS_FILE))
            {
                BLREditSettings settings = IOResources.Deserialize<BLREditSettings>(IOResources.SETTINGS_FILE); //Load settings file
                settings.ApplySettings();                                               //apply settings
                IOResources.Serialize(IOResources.SETTINGS_FILE, settings);                                     //write it back to disk to clean out settings that don't exist anymore from old builds/versions
                return settings;
            }
            else
            {
                var tmp = new BLREditSettings();
                tmp.ApplySettings();
                IOResources.Serialize(IOResources.SETTINGS_FILE, tmp);
                return tmp;
            }
        }
    }
}