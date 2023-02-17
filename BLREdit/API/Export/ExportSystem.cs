using BLREdit.Game;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLREdit.Export;

public sealed class ExportSystem
{
    public static DirectoryInfo CurrentBackupFolder { get; private set; }
    public static ObservableCollection<ExportSystemProfile> Profiles { get; private set; } = LoadAllProfiles();

    private static int currentProfile = 0;
    public static ExportSystemProfile ActiveProfile { get { return GetCurrentProfile(); } set { SetCurrentProfile(value); } }
    static Dictionary<string, BLRProfileSettingsWrapper> ProfileSettings { get; set; } = LoadSettingProfiles();

    private static ExportSystemProfile GetCurrentProfile()
    {
        if (currentProfile <= 0 && currentProfile >= Profiles.Count)
        {
            AddProfile();
            currentProfile = 0;
        }
        else if (currentProfile < 0)
        {
            currentProfile = 0;
        }
        else if (currentProfile >= Profiles.Count)
        {
            currentProfile = Profiles.Count - 1;
        }
        return Profiles[currentProfile];
    }

    private static void SetCurrentProfile(ExportSystemProfile profile)
    {
        if (profile == null)
        {
            LoggingSystem.Log("Profile was null when settings currentProfile");
            throw new ArgumentNullException(nameof(profile), "target profile can't be null for currentProfile");
        }
        int tempProfileIndex = Profiles.IndexOf(profile);
        if (tempProfileIndex < 0)
        {
            currentProfile = 0;
        }
        else
        {
            currentProfile = tempProfileIndex;
        }
        MainWindow.ActiveProfile = profile;
    }

    public static void CopyToClipBoard(BLRProfile profile)
    {
        var magiProfile = new MagiCowsProfile { PlayerName = ExportSystem.ActiveProfile.PlayerName };
        profile.WriteMagiCowsProfile(magiProfile);

        string clipboard = $"register {Environment.NewLine}{IOResources.Serialize(profile, true)}";

        try
        {
            SetClipboard(clipboard);
            LoggingSystem.Log("Clipboard copy succes!");
        }
        catch (Exception error)
        { LoggingSystem.Log($"failed CopyToClipboard {error}"); }
    }

    public static void SetClipboard(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "value to beinserted into clipboard was null");

        Process clipboardExecutable = new()
        {
            StartInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                FileName = @"clip",
                UseShellExecute = false
            }
        };
        clipboardExecutable.Start();

        clipboardExecutable.StandardInput.Write(value);
        clipboardExecutable.StandardInput.Close();

        return;
    }

    private static Dictionary<string, BLRProfileSettingsWrapper> LoadSettingProfiles()
    {
        var dict = new Dictionary<string, BLRProfileSettingsWrapper>();
        var dirs = Directory.EnumerateDirectories($"{IOResources.PROFILE_DIR}");
        foreach (var dir in dirs)
        {
            var data = dir.Split('\\');
            var name = data[data.Length - 1];

            var onlineProfile = IOResources.DeserializeFile<BLRProfileSettings[]>($"{dir}\\UE3_online_profile.json");
            var keyBinds = IOResources.DeserializeFile<BLRKeyBindings>($"{dir}\\keybinding.json");

            var profile = new BLRProfileSettingsWrapper(name, onlineProfile, keyBinds);
            dict.Add(name, profile);
        }
        return dict;
    }

    public static void UpdateOrAddProfileSettings(string profileName, BLRProfileSettingsWrapper settings)
    {
        if (settings is null) return;

        if (ProfileSettings.TryGetValue(profileName, out var _))
        {
            ProfileSettings[profileName] = settings;
        }
        else
        {
            ProfileSettings.Add(profileName, settings);
        }
    }

    public static BLRProfileSettingsWrapper GetOrAddProfileSettings(string profileName)
    {
        if (ProfileSettings.TryGetValue(profileName, out var value))
        {
            return value;
        }
        else
        {
            foreach (var profile in BLREditSettings.Settings.DefaultClient.ProfileSettings)
            {
                if (profile.Value.ProfileName == profileName)
                {
                    LoggingSystem.Log($"[ProfileSettings]({profileName}): found existing settings in default client");
                    ProfileSettings.Add(profileName, profile.Value);
                    return profile.Value;
                }
            }
            LoggingSystem.Log($"[ProfileSettings]({profileName}): creating new profile");
            var newProfile = new BLRProfileSettingsWrapper(profileName, null, null);
            ProfileSettings.Add(profileName, newProfile);
            return newProfile;
        }
    }

    private static ObservableCollection<ExportSystemProfile> LoadAllProfiles()
    {
        List<ExportSystemProfile> profiles = new();

        CurrentBackupFolder = Directory.CreateDirectory($"Backup\\{System.DateTime.Now:dd-MM-yy}\\{System.DateTime.Now:HH-mm}\\");
        LoggingSystem.Log($"Backup folder:{CurrentBackupFolder.FullName}");
        Regex regex = new(@"\((.*)\)");

        bool oldProfiles = false;
        int i = 0;
        foreach (string file in Directory.EnumerateFiles($"{IOResources.PROFILE_DIR}"))
        {
            IOResources.CopyToBackup(file);
            ExportSystemProfile profile;

            bool requestDelete = false;

            try { profile = IOResources.DeserializeFile<ExportSystemProfile>(file); }
            catch { LoggingSystem.Log("Found an old profile converting it to new profile format"); profile = IOResources.DeserializeFile<MagiCowsOldProfile>(file).ConvertToNew(); }
            profiles.Add(profile);

            if (!profile.IsHealthOkAndRepair())
            {
                requestDelete = true;
            }

            if (!regex.IsMatch(file))
            {
                LoggingSystem.Log($"Old Profile: {file}");
                oldProfiles = true;
                requestDelete = true;
                profile.Index = i;
            }

            if (requestDelete)
            {
                try { File.Delete(file); }
                catch (Exception error) { LoggingSystem.MessageLog($"Could not delete file: {file} {error}"); }
            }

            i++;
        }

        //initialize profiles with atleast one profile
        if (profiles.Count <= 0)
        {
            profiles.Add(new ExportSystemProfile());
        }

        profiles.Sort((x,y) => x.Index.CompareTo(y.Index));

        if (oldProfiles)
        {
            //Not Good!
            Profiles = new ObservableCollection<ExportSystemProfile>(profiles);
            SaveProfiles();
        }

        return new ObservableCollection<ExportSystemProfile>(profiles); ;
    }

    public static void SaveProfiles()
    {
        foreach (var profile in Profiles)
        {
            IOResources.SerializeFile($"{IOResources.PROFILE_DIR}{profile.Name}.json", profile);
        }

        foreach (var profileSettings in ProfileSettings)
        {
            Directory.CreateDirectory($"{IOResources.PROFILE_DIR}{profileSettings.Value.ProfileName}");

            IOResources.SerializeFile($"{IOResources.PROFILE_DIR}{profileSettings.Value.ProfileName}\\UE3_online_profile.json", profileSettings.Value.Settings.Values.ToArray());
            IOResources.SerializeFile($"{IOResources.PROFILE_DIR}{profileSettings.Value.ProfileName}\\keybinding.json", profileSettings.Value.KeyBindings);
        }
    }

    public static void RemoveActiveProfileFromDisk()
    {
        File.Delete($"{IOResources.PROFILE_DIR}{ActiveProfile.Name}.json");
    }

    public static ExportSystemProfile AddProfile(string Name)
    {
        var prof = new ExportSystemProfile() { Index = Profiles.Count, PlayerName = Name };
        Profiles.Add(prof);
        return prof;
    }

    public static ExportSystemProfile AddProfile()
    {
        return AddProfile(ActiveProfile.PlayerName);
    }
}