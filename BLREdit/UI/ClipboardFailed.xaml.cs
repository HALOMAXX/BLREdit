using System.Windows;

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
