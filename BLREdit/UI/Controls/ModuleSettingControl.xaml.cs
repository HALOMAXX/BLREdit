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
    /// Interaction logic for ModuleSettingControl.xaml
    /// </summary>
    public partial class ModuleSettingControl : UserControl
    {
        public ModuleSettingControl()
        {
            InitializeComponent();
        }

        public void UpdateTextBox()
        {
            SettingTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
