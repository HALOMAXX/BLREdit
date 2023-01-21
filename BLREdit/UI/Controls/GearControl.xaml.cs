using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
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

namespace BLREdit.UI.Controls
{
    /// <summary>
    /// Interaction logic for GearControl.xaml
    /// </summary>
    public partial class GearControl : UserControl
    {
        public GearControl()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRLoadout loadout && loadout.IsFemale)
            {
                GenderButton.Content = BLREdit.Properties.Resources.btn_GenderToggle_Female;
            }
            else
            {
                GenderButton.Content = BLREdit.Properties.Resources.btn_GenderToggle_Male;
            }
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
                MainWindow.LastSelectedBorder = border;
            }
        }
    }
}
