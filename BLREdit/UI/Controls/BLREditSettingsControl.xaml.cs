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
    public ObservableCollection<CultureInfo> Cultures { get { return AvailableCultures; } }

    public BLREditSettingsControl()
    {
        InitializeComponent();
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
}
