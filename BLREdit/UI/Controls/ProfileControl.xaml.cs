using BLREdit.API.Export;
using BLREdit.UI.Views;

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
    /// Interaction logic for ProfileControl.xaml
    /// </summary>
    public partial class ProfileControl : UserControl, INotifyPropertyChanged
    {
        #region Event
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Event

        private BLRProfile? _profile;
        BLRProfile? Profile { get { return _profile; } set { _profile = value; OnPropertyChanged(); } }

        public ProfileControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ShareableProfile profile)
            { 
                Profile = profile.ToBLRProfile();
                Profile.CalculateStats();
                BLRProfileGrid.DataContext = Profile;
            }
        }
    }
}
