using BLREdit.Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BLREdit.UI.Windows
{
    /// <summary>
    /// Interaction logic for BLRServerWindow.xaml
    /// </summary>
    public partial class BLRServerWindow : Window
    {
        public BLRServerWindow(BLRServer server)
        {
            this.DataContext = server;
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textBox = ((System.Windows.Controls.TextBox)sender);
            var binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            switch (binding.ResolvedSourcePropertyName)
            {
                case "Port":
                    ValidatePortInput(e, textBox, binding);
                    break;
                default:
                    break;
            }
        }

        private static void ValidatePortInput(System.Windows.Input.KeyEventArgs e, System.Windows.Controls.TextBox box, BindingExpression binding)
        {
            string selectedText = box.SelectedText;
            string text = box.Text.Remove(box.Text.IndexOf(selectedText), selectedText.Length);
            int number = NumberKeys(e);
            int numpad = NumPadKeys(e);

            long l;

            if (number >= 0)
            {
                l = long.Parse(text + number);
            }
            else if (numpad >= 0)
            {
                l = long.Parse(text + numpad);
            }
            else
            {
                if (!string.IsNullOrEmpty(text)) 
                { l = long.Parse(text); }
                else
                { l = 7777; }
            }

            if (l > ushort.MaxValue)
            {
                l = ushort.MaxValue;
            }
            else if (l < ushort.MinValue)
            {
                l = ushort.MinValue;
            }
            binding.ResolvedSource.GetType().GetProperty(binding.ResolvedSourcePropertyName).SetValue(binding.ResolvedSource, (ushort)l);
            e.Handled = !ArrowKeys(e) && !DelBackKeys(e);
        }

        private static bool ArrowKeys(System.Windows.Input.KeyEventArgs e)
        {
            return
                e.Key == Key.Left ||
                e.Key == Key.Right ||
                e.Key == Key.Up ||
                e.Key == Key.Down;
        }

        private static bool DelBackKeys(System.Windows.Input.KeyEventArgs e)
        {
            return
                e.Key == Key.Delete ||
                e.Key == Key.Back;
        }

        private static int NumberKeys(System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D0:
                    return 0;
                case Key.D1:
                    return 1;
                case Key.D2:
                    return 2;
                case Key.D3:
                    return 3;
                case Key.D4:
                    return 4;
                case Key.D5:
                    return 5;
                case Key.D6:
                    return 6;
                case Key.D7:
                    return 7;
                case Key.D8:
                    return 8;
                case Key.D9:
                    return 9;
                default:
                    return -1;
            }
        }
        private static int NumPadKeys(System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.NumPad0:
                    return 0;
                case Key.NumPad1:
                    return 1;
                case Key.NumPad2:
                    return 2;
                case Key.NumPad3:
                    return 3;
                case Key.NumPad4:
                    return 4;
                case Key.NumPad5:
                    return 5;
                case Key.NumPad6:
                    return 6;
                case Key.NumPad7:
                    return 7;
                case Key.NumPad8:
                    return 8;
                case Key.NumPad9:
                    return 9;
                default:
                    return -1;
            }
        }
    }
}
