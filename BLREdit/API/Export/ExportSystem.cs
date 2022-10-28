using BLREdit.UI;

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

    private static ObservableCollection<ExportSystemProfile> LoadAllProfiles()
    {
        List<ExportSystemProfile> profiles = new();
        Directory.CreateDirectory($"{IOResources.PROFILE_DIR}");

        CurrentBackupFolder = Directory.CreateDirectory($"\\Backup\\{System.DateTime.Now:dd-MM-yy}\\{System.DateTime.Now:HH-mm}\\");

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
                catch (Exception error) { LoggingSystem.Log($"Could not delete file: {file} {error}"); }
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

    public static void CopyToClipBoard(ExportSystemProfile profile)
    {
        string clipboard = $"register {Environment.NewLine}{IOResources.Serialize(profile as MagiCowsProfile, true)}";
        bool success = false;

        try
        {
            SetClipboard(clipboard);
            success = true;
            LoggingSystem.Log("Copy Succes");
        }
        catch (Exception error)
        { LoggingSystem.Log($"failed CopyToClipboard {error}"); }

        if (!success)
        {
            LoggingSystem.Log("Failed CopyToClipboard too often!");
            ClipboardFailed message = new(clipboard);
            message.ShowDialog();
        }
    }

    public static void SetClipboard(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "SetClipboard value was null shoul never happen");

        Process clipboardExecutable = new()
        {
            StartInfo = new ProcessStartInfo // Creates the process
            {
                RedirectStandardInput = true,
                FileName = @"clip",
                UseShellExecute = false
            }
        };
        clipboardExecutable.Start();

        clipboardExecutable.StandardInput.Write(value); // CLIP uses STDIN as input.
        // When we are done writing all the string, close it so clip doesn't wait and get stuck
        clipboardExecutable.StandardInput.Close();

        return;
    }

    public static void SaveProfiles()
    {
        foreach (ExportSystemProfile profile in Profiles)
        {
            IOResources.SerializeFile($"{IOResources.PROFILE_DIR}{profile.Name}.json", profile);
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