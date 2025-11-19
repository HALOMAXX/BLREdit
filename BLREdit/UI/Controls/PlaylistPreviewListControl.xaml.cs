using BLREdit.API.Export;
using BLREdit.UI.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for PlaylistPreviewListControl.xaml
    /// </summary>
    public partial class PlaylistPreviewListControl : UserControl
    {
        public PlaylistPreviewListControl()
        {
            InitializeComponent();
        }

        private void CreateNewPlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            BLRPlaylist playlist = new() { Name = "NewPlaylist" };
            DataStorage.Playlists.Add(playlist);
            var window = new PlaylistEditorWindow(playlist);
            window.ShowDialog();
        }
    }
}
