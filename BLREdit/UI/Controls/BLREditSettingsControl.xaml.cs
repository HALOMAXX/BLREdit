using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BLREdit.UI.Controls;

/// <summary>
/// Interaction logic for BLREditSettingsControl.xaml
/// </summary>
public sealed partial class BLREditSettingsControl : UserControl
{
    public static ObservableCollection<CultureInfo> AvailableCultures { get; } = new();

    public BLREditSettingsControl()
    {
        InitializeComponent();
        PlayerNameBorder.Background = SolidColorBrush;
        DataContext = BLREditSettings.Settings;
        App.AvailableProxyModuleCheck();
        AvailableCultures.Add(App.DefaultCulture);
        foreach (var locale in App.AvailableLocalizations)
        {
            AvailableCultures.Add(CultureInfo.CreateSpecificCulture(locale.Key));
        }
        LanguageComboBox.SelectedItem = BLREditSettings.Settings.SelectedCulture;
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is CultureInfo culture && DataContext is BLREditSettings settings)
        {
            settings.SelectedCulture = culture;
        }
    }

    private SolidColorBrush SolidColorBrush { get; } = new(Colors.Blue);
    private ColorAnimation AlertAnim { get; } = new()
    {
        From = Color.FromArgb(32, 0, 0, 0),
        To = Color.FromArgb(255, 255, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2)),
        AutoReverse = true,
        RepeatBehavior = RepeatBehavior.Forever
    };

    private ColorAnimation CalmAnim { get; } = new()
    {
        From = Color.FromArgb(255, 255, 0, 0),
        To = Color.FromArgb(32, 0, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2))
    };

    ColorAnimation lastAnim = null;
    private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (PlayerNameTextBox.Text == "BLREdit-Player" && lastAnim != AlertAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, AlertAnim, HandoffBehavior.Compose);
            lastAnim = AlertAnim;
        }
        else if (PlayerNameTextBox.Text != "BLREdit-Player" && lastAnim != CalmAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
            lastAnim = CalmAnim;
        }
    }
}
