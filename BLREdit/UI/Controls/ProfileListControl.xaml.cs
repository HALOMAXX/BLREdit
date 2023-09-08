using BLREdit.API.Export;
using BLREdit.Export;
using BLREdit.Game;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
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
    /// Interaction logic for ProfileListControl.xaml
    /// </summary>
    public partial class ProfileListControl : UserControl
    {
        public ProfileListControl()
        {
            InitializeComponent();
        }

        Point StartPoint;
        bool isDragging = false;

        private void ProfileListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void ProfileListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                StartPoint = e.GetPosition(null);
            }
        }

        private void ProfileListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(position.Y - StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (sender is ListView listView && listView.SelectedItem is ShareableProfile profile)
                    {
                        isDragging = true;
                        DragDrop.DoDragDrop(listView, profile, DragDropEffects.Move);
                        isDragging = false;
                    }
                }
            }
        }

        private void ProfileListView_Drop(object sender, DragEventArgs e)
        {
            ShareableProfile? droppedData = e.Data.GetData(typeof(ShareableProfile)) as ShareableProfile;
            object targetData = e.OriginalSource;
            while (targetData != null && targetData.GetType() != typeof(ProfileControl))
            {
                targetData = ((FrameworkElement)targetData).Parent;
            }
            if (targetData is not null && droppedData is not null && targetData is ProfileControl sControl && sControl.DataContext is ShareableProfile targetProfile) { ExportSystem.Profiles.Move(ExportSystem.Profiles.IndexOf(droppedData), ExportSystem.Profiles.IndexOf(targetProfile)); }
            else
            {
                LoggingSystem.Log("failed to reorder ProfileListView!");
            }
        }
    }
}
