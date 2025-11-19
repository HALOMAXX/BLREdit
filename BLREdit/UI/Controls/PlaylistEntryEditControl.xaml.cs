using BLREdit.API.Export;
using BLREdit.Import;
using BLREdit.UI.Windows;

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
using System.Windows.Navigation;
using System.Windows.Shapes;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace BLREdit.UI.Controls
{
    /// <summary>
    /// Interaction logic for PlaylistEntryEditControl.xaml
    /// </summary>
    public partial class PlaylistEntryEditControl : UserControl
    {
        private Window? _parentWindow;
        private Window ParentWindow { get { _parentWindow ??= Window.GetWindow(this); return _parentWindow; } }

        public PlaylistEntryEditControl()
        {
            InitializeComponent();
        }

        private void Edit_Playlist_Entry_Click(object sender, RoutedEventArgs e)
        {
            
            if (sender is Button btn && btn.DataContext is BLRPlaylistEntry entry && ParentWindow.DataContext is BLRPlaylist playlist) {
                var tempbot = DataStorage.Settings.BotCount;
                var tempplayer = DataStorage.Settings.PlayerCount;
                var temptime = DataStorage.Settings.Timelimit;

                DataStorage.Settings.BotCount = entry.Properties.NumBots;
                DataStorage.Settings.PlayerCount = entry.Properties.MaxPlayers;
                DataStorage.Settings.Timelimit = entry.Properties.TimeLimit;

                var (mode, map, canceled) = MapModeSelect.SelectMapAndMode(playlist.ClientVersion, entry.Map, entry.GameMode);

                if (!canceled && map is not null && mode is not null) {
                    entry.Map = map.PlaylistName;
                    entry.GameMode = mode.PlaylistName;
                    entry.Properties.NumBots = DataStorage.Settings.BotCount;
                    entry.Properties.MaxPlayers = DataStorage.Settings.PlayerCount;
                    entry.Properties.TimeLimit = DataStorage.Settings.Timelimit;
                }

                DataStorage.Settings.BotCount = tempbot;
                DataStorage.Settings.PlayerCount = tempplayer;
                DataStorage.Settings.Timelimit = temptime;
            }
        }

        private void Remove_PlaylistEntry_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BLRPlaylistEntry entry && ParentWindow.DataContext is BLRPlaylist playlist)
            {
                playlist.Entries.Remove(entry);
            }
        }

        private void NumericTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox box && e.Key == Key.Return && box.GetBindingExpression(TextBox.TextProperty) is BindingExpression be)
            {
                be.UpdateSource();
                Keyboard.ClearFocus();
            }
            else if (e.Key == Key.Space) { e.Handled = true; }
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox box && box.GetBindingExpression(TextBox.TextProperty) is BindingExpression be)
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
        }
    }
}
