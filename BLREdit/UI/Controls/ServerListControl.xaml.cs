using BLREdit.API.Utils;
using BLREdit.Game;

using PeNet.Header.Resource;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    }

    private void AddNewServer_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.AddServer(new BLRServer() { ID = $"custom {GetCustomServerCount()}"}, true);
    }

    public static int GetCustomServerCount()
    { 
        int count = 0;
        foreach (var server in DataStorage.ServerList)
        {
            if (server.ID.StartsWith("custom")) count++;
        }
        return count;
    }

    private void ServerListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (CollectionViewSource.GetDefaultView(ServerListView.Items) is CollectionView view)
        {
            view.Filter += new Predicate<object>(ServerFilter.FullFilter);
        }
        ApplySorting();
    }

    public void ApplySorting()
    {
        if (CollectionViewSource.GetDefaultView(ServerListView.ItemsSource) is CollectionView view)
        {
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("PlayerCount", ListSortDirection.Descending));
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
        ApplySorting();
    }
}