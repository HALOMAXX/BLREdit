using BLREdit.API.Utils;
using BLREdit.Game;
using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shell;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace BLREdit.UI.Controls;

/// <summary>
/// Interaction logic for ServerListControl.xaml
/// </summary>
public partial class ServerListControl : UserControl
{
    public ServerListControl()
    {
        InitializeComponent();
        ServerFilter.Instance.RefreshItemLists += RefreshList;
        DataContext = new ServerListView();
    }

    public void RefreshList(object sender, EventArgs e) {
        if (CollectionViewSource.GetDefaultView(ServerListView.ItemsSource) is CollectionView view)
        {
            view.Refresh();
        }
    }

    private void AddNewServer_Click(object sender, RoutedEventArgs e)
    {
        var server = new BLRServer() { ID = $"custom {GetCustomServerCount()}" };
        MainWindow.AddServer(server, true);
        server.EditServer();
    }

    public static int GetCustomServerCount()
    { 
        int count = 0;
        foreach (var server in DataStorage.ServerList)
        {
            if (server.ID.StartsWith("custom", StringComparison.InvariantCulture)) count++;
        }
        return count;
    }

    private void ServerListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (CollectionViewSource.GetDefaultView(ServerListView.Items) is CollectionView view)
        {
            view.Filter += new Predicate<object>(ServerFilter.FullFilter);
        }
        ApplySortingServerList();
    }

    public void ApplySortingServerList(bool resetView = false)
    {
        if (CollectionViewSource.GetDefaultView(ServerListView.ItemsSource) is CollectionView view && DataContext is ServerListView mv)
        {
            view.SortDescriptions.Clear();
            var props = Enum.GetName(typeof(ServerSortingType), Enum.GetValues(typeof(ServerSortingType)).GetValue(ServerSortCombobox.SelectedIndex)).Split('_');
            if (props.Length > 1)
            {
                view.SortDescriptions.Add(new SortDescription(props[0], mv.ServerListSortingDirection));
                view.SortDescriptions.Add(new SortDescription(props[1], mv.ServerListSortingDirection));
            }
        }
    }

    private void QuickMatch_Click(object sender, RoutedEventArgs e)
    {
        LoggingSystem.Log($"Started Matchmaking with Region:{DataStorage.Settings.Region}, Pinging {DataStorage.ServerList.Count} Servers");
        MainWindow.RefreshPing();
        BLRServer.ServersToPing.WaitForEmpty();
        LoggingSystem.Log("Finished Pinging Servers!");

        var playerCountSortedServers = DataStorage.ServerList.OrderByDescending(x => x.PlayerCount);

        Dictionary<string, List<BLRServer>> RegionServers = [];

        foreach (var server in playerCountSortedServers)
        {
            if (RegionServers.TryGetValue(server.Region, out var serverlist))
            {
                serverlist.Add(server);
            }
            else
            {
                List<BLRServer> regionlist =
                [
                    server
                ];
                RegionServers.Add(server.Region, regionlist);
            }
        }

        if (RegionServers.TryGetValue(DataStorage.Settings.Region, out var list))
        {
            foreach (var server in list)
            {
                if (server.PlayerCount < server.MaxPlayers)
                {
                    server.ConnectToServerCommand.Execute(null);
                    return;
                }
            }
        }

        LoggingSystem.Log($"No Server Available for Region:{DataStorage.Settings.Region}, now trying to find the highest populated server available to connect to!");

        BLRServer? highestPop = null;

        foreach (var regionList in RegionServers)
        {
            foreach (var server in regionList.Value)
            {
                if (server.PlayerCount < server.MaxPlayers)
                {
                    if (highestPop is null) { highestPop = server; }
                    else if (highestPop.PlayerCount < server.PlayerCount) { highestPop = server; }
                }
            }
        }

        if (highestPop is not null)
        {
            highestPop.ConnectToServerCommand.Execute(null);
        }
        else
        {
            LoggingSystem.MessageLog($"Unable to find suitable server!\ntry again later or manually connect to one from the server list!", "Info"); //TODO: Add Localization
        }
    }

    private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        ApplySortingServerList();
    }

    private void ServerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ServerFilter.Instance.SearchFilter = ServerSearchBox.Text;
    }

    private void ServerSortCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplySortingServerList();
    }

    private void ServerSortDirection_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ServerListView view)
        {
            if (view.ServerListSortingDirection == ListSortDirection.Ascending)
            {
                view.ServerListSortingDirection = ListSortDirection.Descending;
            }
            else
            {
                view.ServerListSortingDirection = ListSortDirection.Ascending;
            }
        }

        ApplySortingServerList();
    }

    private void ServerSearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox box && e.Key == Key.Return)
        {
            Keyboard.ClearFocus();
        }
    }
}

public sealed class ServerListView : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    public ObservableCollection<BLRServer> ServerList => DataStorage.ServerList;

    private ListSortDirection serverListSortingDirection = ListSortDirection.Descending;
    public ListSortDirection ServerListSortingDirection { get { return serverListSortingDirection; } set { serverListSortingDirection = value; OnPropertyChanged(); OnPropertyChanged(nameof(ServerListSortingDirectionString)); } }
    public string ServerListSortingDirectionString { get { return ServerListSortingDirection == ListSortDirection.Ascending ? Properties.Resources.btn_Ascending : Properties.Resources.btn_Descending; } }
    public ObservableCollection<string> SortTypes { get; } = LanguageResources.GetWordsOfEnum(typeof(ServerSortingType));
    public int ServerSortIndex { get { return DataStorage.Settings.ServerSortIndex; } set { DataStorage.Settings.ServerSortIndex = value; OnPropertyChanged(); } }
}