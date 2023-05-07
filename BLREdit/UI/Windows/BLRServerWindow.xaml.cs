using BLREdit.Game;
using BLREdit.Model.BLR;

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
        public BLRServerWindow(BLRServerModel server)
        {
            this.DataContext = server;
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                switch (binding.ResolvedSourcePropertyName)
                {
                    case "Port":
                        ValidatePortInput(e, textBox, binding);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ValidatePortInput(KeyEventArgs e, TextBox box, BindingExpression binding)
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
            return e.Key switch
            {
                Key.D0 => 0,
                Key.D1 => 1,
                Key.D2 => 2,
                Key.D3 => 3,
                Key.D4 => 4,
                Key.D5 => 5,
                Key.D6 => 6,
                Key.D7 => 7,
                Key.D8 => 8,
                Key.D9 => 9,
                _ => -1,
            };
        }
        private static int NumPadKeys(System.Windows.Input.KeyEventArgs e)
        {
            return e.Key switch
            {
                Key.NumPad0 => 0,
                Key.NumPad1 => 1,
                Key.NumPad2 => 2,
                Key.NumPad3 => 3,
                Key.NumPad4 => 4,
                Key.NumPad5 => 5,
                Key.NumPad6 => 6,
                Key.NumPad7 => 7,
                Key.NumPad8 => 8,
                Key.NumPad9 => 9,
                _ => -1,
            };
        }

        private void RemoveServerButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
