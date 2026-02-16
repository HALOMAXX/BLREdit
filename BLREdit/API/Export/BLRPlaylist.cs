using BLREdit.Import;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace BLREdit.API.Export
{
    public sealed class BLRPlaylist : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Events

        #region PrivatFields
        [JsonIgnore] private string _Name = "Playlist Name";
        [JsonIgnore] private string _ClientVersion = "v302";
        [JsonIgnore] private ObservableCollection<BLRPlaylistEntry> _Entries = [];
        #endregion PrivateFields

        public string Name { get { return _Name; } set { _Name = value; OnPropertyChanged(); } }
        public string ClientVersion { get { return _ClientVersion; } set { _ClientVersion = value; OnPropertyChanged(); } }
        public ObservableCollection<BLRPlaylistEntry> Entries { get { return _Entries; } set { _Entries = value; OnPropertyChanged(); } }
        [JsonIgnore] public BitmapImage MapImage {
            get {
                if (_Entries != null && _Entries.Count > 0 && !string.IsNullOrEmpty(_Entries[0].Map) && BLRMap.FindPlaylistName(_Entries[0].Map) is BLRMap map) {
                    return new(new Uri(map.SquareImage)); }
                else {
                    return new(new Uri($"{IOResources.BaseDirectory}Assets\\textures\\t_bluescreen2.png")); }
            }
        }

        public static void CheckDefaultPlaylists()
        {
            foreach (var playlist in DefaultPlaylists)
            {
                foreach (var map in playlist.Entries)
                {
                    if (BLRMap.FindPlaylistName(map.Map) is BLRMap blrMap)
                    {
                        if (!blrMap.SupportedPlaylists.Contains(map.GameMode))
                        {
                            LoggingSystem.DebugBreak();
                        }
                    }
                }
            }
        }

        [JsonIgnore] public static BLRServerProperties DefaultModeProperties { get; } = new BLRServerProperties { GameRespawnTime = 1, GameForceRespawnTime = 9999, MaxIdleTime = -1, NumBots = 8, PlayerSearchTime = 5};
        [JsonIgnore] public static BLRServerProperties DefaultOnslaughtProperties { get; } = new BLRServerProperties { GameRespawnTime = 1, GameForceRespawnTime = 9999, MaxIdleTime = -1, NumBots = 8, PlayerSearchTime = 5, TimeLimit = 9999 };

        public static Collection<BLRPlaylist> DefaultPlaylists { get; } =
        [
            new BLRPlaylist {
                Name = "Capture-the-Flag",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "CTF", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "CTF", Properties = DefaultModeProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Deathmatch",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", Properties = DefaultModeProperties },
                ] 
            },
            new BLRPlaylist {
                Name = "Team-Deathmatch",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "TDM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "TDM", Properties = DefaultModeProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Domination",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "DOM", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "DOM", Properties = DefaultModeProperties },
                ]
            },
            new BLRPlaylist {
                Name = "King-of-the-Hill",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "KOTH", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "KOTH", Properties = DefaultModeProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Last-Man-Standing",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "LMS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "LMS", Properties = DefaultModeProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Last-Team-Standing",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "LTS", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "LTS", Properties = DefaultModeProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Search-and-Destroy",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "SND", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "SND", Properties = DefaultModeProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Kill-Confirmed",
                Entries = [
                    new BLRPlaylistEntry { Map = "containment", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "convoy", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "crashsite", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "deadlock", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "decay", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "evac", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "heavymetal", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "helodeck", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "metro", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "Outpost", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "piledriver", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "rig", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "safehold", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "seaport", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "trench", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vertigo", GameMode = "KC", Properties = DefaultModeProperties },
                    new BLRPlaylistEntry { Map = "vortex", GameMode = "KC", Properties = DefaultModeProperties },
                ]
            },

            //------- Onslaught ----------//
            
            new BLRPlaylist {
                Name = "Onslaught-Easy",
                Entries = [
                    new BLRPlaylistEntry { Map = "centre", GameMode = "OS_Easy", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "deathmetal", GameMode = "OS_Easy", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "lockdown", GameMode = "OS_Easy", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "shelter", GameMode = "OS_Easy", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "Terminus", GameMode = "OS_Easy", Properties = DefaultOnslaughtProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Onslaught-Medium",
                Entries = [
                    new BLRPlaylistEntry { Map = "centre", GameMode = "OS_Medium", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "deathmetal", GameMode = "OS_Medium", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "lockdown", GameMode = "OS_Medium", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "shelter", GameMode = "OS_Medium", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "Terminus", GameMode = "OS_Medium", Properties = DefaultOnslaughtProperties },
                ]
            },
            new BLRPlaylist {
                Name = "Onslaught-Hard",
                Entries = [
                    new BLRPlaylistEntry { Map = "centre", GameMode = "OS_Hard", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "deathmetal", GameMode = "OS_Hard", Properties = DefaultOnslaughtProperties},
                    new BLRPlaylistEntry { Map = "lockdown", GameMode = "OS_Hard", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "shelter", GameMode = "OS_Hard", Properties = DefaultOnslaughtProperties },
                    new BLRPlaylistEntry { Map = "Terminus", GameMode = "OS_Hard", Properties = DefaultOnslaughtProperties},
                ]
            },
        ];
    }
}
