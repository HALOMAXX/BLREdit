using BLREdit.API.Utils;
using BLREdit.Game;

using PeNet.Header.Resource;

using System;
using System.Collections.Generic;
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

    Point StartPoint;
    bool isDragging= false;
    private void ServerListView_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
        {
            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
               Math.Abs(position.Y - StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is ListView listView && listView.SelectedItem is BLRServer server)
                {
                    isDragging = true;
                    DragDrop.DoDragDrop(listView, server, DragDropEffects.Move);
                    isDragging = false;
                }
            }
        }
    }

    private void ServerListView_Drop(object sender, DragEventArgs e)
    {
        BLRServer? droppedData = e.Data.GetData(typeof(BLRServer)) as BLRServer;
        object targetData = e.OriginalSource;
        while (targetData != null && targetData.GetType() != typeof(ServerControl))
        {
            targetData = ((FrameworkElement)targetData).Parent;
        }
        if (targetData is not null && droppedData is not null && targetData is ServerControl sControl && sControl.DataContext is BLRServer targetServer) { DataStorage.ServerList.Move(DataStorage.ServerList.IndexOf(droppedData), DataStorage.ServerList.IndexOf(targetServer)); }
        else
        {
            LoggingSystem.Log("failed to reorder ServerListView!");
        }
    }

    private void AddNewServer_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.AddServer(new BLRServer(), true);
    }

    private void ServerListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        { 
            StartPoint = e.GetPosition(null);
        }
    }

    private void ServerListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (CollectionViewSource.GetDefaultView(ServerListView.Items) is CollectionView view)
        {
            view.Filter += new Predicate<object>((o) => 
                { 
                    if (o is BLRServer server && (DataStorage.Settings.ShowHiddenServers.Is || !server.Hidden || server.IsOnline.Is)) 
                    { return true; } 
                    else 
                    { return false; } 
                }
                );
        }
    }

    private void QuickMatch_Click(object sender, RoutedEventArgs e)
    {
        LoggingSystem.Log($"Started Matchmaking with Region:{DataStorage.Settings.Region}, Pinging {DataStorage.ServerList.Count} Servers");
        MainWindow.RefreshPing();
        BLRServer.ServersToPing.WaitForEmpty();
        LoggingSystem.Log("Finished Pinging Servers!");

        var playerCountSortedServers = DataStorage.ServerList.OrderByDescending(x => x.PlayerCount);

        Dictionary<string, List<BLRServer>> RegionServers = new();

        foreach (var server in playerCountSortedServers)
        {
            if (RegionServers.TryGetValue(server.Region, out var serverlist))
            {
                serverlist.Add(server);
            }
            else
            {
                List<BLRServer> regionlist = new()
                {
                    server
                };
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
            LoggingSystem.MessageLog($"Unable to find suitable server!\ntry again later or manually connect to one from the server list!");
        }
    }
}