using BLREdit.Export;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace BLREdit.UI.Controls;

public partial class ProfileControl : UserControl, INotifyPropertyChanged
{
    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Event

    public ProfileControl()
    {
        InitializeComponent();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is BLRLoadoutStorage loadout)
        {
            BLRProfileGrid.DataContext = loadout.BLR;
        }
    }

    private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            if (sender is FrameworkElement element && element.DataContext is BLRLoadoutStorage loadoutStorage)
            {
                MainWindow.Instance.ProfileComboBox.SelectedValue = loadoutStorage.Shareable;
            }
        }
    }
}
