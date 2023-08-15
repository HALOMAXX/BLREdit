using BLREdit.Import;

using System;
using System.Collections.Generic;
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
    public static List<BLRMap> Maps { get; } = IOResources.DeserializeFile<List<BLRMap>>($"Assets\\json\\maps.json") ?? new();
    public static List<BLRMode> Modes { get; } = IOResources.DeserializeFile<List<BLRMode>>($"Assets\\json\\modes.json") ?? new();

    private BLRMap? SelectedMap = null;
    private BLRMode? SelectedMode = null;
    private bool IsCanceled = true;

    static MapModeSelect()
    {
        foreach (var map in Maps)
        {
            foreach (var mode in Modes)
            {
                if (map.SupportedPlaylists.Contains(mode.PlaylistName) || map.SupportedPlaylists.Contains(mode.ModeName))
                {
                    map.SupportedGameModes.Add(mode);
                    
                }
            }
            LoggingSystem.Log($"{map.MapName} GameModes:({map.SupportedGameModes.Count}/{map.SupportedPlaylists.Count})");
        }
    }

    public MapModeSelect(string clientVersion)
    {
        InitializeComponent();
        this.MapList.Items.Filter += new Predicate<object>(o =>
        {
            if(BLREditSettings.Settings.AdvancedModding.Is) return true;
            if (clientVersion == "Unknown") return true;
            if (o is BLRMap map)
            {
                return map.Available.Contains(clientVersion);
            }
            return false;
        });
    }

    public static (BLRMode? Mode, BLRMap? Map, bool Canceled) SelectMapAndMode(string clientVersion)
    {
        MapModeSelect window = new(clientVersion);
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
        this.DataContext = BLREditSettings.Settings;
        this.MapList.ItemsSource = Maps;
    }
}
