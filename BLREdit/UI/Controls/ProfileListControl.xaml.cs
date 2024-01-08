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
            SetSortingType(typeof(ProfileSortingType));
            ApplySorting();
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
                    view.SortDescriptions.Add(new SortDescription(MainWindow.MainView.CurrentProfileSortingPropertyName, MainWindow.MainView.ProfileListSortingDirection));
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

        private void ToggleLoadouts_Click(object sender, RoutedEventArgs e)
        {
            if (DataStorage.Loadouts is not null && DataStorage.Loadouts.Count > 0)
                foreach (var loadout in DataStorage.Loadouts)
                {
                    string message = string.Empty;
                    if (loadout.BLR.ValidateLoadout(ref message))
                    {
                        loadout.BLR.Apply = true;
                    }
                }
        }
    }
}
