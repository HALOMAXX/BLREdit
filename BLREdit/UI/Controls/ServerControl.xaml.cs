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

namespace BLREdit.UI.Controls
{
    /// <summary>
    /// Interaction logic for ServerControl.xaml
    /// </summary>
    public partial class ServerControl : UserControl
    {
        public ServerControl()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (this.DataContext is BLRServerModel server)
                {
                    server.ConnectToServerCommand.Execute(null);
                }
            }
        }
    }
}
