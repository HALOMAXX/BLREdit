using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BLREdit.UI
{
    public class UIBool : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private bool isBool = false;

        public bool Is { get { return isBool; } private set { OnPropertyChanged(); } }
        public bool IsNot { get { return !isBool; } private set { OnPropertyChanged(); } }

        public Visibility Visibility { get { if (isBool) { return Visibility.Visible; } else { return Visibility.Collapsed; } } private set { OnPropertyChanged(); } }
        public Visibility VisibilityInverted { get { if (isBool) { return Visibility.Collapsed; } else { return Visibility.Visible; } } private set { OnPropertyChanged(); } }

        public void SetBool(bool target)
        {
            isBool = target;
            Is = target;
            IsNot = target;
            Visibility = Visibility.Visible;
            VisibilityInverted = Visibility.Collapsed;
        }
    }
}
