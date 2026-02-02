using BLREdit.API.Export;
using BLREdit.Game;
using BLREdit.UI.Windows;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BLREdit.UI.Controls
{
    /// <summary>
    /// Interaction logic for PlaylistPreviewControl.xaml
    /// </summary>
    public partial class PlaylistPreviewControl : UserControl, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Events


        public PlaylistPreviewControl()
        {
            InitializeComponent();
        }

        private void Play_Playlist(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BLRPlaylist playlist) {
                if (playlist.Entries.Count <= 0) { MainWindow.ShowAlert("Playlist has no Entries!"); return; }
                if (DataStorage.Settings.DefaultClient is BLRClient client)
                {
                    if (client.ClientVersion == playlist.ClientVersion) { 
                        IOResources.SerializeFile($"{client.BLReviveConfigsPath}server_utils\\playlists\\{playlist.Name}.json", playlist.Entries);

                        var launchArgs = $"server {(string.IsNullOrEmpty(client.ConfigName) ? "" : $"?config={client.ConfigName}-Server")}?ServerName=BLREdit-{playlist.Name}-Server?Playlist={playlist.Name}?Port=7777?blre.server.authenticateusers=false";
                        var options = new LaunchOptions() { UserName = DataStorage.Settings.PlayerName, Server = BLRClient.LocalHost };
                        client.PrepClientLaunch(options);

                        Task.Run(() => {
                            BLRProcess.KillAll();
                            client.StartProcessAsync(launchArgs, true, DataStorage.Settings.ServerWatchDog.Is, null, null);
                            client.LaunchClient(options);
                        }).ConfigureAwait(false);
                    }
                }                
            }
        }

        private void Edit_Playlist(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRPlaylist playlist)
            {
                var window = new PlaylistEditorWindow(playlist);
                window.ShowDialog();
            }
        }

        private void Remove_Playlist_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRPlaylist playlist)
            {
                DataStorage.Playlists.Remove(playlist);
            }
        }
    }
}
