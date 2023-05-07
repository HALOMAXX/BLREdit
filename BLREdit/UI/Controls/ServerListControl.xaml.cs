using BLREdit.Game;
using BLREdit.Model.BLR;

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
                if (sender is ListView listView && listView.SelectedItem is BLRServerModel server)
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
        BLRServerModel droppedData = e.Data.GetData(typeof(BLRServerModel)) as BLRServerModel;
        object targetData = e.OriginalSource;
        while (targetData != null && targetData.GetType() != typeof(ServerControl))
        {
            targetData = ((FrameworkElement)targetData).Parent;
        }
        if (targetData != null) { BLRServerModel.Servers.Move(BLRServerModel.Servers.IndexOf(droppedData), BLRServerModel.Servers.IndexOf(((ServerControl)targetData).DataContext as BLRServerModel)); }
    }

    private void AddNewServer_Click(object sender, RoutedEventArgs e)
    {
        BLRServerModel.Servers.Add(new BLRServerModel());
    }

    private void ServerListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        { 
            StartPoint = e.GetPosition(null);
        }
    }
}