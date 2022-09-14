using BLREdit.Game;

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

namespace BLREdit.UI
{
    /// <summary>
    /// Interaction logic for BLRClientWindow.xaml
    /// </summary>
    public partial class BLRClientWindow : Window
    {
        public static BLRClient Client { get; private set; }

        public BLRClientWindow(object dataContext)
        {
            Client = (BLRClient)dataContext;
            this.DataContext = dataContext;
            InitializeComponent();
        }
    }
}
