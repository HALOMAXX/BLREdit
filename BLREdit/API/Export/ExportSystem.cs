using BLREdit.API.Export;
using BLREdit.Game;
using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace BLREdit.Export;

public sealed class ExportSystem
{
    private static FileInfo? _currentBackupFile;
    public static FileInfo CurrentBackupFile { get { _currentBackupFile ??= new FileInfo($"Backup\\{DateTime.Now:yy-MM-dd-HH-mm-ss}.zip"); return _currentBackupFile; } }

    public static void CopyMagiCowToClipboard(BLRLoadoutStorage loadout)
    {
        var magiProfile = new MagiCowsProfile { PlayerName = loadout.Shareable.Name };
        //loadout.BLR.Write(magiProfile);
        //TODO: Broke MagiCow Export
        string clipboard = $"register{Environment.NewLine}{IOResources.RemoveWhiteSpacesFromJson.Replace(IOResources.Serialize(magiProfile, true), "$1")}";

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
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        clipboardExecutable.Start();
        clipboardExecutable.StandardInput.Write(value);
        clipboardExecutable.StandardInput.Close();
    }

    public static string? GetClipboard()
    {
        if (Clipboard.ContainsText(TextDataFormat.Text))
        {
            return Clipboard.GetText(TextDataFormat.Text);
        }
        return null;
    }

    public static ObservableCollection<BLRLoadoutStorage> LoadStorage()
    {
        LoggingSystem.Log("Started Loading Shareable and BLR Profile Combos");
        BLRLoadoutStorage[] storage = new BLRLoadoutStorage[DataStorage.ShareableLoadouts.Count];
        for (int i = 0; i < DataStorage.ShareableLoadouts.Count; i++)
        {
            DataStorage.ShareableLoadouts[i].RegisterWithChildren();
            storage[i] = new(DataStorage.ShareableLoadouts[i]);
        }

        LoggingSystem.Log("Finished Loading Shareable and BLR Profile Combos");
        return new(storage);
    }

    public static ObservableCollection<ShareableProfile> LoadShareableProfiles()
    {
        LoggingSystem.Log("Started Loading ShareableProfiles");
        ImportSystem.Initialize();

        LoggingSystem.Log($"Backup:{CurrentBackupFile.FullName}");
        LoggingSystem.Log("Compressing Profile folder!");
        LoggingSystem.ResetWatch();
        ZipFile.CreateFromDirectory($"{IOResources.PROFILE_DIR}", CurrentBackupFile.FullName, CompressionLevel.Optimal, false);
        LoggingSystem.PrintElapsedTime("Finished Compressing in {0}ms");

        var profiles = IOResources.DeserializeFile<ObservableCollection<ShareableProfile>>($"{IOResources.PROFILE_DIR}profileList.json") ?? [];

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
        LoggingSystem.Log("Finished Loading ShareableProfiles");
        return profiles;
    }

    public static ObservableCollection<ShareableLoadout> LoadShareableLoadouts()
    {
        LoggingSystem.Log("Started Loading ShareableLoadouts");
        ImportSystem.Initialize();

        var loadouts = IOResources.DeserializeFile<ObservableCollection<ShareableLoadout>>($"{IOResources.PROFILE_DIR}loadoutList.json") ?? [];

        if(DataStorage.ShareableProfiles is not null && DataStorage.ShareableProfiles.Count > 0)
            foreach (var profile in DataStorage.ShareableProfiles)
            {
                int loadoutID = 0;
                if (profile.Loadouts is not null && profile.Loadouts.Count > 0)
                    foreach (var loadout in profile.Loadouts)
                    {
                        loadout.Name = $"{profile.Name} Loadout{loadoutID}";
                        loadouts.Add(loadout);
                        loadoutID++;
                    }
            }

        if (File.Exists($"{IOResources.PROFILE_DIR}profileList.json")) File.Delete($"{IOResources.PROFILE_DIR}profileList.json");

        if (loadouts.Count <= 0)
        { 
            //TODO: Create Default Loadouts!!!!!!!!!!!!!!!!!!!!!!!
        }

        LoggingSystem.Log("Finished Loading ShareableLoadouts");
        return loadouts;
    }

    public static void UpdateOrAddProfileSettings(string profileName, BLRProfileSettingsWrapper settings)
    {
        if (settings is null) return;

        if (DataStorage.ProfileSettings.TryGetValue(profileName, out var oldProfile))
        {
            if (settings.PlayTime >= oldProfile.PlayTime)
            {
                DataStorage.ProfileSettings[profileName] = settings;
            }  
        }
        else
        {
            DataStorage.ProfileSettings.Add(profileName, settings);
        }
    }

    public static BLRProfileSettingsWrapper GetOrAddProfileSettings(string profileName)
    {
        if (DataStorage.ProfileSettings.TryGetValue(profileName, out var value))
        {
            return value;
        }
        else
        {
            List<BLRProfileSettingsWrapper> settingsWrappers = [];
            foreach (var client in DataStorage.GameClients)
            {
                client.UpdateProfileSettings();
            }
            if (settingsWrappers.Count > 0)
            {
                foreach (var filteredSetting in settingsWrappers)
                {
                    DataStorage.ProfileSettings.Add(filteredSetting.ProfileName, filteredSetting);
                }
            }

            if (DataStorage.ProfileSettings.TryGetValue(profileName, out var valu))
            {
                return valu;
            }
            else
            {
                LoggingSystem.Log($"[ProfileSettings]({profileName}): creating new profile");
                var newProfile = new BLRProfileSettingsWrapper(profileName, null);
                DataStorage.ProfileSettings.Add(profileName, newProfile);
                return newProfile;
            }
        }
    }
}