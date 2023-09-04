using BLREdit.API.Export;
using BLREdit.Game;
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;

using PeNet;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BLREdit.Export;

public sealed class ExportSystem
{
    private static DirectoryInfo? _currentBackupFolder = null;
    public static DirectoryInfo CurrentBackupFolder { get { _currentBackupFolder ??= Directory.CreateDirectory($"Backup\\{DateTime.Now:dd-MM-yy}\\{DateTime.Now:HH-mm}\\"); return _currentBackupFolder; } }
    public static ObservableCollection<ShareableProfile> Profiles { get; private set; } = LoadAllProfiles();

    private static int currentProfile = 0;
    public static ShareableProfile ActiveProfile { get { return GetCurrentProfile(); } set { SetCurrentProfile(value); } }
    static Dictionary<string, BLRProfileSettingsWrapper> ProfileSettings { get; set; } = LoadSettingProfiles();

    private static ShareableProfile GetCurrentProfile()
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

    private static void SetCurrentProfile(ShareableProfile profile)
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
        MainWindow.View.ActiveLoadoutSet = profile;
    }

    public static void CopyMagiCowToClipboard(BLRProfile profile)
    {
        var magiProfile = new MagiCowsProfile { PlayerName = ActiveProfile.Name };
        profile.Write(magiProfile);

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
        try
        {
            var dirs = Directory.EnumerateDirectories($"{IOResources.PROFILE_DIR}GameSettings\\");
            foreach (var dir in dirs)
            {
                var data = dir.Split('\\');
                var name = data[data.Length - 1];

                var onlineProfile = IOResources.DeserializeFile<BLRProfileSettings[]>($"{dir}\\UE3_online_profile.json");
                var keyBinds = IOResources.DeserializeFile<BLRKeyBindings>($"{dir}\\keybinding.json");

                var profile = new BLRProfileSettingsWrapper(name, onlineProfile, keyBinds);

                if(File.Exists($"{dir}\\UE3_online_profile.json")) IOResources.CopyToBackup($"{dir}\\UE3_online_profile.json", $"GameSettings\\{name}\\");
                if(File.Exists($"{dir}\\keybinding.json")) IOResources.CopyToBackup($"{dir}\\keybinding.json", $"GameSettings\\{name}\\");

                dict.Add(name, profile);
            }
        }
        catch { }

        return dict;
    }

    public static void UpdateOrAddProfileSettings(string profileName, BLRProfileSettingsWrapper settings)
    {
        if (settings is null) return;

        if (ProfileSettings.TryGetValue(profileName, out var oldProfile))
        {
            if (settings.PlayTime >= oldProfile.PlayTime)
            {
                ProfileSettings[profileName] = settings;
            }  
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
            List<BLRProfileSettingsWrapper> settingsWrappers = new();
            foreach (var client in MainWindow.View.GameClients)
            {
                client.UpdateProfileSettings();
            }
            if (settingsWrappers.Count > 0)
            {
                foreach (var filteredSetting in settingsWrappers)
                {
                    ProfileSettings.Add(filteredSetting.ProfileName, filteredSetting);
                }
            }

            if (ProfileSettings.TryGetValue(profileName, out var valu))
            {
                return valu;
            }
            else
            {
                LoggingSystem.Log($"[ProfileSettings]({profileName}): creating new profile");
                var newProfile = new BLRProfileSettingsWrapper(profileName, null, null);
                ProfileSettings.Add(profileName, newProfile);
                return newProfile;
            }
        }
    }

    private static ObservableCollection<ShareableProfile> LoadAllProfiles()
    {
        ImportSystem.Initialize();

        LoggingSystem.Log($"Backup folder:{CurrentBackupFolder.FullName}");

        var profiles = IOResources.DeserializeFile<ObservableCollection<ShareableProfile>>($"{IOResources.PROFILE_DIR}profileList.json") ?? new();        

        LoggingSystem.Log("Copying all Profiles to Backup folder!");
        foreach (string file in Directory.EnumerateFiles($"{IOResources.PROFILE_DIR}"))
        {
            IOResources.CopyToBackup(file);
        }
        LoggingSystem.Log("Finished Copying!");

        foreach (string file in Directory.EnumerateFiles($"{IOResources.PROFILE_DIR}"))
        {
            var esProfile = IOResources.DeserializeFile<ExportSystemProfile>(file);
            if (esProfile is null)
            {
                var mcProfile = IOResources.DeserializeFile<MagiCowsOldProfile>(file);
                if (mcProfile is not null)
                {
                    LoggingSystem.Log("Found an old profile converting it to new profile format");
                    esProfile = mcProfile.ConvertToNew();
                }
            }

            if (esProfile is not null && esProfile.IsHealthOkAndRepair())
            {
                profiles.Add(esProfile.ConvertToShareable());
                File.Delete(file);
            }
        }

        return profiles;
    }

    public static void SaveProfiles()
    {
        IOResources.SerializeFile($"{IOResources.PROFILE_DIR}profileList.json", Profiles);

        foreach (var profileSettings in ProfileSettings)
        {
            Directory.CreateDirectory($"{IOResources.PROFILE_DIR}GameSettings\\{profileSettings.Value.ProfileName}");

            IOResources.SerializeFile($"{IOResources.PROFILE_DIR}GameSettings\\{profileSettings.Value.ProfileName}\\UE3_online_profile.json", profileSettings.Value.Settings.Values.ToArray());
            IOResources.SerializeFile($"{IOResources.PROFILE_DIR}GameSettings\\{profileSettings.Value.ProfileName}\\keybinding.json", profileSettings.Value.KeyBindings);
        }
    }

    public static void RemoveActiveProfileFromDisk()
    {
        File.Delete($"{IOResources.PROFILE_DIR}{ActiveProfile.Name}.json");
    }

    public static ShareableProfile AddProfile(string Name)
    {
        var prof = new ShareableProfile() { Name = Name };
        Profiles.Add(prof);
        return prof;
    }

    public static ShareableProfile AddProfile()
    {
        return AddProfile("New Profile!");
    }
}