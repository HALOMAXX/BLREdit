using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for NumberUpDown.xaml
    /// </summary>
    public partial class NumberUpDown : UserControl
    {


        public int Number
        {
            get { return (int)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }


        private int minNumber = 0;
        public int MinNumber
        {
            get { return (int)GetValue(MinNumberProperty); }
            set { SetValue(MinNumberProperty, value); }
        }

        private int maxNumber = 0;
        public int MaxNumber
        {
            get { return (int)GetValue(MaxNumberProperty); }
            set { SetValue(MaxNumberProperty, value); }
        }

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0) { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty MinNumberProperty =
    DependencyProperty.Register("MinNumber", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0) { PropertyChangedCallback = new PropertyChangedCallback(OnMinNumberChanged) });
        public static readonly DependencyProperty MaxNumberProperty =
    DependencyProperty.Register("MaxNumber", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0) { PropertyChangedCallback = new PropertyChangedCallback(OnMaxNumberChanged) });



        public NumberUpDown()
        {
            InitializeComponent();
        }

        public static void OnMinNumberChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) 
        {
            var control = ((NumberUpDown)sender);
            control.minNumber = (int)args.NewValue;
            control.ClampNumber();
        }

        public static void OnMaxNumberChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) 
        {
            var control = ((NumberUpDown)sender);
            control.maxNumber = (int)args.NewValue;
            control.ClampNumber();
        }

        public void ClampNumber()
        {
            Number = BLRWeapon.Clamp(Number, MinNumber, MaxNumber);
        }

        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = ((TextBox)sender);
            var text = textBox.Text;
            text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            text = text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !int.TryParse(text, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out int count);

            if (count > maxNumber)
            {
                e.Handled = true;
            }
            if (count < minNumber)
            {
                e.Handled = true;
            }
        }

        private void NumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Number = int.Parse(((TextBox)sender).Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int num = Number;
            if (button.Name == "Add")
            { num++; }
            else
            { num--; }

            Number = BLRWeapon.Clamp(num, minNumber, maxNumber);
        }
    }
}
