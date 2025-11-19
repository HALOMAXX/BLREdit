using BLREdit.Game;
using BLREdit.Properties;

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
        {
            string searchText = Instance.SearchFilter.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(searchText)) { return true; }

            string serverName = server.ServerInfo.ServerName?.ToUpperInvariant() ?? string.Empty;
            if(serverName.Contains(searchText)) { return true; }

            string mapName = server.ServerInfo.Map?.ToUpperInvariant() ?? string.Empty;
            if (mapName.Contains(searchText)) { return true; }

            string modeName = server.ServerInfo.GameModeFullName?.ToUpperInvariant() ?? string.Empty;
            if (modeName.Contains(searchText)) { return true; }

            string mdName = server.ServerInfo.GameMode?.ToUpperInvariant() ?? string.Empty;
            if (mdName.Contains(searchText)) { return true; }

            string usrName = server.ServerInfo.GetAllPlayerNames()?.ToUpperInvariant() ?? string.Empty;
            if (usrName.Contains(searchText)) { return true; }
            return false;
        }
        else
        { return false; }
    }
}