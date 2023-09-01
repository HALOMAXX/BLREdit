using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
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
        //DataContext = BLREditSettings.Settings;
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
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }
    }

    private SolidColorBrush SolidColorBrush { get; } = new(Color.FromArgb(32,0,0,0));
    private ColorAnimation AlertAnim { get; } = new()
    {
        To = Color.FromArgb(255, 255, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2)),
        AutoReverse = true,
        RepeatBehavior = RepeatBehavior.Forever
    };
    private ColorAnimation WrongAnim { get; } = new()
    {
        From = Color.FromArgb(32, 0, 0, 0),
        To = Color.FromArgb(255, 255, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(0.333)),
        AutoReverse = true
    };

    private ColorAnimation CalmAnim { get; } = new()
    {
        To = Color.FromArgb(32, 0, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2))
    };

    ColorAnimation? lastAnim = null;
    private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (PlayerNameTextBox.Text == "BLREdit-Player" && lastAnim != AlertAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, AlertAnim, HandoffBehavior.Compose);
            lastAnim = AlertAnim;
        }
        else if (PlayerNameTextBox.Text != "BLREdit-Player" && lastAnim == AlertAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
            lastAnim = CalmAnim;
        }
    }

    private void PlayerNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var text = textBox.Text;
            text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            text = text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !text.Equals(SimpleTextFilter(text));
            if (e.Handled)
            {
                if (PlayerNameTextBox.Text != "BLREdit-Player")
                {
                    SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, WrongAnim, HandoffBehavior.Compose);
                    lastAnim = WrongAnim;
                }
                if (textBox.ToolTip is System.Windows.Controls.ToolTip toolTip)
                {
                    toolTip.IsOpen = true;
                }
                else
                {
                    textBox.ToolTip = new ToolTip()
                    {
                        Content = textBox.ToolTip,
                        IsOpen = true
                    };
                }
            }
        }
    }


    private static readonly char[] invalidChars = (new string(System.IO.Path.GetInvalidPathChars()) + new string(System.IO.Path.GetInvalidFileNameChars())).ToArray();
    public static string SimpleTextFilter(string input)
    { 
        return new string(input.Where(x => !invalidChars.Contains(x)).ToArray());
    }
}
