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

namespace BLREdit.UI.Controls;

/// <summary>
/// Interaction logic for WeaponViewControl.xaml
/// </summary>
public partial class WeaponViewControl : UserControl
{
    public WeaponViewControl()
    {
        InitializeComponent();
        UIKeys.Keys[Key.LeftShift].PropertyChanged += SkinModifierChanged;
    }

    public void SkinModifierChanged(object sender, PropertyChangedEventArgs args)
    {
        if (UIKeys.Keys[Key.LeftShift].Is)
        {
            this.StatPercentageGrid.Visibility = Visibility.Visible;
        }
        else
        {
            this.StatPercentageGrid.Visibility = Visibility.Collapsed;
        }
    }
}
