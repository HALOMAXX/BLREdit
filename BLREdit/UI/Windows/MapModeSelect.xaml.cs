using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
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
using System.Windows.Shapes;

namespace BLREdit.UI.Windows;

/// <summary>
/// Interaction logic for MapModeSelect.xaml
/// </summary>
public sealed partial class MapModeSelect : Window
{
    private BLRMap? SelectedMap;
    private BLRMode? SelectedMode;
    private bool IsCanceled = true;

    static MapModeSelect()
    {
        foreach (var map in DataStorage.Maps)
        {
            foreach (var mode in DataStorage.Modes)
            {
                if (map.SupportedPlaylists.Contains(mode.PlaylistName) || map.SupportedPlaylists.Contains(mode.ModeName))
                {
                    map.SupportedGameModes.Add(mode);
                }
            }
            LoggingSystem.Log($"{map.MapName} GameModes:({map.SupportedGameModes.Count}/{map.SupportedPlaylists.Count})");
        }
    }

    public MapModeSelect(string clientVersion, string? map, string? mode)
    {
        InitializeComponent();

        if (map is null || mode is null) { this.ApplyButton.IsEnabled = false; Visibility = Visibility.Collapsed; }
        else {
            SelectedMode = BLRMode.FindPlaylistName(mode);
            SelectedMap = BLRMap.FindPlaylistName(map);
        }

            this.MapList.Items.Filter += new Predicate<object>(o =>
            {
                //if(DataStorage.Settings.AdvancedModding.Is) return true;
                if (clientVersion == "Unknown") return true;
                if (o is BLRMap map)
                {
                    return map.Available.Contains(clientVersion);
                }
                return false;
            });
    }

    public static (BLRMode? Mode, BLRMap? Map, bool Canceled) SelectMapAndMode(string clientVersion, string? map = null, string? mode = null)
    {
        MapModeSelect window = new(clientVersion, map, mode);
        window.ShowDialog();
        return (window.SelectedMode, window.SelectedMap, window.IsCanceled);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (e.Source is Button button && button.Parent is Grid grid && grid.Children[2] is ComboBox comboBox && comboBox.SelectedItem is BLRMode mode && button.DataContext is BLRMap map)
        {
            SelectedMode = mode;
            SelectedMap = map;
            IsCanceled = false;
            this.Close();
        }
    }

    private void Window_Initialized(object sender, EventArgs e)
    {
        this.DataContext = DataStorage.Settings;
        this.MapList.ItemsSource = DataStorage.Maps;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        IsCanceled = false;
        this.Close();
    }
}
