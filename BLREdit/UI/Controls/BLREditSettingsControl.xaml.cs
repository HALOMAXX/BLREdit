﻿using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public static ObservableCollection<CultureInfo> AvailableCultures { get; } = [];
    public static ObservableCollection<string?> BLReviveVersions => BLREditSettings.AvailableBLReviveVersions;
    public static ObservableCollection<string?> BLREditVersions => BLREditSettings.AvailableBLREditVersions;

    public static Regex PlayerNameFilter { get; } = new(@"^[a-zA-Z0-9\-_.]*$");
    public static Regex PlayerNameSanitizer { get; } = new(@"[^a-zA-Z0-9\-_.]*");

    public BLREditSettingsControl()
    {
        InitializeComponent();
        PlayerNameBorder.Background = SolidColorBrush;
        App.AvailableProxyModuleCheck();
        AvailableCultures.Add(App.DefaultCulture);
        foreach (var locale in App.AvailableLocalizations)
        {
            AvailableCultures.Add(CultureInfo.CreateSpecificCulture(locale.Key));
        }
        LanguageComboBox.SelectedItem = DataStorage.Settings.SelectedCulture;
        BLREditVersionComboBox.SelectedItem = DataStorage.Settings.SelectedBLREditVersion;
        BLReviveVersionComboBox.SelectedItem = DataStorage.Settings.SelectedBLReviveVersion;
        DataStorage.Settings.PlayerName = PlayerNameSanitizer.Replace(DataStorage.Settings.PlayerName, string.Empty);
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

    ColorAnimation? lastAnim;
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
            e.Handled = !PlayerNameFilter.IsMatch(text);
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

    private void BLReviveVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is BLREditSettings settings && e.AddedItems.Count > 0 && e.AddedItems[0] is string version)
        {
            settings.SelectedBLReviveVersion = version;
        }
    }

    private void BLREditVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is BLREditSettings settings && e.AddedItems.Count > 0 && e.AddedItems[0] is string version)
        {
            settings.SelectedBLREditVersion = version;
        }
    }
}
