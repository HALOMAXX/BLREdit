using BLREdit.Export;

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
    public partial class BLRLoadoutStorageView : UserControl
    {
        public BLRLoadoutStorageView()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (sender is FrameworkElement element && element.DataContext is BLRLoadoutStorage loadoutStorage)
                {
                    MainWindow.MainView.Profile = loadoutStorage;
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is BLRLoadoutStorage loadoutStorage)
            {
                string message = "Can't enable loadout\n";
                if (!loadoutStorage.BLR.ValidateLoadout(ref message))
                {
                    LoggingSystem.MessageLog(message, "Info");
                    e.Handled = true;
                    loadoutStorage.BLR.Apply = false;
                }
                else if (UIKeys.Keys[Key.LeftShift].Is) {
                    var oldDefaultLoadout = DataStorage.Settings.DefaultLoadout;
                    DataStorage.Settings.DefaultLoadout = loadoutStorage;
                    oldDefaultLoadout?.TriggerChangeNotify();
                    loadoutStorage.TriggerChangeNotify();
                    e.Handled = true;
                    loadoutStorage.BLR.Apply = true;
                }
            }
        }
    }
}
