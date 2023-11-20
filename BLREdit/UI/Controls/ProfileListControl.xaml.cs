using BLREdit.Export;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace BLREdit.UI.Controls
{
    public partial class ProfileListControl : UserControl
    {
        public ProfileListControl()
        {
            InitializeComponent();
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view)
            {
                view.Filter += new Predicate<object>(ProfileFilter.Instance.FullFilter);
            }
            ProfileFilter.Instance.RefreshItemLists += ApplySearchAndFilter;
        }

        Point StartPoint;
        bool isDragging = false;

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
                    if (sender is ListView listView && listView.SelectedItem is BLRLoadoutStorage profile)
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
            BLRLoadoutStorage? droppedData = e.Data.GetData(typeof(BLRLoadoutStorage)) as BLRLoadoutStorage;

            var pos = e.GetPosition(MainWindow.Instance);

            var hitResult = MainWindow.Instance.HitTestProfileControls(ProfileListView, pos);

            if (droppedData is not null && hitResult is ProfileControl sControl && sControl.DataContext is BLRLoadoutStorage targetProfile)
            {
                var droppedIndex = DataStorage.Loadouts.IndexOf(droppedData);
                var targetIndex = DataStorage.Loadouts.IndexOf(targetProfile);

                if (droppedIndex < 0 || targetIndex < 0)
                { return; }

                BLRLoadoutStorage.Exchange(droppedIndex, targetIndex);
                if (droppedIndex == DataStorage.Settings.CurrentlyAppliedLoadout)
                {
                    DataStorage.Settings.CurrentlyAppliedLoadout = targetIndex;
                }
                else if (targetIndex == DataStorage.Settings.CurrentlyAppliedLoadout)
                {
                    DataStorage.Settings.CurrentlyAppliedLoadout = droppedIndex;
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ProfileFilter.Instance.SearchFilter = SearchBox.Text;
        }

        public void ApplySearchAndFilter(object sender, EventArgs e)
        {
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view) { view.Filter ??= new Predicate<object>(ProfileFilter.Instance.FullFilter); view.Refresh(); }
        }

        private void ProfileListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view)
            {
                view.Filter += new Predicate<object>(ProfileFilter.Instance.FullFilter);
            }
        }
    }
}
