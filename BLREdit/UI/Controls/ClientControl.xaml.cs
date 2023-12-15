using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ClientControl.xaml
    /// </summary>
    public partial class ClientControl : UserControl
    {
        public ClientControl()
        {
            InitializeComponent();
            UIKeys.Keys[Key.LeftShift].PropertyChanged += ShiftKeyModifier;
        }

        public void ShiftKeyModifier(object sender, PropertyChangedEventArgs args)
        {
            if (UIKeys.Keys[Key.LeftShift].Is)
            {
                this.BotMachButton.Visibility = Visibility.Collapsed;
                this.TrainingButton.Visibility = Visibility.Visible;
                this.StartServerButton.Visibility = Visibility.Collapsed;
                this.SafeMatchButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.BotMachButton.Visibility = Visibility.Visible;
                this.TrainingButton.Visibility = Visibility.Collapsed;
                this.StartServerButton.Visibility = Visibility.Visible;
                this.SafeMatchButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
