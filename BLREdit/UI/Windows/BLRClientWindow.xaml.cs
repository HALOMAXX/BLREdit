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
using System.Windows.Shapes;

namespace BLREdit.UI.Windows;

/// <summary>
/// Interaction logic for BLRClientWindow.xaml
/// </summary>
public sealed partial class BLRClientWindow : Window
{
    public BLRClientWindow(BLRClientModel client)
    {
        this.DataContext = new BLRClientModifyView(client);
        InitializeComponent();
    }
}