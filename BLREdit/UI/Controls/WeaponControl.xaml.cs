using BLREdit.UI.Views;

using System;
using System.Resources;
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

namespace BLREdit.UI.Controls;

/// <summary>
/// Interaction logic for WeaponControl.xaml
/// </summary>
public sealed partial class WeaponControl : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Visibility gripVisibility = Visibility.Collapsed;
    public Visibility GripVisibility { get { return gripVisibility; } }

    public WeaponControl()
    {
        InitializeComponent();
        BLREditSettings.Settings.AdvancedModding.PropertyChanged += SettingsChanged;
        gripVisibility = BLREditSettings.Settings.AdvancedModding.Visibility;
    }

    public void SetGripVisibility(Visibility visibility)
    {
        gripVisibility = visibility;
        OnPropertyChanged(nameof(GripVisibility));
    }

    public void SettingsChanged(object sender, PropertyChangedEventArgs e)
    {
        LoggingSystem.Log($"Recieved Event Property:{e.PropertyName}");
        if (e.PropertyName == nameof(BLREditSettings.Settings.AdvancedModding.Visibility))
        {
            if (DataContext is BLRWeapon weapon)
            {
                if (weapon.IsPrimary)
                {
                    gripVisibility = BLREditSettings.Settings.AdvancedModding.Visibility;
                    OnPropertyChanged(nameof(GripVisibility));
                }
                else
                {
                    gripVisibility = Visibility.Visible;
                    OnPropertyChanged(nameof(GripVisibility));
                }
            }
        }
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        ScopePreviewImage.DataContext = this.DataContext;
    }
}
