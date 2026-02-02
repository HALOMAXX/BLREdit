using BLREdit.API.Export;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using static System.Net.Mime.MediaTypeNames;

namespace BLREdit.UI.Windows
{
    /// <summary>
    /// Interaction logic for PlaylistEditorWindow.xaml
    /// </summary>
    public partial class PlaylistEditorWindow : Window
    {
        public PlaylistEditorWindow(BLRPlaylist playlist)
        {
            DataContext = playlist;
            InitializeComponent();
        }

        private void AddPlaylistEntry(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRPlaylist playlist)
            {
                var (mode, map, canceled) = MapModeSelect.SelectMapAndMode(playlist.ClientVersion);
                if (!canceled)
                {
                    playlist.Entries.Add(new() { Map = map.PlaylistName, GameMode = mode.PlaylistName, Properties = { NumBots = DataStorage.Settings.BotCount, MaxPlayers = DataStorage.Settings.PlayerCount, TimeLimit = DataStorage.Settings.Timelimit } });
                }
            }
        }

        private void PlaylistName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox box && e.Key == Key.Return && box.GetBindingExpression(TextBox.TextProperty) is BindingExpression be)
            {
                be.UpdateSource();
                Keyboard.ClearFocus();
            }
            else if (e.Key == Key.Space) { e.Handled = true; }
        }

        private static readonly Regex _regex = new("[^a-zA-Z0-9,._+!=;{}@#$%^&()\\]\\['\"-]+"); //regex that matches disallowed text

        private void PlaylistName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = (_regex.IsMatch(e.Text) && !e.Text.Contains(' '));
        }
    }
}
