﻿using BLREdit.UI.Views;

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

namespace BLREdit.UI.Controls;

/// <summary>
/// Interaction logic for NumberUpDown.xaml
/// </summary>
public sealed partial class NumberUpDown : UserControl
{
    public int Number
    {
        get { return (int)GetValue(NumberProperty); }
        set { SetValue(NumberProperty, value); }
    }

    public int MinNumber
    {
        get { return (int)GetValue(MinNumberProperty); }
        set { SetValue(MinNumberProperty, value); }
    }

    public int MaxNumber
    {
        get { return (int)GetValue(MaxNumberProperty); }
        set { SetValue(MaxNumberProperty, value); }
    }

    public int NumberChange { get; set; } = 1;

    public bool NumberWrap { set; get; } = true;

    public Orientation Orientation
    {
        get { return (Orientation)GetValue(OrientationProperty); }
        set { SetValue(OrientationProperty, value); }
    }

    public static readonly DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0) { BindsTwoWayByDefault = true });
    public static readonly DependencyProperty MinNumberProperty = DependencyProperty.Register("MinNumber", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0) { PropertyChangedCallback = new PropertyChangedCallback(OnMinNumberChanged) });
    public static readonly DependencyProperty MaxNumberProperty = DependencyProperty.Register("MaxNumber", typeof(int), typeof(NumberUpDown), new FrameworkPropertyMetadata(0) { PropertyChangedCallback = new PropertyChangedCallback(OnMaxNumberChanged) });
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(NumberUpDown), new FrameworkPropertyMetadata(Orientation.Vertical) { PropertyChangedCallback = new PropertyChangedCallback(OnOrientationChanged) });

    public NumberUpDown()
    {
        InitializeComponent();
        ApplyOrientation();
    }

    public static void OnMinNumberChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) 
    {
        if (sender is NumberUpDown number)
        {
            number.ClampNumber();
        }
    }

    public static void OnMaxNumberChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) 
    {
        if (sender is NumberUpDown number)
        {
            number.ClampNumber();
        }
    }

    public static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is NumberUpDown number)
        {
            number.ApplyOrientation();
        }
    }

    public void ApplyOrientation()
    {
        if (Orientation == Orientation.Horizontal)
        {
            HorizontalGrid.Visibility = Visibility.Visible;
            VerticalGrid.Visibility = Visibility.Collapsed;
        }
        else
        {
            HorizontalGrid.Visibility = Visibility.Collapsed;
            VerticalGrid.Visibility = Visibility.Visible;
        }
    }

    public void ClampNumber()
    {
        Number = BLREditWeapon.Clamp(Number, MinNumber, MaxNumber);
    }

    private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var text = textBox.Text;
            text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            text = text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !int.TryParse(text, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out int _);
        }
    }

    private void NumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int result))
            {
                Number = result;
            }
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            int num = Number;
            int max = MaxNumber;
            int min = MinNumber;
            switch (tag)
            {
                case "Add":
                    num += NumberChange;
                    break;
                case "Sub":
                    num -= NumberChange;
                    break;
            }

            if (NumberWrap)
            {
                if (num < min)
                {
                    num = max;
                }
                else if (num > max)
                {
                    num = min;
                }
            }

            Number = BLREditWeapon.Clamp(num, min, max);
        }
        else
        {
            LoggingSystem.Log("NumberButton was wrong!!!");
        }
    }

    private void NumberTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ClampNumber();
    }
}
