using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace BLREdit.UI.Controls;

public class EquipmentControl : UserControl
{
    protected Grid EquipmentControlGrid;
    public static int SelectedBorder { get; private set; } = 1;
    protected void Grid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.Source is Image image && image.Parent is Border border)
        {
            SelectedBorder = EquipmentControlGrid.Children.IndexOf(border);
        }
        else if (e.Source is Border border2)
        {
            SelectedBorder = EquipmentControlGrid.Children.IndexOf(border2);
        }
    }

    internal void ApplyBorder()
    {
        if (SelectedBorder > -1 && SelectedBorder < EquipmentControlGrid.Children.Count && EquipmentControlGrid.Children[SelectedBorder] is Border border)
        {
            border.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseUpEvent });
        }
    }
}
