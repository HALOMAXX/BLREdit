using BLREdit.Export;
using BLREdit.Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
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
/// Interaction logic for KeyBindControl.xaml
/// </summary>
public sealed partial class KeyBindControl : UserControl
{
    static readonly PropertyInfo primaryInfo = typeof(BLRKeyBind).GetProperty("Primary");
    static readonly PropertyInfo alternateInfo = typeof(BLRKeyBind).GetProperty("Alternate");

    BLREButtonInfo? lastButton;
    BLREButtonInfo? LastButton { get { return lastButton; } set { lastButton?.Reset(); lastButton = value; } }

    public KeyBindControl()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var buttonContext = button.GetBindingExpression(Button.ContentProperty);
            if (buttonContext.ResolvedSourcePropertyName == "Primary")
            {
                LastButton = new(primaryInfo, buttonContext.DataItem, button);
            }else
            {
                LastButton = new(alternateInfo, buttonContext.DataItem, button);
            }
        }
    }

    private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (LastButton == null) return;
        e.Handled = true;
        ApplyKeyBind(e.Key.ToString());
    }



    private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (LastButton == null) return;
        e.Handled = true;
        ApplyKeyBind(e.ChangedButton.ToString()+"MouseButton");
    }

    private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (LastButton == null) return;
        
        e.Handled = true;
        if (e.Delta > 0)
        {
            ApplyKeyBind("MouseScrollUp");
        }
        else if (e.Delta < 0)
        {
            ApplyKeyBind("MouseScrollDown");
        }
    }

    private void ApplyKeyBind(string key)
    {
        if (LastButton == null) return;
        
        switch (key)
        {
            case "D0":
                key = "Zero";
                break;
            case "D1":
                key = "One";
                break;
            case "D2":
                key = "Two";
                break;
            case "D3":
                key = "Three";
                break;
            case "D4":
                key = "Four";
                break;
            case "D5":
                key = "Five";
                break;
            case "D6":
                key = "Six";
                break;
            case "D7":
                key = "Seven";
                break;
            case "D8":
                key = "Eight";
                break;
            case "D9":
                key = "Nine";
                break;

            case "NumPad0":
                key = "NumPadZero";
                break;
            case "NumPad1":
                key = "NumPadOne";
                break;
            case "NumPad2":
                key = "NumPadTwo";
                break;
            case "NumPad3":
                key = "NumPadThree";
                break;
            case "NumPad4":
                key = "NumPadFour";
                break;
            case "NumPad5":
                key = "NumPadFive";
                break;
            case "NumPad6":
                key = "NumPadSix";
                break;
            case "NumPad7":
                key = "NumPadSeven";
                break;
            case "NumPad8":
                key = "NumPadEight";
                break;
            case "NumPad9":
                key = "NumPadNine";
                break;

            case "XButton1MouseButton":
                key = "ThumbMouseButton1";
                break;
            case "XButton2MouseButton":
                key = "ThumbMouseButton2";
                break;

            case "LeftCtrl":
                key = "LeftControl";
                break;
            case "RightCtrl":
                key = "RightControl";
                break;


            case "OemQuotes":
                key = "Quote";
                break;
            case "Back":
                key = "BackSpace";
                break;
            case "Space":
                key = "SpaceBar";
                break;
        }

        LastButton.SetValue(key);
        LastButton = null;
    }
}

public sealed class BLREButtonInfo
{
    private readonly PropertyInfo PropertyInfo;
    private readonly object DataItem;
    private readonly Button Button;

    public BLREButtonInfo(PropertyInfo info, object dataItem, Button button)
    { 
        PropertyInfo= info;
        DataItem= dataItem;
        Button = button;

        Button.IsEnabled = false;
    }

    public void SetValue(string value)
    {
        PropertyInfo.SetValue(DataItem, value);
    }

    public void Reset()
    { 
        Button.IsEnabled = true;
    }
}