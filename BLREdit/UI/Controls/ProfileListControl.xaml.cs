using BLREdit.API.Utils;
using BLREdit.Export;
using BLREdit.Import;
using BLREdit.UI.Views;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using static BLREdit.API.Utils.HelperFunctions;

namespace BLREdit.UI.Controls
{
    public partial class ProfileListControl : UserControl
    {
        private Type? lastSelectedSortingType;
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
            ApplySortingProfileList();
        }

        private void SortComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySortingProfileList();
        }

        private void ChangeSortingDirection(object sender, RoutedEventArgs e)
        {
            if (MainWindow.MainView.ProfileListSortingDirection == ListSortDirection.Ascending)
            {
                MainWindow.MainView.ProfileListSortingDirection = ListSortDirection.Descending;
            }
            else
            {
                MainWindow.MainView.ProfileListSortingDirection = ListSortDirection.Ascending;
            }
            ApplySortingProfileList();
        }

        public void Refresh(object? sender = null, EventArgs? e = null)
        {
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view)
            { view.Refresh(); }
        }

        public void ApplySortingProfileList()
        {
            if (CollectionViewSource.GetDefaultView(ProfileListView.ItemsSource) is CollectionView view)
            {
                view.SortDescriptions.Clear();

                if (SortComboBox1.Items.Count > 0 && SortComboBox1.SelectedItem != null)
                {
                    MainWindow.MainView.CurrentProfileSortingPropertyName = $"Shareable.{Enum.GetName(MainWindow.MainView.CurrentProfileSortingEnumType, Enum.GetValues(MainWindow.MainView.CurrentProfileSortingEnumType).GetValue(SortComboBox1.SelectedIndex))}";
                    switch (MainWindow.MainView.CurrentProfileSortingPropertyName)
                    {
                        case "Shareable.Name":
                            view.SortDescriptions.Add(new SortDescription(MainWindow.MainView.CurrentProfileSortingPropertyName, MainWindow.MainView.ProfileListSortingDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                            break;
                        default:
                            view.SortDescriptions.Add(new SortDescription(MainWindow.MainView.CurrentProfileSortingPropertyName, MainWindow.MainView.ProfileListSortingDirection));
                            break;
                    }
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
            ApplySortingProfileList();
        }

        private void EnableAlloadouts_Click(object sender, RoutedEventArgs e)
        {
            if (DataStorage.Loadouts is null || DataStorage.Loadouts.Count <= 0) { return; }
            int appliedCount = 0;
            foreach (var loadout in DataStorage.Loadouts)
            {
                string message = string.Empty;
                if (loadout.BLR.ValidateLoadout(ref message))
                {
                    loadout.BLR.Apply = true;
                    appliedCount++;
                }
            }
            LoggingSystem.MessageLog($"Enabled: {appliedCount}/{DataStorage.Loadouts.Count}", "Info");
        }

        private void DisableAlloadouts_Click(object sender, RoutedEventArgs e)
        {
            if (DataStorage.Loadouts is null || DataStorage.Loadouts.Count <= 0) { return; }
            int appliedCount = 0;
            foreach (var loadout in DataStorage.Loadouts)
            {
                string message = string.Empty;
                if (loadout.BLR.Apply)
                {
                    loadout.BLR.Apply = false;
                    appliedCount++;
                }
            }
            LoggingSystem.MessageLog($"Disabled: {appliedCount}/{DataStorage.Loadouts.Count}", "Info");
        }

        private void RepairLoadouts_Click(object sender, RoutedEventArgs e)
        {
            if (!LoggingSystem.MessageLog("Are you sure you want to Repair all Loadouts?\nthis might be a destructive operation you can still revert some changes with the Undo/Redo feature!", "Warning", MessageBoxButton.YesNo)) return;

            int primariesRepaired = 0;
            int secondariesRepaired = 0;
            int gear4Dup = 0, gear3Dup = 0, gear2Dup = 0, gear1Dup = 0;
            int missingDep1 = 0, missingDep2 = 0, missingDep3 = 0, missingDep4 = 0, missingDep5 = 0;
            int missingTaunt1 = 0, missingTaunt2 = 0, missingTaunt3 = 0, missingTaunt4 = 0, missingTaunt5 = 0, missingTaunt6 = 0, missingTaunt7 = 0, missingTaunt8 = 0;
            int topIcon = 0, topColor = 0, middleIcon = 0, middleColor = 0, bottomIcon = 0, bottomColor = 0;
            int announcer = 0, player = 0, title = 0;

            int helmet = 0, upper = 0, lower = 0;
            int tactical = 0, trophy = 0, avatar = 0, camo = 0;

            if (DataStorage.Loadouts is null || DataStorage.Loadouts.Count <= 0) { return; }
            foreach (var loadout in DataStorage.Loadouts)
            {
                if (loadout.BLR is BLREditLoadout l && l.LoadoutReport is LoadoutErrorReport report)
                {
                    if (!report.PrimaryReport.IsValid)
                    { 
                        l.Primary.RemoveIncompatibleAttachments();
                        l.Primary.AddMissingDefaultAttachments();
                        primariesRepaired++;
                    }

                    if (!report.SecondaryReport.IsValid)
                    {
                        l.Secondary.RemoveIncompatibleAttachments();
                        l.Secondary.AddMissingDefaultAttachments();
                        secondariesRepaired++;
                    }

                    if (!report.GearReport.IsValid || report.GearReport.HasDuplicates)
                    {
                        if (HasAnyFlags(report.GearReport.Gear4Report, ItemReport.Invalid, ItemReport.Duplicate))
                        {
                            loadout.BLR.Gear4 = null;
                            gear4Dup++;
                        }
                        if (HasAnyFlags(report.GearReport.Gear3Report, ItemReport.Invalid, ItemReport.Duplicate))
                        {
                            loadout.BLR.Gear3 = null;
                            gear3Dup++;
                        }
                        if (HasAnyFlags(report.GearReport.Gear2Report, ItemReport.Invalid, ItemReport.Duplicate))
                        {
                            loadout.BLR.Gear2 = null;
                            gear2Dup++;
                        }
                        if (report.GearReport.Gear1Report.HasFlag(ItemReport.Invalid))
                        {
                            loadout.BLR.Gear1 = null;
                            gear1Dup++;
                        }
                    }

                    if (report.ExtraReport.HasMissingItems)
                    {
                        #region DepotCheck
                        if (HasAnyFlags(report.ExtraReport.Depot1Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 0);
                            missingDep1++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Depot2Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 1);
                            missingDep2++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Depot3Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 2);
                            missingDep3++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Depot4Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 3);
                            missingDep4++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Depot5Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 4);
                            missingDep5++;
                        }
                        #endregion DepotCheck

                        #region TauntCheck
                        if (HasAnyFlags(report.ExtraReport.Taunt1Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 0);
                            missingTaunt1++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Taunt2Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 1);
                            missingTaunt2++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Taunt3Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 2);
                            missingTaunt3++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Taunt4Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 3);
                            missingTaunt4++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Taunt5Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 4);
                            missingTaunt5++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Taunt6Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 5);
                            missingTaunt6++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Taunt7Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 6);
                            missingTaunt7++;
                        }
                        if (HasAnyFlags(report.ExtraReport.Taunt8Report, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 7);
                            missingTaunt8++;
                        }
                        #endregion TauntCheck

                        #region EmblemCheck
                        if (HasAnyFlags(report.ExtraReport.TopIconReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.EmblemIcon = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_ICON_CATEGORY, 17);
                            topIcon++;
                        }
                        if (HasAnyFlags(report.ExtraReport.TopColorReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.EmblemIconColor = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, 2);
                            topColor++;
                        }
                        if (HasAnyFlags(report.ExtraReport.MiddleIconReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.EmblemShape = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_SHAPE_CATEGORY, 0);
                            middleIcon++;
                        }
                        if (HasAnyFlags(report.ExtraReport.MiddleColorReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.EmblemShapeColor = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, 6);
                            middleColor++;
                        }
                        if (HasAnyFlags(report.ExtraReport.BottomIconReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.EmblemBackground = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_BACKGROUND_CATEGORY, 0);
                            bottomIcon++;
                        }
                        if (HasAnyFlags(report.ExtraReport.BottomColorReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.EmblemBackgroundColor = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, 6);
                            bottomColor++;
                        }
                        #endregion EmblemCheck

                        #region ExtraETC
                        if (HasAnyFlags(report.ExtraReport.AnnouncerReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.AnnouncerVoice = ImportSystem.GetItemByIDAndType(ImportSystem.ANNOUNCER_VOICE_CATEGORY, 0);
                            announcer++;
                        }
                        if (HasAnyFlags(report.ExtraReport.PlayerReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.PlayerVoice = ImportSystem.GetItemByIDAndType(ImportSystem.PLAYER_VOICE_CATEGORY, 0);
                            player++;
                        }
                        if (HasAnyFlags(report.ExtraReport.TitleReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Title = ImportSystem.GetItemByIDAndType(ImportSystem.TITLES_CATEGORY, 0);
                            title++;
                        }
                        #endregion ExtraETC

                        #region Armor
                        if (HasAnyFlags(report.GearReport.HelmetReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.Helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, 0);
                            helmet++;
                        }
                        if (HasAnyFlags(report.GearReport.UpperBodyReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.UpperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, 0);
                            upper++;
                        }
                        if (HasAnyFlags(report.GearReport.LowerBodyReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.LowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, 0);
                            lower++;
                        }

                        if (HasAnyFlags(report.GearReport.TacticalReport, ItemReport.Invalid))
                        {
                            loadout.BLR.Tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, 0);
                            tactical++;
                        }
                        if (HasAnyFlags(report.GearReport.AvatarReport, ItemReport.Invalid))
                        {
                            loadout.BLR.Avatar = null;
                            avatar++;
                        }
                        if (HasAnyFlags(report.GearReport.BodyCamoReport, ItemReport.Invalid, ItemReport.Missing))
                        {
                            loadout.BLR.BodyCamo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, 0);
                            camo++;
                        }
                        if (HasAnyFlags(report.GearReport.TrophyReport, ItemReport.Invalid))
                        {
                            loadout.BLR.Trophy = null;
                            trophy++;
                        }
                        #endregion Armor
                        loadout.BLR.IsAdvanced.Set(false);
                    }
                }
            }

            var count = DataStorage.Loadouts.Count;

            LoggingSystem.MessageLog(
                $"Primaries Repaired: {primariesRepaired}/{count}\n" +
                $"Secondaries Repaired: {secondariesRepaired}/{count}\n" +
                $"Gear4 Duplicates: {gear4Dup}/{count}\n" +
                $"Gear3 Duplicates: {gear3Dup}/{count}\n" +
                $"Gear2 Duplicates: {gear2Dup}/{count}\n" +
                $"Gear1 Duplicates: {gear1Dup}/{count}\n" +
                $"Armor:\n" +
                    $"\tHelmet:{helmet}/{count} | Upper:{upper}/{count} | Lower:{lower}/{count}\n" +
                    $"\tTactical:{tactical}/{count} | Trophy:{trophy}/{count}\n" +
                    $"\tCamo:{camo}/{count} | Avatar:{avatar}/{count}\n" +
                $"Depot N/A:\n" +
                    $"\t1:{missingDep1}/{count} | 2:{missingDep2}/{count} | 3:{missingDep3}/{count} | 4:{missingDep4}/{count} | 5:{missingDep5}/{count}\n" +
                $"Taunt N/A:\n" +
                    $"\t1:{missingTaunt1}/{count} | 2:{missingTaunt2}/{count} | 3:{missingTaunt3}/{count} | 4:{missingTaunt4}/{count}\n" +
                    $"\t5:{missingTaunt5}/{count} | 6:{missingTaunt6}/{count} | 7:{missingTaunt7}/{count} | 8:{missingTaunt8}/{count}\n" +
                $"Emblem:\n" +
                    $"\tTop:{topIcon}/{count} | TopColor:{topColor}/{count}\n" +
                    $"\tMiddle:{middleIcon}/{count} | MiddleColor: {middleColor}/{count}\n" +
                    $"\tBottom:{bottomIcon}/{count} | BottomColor:{bottomColor}/{count}\n" +
                $"Announcer:{announcer}/{count} | Player:{player}/{count} | Title:{title}/{count}", "Info");
        }
    }
}
