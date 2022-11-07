using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for LoadoutControl.xaml
    /// </summary>
    public partial class LoadoutControl : UserControl
    {
        #region Event
        public static event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Event

        public static int SelectedIndex { get; private set; } = 0;
        public LoadoutControl()
        {
            InitializeComponent();
            LoadoutControl.PropertyChanged += GlobalTabIndexChanged;
            SecondaryControl.SetGripVisibility(Visibility.Visible);
            SecondaryControl.SetTagVisibility(Visibility.Collapsed);
        }

        ~LoadoutControl()
        {
            LoadoutControl.PropertyChanged -= GlobalTabIndexChanged;
        }

        public void GlobalTabIndexChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(SelectedIndex))
            { this.LoadoutTabControl.SelectedIndex = SelectedIndex; }
        }

        private void LoadoutTabControl_Selected(object sender, RoutedEventArgs e)
        {
            SelectedIndex = this.LoadoutTabControl.SelectedIndex;
            OnPropertyChanged(nameof(SelectedIndex));
        }
    }
}
