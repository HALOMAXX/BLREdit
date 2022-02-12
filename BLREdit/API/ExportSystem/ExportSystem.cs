using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace BLREdit
{
    public class ExportSystem
    {
        public static ObservableCollection<MagiCowsProfile> Profiles { get; } = LoadAllProfiles();

        private static int currentProfile = 0;
        public static MagiCowsProfile ActiveProfile { get { return GetCurrentProfile(); } set { SetCurrentProfile(value); } }

        private static MagiCowsProfile GetCurrentProfile()
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

        private static void SetCurrentProfile(MagiCowsProfile profile)
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

        private static ObservableCollection<MagiCowsProfile> LoadAllProfiles()
        {
            ObservableCollection<MagiCowsProfile> profiles = new ObservableCollection<MagiCowsProfile>();
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR);
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + IOResources.SEPROFILE_DIR);
            foreach (string file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR))
            {
                MagiCowsProfile profile;
                try { profile = IOResources.DeserializeFile<MagiCowsProfile>(file); }
                catch { LoggingSystem.LogInfo("Found an old profile converting it to new profile format"); profile = IOResources.DeserializeFile<MagiCowsOldProfile>(file).ConvertToNew(); }
                profiles.Add(profile);
            }

            //initialize profiles with atleast one profile
            if (profiles.Count <= 0)
            {
                profiles.Add(new MagiCowsProfile());
            }

            return profiles;
        }

        public static void CreateSEProfile(MagiCowsProfile profile)
        { 
            SELoadout[] player = SELoadout.CreateFromMagiCowsProfile(profile);
            IOResources.SerializeFile(IOResources.SEPROFILE_DIR + profile.PlayerName + ".json", player);
        }

        public static void CopyToClipBoard(MagiCowsProfile profile)
        {
            string clipboard = "register " + Environment.NewLine + IOResources.Serialize(profile, true);
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
            foreach (MagiCowsProfile profile in Profiles)
            {
                profile.SaveProfile();
            }
        }

        public static void RemoveActiveProfileFromDisk()
        {
            File.Delete(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR + ActiveProfile.PlayerName + ".json");
        }

        public static void AddProfile()
        {
            int count = 0;
            foreach (MagiCowsProfile profile in Profiles)
            {
                if (profile.PlayerName.Contains("Player"))
                {
                    count++;
                }
            }
            if (count == 0)
            {
                Profiles.Add(new MagiCowsProfile() { PlayerName = "Player" });
            }
            else
            {
                Profiles.Add(new MagiCowsProfile() { PlayerName = "Player" + count });
            }
        }
    }
}