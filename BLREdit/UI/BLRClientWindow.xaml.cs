using BLREdit.Game;

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
using System.Windows.Shapes;

namespace BLREdit.UI;

/// <summary>
/// Interaction logic for BLRClientWindow.xaml
/// </summary>
public sealed partial class BLRClientWindow : Window, INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    private bool ShouldCancelClose = true;

    public BLRClient Client { get { return (BLRClient)DataContext; } set { DataContext = value; OnPropertyChanged(); } }

    public BLRClientWindow()
    {
        InitializeComponent();
    }

    public void ForceClose()
    {
        ShouldCancelClose = false;
        this.Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        e.Cancel = ShouldCancelClose;
        this.Hide();
    }
}
