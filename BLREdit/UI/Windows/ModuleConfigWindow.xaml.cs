using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.UI.Controls;
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
using System.Windows.Shapes;

namespace BLREdit.UI.Windows
{
    /// <summary>
    /// Interaction logic for ModuleConfigWindow.xaml
    /// </summary>
    public partial class ModuleConfigWindow : Window
    {
        public ModuleConfigWindow(ModuleConfigView config)
        {
            this.DataContext = config;
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ModuleConfigView view)
            {
                var children = MainWindow.FindVisualChildren<ModuleSettingControl>(this);
                foreach (var settingControl in children)
                {
                    if (settingControl.DataContext is ProxyModuleSetting setting &&  setting.SettingType == ModuleSettingType.String)
                    {
                        settingControl.UpdateTextBox();
                    }
                }
                view.Client.SaveModuleSettings(view.Module);
            }
        }
    }
}
