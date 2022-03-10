using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace BLREdit
{
    public class ExportSystem
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
                LoggingSystem.LogError("Profile was null when settings currentProfile");
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
        }

        private static ObservableCollection<ExportSystemProfile> LoadAllProfiles()
        {
            ObservableCollection<ExportSystemProfile> profiles = new ObservableCollection<ExportSystemProfile>();
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR);
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + IOResources.SEPROFILE_DIR);
            CurrentBackupFolder = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Backup\\" + System.DateTime.Now.ToString("dd-MM-yy") + "\\" + System.DateTime.Now.ToString("HH-mm") + "\\");

            Regex regex = new Regex(@"\((.)\)");

            bool oldProfiles = false;

            foreach (string file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR))
            {
                IOResources.CopyToBackup(file);
                ExportSystemProfile profile;

                try { profile = IOResources.DeserializeFile<ExportSystemProfile>(file); }
                catch { LoggingSystem.LogInfo("Found an old profile converting it to new profile format"); profile = IOResources.DeserializeFile<MagiCowsOldProfile>(file).ConvertToNew(); }
                profiles.Add(profile);

                if (!regex.IsMatch(file))
                {
                    LoggingSystem.LogInfo("Old Profile: " + file);
                    oldProfiles = true;
                    File.Delete(file);
                }
            }

            //initialize profiles with atleast one profile
            if (profiles.Count <= 0)
            {
                profiles.Add(new ExportSystemProfile());
            }

            if (oldProfiles)
            {
                //Not Good!
                Profiles = profiles;
                SaveProfiles();
            }

            return profiles;
        }

        public static void CreateSEProfile(ExportSystemProfile profile)
        { 
            SELoadout[] player = SELoadout.CreateFromMagiCowsProfile(profile);
            IOResources.SerializeFile(IOResources.SEPROFILE_DIR + profile.PlayerName + ".json", player);
        }

        public static void CopyToClipBoard(ExportSystemProfile profile)
        {
            string clipboard = "register " + Environment.NewLine + IOResources.Serialize(profile as MagiCowsProfile, true);
            bool success = false;

            try
            {
                SetClipboard(clipboard);
                success = true;
                LoggingSystem.LogInfo("Copy Succes");
            }
            catch
            { }

            if (!success)
            {
                LoggingSystem.LogWarning("Failed CopyToClipboard too often!");
                ClipboardFailed message = new ClipboardFailed(clipboard);
                message.ShowDialog();
            }
        }

        public static void SetClipboard(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "SetClipboard value was null shoul never happen");

            Process clipboardExecutable = new Process
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
                IOResources.SerializeFile(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR + "\\" + profile.Name + ".json", profile);
            }
        }

        public static void RemoveActiveProfileFromDisk()
        {
            File.Delete(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR + ActiveProfile.ProfileName + ".json");
        }

        public static void AddProfile()
        {
            Profiles.Add(new ExportSystemProfile() { ProfileName = Profiles.Count.ToString(), PlayerName = ActiveProfile.PlayerName });
        }
    }
}