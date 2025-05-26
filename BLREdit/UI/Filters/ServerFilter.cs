using BLREdit.Game;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BLREdit.UI;

sealed class ServerFilter : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    public event EventHandler? RefreshItemLists;

    public static ServerFilter Instance { get; private set; } = new ServerFilter();

    private string searchFilter = "";
    public string SearchFilter { get { return searchFilter; } set { searchFilter = value; RefreshItemLists?.Invoke(this, new EventArgs()); OnPropertyChanged(); } }

    public static bool FullFilter(object o)
    {
        if (o is BLRServer server && (DataStorage.Settings.ShowHiddenServers.Is || !server.Hidden || server.IsOnline.Is))
        { return true; }
        else
        { return false; }
    }
}