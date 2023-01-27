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
    /// Interaction logic for ExtraControl.xaml
    /// </summary>
    public partial class ExtraControl : UserControl
    {
        public ExtraControl()
        {
            InitializeComponent();
        }

        public static int SelectedBorder { get; private set; } = 1;
        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Image image && image.Parent is Border border)
            {
                SelectedBorder = this.ControlGrid.Children.IndexOf(border);
            }
        }

        internal void ApplyBorder()
        {
            if (SelectedBorder > -1 && SelectedBorder < this.ControlGrid.Children.Count && this.ControlGrid.Children[SelectedBorder] is Border border)
            {
                border.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseUpEvent });
            }
        }
    }
}
