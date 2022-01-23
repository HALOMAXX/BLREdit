using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace BLREdit
{
    public class ExportSystem
    {
        public static ObservableCollection<Profile> Profiles { get; } = LoadAllProfiles();

        private static int currentProfile = 0;
        public static Profile ActiveProfile { get { if (currentProfile < 0) { LoggingSystem.LogError("currentProfile was not found"); return Profiles[0]; } else { return Profiles[currentProfile]; } } set { currentProfile = Profiles.IndexOf(value); } }

        public static ObservableCollection<Profile> LoadAllProfiles()
        {
            ObservableCollection<Profile> profiles = new ObservableCollection<Profile>();
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR);
            foreach (string file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR))
            {
                profiles.Add(LoadProfile(file));
            }

            if (profiles.Count <= 0)
            {
                profiles.Add(new Profile());
            }

            return profiles;
        }

        public static void CopyToClipBoard(Profile profile)
        {
            string clipboard = "register " + Environment.NewLine + JsonSerializer.Serialize<Profile>(profile, IOResources.JSO);
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

        public static Profile LoadProfile(string file)
        {
            return JsonSerializer.Deserialize<Profile>(File.ReadAllText(file), IOResources.JSO);
        }

        public static void SaveProfiles()
        {
            foreach (Profile profile in Profiles)
            {
                StreamWriter sw = new StreamWriter(File.Create(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR + profile.PlayerName + ".json"));
                sw.Write(JsonSerializer.Serialize<Profile>(profile, IOResources.JSO));
                sw.Close();
            }
        }

        public static void RemoveActiveProfileFromDisk()
        {
            File.Delete(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR + ActiveProfile.PlayerName + ".json");
        }

        public static void AddProfile()
        {
            int count = 0;
            foreach (Profile profile in Profiles)
            {
                if (profile.PlayerName.Contains("Player"))
                {
                    count++;
                }
            }
            if (count == 0)
            {
                Profiles.Add(new Profile() { PlayerName = "Player" });
            }
            else
            {
                Profiles.Add(new Profile() { PlayerName = "Player" + count });
            }
        }
    }
}