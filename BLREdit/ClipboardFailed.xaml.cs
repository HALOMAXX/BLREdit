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

namespace BLREdit
{
    /// <summary>
    /// Interaction logic for ClipboardFailed.xaml
    /// </summary>
    public partial class ClipboardFailed : Window
    {
        public ClipboardFailed(string ExportString)
        {
            InitializeComponent();
            ExportBox.IsReadOnlyCaretVisible = true;
            ExportBox.IsReadOnly = true;
            ExportBox.Text = ExportString;
            ExportBox.SelectAll();
        }
    }
}
