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
using System.Windows.Shapes;

namespace BLREdit.InfoPopups
{
    /// <summary>
    /// Interaction logic for DownloadRuntimes.xaml
    /// </summary>
    public partial class DownloadRuntimes : Window
    {
        public DownloadRuntimes()
        {
            InitializeComponent();
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                System.Diagnostics.Process.Start(link.NavigateUri.ToString());
            }
        }
    }
}
