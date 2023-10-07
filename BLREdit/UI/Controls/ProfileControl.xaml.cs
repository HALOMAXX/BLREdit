using BLREdit.API.Export;
using BLREdit.Export;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
}
