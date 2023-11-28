using BLREdit.Game;

using Microsoft.Win32;

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

namespace BLREdit.UI.Controls;

/// <summary>
/// Interaction logic for ClientListControl.xaml
/// </summary>
public partial class ClientListControl : UserControl
{
    public ClientListControl()
    {
        InitializeComponent();
    }

    Point StartPoint;
    bool isDragging = false;
    private void ClientListView_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
        {
            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
               Math.Abs(position.Y - StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is ListView listView && listView.SelectedItem is BLRClient client)
                {
                    isDragging = true;
                    DragDrop.DoDragDrop(listView, client, DragDropEffects.Move);
                    isDragging = false;
                }
            }
        }
    }

    private void ClientListView_Drop(object sender, DragEventArgs e)
    {
        BLRClient? droppedData = e.Data.GetData(typeof(BLRClient)) as BLRClient;
        object targetData = e.OriginalSource;
        while (targetData != null && targetData.GetType() != typeof(ClientControl))
        {
            targetData = ((FrameworkElement)targetData).Parent;
        }
        if (targetData is not null && droppedData is not null && targetData is ClientControl cControl && cControl.DataContext is BLRClient targetClient) { DataStorage.GameClients.Move(DataStorage.GameClients.IndexOf(droppedData), DataStorage.GameClients.IndexOf(targetClient)); }
        else
        {
            LoggingSystem.Log("failed to reorder ClientListView!");
        }
    }

    private void OpenNewGameClient_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = "Game Client|*.exe",
            InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}",
            Multiselect = false
        };
        dialog.ShowDialog();
        if (!string.IsNullOrEmpty(dialog.FileName))
        {
            DataStorage.GameClients.Add(new BLRClient() { OriginalPath = dialog.FileName });
        }
        MainWindow.CheckGameClients();
    }

    private void ClientListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            StartPoint = e.GetPosition(null);
        }
    }
}
