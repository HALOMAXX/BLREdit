using BLREdit.UI.Views;

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
            if (((BLRLoadout)this.DataContext).IsFemale)
            {
                GenderButton.Content = BLREdit.Properties.Resources.btn_GenderToggle_Female;
            }
            else
            {
                GenderButton.Content = BLREdit.Properties.Resources.btn_GenderToggle_Male;
            }
        }
    }
}
