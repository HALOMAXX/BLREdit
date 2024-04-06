using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using BLREdit.Game.Proxy;
using BLREdit.UI.Views;
using BLREdit.Import;
using BLREdit.Export;
using BLREdit.UI.Controls;
using BLREdit.UI.Windows;
using BLREdit.API.InterProcess;
using BLREdit.API.Export;
using BLREdit.API.Utils;
using BLREdit.Game;
using System.IO.Ports;

namespace BLREdit.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public static readonly BLRClientWindow ClientWindow = new();
    static MainWindowView? mainWindowView = null;
    public static MainWindowView MainView { get { mainWindowView ??= new(); return mainWindowView; } }
    public static MainWindow? Instance { get; private set; } = null;

    private readonly string[] Args;

    private readonly static char[] InvalidNameChars = [.. Path.GetInvalidPathChars(), .. Path.GetInvalidFileNameChars()];

    public bool wasLastImageScopePreview = false;
    public bool wasLastSelectedBorderPrimary = true;
    private string lastSelectedType = "";
    static readonly Stopwatch PingWatch = Stopwatch.StartNew();
    static bool firstStart = true;
    private Type? lastSelectedSortingType = null;
    private int buttonIndex = 0;

    private static SolidColorBrush SolidColorBrush { get; } = new(Colors.Blue);
    private static ColorAnimation AlertAnim { get; } = new()
    {
        From = Color.FromArgb(32, 0, 0, 0),
        To = Color.FromArgb(255, 255, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2)),
        AutoReverse = true,
        RepeatBehavior = RepeatBehavior.Forever
    };

    private static ColorAnimation CalmAnim { get; } = new()
    {
        From = Color.FromArgb(255, 255, 0, 0),
        To = Color.FromArgb(32, 0, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2))
    };

    ColorAnimation? lastAnim;

    public bool BlockChangeNotif { get; set; } = false;

    public MainWindow(string[] args)
    {
        Args = args;
        MainView.IsPlayerProfileChanging = true;
        MainView.IsPlayerNameChanging = true;

        PreviewKeyDown += UIKeys.Instance.KeyDown;
        PreviewKeyUp += UIKeys.Instance.KeyUp;
        PreviewKeyUp += PreviewKeyUpMainWindow;

        InitializeComponent();

        MainView.IsPlayerProfileChanging = false;
        MainView.IsPlayerNameChanging = false;
    }

    public void ApplySearchAndFilter()
    {
        if (CollectionViewSource.GetDefaultView(ItemList.ItemsSource) is CollectionView view) view.Refresh();
    }

    public static void CheckGameClients()
    {
        if (DataStorage.GameClients.Count <= 0)
        {
            MessageBox.Show("You have to locate and add atleast one Client"); //TODO: Add Localization
        }
    }

    public static void ResetPauseForPing()
    {
        firstStart = true;
    }

    public static void RefreshPing(bool force = false)
    {
        if (force || firstStart || PingWatch.ElapsedMilliseconds > 30000)
        {
            firstStart = false;
            foreach (BLRServer server in DataStorage.ServerList)
            {
                server.PingServer();
            }
            PingWatch.Restart();
        }
    }

    private static void AddOrUpdateDefaultServers()
    {
        List<BLRServer> remove = [];
        foreach (var server in DataStorage.ServerList)
        {
            if (server.ID.Equals(string.Empty, StringComparison.Ordinal))
            { 
                remove.Add(server);
            }
        }

        foreach (var rem in remove)
        { 
            DataStorage.ServerList.Remove(rem);
        }

        foreach (BLRServer defaultServer in App.DefaultServers)
        {
            AddOrUpdateDefaultServer(defaultServer);
        }
    }

    public static void AddOrUpdateDefaultServer(BLRServer? server)
    {
        if (server is null) return;
        var index = IsInCollection(DataStorage.ServerList, server);
        if (index == -1) { DataStorage.ServerList.Add(server); return; }
        DataStorage.ServerList[index].ServerAddress = server.ServerAddress;
        DataStorage.ServerList[index].Port = server.Port;
        DataStorage.ServerList[index].InfoPort = server.InfoPort;
        DataStorage.ServerList[index].Hidden = server.Hidden;
        DataStorage.ServerList[index].Region = server.Region;
        DataStorage.ServerList[index].ValidatesLoadout = server.ValidatesLoadout;
    }

    public static void AddServer(BLRServer server, bool forceAdd = false)
    {
        if (forceAdd || IsInCollection(DataStorage.ServerList, server) == -1)
        {
            DataStorage.ServerList.Add(server);
        }
    }

    /// <summary>
    /// Returns first occurrence other wise returns -1 if it didn't contain item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static int IsInCollection<T>(ObservableCollection<T> collection, T item)
    {
        if (item is null) return -1;
        for (int i = 0; i < collection.Count; i++)
        {
            if (item.Equals(collection[i])) return i;
        }
        return -1;
    }

    public static void AddGameClient(BLRClient client)
    {
        if (IsInCollection(DataStorage.GameClients, client) == -1)
        {
            DataStorage.GameClients.Add(client);
        }
    }

    #region Events
    private void ChangeCurrentServer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is BLRServer server)
            {
                DataStorage.Settings.DefaultServer = server;
                foreach (BLRServer s in DataStorage.ServerList)
                {
                    s.IsDefaultServer = false;
                }
            }
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        ClientWindow.ForceClose();
        DataStorage.Save();
    }

    private void Border_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Serializable)) e.Effects = DragDropEffects.Copy;
        else e.Effects = DragDropEffects.None;
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(BLRItem)) is BLRItem item)
        {
            Border? border = null;
            if (e.OriginalSource is Image mage)
            {
                if (mage.Parent is Border order) border = order;
            }
            else if (e.OriginalSource is Border order)
            {
                border = order;
            }

            if (border is null) { return; }

            SetItemToBorder(border, item);
        }
    }

    private void Border_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Image? image = null;
        Border? border = null;
        if (e.OriginalSource is Image mage)
        {
            image = mage;
            if (mage.Parent is Border order) border = order;
        }
        else if (e.OriginalSource is Border order)
        {
            border = order;
            if (order.Child is Image mage2) image = mage2;
        }

        if (image is null || border is null) { return; }

        if (e.ChangedButton == MouseButton.Left)
        {
            if (wasLastImageScopePreview)
            {
                if (ItemList.ItemsSource is not null)
                {
                    foreach (var i in ItemList.ItemsSource)
                    {
                        if(i is BLRItem item) item.RemoveCrosshair();
                    }
                }
            }

            var parent = ((FrameworkElement)border.Parent);

            var weapon = parent?.DataContext as BLRWeapon;
            var loadout = parent?.DataContext as BLRLoadout;
            var profile = parent?.DataContext as BLRProfile;

            if (weapon is not null) { ItemFilters.Instance.WeaponFilter = weapon; }
            else
            {
                if (loadout is not null) ItemFilters.Instance.WeaponFilter = loadout.Primary;
                else if (profile is not null) ItemFilters.Instance.WeaponFilter = profile.Loadout1.Primary;
            }
            MainView.LastSelectedItemBorder = border;
            MainView.IsScopePreviewVisible.Set(false);
            switch (border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName)
            {
                case "Receiver":
                    if (weapon?.IsPrimary ?? true)
                    { SetItemList(ImportSystem.PRIMARY_CATEGORY); wasLastSelectedBorderPrimary = true; }
                    else
                    { SetItemList(ImportSystem.SECONDARY_CATEGORY); wasLastSelectedBorderPrimary = false; }
                    break;
                case "Muzzle":
                    SetItemList(ImportSystem.MUZZELS_CATEGORY);
                    break;
                case "Magazine":
                    SetItemList(ImportSystem.MAGAZINES_CATEGORY);
                    break;
                case "Barrel":
                    SetItemList(ImportSystem.BARRELS_CATEGORY);
                    break;
                case "Scope":
                    if (image.DataContext is not BLRWeapon)
                    {
                        SetItemList(ImportSystem.SCOPES_CATEGORY);
                    }
                    else
                    {
                        if (weapon?.Scope is not null)
                        {
                            weapon.Scope.LoadCrosshair(weapon);
                            ItemList.ItemsSource = new BLRItem[] { weapon.Scope };
                            MainView.IsScopePreviewVisible.Set(true);
                        }
                    }
                    break;
                case "Stock":
                    SetItemList(ImportSystem.STOCKS_CATEGORY);
                    break;
                case "Grip":
                    SetItemList(ImportSystem.GRIPS_CATEGORY);
                    break;
                case "Tag":
                    SetItemList(ImportSystem.HANGERS_CATEGORY);
                    break;
                case "Camo":
                    SetItemList(ImportSystem.CAMOS_WEAPONS_CATEGORY);
                    break;
                case "Ammo":
                    SetItemList(ImportSystem.AMMO_CATEGORY);
                    break;
                case "Skin":
                    SetItemList(ImportSystem.PRIMARY_SKIN_CATEGORY);
                    break;

                case "BodyCamo":
                    SetItemList(ImportSystem.CAMOS_BODIES_CATEGORY);
                    break;
                case "Helmet":
                    SetItemList(ImportSystem.HELMETS_CATEGORY);
                    break;
                case "UpperBody":
                    SetItemList(ImportSystem.UPPER_BODIES_CATEGORY);
                    break;
                case "LowerBody":
                    SetItemList(ImportSystem.LOWER_BODIES_CATEGORY);
                    break;
                case "Tactical":
                    SetItemList(ImportSystem.TACTICAL_CATEGORY);
                    break;
                case "Trophy":
                    SetItemList(ImportSystem.BADGES_CATEGORY);
                    break;
                case "Avatar":
                    SetItemList(ImportSystem.AVATARS_CATEGORY);
                    break;

                case "EmblemBackground":
                    SetItemList(ImportSystem.EMBLEM_BACKGROUND_CATEGORY);
                    break;
                case "EmblemIcon":
                    SetItemList(ImportSystem.EMBLEM_ICON_CATEGORY);
                    break;
                case "EmblemShape":
                    SetItemList(ImportSystem.EMBLEM_SHAPE_CATEGORY);
                    break;
                case "EmblemBackgroundColor":
                case "EmblemIconColor":
                case "EmblemShapeColor":
                    SetItemList(ImportSystem.EMBLEM_COLOR_CATEGORY);
                    break;

                case "AnnouncerVoice":
                    SetItemList(ImportSystem.ANNOUNCER_VOICE_CATEGORY);
                    break;
                case "PlayerVoice":
                    SetItemList(ImportSystem.PLAYER_VOICE_CATEGORY);
                    break;

                case "Title":
                    SetItemList(ImportSystem.TITLES_CATEGORY);
                    break;

                case "Gear1":
                case "Gear2":
                case "Gear3":
                case "Gear4":
                    SetItemList(ImportSystem.ATTACHMENTS_CATEGORY);
                    break;

                case "Taunt1":
                case "Taunt2":
                case "Taunt3":
                case "Taunt4":
                    SetItemList(ImportSystem.EMOTES_CATEGORY);
                    break;
                case "Depot1":
                case "Depot2":
                case "Depot3":
                case "Depot4":
                case "Depot5":
                    SetItemList(ImportSystem.SHOP_CATEGORY);
                    break;
            }
            return;
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            if (((FrameworkElement)border.Parent).DataContext is BLRWeapon weapon)
            {
                UndoRedoSystem.DoValueChange(null, weapon.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), weapon);
                UndoRedoSystem.EndUndoRecord();
            }
            else if (((FrameworkElement)border.Parent).DataContext is BLRLoadout loadout)
            {
                UndoRedoSystem.DoValueChange(null, loadout.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), loadout);
                UndoRedoSystem.EndUndoRecord();
            }
        }
    }

    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
//        if (MainView.IsPlayerNameChanging || UndoRedoSystem.UndoRedoSystemWorking || UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) return;
//        if (ProfileComboBox.SelectedValue is ShareableProfile profile)
//        {
//#if DEBUG
//            if (UIKeys.Keys[Key.LeftShift].Is)
//            {
//                MainView.IsPlayerNameChanging = true;
//                BlockChangeNotif = true;
//                var loadout = DataStorage.Loadouts[DataStorage.ShareableProfiles.IndexOf(profile)];
//                LoggingSystem.Log($"Removing Loadout[{loadout.Shareable.Name}]");
//                loadout.Remove();

//                ProfileComboBox.SelectedValue = e.RemovedItems[0];

//                MainView.IsPlayerNameChanging = false;
//                BlockChangeNotif = false;
//                return;
//            }
//#endif

//            MainView.IsPlayerProfileChanging = true;
//            BlockChangeNotif = true;

//            object? removed = null;
//            if (e.RemovedItems.Count > 0)
//            { removed = e.RemovedItems[0]; }
//            else
//            { LoggingSystem.Log("GG Empty"); }

//            var index = DataStorage.ShareableProfiles.IndexOf(profile);

//            UndoRedoSystem.CreateValueChange(removed, ProfileComboBox.SelectedValue, ProfileComboBox.GetType().GetProperty(nameof(ProfileComboBox.SelectedValue)), ProfileComboBox);
//            UndoRedoSystem.DoValueChange(DataStorage.Loadouts[index], typeof(MainWindowView).GetProperty(nameof(MainView.Profile)), MainView);
//            UndoRedoSystem.DoValueChange(profile.Name, PlayerNameTextBox.GetType().GetProperty(nameof(PlayerNameTextBox.Text)), PlayerNameTextBox);
//            UndoRedoSystem.EndUndoRecord();
//            MainView.IsPlayerProfileChanging = false;
//            BlockChangeNotif = false;

//            ItemFilters.Instance.WeaponFilter = wasLastSelectedBorderPrimary ? MainView.Profile.BLR.Primary : MainView.Profile.BLR.Secondary;

//            ApplySearchAndFilter();

//            if (index != DataStorage.Settings.CurrentlyAppliedLoadout)
//            { MainView.Profile.BLR.IsChanged = true; }
//            else
//            { MainView.Profile.BLR.IsChanged = false; }
//        }
//        else
//        {
//            MainView.IsPlayerNameChanging = true;
//            if (e.RemovedItems.Count > 0) { ProfileComboBox.SelectedValue = e.RemovedItems[0]; }
//            MainView.IsPlayerNameChanging = false;
//        }
    }

    private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (MainView.IsPlayerProfileChanging || UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) return;

        MainView.IsPlayerNameChanging = true;

        //MainView.Profile.Shareable.Name = PlayerNameTextBox.Text;
        //MainView.Profile.Shareable.RefreshInfo();

        MainView.IsPlayerNameChanging = false;
    }

    private void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {
        MainView.Profile = BLRLoadoutStorage.AddNewLoadoutSet();
    }

    private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        if (UIKeys.Keys[Key.LeftShift].Is || UIKeys.Keys[Key.RightShift].Is)
        {
            ExportSystem.CopyMagiCowToClipboard(MainView.Profile);
            ShowAlert($"MagiCow Profile: {MainView.Profile.Shareable.Name} Copied!"); //TODO: Add Localization
        }
        //else if (UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is)
        //{
        //    var json = IOResources.Serialize(new Shareable3LoadoutSet(MainView.Profile.BLR), true);
        //    var jsonNoWhitespaces = IOResources.RemoveWhiteSpacesFromJson.Replace(json, "$1");
        //    var zipedJson = IOResources.Zip(jsonNoWhitespaces);
        //    var base64 = IOResources.DataToBase64(zipedJson);

        //    LoggingSystem.Log($"{MainView.Profile.Shareable.Name} Profile Compression: {jsonNoWhitespaces.Length} vs {base64.Length}");

        //    string link = $"<blredit://import-profile/{base64}>";
        //    ExportSystem.SetClipboard(link);
        //    ShowAlert($"{MainView.Profile.Shareable.Name} Share Link Created!"); //TODO: Add Localization
        //}
        //else if (UIKeys.Keys[Key.LeftAlt].Is || UIKeys.Keys[Key.RightAlt].Is)
        //{
        //    if (DataStorage.Settings.DefaultClient is not null)
        //    {
        //        foreach (var process in BLRProcess.RunningGames)
        //        {
        //            if (process.Client.Equals(DataStorage.Settings.DefaultClient) && process.ConnectedServer is not null)
        //            {
        //                string message = "Current loadout is not supported on this server\nOnly Vanilla loadouts are allowed!\nApply a non Advanced or modify this loadout!\n";
        //                if (!BLRClient.ValidProfile(MainView.Profile.BLR, process.ConnectedServer, ref message))
        //                {
        //                    LoggingSystem.MessageLog(message, "warning");
        //                    return;
        //                }
        //            }
        //        }
        //        if (DataStorage.Settings.SelectedSDKType == "BLRevive")
        //            ApplyProxyLoadouts(DataStorage.Settings.DefaultClient);
        //        else
        //            ApplyBLReviveLoadouts(DataStorage.Settings.DefaultClient);

        //        MainView.Profile.Shareable.LastApplied = DateTime.Now;
        //        DataStorage.Settings.CurrentlyAppliedLoadout = ProfileComboBox.SelectedIndex;
        //        MainView.Profile.BLR.IsChanged = false;
        //    }
        //}
        else
        {
            if (DataStorage.Settings.DefaultClient is not null)
            {
                foreach (var process in BLRProcess.RunningGames)
                {
                    if (process.Client.Equals(DataStorage.Settings.DefaultClient) && process.ConnectedServer is not null)
                    {
                        string message = "Current loadout is not supported on this server\nOnly Vanilla loadouts are allowed!\nApply a non Advanced or modify this loadout!\n";
                        if (!MainView.Profile.BLR.ValidateLoadout(ref message))
                        {
                            LoggingSystem.MessageLog(message, "warning");
                            return;
                        }
                    }
                }

                if(DataStorage.Settings.SelectedSDKType == "BLRevive")
                    ApplyBLReviveLoadouts(DataStorage.Settings.DefaultClient);
                else
                    ApplyProxyLoadouts(DataStorage.Settings.DefaultClient);
            }
        }
    }

    public static void ApplyProxyLoadouts(BLRClient client)
    {
        var directory = $"{client.BLReviveConfigsPath}profiles\\";
        Directory.CreateDirectory(directory);

        List<LoadoutManagerLoadout> loadouts = [];

        string message = string.Empty;
        int count = 0;
        foreach (var profile in DataStorage.Loadouts)
        {
            if (profile.BLR.Apply && profile.BLR.ValidateLoadout(ref message))
            {
                loadouts.Add(new LoadoutManagerLoadout(profile.BLR));
                profile.Shareable.LastApplied = DateTime.Now;
                profile.BLR.IsChanged = false;
                count++;
            }
            else
            { profile.BLR.Apply = false; }
            if (count >= 2) break;
        }

        IOResources.SerializeFile($"{directory}{DataStorage.Settings.PlayerName}.json", loadouts.ToArray());
        ShowAlert($"Applied Proxy Loadouts!\nScroll through your loadouts to\nrefresh ingame Loadouts!", 8); //TODO: Add Localization
        if (Instance is not null && Instance.lastAnim != CalmAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
        }
    }

    public static void ApplyBLReviveLoadouts(BLRClient client)
    {
        var directory = $"{client.BLReviveConfigsPath}profiles\\";
        Directory.CreateDirectory(directory);
        string message = string.Empty;
        List<LMLoadout> loadouts = [];

        var exportLoadouts = DataStorage.Loadouts.Where(l => l.BLR.ValidateLoadout(ref message)).Where(l => l.BLR.Apply).OrderBy(l => l.BLR.Name);

        foreach (var profile in exportLoadouts)
        {
            loadouts.Add(new(profile.BLR, profile.BLR.Name));
            profile.Shareable.LastApplied = DateTime.Now;
        }

        IOResources.SerializeFile($"{directory}{DataStorage.Settings.PlayerName}.json", loadouts.ToArray());
        ShowAlert($"Applied BLRevive Loadouts!\nScroll through your loadouts to\nrefresh ingame Loadouts!", 8); //TODO: Add Localization
        if (Instance is not null && Instance.lastAnim != CalmAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
        }
    }

    private void SortComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplySorting();
    }

    private void RandomLoadout_Click(object sender, RoutedEventArgs e)
    {
        MainView?.Profile?.BLR?.Randomize();
    }

    #region Hotkeys
    private void PreviewKeyUpMainWindow(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Tab:
                if (UIKeys.Keys[Key.LeftShift].Is || UIKeys.Keys[Key.RightShift].Is)
                {
                    buttonIndex--;
                }
                else
                {
                    buttonIndex++;
                }
                if (buttonIndex < 0)
                {
                    buttonIndex = MainWindowTabs.Items.Count - 1;
                }
                if (buttonIndex > MainWindowTabs.Items.Count - 1)
                {
                    buttonIndex = 0;
                }
                ((TabItem)MainWindowTabs.Items[buttonIndex]).Focus();
                break;
            case Key.A:
                if ((UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is) && (UIKeys.Keys[Key.LeftAlt].Is || UIKeys.Keys[Key.RightAlt].Is) && !SearchBox.IsFocused)
                {
                    string message = "Can't disable Advanced mode!\n";
                    if (!MainView.Profile.BLR.ValidateLoadout(ref message))
                    {
                        LoggingSystem.MessageLog(message, "warning");
                        return;
                    }

                    MainView.Profile.BLR.IsAdvanced.Set(!MainView.Profile.BLR.IsAdvanced.Is);
                    MainView.Profile.BLR.WriteToBackingStructure();
                    MainWindow.MainView.Profile.BLR.CalculateStats();
                    ApplySearchAndFilter();
                    ShowAlert($"{Properties.Resources.msg_AdvancedModding}:{(MainView.Profile.BLR.IsAdvanced.Is ? "On" : "Off")}");
                }
                break;
            case Key.Z:
                if (UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is) UndoRedoSystem.Undo();
                break;
            case Key.Y:
                if (UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is) UndoRedoSystem.Redo();
                break;
            case Key.C:
                if (UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is)
                {
                    if (HitTestLoadoutControls(this) is FrameworkElement target)
                    {
                        switch (target)
                        {
                            case WeaponControl weaponControl:
                                if (weaponControl.DataContext is BLRWeapon weapon)
                                {
                                    var copy = weapon.Copy();
                                    if (weapon.IsPrimary)
                                    {
                                        MainView.PrimaryWeaponCopy = copy;
                                        ShowAlert($"Copied Primary Weapon!"); //TODO: Add Localization
                                    }
                                    else
                                    {
                                        MainView.SecondaryWeaponCopy = copy;
                                        ShowAlert($"Copied Secondary Weapon!"); //TODO: Add Localization
                                    }
                                    if (weapon.InternalWeapon is ShareableWeapon wpn)
                                    {
                                        wpn.IsPrimary = weapon.IsPrimary;
                                        string json = IOResources.Serialize(wpn, true);
                                        string jsonNoWhitespaces = IOResources.RemoveWhiteSpacesFromJson.Replace(json, "$1");
                                        ExportSystem.SetClipboard(jsonNoWhitespaces);
                                    }
                                }
                                break;
                            case GearControl gearControl:
                                if (gearControl.DataContext is BLRLoadout gearLoadout)
                                {
                                    MainView.GearCopy = gearLoadout.CopyGear();
                                    ShowAlert($"Copied Gear!"); //TODO: Add Localization
                                    if (gearLoadout.InternalLoadout is ShareableLoadout ldt)
                                    {
                                        string json = IOResources.Serialize(ldt, true);
                                        string jsonNoWhitespaces = IOResources.RemoveWhiteSpacesFromJson.Replace(json, "$1");
                                        ExportSystem.SetClipboard(jsonNoWhitespaces);
                                    }
                                }
                                break;
                            case ExtraControl extraControl:
                                if (extraControl.DataContext is BLRLoadout extraLoadout)
                                {
                                    MainView.ExtraCopy = extraLoadout.CopyExtra();
                                    ShowAlert($"Copied Extra!"); //TODO: Add Localization
                                    if (extraLoadout.InternalLoadout is ShareableLoadout ldt)
                                    {
                                        string json = IOResources.Serialize(ldt, true);
                                        string jsonNoWhitespaces = IOResources.RemoveWhiteSpacesFromJson.Replace(json, "$1");
                                        ExportSystem.SetClipboard(jsonNoWhitespaces);
                                    }
                                }
                                break;
                            case WeaponViewControl weaponViewControl:
                                if (weaponViewControl.DataContext is BLRWeapon viewWeapon)
                                {
                                    if (viewWeapon.IsPrimary)
                                    {
                                        MainView.PrimaryWeaponCopy = viewWeapon.Copy();
                                        ShowAlert($"Copied Primary Weapon!"); //TODO: Add Localization
                                    }
                                    else
                                    {
                                        MainView.SecondaryWeaponCopy = viewWeapon.Copy();
                                        ShowAlert($"Copied Secondary Weapon!"); //TODO: Add Localization
                                    }
                                    if (viewWeapon.InternalWeapon is ShareableWeapon wpn)
                                    {
                                        wpn.IsPrimary = viewWeapon.IsPrimary;
                                        string json = IOResources.Serialize(wpn, true);
                                        string jsonNoWhitespaces = IOResources.RemoveWhiteSpacesFromJson.Replace(json, "$1");
                                        ExportSystem.SetClipboard(jsonNoWhitespaces);
                                    }
                                }
                                break;
                            case LoadoutViewControl loadoutViewControl:
                                if (loadoutViewControl.DataContext is BLRLoadout viewLoadout)
                                {
                                    MainView.ExtraCopy = viewLoadout.CopyExtra();
                                    MainView.GearCopy = viewLoadout.CopyGear();
                                    ShowAlert($"Copied Gear & Extra!"); //TODO: Add Localization
                                    if (viewLoadout.InternalLoadout is ShareableLoadout ldt)
                                    {
                                        string json = IOResources.Serialize(ldt, true);
                                        string jsonNoWhitespaces = IOResources.RemoveWhiteSpacesFromJson.Replace(json, "$1");
                                        ExportSystem.SetClipboard(jsonNoWhitespaces);
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
            case Key.V:
                if (UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is)
                {
                    if (HitTestLoadoutControls(this) is FrameworkElement target)
                    {
                        var clip = ExportSystem.GetClipboard();
                        switch (target)
                        {
                            case WeaponControl weaponControl:
                                if (weaponControl.DataContext is BLRWeapon weapon)
                                {
                                    if (weapon.IsPrimary)
                                    {
                                        if (clip is not null && IOResources.Deserialize<ShareableWeapon>(clip) is ShareableWeapon wpn && wpn.IsPrimary is not null && wpn.IsPrimary == true)
                                        {
                                            MainView.PrimaryWeaponCopy = wpn.ToBLRWeapon(true);
                                        }
                                        weapon.ApplyCopy(MainView.PrimaryWeaponCopy);
                                    }
                                    else
                                    {
                                        if (clip is not null && IOResources.Deserialize<ShareableWeapon>(clip) is ShareableWeapon wpn && wpn.IsPrimary is not null && wpn.IsPrimary == false)
                                        {
                                            MainView.SecondaryWeaponCopy = wpn.ToBLRWeapon(false);
                                        }
                                        weapon.ApplyCopy(MainView.SecondaryWeaponCopy);
                                    }
                                }
                                break;
                            case GearControl gearControl:
                                if (gearControl.DataContext is BLRLoadout gearLoadout)
                                {
                                    if (clip is not null && IOResources.Deserialize<ShareableLoadout>(clip) is ShareableLoadout ldt)
                                    {
                                        MainView.GearCopy = ldt.ToBLRLoadout().CopyGear();
                                    }
                                    gearLoadout.ApplyExtraGearCopy(null, MainView.GearCopy);
                                }
                                break;
                            case ExtraControl extraControl:
                                if (extraControl.DataContext is BLRLoadout extraLoadout)
                                {
                                    if (clip is not null && IOResources.Deserialize<ShareableLoadout>(clip) is ShareableLoadout ldt)
                                    {
                                        MainView.ExtraCopy = ldt.ToBLRLoadout().CopyExtra();
                                    }
                                    extraLoadout.ApplyExtraGearCopy(MainView.ExtraCopy);
                                }
                                break;
                            case LoadoutViewControl loadoutViewControl:
                                if (loadoutViewControl.DataContext is BLRLoadout viewLoadout)
                                {
                                    if (clip is not null && IOResources.Deserialize<ShareableLoadout>(clip) is ShareableLoadout ldt)
                                    {
                                        MainView.GearCopy = ldt.ToBLRLoadout().CopyGear();
                                        MainView.ExtraCopy = ldt.ToBLRLoadout().CopyExtra();
                                    }
                                    viewLoadout.ApplyExtraGearCopy(MainView.ExtraCopy, MainView.GearCopy);
                                }
                                break;
                            case WeaponViewControl weaponViewControl:
                                if (weaponViewControl.DataContext is BLRWeapon viewWeapon)
                                {
                                    if (viewWeapon.IsPrimary)
                                    {
                                        if (clip is not null && IOResources.Deserialize<ShareableWeapon>(clip) is ShareableWeapon wpn && wpn.IsPrimary is not null && wpn.IsPrimary == true)
                                        {
                                            MainView.PrimaryWeaponCopy = wpn.ToBLRWeapon(true);
                                        }
                                        viewWeapon.ApplyCopy(MainView.PrimaryWeaponCopy);
                                    }
                                    else
                                    {
                                        if (clip is not null && IOResources.Deserialize<ShareableWeapon>(clip) is ShareableWeapon wpn && wpn.IsPrimary is not null && wpn.IsPrimary == false)
                                        {
                                            MainView.SecondaryWeaponCopy = wpn.ToBLRWeapon(false);
                                        }
                                        viewWeapon.ApplyCopy(MainView.SecondaryWeaponCopy);
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
        }
    }
    #endregion Hotkeys

    private void DuplicateProfile_Click(object sender, RoutedEventArgs e)
    {
        var duplicateLoadout = MainView.Profile.Duplicate();
        MainView.Profile = duplicateLoadout;
    }

    private void PlayerNameTextBox_PreviewInput(object sender, TextCompositionEventArgs e)
    {
        int index = e.Text.IndexOfAny(InvalidNameChars);
        if (index >= 0)
        { e.Handled = true; }
    }

    private void LoadoutTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BlockChangeNotif = true;
        if (sender is TabControl control)
        {
            if (control.SelectedItem is TabItem tab)
            {
                if (tab.Content is LoadoutControl lcontrol)
                {
                    lcontrol.ApplyBorder();
                }
            }
            switch (control.SelectedIndex)
            {
                case 0:
                    ImportSystem.UpdateArmorImages(MainView.Profile.BLR.IsFemale);
                    break;
            }
        }
        BlockChangeNotif = false;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ItemFilters.Instance.SearchFilter = SearchBox.Text;
    }

    private void Window_Closed(object sender, EventArgs e)
    {

    }

    private void Window_Initialized(object sender, EventArgs e)
    {
        LoggingSystem.Log("MainWindow Initialized Start");

#if DEBUGWAIT
        LoggingSystem.MessageLog("Waiting!", "Debug"); //TODO: Add Localization
#endif

        #region Backend Init
        var watch = Stopwatch.StartNew();
        App.DownloadLocalization();
        LoggingSystem.Log($"[MainWindow]: Localization took {watch.ElapsedMilliseconds}ms");

        watch.Restart();
        App.RuntimeCheck();
        LoggingSystem.Log($"[MainWindow]: Runtime Check took {watch.ElapsedMilliseconds}ms");

        watch.Restart();
        ImportSystem.Initialize();
        LoggingSystem.Log($"[MainWindow]: ImportSystem took {watch.ElapsedMilliseconds}ms");
        watch.Restart();
        #endregion Backend Init

        #region Frontend Init
        Instance = this;

        MainView.LastSelectedItemBorder = ((WeaponControl)((Grid)((ScrollViewer)((TabItem)((TabControl)((Grid)(Loadout1.Content)).Children[0]).Items[0]).Content).Content).Children[0]).Receiver;
        ItemFilters.Instance.WeaponFilter = MainView.Profile.BLR.Primary;

        this.DataContext = MainView;
        #endregion Frontend Init

        SetItemList(ImportSystem.PRIMARY_CATEGORY);
        if (App.IsNewVersionAvailable && DataStorage.Settings.ShowUpdateNotice.Is)
        {
            Process.Start($"{App.RepositoryBaseURL}/releases");
        }
        if (DataStorage.Settings.DoRuntimeCheck.Is || DataStorage.Settings.ForceRuntimeCheck.Is)
        {
            if (App.IsVC2015x89Missing || App.IsVC2012Update4x89Missing || DataStorage.Settings.ForceRuntimeCheck.Is)
            {
                var info = new InfoPopups.DownloadRuntimes();
                if (!App.IsVC2012Update4x89Missing)
                {
                    info.Link2012Updatet4Content.Text = "Microsoft Visual C++ 2012 Update 4(x86/32bit) is already installed!"; //TODO: Add Localization
                }
                info.ShowDialog();
            }
        }

        IOResources.GetGameLocationsFromSteam();
        foreach (string folder in IOResources.GameFolders)
        {
            var GameInstance = $"{folder}{IOResources.GAME_DEFAULT_EXE}";
            if (File.Exists(GameInstance))
            {
                bool alreadyRegistered = false;
                foreach (var client in DataStorage.GameClients)
                {
                    if (client.OriginalPath == GameInstance) { alreadyRegistered = true; continue; }
                }
                if (!alreadyRegistered)
                {
                    AddGameClient(new BLRClient() { OriginalPath = GameInstance });
                }
            }
        }

        if (DataStorage.GameClients.Count <= 0)
        {
            MessageBox.Show("You have to locate and add atleast one Client"); //TODO: Add Localization
        }
        else
        {
            LoggingSystem.Log($"Validating Client List {DataStorage.GameClients.Count}"); //TODO: Add Localization
            for (int i = 0; i < DataStorage.GameClients.Count; i++)
            {
                if (!DataStorage.GameClients[i].OriginalFileValidation())
                { DataStorage.GameClients.RemoveAt(i); i--; }
                else
                {
                    var beforeClean = DataStorage.GameClients[i].InstalledModules.Count;
                    if (DataStorage.GameClients[i].InstalledModules.Count > 0)
                    {
                        DataStorage.GameClients[i].InstalledModules = new ObservableCollection<ProxyModule>(DataStorage.GameClients[i].InstalledModules.Distinct(new ProxyModuleComparer()));
                        
                    }
                    LoggingSystem.Log($"{DataStorage.GameClients[i]} has {DataStorage.GameClients[i].InstalledModules.Count}/{beforeClean} installed modules");
                }
            }
            if (DataStorage.GameClients.Count > 0 && DataStorage.Settings.DefaultClient is null)
            {
                DataStorage.GameClients[0].SetCurrentClient();
            }

        }

        RefreshDefaultClient();

        AddOrUpdateDefaultServers();

        if (DataStorage.Settings.DefaultServer is null)
        {
            DataStorage.Settings.DefaultServer = DataStorage.ServerList[0];
        }

        BLREditPipe.ProcessArgs(Args);

        SetProfileSettings();

        ApplyLoadoutBorder.Background = SolidColorBrush;
        SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
        lastAnim = CalmAnim;

        MainView.UpdateWindowTitle();

        UndoRedoSystem.ClearUndoRedoStack();

        LoggingSystem.Log($"Window Init took {watch.ElapsedMilliseconds}ms");
    }

    private static void RefreshDefaultClient()
    {
        foreach (var client in DataStorage.GameClients)
        {
            client.CurrentClient.Set(false);
        }
        if (DataStorage.Settings.SelectedClient >= 0 && DataStorage.Settings.SelectedClient < DataStorage.GameClients.Count)
        { DataStorage.GameClients[DataStorage.Settings.SelectedClient].CurrentClient.Set(true); }
    }

    public void LoadoutChanged(object sender, PropertyChangedEventArgs e)
    {
        if (BlockChangeNotif) return;
        if (MainView.Profile.BLR.IsChanged && lastAnim != AlertAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, AlertAnim, HandoffBehavior.Compose);
            lastAnim = AlertAnim;
        }
        //else if (!MainView.Profile.BLR.IsChanged && lastAnim != CalmAnim)
        //{
        //    SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
        //    lastAnim = CalmAnim;
        //}
    }

    private void MainWindowTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Contains(ServerListTab))
        {
            RefreshPing();
        }
        if (e.RemovedItems.Contains(SettingsTab))
        {
            SetProfileSettings();
        }
        if (e.AddedItems.Contains(SettingsTab))
        {
            DataStorage.Settings.LastPlayerName = DataStorage.Settings.PlayerName;
        }
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        ProfileListTab.IsSelected = true;
        this.WindowState = WindowState.Maximized;
        ItemListTab.IsSelected = true;
        if (DataStorage.Settings.PlayerName == "BLREdit-Player")
        {
            SettingsTab.IsSelected = true;
            LoggingSystem.MessageLog(Properties.Resources.msg_ChangePlayerName, "Info"); //TODO: Add Localization
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        UIKeys.SetAll(false);
    }
    #endregion Events

    public void SetItemList(string? Type = null)
    {
        Type ??= lastSelectedType;
        lastSelectedType = Type;
        var list = ImportSystem.GetItemListOfType(Type);
        if (list is not null && list.Count > 0)
        {
            switch (list[0].Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                    SetSortingType(typeof(ImportHelmetSortingType));
                    break;

                case ImportSystem.UPPER_BODIES_CATEGORY:
                case ImportSystem.LOWER_BODIES_CATEGORY:
                    SetSortingType(typeof(ImportArmorSortingType));
                    break;

                case ImportSystem.ATTACHMENTS_CATEGORY:
                    SetSortingType(typeof(ImportGearSortingType));
                    break;

                case ImportSystem.SCOPES_CATEGORY:
                    SetSortingType(typeof(ImportScopeSortingType));
                    break;

                case ImportSystem.AVATARS_CATEGORY:
                case ImportSystem.CAMOS_BODIES_CATEGORY:
                case ImportSystem.CAMOS_WEAPONS_CATEGORY:
                case ImportSystem.HANGERS_CATEGORY:
                case ImportSystem.BADGES_CATEGORY:
                case ImportSystem.EMOTES_CATEGORY:
                case ImportSystem.TACTICAL_CATEGORY:
                case ImportSystem.AMMO_CATEGORY:
                    SetSortingType(typeof(ImportNoStatsSortingType));
                    break;

                case ImportSystem.GRIPS_CATEGORY:
                    SetSortingType(typeof(ImportGripSortingType));
                    break;

                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    SetSortingType(typeof(ImportWeaponSortingType));
                    break;

                case ImportSystem.SHOP_CATEGORY:
                    SetSortingType(typeof(ImportShopSortingType));
                    break;

                default:
                    SetSortingType(typeof(ImportModificationSortingType));
                    break;
            }
            ItemList.ItemsSource = list;
            ApplySorting(true);
            if (!ItemListTab.IsFocused) ItemListTab.Focus();
        }
        else
        {
            LoggingSystem.Log($"Failed to Set ItemList to {Type}");
        }
    }

    public void ApplySorting(bool resetView = false)
    {
        if (CollectionViewSource.GetDefaultView(ItemList.ItemsSource) is CollectionView view)
        {
            view.SortDescriptions.Clear();

            if (SortComboBox1.Items.Count > 0 && SortComboBox1.SelectedItem != null)
            {
                MainView.CurrentSortingPropertyName = Enum.GetName(MainView.CurrentSortingEnumType, Enum.GetValues(MainView.CurrentSortingEnumType).GetValue(SortComboBox1.SelectedIndex));
                view.SortDescriptions.Add(new SortDescription(MainView.CurrentSortingPropertyName, MainView.ItemListSortingDirection));
            }
            if (resetView)
            { 
                view.Refresh();
            }
        }
    }

    private void SetSortingType(Type SortingEnumType)
    {
        if (lastSelectedSortingType != SortingEnumType)
        {
            lastSelectedSortingType = SortingEnumType;
            int index = SortComboBox1.SelectedIndex;

            MainView.CurrentSortingEnumType = SortingEnumType;
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

    public static void SetItemToBorder(Border? border, BLRItem item)
    {
        if (border is null) { return; }
        if (border.Parent is FrameworkElement parent)
        {
            if (parent.DataContext is BLRWeapon weapon)
            {
                UndoRedoSystem.DoValueChange(item, weapon.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), weapon, BlockEvents.None);
                UndoRedoSystem.EndUndoRecord();
            }
            if (parent.DataContext is BLRLoadout loadout)
            {
                UndoRedoSystem.DoValueChange(item, loadout.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), loadout, BlockEvents.None);
                UndoRedoSystem.EndUndoRecord();
            }
        }
    }

    private void ChangeSortingDirection(object sender, RoutedEventArgs e)
    {
        if (MainView.ItemListSortingDirection == ListSortDirection.Ascending)
        {
            MainView.ItemListSortingDirection = ListSortDirection.Descending;
            SortDirectionButton.Content = Properties.Resources.btn_Descending;
        }
        else
        {
            MainView.ItemListSortingDirection = ListSortDirection.Ascending;
            SortDirectionButton.Content = Properties.Resources.btn_Ascending;
        }
        ApplySorting();
    }

    #region Server UI

    private void PingServers_Click(object sender, RoutedEventArgs e)
    {
        RefreshPing();
    }
    #endregion Server UI

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) yield return (T)Enumerable.Empty<T>();
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
            if (ithChild == null) continue;
            if (ithChild is T t) yield return t;
            foreach (T childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
        }
    }

    public static FrameworkElement? HitTestLoadoutControls(DependencyObject depObj)
    {
        var AllExtras = FindVisualChildren<ExtraControl>(depObj).ToList();
        var AllGears = FindVisualChildren<GearControl>(depObj).ToList();
        var AllWeapons = FindVisualChildren<WeaponControl>(depObj).ToList();
        var AllLoadoutViews = FindVisualChildren<LoadoutViewControl>(depObj).ToList();
        var AllWeaponViews = FindVisualChildren<WeaponViewControl>(depObj).ToList();



        if (AllExtras.Count > 0)
        {
            foreach (var extra in AllExtras)
            {
                var pos = Mouse.GetPosition(extra);
                if (VisualTreeHelper.GetDescendantBounds(extra).Contains(pos))
                { 
                    return extra;
                }
            }
        }

        if (AllGears.Count > 0)
        {
            foreach (var gear in AllGears)
            {
                var pos = Mouse.GetPosition(gear);
                if (VisualTreeHelper.GetDescendantBounds(gear).Contains(pos))
                {
                    return gear;
                }
            }
        }

        if (AllWeapons.Count > 0)
        {
            foreach (var weapon in AllWeapons)
            {
                var pos = Mouse.GetPosition(weapon);
                if (VisualTreeHelper.GetDescendantBounds(weapon).Contains(pos))
                {
                    return weapon;
                }
            }
        }

        if (AllLoadoutViews.Count > 0)
        {
            foreach (var weapon in AllLoadoutViews)
            {
                var pos = Mouse.GetPosition(weapon);
                if (VisualTreeHelper.GetDescendantBounds(weapon).Contains(pos))
                {
                    return weapon;
                }
            }
        }

        if (AllWeaponViews.Count > 0)
        {
            foreach (var weapon in AllWeaponViews)
            {
                var pos = Mouse.GetPosition(weapon);
                if (VisualTreeHelper.GetDescendantBounds(weapon).Contains(pos))
                {
                    return weapon;
                }
            }
        }

        return null;
    }

    public static void ShowAlert(string message, double displayTime = 4, double displayWidth = 400)
    {
        //TODO: Add Localization for alerts
        if (Instance is null) return;
        var grid = CreateAlertGrid(message);
        Instance.AlertList.Items.Add(grid);
        new TripleAnimationDouble(0, displayWidth, 1, displayTime, 1, grid, Grid.WidthProperty, Instance.AlertList.Items).Begin(Instance.AlertList);
    }

    private static Grid CreateAlertGrid(string Alert)
    {
        TextBox alertText = new() { Text = Alert, TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 136, 0)), IsReadOnly = true, FontSize = 26 };
        Grid alertGrid = new() { Background = new SolidColorBrush(Color.FromArgb(159, 0, 0, 0)), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Width = 400 };
        alertGrid.Children.Add(alertText);
        return alertGrid;
    }

    public void BringWindowIntoBounds()
    {
        if (this.Top < SystemParameters.VirtualScreenTop)
        {
            this.Top = SystemParameters.VirtualScreenTop;
        }

        if (this.Left < SystemParameters.VirtualScreenLeft)
        {
            this.Left = SystemParameters.VirtualScreenLeft;
        }

        if (this.Top > SystemParameters.VirtualScreenHeight)
        {
            this.Top = SystemParameters.VirtualScreenHeight/2.0D;
        }

        if (this.Left > SystemParameters.VirtualScreenWidth)
        {
            this.Left = SystemParameters.VirtualScreenWidth/2.0D;
        }
    }

    public void SetProfileSettings()
    {
        foreach (var item in ((TabControl)ProfileSettingsTab.Content).Items)
        {
            if (item is FrameworkElement element)
            {
                element.DataContext = ExportSystem.GetOrAddProfileSettings(DataStorage.Settings.PlayerName);
            }
        }
    }

    internal void RefreshServerList()
    {
        if (ServerListView is ServerListControl serverControl && serverControl.ServerListView is ListView listView && CollectionViewSource.GetDefaultView(listView.ItemsSource) is CollectionView view)
        {
            view.Refresh();
        }
    }
}