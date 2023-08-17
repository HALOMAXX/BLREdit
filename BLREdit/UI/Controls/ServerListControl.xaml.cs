using BLREdit.API.Utils;
using BLREdit.Game;

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
        if (targetData is not null && droppedData is not null && targetData is ServerControl sControl && sControl.DataContext is BLRServer targetServer) { MainWindow.View.ServerList.Move(MainWindow.View.ServerList.IndexOf(droppedData), MainWindow.View.ServerList.IndexOf(targetServer)); }
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
                    if (o is BLRServer server && (BLREditSettings.Settings.ShowHiddenServers.Is || !server.Hidden || server.IsOnline.Is)) 
                    { return true; } 
                    else 
                    { return false; } 
                }
                );
        }
    }

    private void QuickMatch_Click(object sender, RoutedEventArgs e)
    {
        LoggingSystem.Log($"Started Matchmaking with Region:{BLREditSettings.Settings.Region}, Pinging:{MainWindow.View.ServerList.Count}");
        MainWindow.RefreshPing();
        BLRServer.ServersToPing.WaitForEmpty();
        LoggingSystem.Log("Finished Pinging Servers!");

        var playerCountSortedServers = MainWindow.View.ServerList.OrderByDescending(x => x.PlayerCount);
        var first = playerCountSortedServers.First();
        var regionParts = BLREditSettings.Settings.Region.Split('-');
        if (first.PlayerCount > 0 || regionParts.Length <= 0)
        {
            first.ConnectToServerCommand.Execute(null);
            return;
        }
        else
        {
            foreach (var server in playerCountSortedServers)
            {
                if (server.Region.Contains(regionParts[0]))
                {
                    server.ConnectToServerCommand.Execute(null);
                    return;
                }
            }
        }
        LoggingSystem.MessageLog($"Couldn't find a Matching Server for Region:{BLREditSettings.Settings.Region} or Highest PlayerCount:{first.PlayerCount}");
    }
}