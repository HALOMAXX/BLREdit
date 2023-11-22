using BLREdit.API.Utils;
using BLREdit.Export;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace BLREdit.UI.Controls
{
    public partial class ProfileListControl : UserControl
    {
        private Type? lastSelectedSortingType = null;
        public ProfileListControl()
        {
            InitializeComponent();
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view)
            {
                view.Filter += new Predicate<object>(ProfileFilter.Instance.FullFilter);
            }
            ProfileFilter.Instance.RefreshItemLists += ApplySearchAndFilter;
            BLRLoadoutStorage.ProfileGotRemoved += Refresh;
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

        private void SortComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySorting();
        }

        private void ChangeSortingDirection(object sender, RoutedEventArgs e)
        {
            if (MainWindow.MainView.ProfileListSortingDirection == ListSortDirection.Ascending)
            {
                MainWindow.MainView.ProfileListSortingDirection = ListSortDirection.Descending;
                SortDirectionButton.Content = Properties.Resources.btn_Descending;
            }
            else
            {
                MainWindow.MainView.ProfileListSortingDirection = ListSortDirection.Ascending;
                SortDirectionButton.Content = Properties.Resources.btn_Ascending;
            }
            ApplySorting();
        }

        public void Refresh(object? sender = null, EventArgs? e = null)
        {
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view)
            { view.Refresh(); }
        }

        public void ApplySorting()
        {
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view)
            {
                view.SortDescriptions.Clear();

                if (SortComboBox1.Items.Count > 0 && SortComboBox1.SelectedItem != null)
                {
                    MainWindow.MainView.CurrentProfileSortingPropertyName = $"Shareable.{Enum.GetName(MainWindow.MainView.CurrentProfileSortingEnumType, Enum.GetValues(MainWindow.MainView.CurrentProfileSortingEnumType).GetValue(SortComboBox1.SelectedIndex))}";
                    var sortDir = MainWindow.MainView.ProfileListSortingDirection;
                    switch (MainWindow.MainView.CurrentProfileSortingPropertyName)
                    {
                        case "Shareable.LastApplied":
                        case "Shareable.LastModified":
                        case "Shareable.LastViewed":
                            break;
                        default:
                            sortDir = sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                            break;
                    }
                    view.SortDescriptions.Add(new SortDescription(MainWindow.MainView.CurrentProfileSortingPropertyName, sortDir));
                }
            }
        }

        private void SetSortingType(Type SortingEnumType)
        {
            if (lastSelectedSortingType != SortingEnumType)
            {
                lastSelectedSortingType = SortingEnumType;
                int index = SortComboBox1.SelectedIndex;

                MainWindow.MainView.CurrentProfileSortingEnumType = SortingEnumType;
                SortComboBox1.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = LanguageResources.GetWordsOfEnum(SortingEnumType) });

                if (index > SortComboBox1.Items.Count)
                {
                    index = SortComboBox1.Items.Count - 1;
                }
                if (index < 0)
                {
                    index = 0;
                }
                SortComboBox1.SelectedIndex = index;
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetSortingType(typeof(ProfileSortingType));
            ApplySorting();
        }
    }
}
