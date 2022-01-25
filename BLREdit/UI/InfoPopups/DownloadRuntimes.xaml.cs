using System.Windows;
using System.Windows.Documents;

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
