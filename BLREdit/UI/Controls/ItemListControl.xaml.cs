using BLREdit.Import;

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

namespace BLREdit.UI.Controls
{
    /// <summary>
    /// Interaction logic for ItemListControl.xaml
    /// </summary>
    public partial class ItemListControl : UserControl
    {
        public ItemListControl()
        {
            InitializeComponent();
        }

        private void ItemList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is BLREditItem item)
            {
                if (e.ClickCount >= 2)
                {
                    LoggingSystem.Log($"Double Clicking:{item.Name}");
                    MainWindow.SetItemToBorder(MainWindow.MainView.LastSelectedItemBorder, item);
                }
                else
                {
                    LoggingSystem.Log($"Dragging:{item.Name}");
                    DragDrop.DoDragDrop(element, item, DragDropEffects.Copy);
                }
            }
        }
    }
}
