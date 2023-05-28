using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using BLREdit.Game;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using BLREdit.Game.Proxy;
using BLREdit.UI.Views;
using System.IO;
using BLREdit.Import;
using BLREdit.Export;
using BLREdit.UI.Controls;
using BLREdit.UI.Windows;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Threading;
using BLREdit.API.InterProcess;
using System.Diagnostics;
using PeNet;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Markup;
using System.Text.Json;
using System.Text;
using System.IO.Compression;
using System.Buffers.Text;
using Microsoft.IdentityModel.Tokens;
using BLREdit.API.Export;
using BLREdit.API.Utils;
using Gameloop.Vdf.Linq;
using System.Diagnostics.CodeAnalysis;

namespace BLREdit.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public static readonly BLRClientWindow ClientWindow = new();

    private static readonly Color DefaultBorderColor = Color.FromArgb(14, 158, 158, 158);
    private static readonly Color ActiveBorderColor = Color.FromArgb(255, 255, 136, 0);

    /// <summary>
    /// Contains the last selected Border for setting the ItemList
    /// </summary>
    public static Border LastSelectedBorder { get { return lastSelectedBorder; } set { SetBorderColor(lastSelectedBorder, DefaultBorderColor); lastSelectedBorder = value; SetBorderColor(lastSelectedBorder, ActiveBorderColor); } }
    private static Border lastSelectedBorder = null;

    /// <summary>
    /// Contains the current active loadout
    /// </summary>
    public static MagiCowsProfile ActiveProfile { get { return activeProfile; } set { activeProfile = value; Profile.LoadMagiCowsProfile(value); } }
    private static MagiCowsProfile activeProfile = null;

    public static BLRProfile Profile { get; } = new();
    /// <summary>
    /// Contains the Sorting Direction for the ItemList
    /// </summary>
    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Descending;

    public Type CurrentSortingEnumType { get; private set; }
    public string CurrentSortingPropertyName { get; private set; } = "None";

    /// <summary>
    /// Prevents Profile Changes
    /// </summary>
    public bool IsPlayerNameChanging { get; private set; } = false;
    /// <summary>
    /// Prevents Profile Name Changes
    /// </summary>
    public bool IsPlayerProfileChanging { get; private set; } = false;

    public bool IsCheckingGameClient { get; private set; } = false;

    public static ObservableCollection<BLRClient> GameClients { get; set; }  
    public static ObservableCollection<BLRServer> ServerList { get; set; }

    public static BLRWeapon Copy { get; set; } = null;

    public static MainWindow Self { get; private set; } = null;

    private readonly string[] Args;
    public MainWindow(string[] args)
    {
        Args = args;
        IsPlayerProfileChanging = true;
        IsPlayerNameChanging = true;

        PreviewKeyDown += UIKeys.Instance.KeyDown;
        PreviewKeyUp += UIKeys.Instance.KeyUp;

        InitializeComponent();

        IsPlayerProfileChanging = false;
        IsPlayerNameChanging = false;
    }

    public void ApplySearchAndFilter()
    {
        if (CollectionViewSource.GetDefaultView(ItemList.ItemsSource) is CollectionView view) view.Refresh();
    }

    private static void SetBorderColor(Border border, Color color)
    {
        if (border is not null) border.BorderBrush = new SolidColorBrush(color);
    }

    public static void CheckGameClients()
    {
        LoggingSystem.Log("Checking for patched clients");
        if (GameClients.Count <= 0)
        {
            MessageBox.Show("You have to locate and add atleast one Client");
        }
    }

    static readonly Stopwatch PingWatch = Stopwatch.StartNew();
    static bool firstStart = true;
    public static void RefreshPing()
    {
        if (firstStart || PingWatch.ElapsedMilliseconds > 30000)
        {
            firstStart= false;
            foreach (BLRServer server in ServerList)
            {
                server.PingServer();
            }
            PingWatch.Restart();
        }
    }

    private static void AddDefaultServers()
    {
        foreach (BLRServer defaultServer in App.DefaultServers)
        {
            AddServer(defaultServer);
        }
    }

    public static void AddServer(BLRServer server, bool forceAdd = false)
    {
        if (forceAdd || !IsInCollection(ServerList, server))
        {
            ServerList.Add(server);
        }
    }

    public static bool IsInCollection<T>(ObservableCollection<T> collection, T item)
    {
        foreach (T item2 in collection) 
        { 
            if(item.Equals(item2)) return true;
        }
        return false;
    }

    public static void AddGameClient(BLRClient client)
    {
        if(!IsInCollection(GameClients, client))
        {
            GameClients.Add(client);
            LoggingSystem.Log($"Adding New Client: {client}");
        }
    }

    private void ChangeCurrentServer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is BLRServer server)
            {
                BLREditSettings.Settings.DefaultServer = server;
                foreach (BLRServer s in ServerList)
                {
                    s.IsDefaultServer = false;
                }
            }
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ExportSystem.SaveProfiles();
        IOResources.SerializeFile($"GameClients.json", GameClients);
        IOResources.SerializeFile($"ServerList.json", ServerList);
        BLREditSettings.Save();
        ProxyModule.Save();
        ClientWindow.ForceClose();
    }

    private void Border_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Serializable)) e.Effects = DragDropEffects.Copy;
        else e.Effects = DragDropEffects.None;
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        LoggingSystem.Log($"Recieved Drop {e.Data.GetDataPresent(typeof(BLRItem))}");
        if (e.Data.GetData(typeof(BLRItem)) is BLRItem item)
        {
            Image image = null;
            Border border = null;
            if (e.OriginalSource is Image mage)
            {
                image = mage;
                if (mage.Parent is Border order) border = order;
            }
            else if (e.OriginalSource is Border order)
            {
                border = order;
                image = (Image)order.Child;
            }

            if (image is null || border is null) LoggingSystem.Log("Image or Border is null");

            if (((FrameworkElement)border.Parent).DataContext is BLRWeapon weapon)
            {
                UndoRedoSystem.DoAction(item, weapon.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), weapon);
                UndoRedoSystem.EndAction();
            }
            else if (((FrameworkElement)border.Parent).DataContext is BLRLoadout loadout)
            {
                UndoRedoSystem.DoAction(item, loadout.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), loadout);
                UndoRedoSystem.EndAction();
            }
        }
    }

    public bool wasLastImageScopePreview = false;
    private void Border_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Image image = null;
        Border border = null;
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

        if (image is null || border is null) { LoggingSystem.Log("Image or Border is null"); return; }

        if (e.ChangedButton == MouseButton.Left)
        {
            if (wasLastImageScopePreview)
            {
                foreach (var item in ImportSystem.GetItemListOfType(ImportSystem.SCOPES_CATEGORY))
                {
                    item.RemoveCrosshair();
                }
            }
            var weapon = ((FrameworkElement)border.Parent).DataContext as BLRWeapon;
            if (weapon is not null) ItemFilters.Instance.WeaponFilter = weapon.Reciever;
            LastSelectedBorder = border;
            wasLastImageScopePreview = false;
            switch (border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName)
            {
                case "Reciever":
                    if (weapon.IsPrimary) SetItemList(ImportSystem.PRIMARY_CATEGORY);
                    if (!weapon.IsPrimary) SetItemList(ImportSystem.SECONDARY_CATEGORY);
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
                        weapon?.Scope?.LoadCrosshair(weapon);
                        wasLastImageScopePreview = true;
                        ItemList.ItemsSource = new BLRItem[] { weapon.Scope };
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
                UndoRedoSystem.DoAction(null, weapon.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), weapon);
                UndoRedoSystem.EndAction();
            }
            else if (((FrameworkElement)border.Parent).DataContext is BLRLoadout loadout)
            {
                UndoRedoSystem.DoAction(null, loadout.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), loadout);
                UndoRedoSystem.EndAction();
            }
        }
    }

    private string lastSelectedType = "";
    public void SetItemList(string Type = null)
    {
        if (Type is null) Type = lastSelectedType;
        else lastSelectedType = Type;
        var list = ImportSystem.GetItemListOfType(Type);
        if (list.Count > 0)
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
            ApplySorting();
        }
        LoggingSystem.Log($"ItemList Set for {Type}");
        if (!ItemListTab.IsFocused) ItemListTab.Focus();
    }

    public void ApplySorting()
    {
        if (CollectionViewSource.GetDefaultView(ItemList.ItemsSource) is CollectionView view)
        {
            view.SortDescriptions.Clear();

            if (SortComboBox1.Items.Count > 0 && SortComboBox1.SelectedItem != null)
            {
                CurrentSortingPropertyName = Enum.GetName(CurrentSortingEnumType, Enum.GetValues(CurrentSortingEnumType).GetValue(SortComboBox1.SelectedIndex));
                view.SortDescriptions.Add(new SortDescription(CurrentSortingPropertyName, SortDirection));
            }
        }
    }

    private Type lastSelectedSortingType = null;
    private void SetSortingType(Type SortingEnumType)
    {
        if (lastSelectedSortingType != SortingEnumType)
        {
            lastSelectedSortingType = SortingEnumType;
            int index = SortComboBox1.SelectedIndex;

            CurrentSortingEnumType = SortingEnumType;
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

    public static void SetItemToBorder(Border border, BLRItem item, bool blockEvent = false, bool blockUpdate = false)
    {
        if (border is null) { LoggingSystem.Log($"Missing Border in SetItemToBorder()"); return; }
        if (((FrameworkElement)border.Parent).DataContext is BLRWeapon weapon)
        {
            UndoRedoSystem.DoAction(item, weapon.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), weapon, blockEvent, blockUpdate);
            UndoRedoSystem.EndAction();
        }
        else if (((FrameworkElement)border.Parent).DataContext is BLRLoadout loadout)
        {
            UndoRedoSystem.DoAction(item, loadout.GetType().GetProperty(border.GetBindingExpression(Border.DataContextProperty).ResolvedSourcePropertyName), loadout, blockEvent, blockUpdate);
            UndoRedoSystem.EndAction();
        }
    }

    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsPlayerNameChanging || UndoRedoSystem.BlockEvent) return;
        IsPlayerProfileChanging = true;
        BlockChangeNotif = true;

        LoggingSystem.Log("Changing Profile");
        
        if (ProfileComboBox.SelectedValue is ExportSystemProfile profile)
        {
            UndoRedoSystem.CreateAction(e.RemovedItems[0], ProfileComboBox.SelectedValue, ProfileComboBox.GetType().GetProperty(nameof(ProfileComboBox.SelectedValue)), ProfileComboBox, true);
            UndoRedoSystem.DoAction(profile, typeof(ExportSystem).GetProperty(nameof(ExportSystem.ActiveProfile)), null);
            UndoRedoSystem.DoAction(profile.PlayerName, PlayerNameTextBox.GetType().GetProperty(nameof(PlayerNameTextBox.Text)), PlayerNameTextBox);
            UndoRedoSystem.EndAction();
        }
        IsPlayerProfileChanging = false;
        BlockChangeNotif = false;
        Profile.IsChanged = true;
    }
    private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsPlayerProfileChanging || UndoRedoSystem.BlockEvent) return;
        
        IsPlayerNameChanging = true;

        int index = ProfileComboBox.SelectedIndex;
        ExportSystem.RemoveActiveProfileFromDisk();
        ExportSystem.ActiveProfile.PlayerName = PlayerNameTextBox.Text;
        ProfileComboBox.SelectedIndex = index;
        ExportSystem.ActiveProfile.RefreshInfo();

        IsPlayerNameChanging = false;
    }
    private void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ExportSystem.AddProfile();
    }

    private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        if (UIKeys.Keys[Key.LeftShift].Is || UIKeys.Keys[Key.RightShift].Is)
        {
            ExportSystem.CopyToClipBoard(Profile);
            ShowAlert($"MagiCow Profile: {ExportSystem.ActiveProfile.Name} Copied!");
        }
        else if (UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is)
        {
            string link = $"<blredit://import-profile/{IOResources.JsonToBase64(IOResources.Serialize(new ShareableProfile(Profile), true))}>";
            ExportSystem.SetClipboard(link);
            ShowAlert($"{ExportSystem.ActiveProfile.Name} Share Link Created!");
        }
        else
        {
            var directory = $"{BLREditSettings.Settings.DefaultClient.ConfigFolder}profiles\\";
            Directory.CreateDirectory(directory);
            IOResources.SerializeFile($"{directory}{BLREditSettings.Settings.PlayerName}.json", new[] { new LoadoutManagerLoadout(Profile.Loadout1), new LoadoutManagerLoadout(Profile.Loadout2), new LoadoutManagerLoadout(Profile.Loadout3) });
            ShowAlert($"Applied Loadouts!\nScroll trought your loadouts\nto Refresh ingame Loadouts!");
            Profile.IsChanged = false;
        }
    }

    private void SortComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplySorting();
    }

    private void ChangeSortingDirection(object sender, RoutedEventArgs e)
    {
        if (SortDirection == ListSortDirection.Ascending)
        {
            SortDirection = ListSortDirection.Descending;
            SortDirectionButton.Content = Properties.Resources.btn_Descending;
        }
        else
        {
            SortDirection = ListSortDirection.Ascending;
            SortDirectionButton.Content = Properties.Resources.btn_Ascending;
        }
        ApplySorting();
    }

    private void RandomLoadout_Click(object sender, RoutedEventArgs e)
    {
        if (LoadoutTabs.SelectedItem is TabItem item)
        {
            if (item.Content is LoadoutControl control)
            {
                if (control.DataContext is BLRLoadout loadout)
                {
                    loadout.Randomize();
                }
            }
        }
    }


    #region Server UI

    private void PingServers_Click(object sender, RoutedEventArgs e)
    {
        RefreshPing();
    }
    #endregion Server UI

    #region Hotkeys


    private int buttonIndex = 0;
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
                    BLREditSettings.Settings.AdvancedModding.Set(!BLREditSettings.Settings.AdvancedModding.Is);
                    BLREditSettings.Save();
                    ShowAlert($"{Properties.Resources.msg_AdvancedModding}:{BLREditSettings.Settings.AdvancedModding.Is}", 400);
                    LoggingSystem.Log($"{Properties.Resources.msg_AdvancedModding}:{BLREditSettings.Settings.AdvancedModding.Is}");
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
                    Point p = Mouse.GetPosition(this);
                    var hit = VisualTreeHelper.HitTest(this, p);
                    if (hit.VisualHit is FrameworkElement element)
                    {
                        while (element is not null && element.GetType() != typeof(WeaponControl))
                        {
                            element = element.Parent as FrameworkElement;
                        }
                        if (element is WeaponControl control)
                        {
                            if (control.DataContext is BLRWeapon weapon)
                            {
                                Copy = weapon.Copy();
                            }
                        }
                        
                    }
                }
                break;
            case Key.V:
                if (UIKeys.Keys[Key.LeftCtrl].Is || UIKeys.Keys[Key.RightCtrl].Is)
                {
                    Point p = Mouse.GetPosition(this);
                    var hit = VisualTreeHelper.HitTest(this, p);
                    if (hit.VisualHit is FrameworkElement element)
                    {
                        while (element is not null && element.GetType() != typeof(WeaponControl))
                        {
                            element = element.Parent as FrameworkElement;
                        }
                        if (element is WeaponControl control)
                        {
                            if (control.DataContext is BLRWeapon weapon)
                            {
                                weapon.ApplyCopy(Copy);
                            }
                        }
                    }

                }
                break;
        }
    }
    #endregion Hotkeys

    public static void ShowAlert(string message, double displayTime = 400)
    {
        //TODO: Add Localization for alerts
        if (Self is null) return;
        var grid = CreateAlertGrid(message);
        Self.AlertList.Items.Add(grid);
        new TripleAnimationDouble(0, displayTime, 1, 3, 1, grid, Grid.WidthProperty, Self.AlertList.Items).Begin(Self.AlertList);
    }
    private static Grid CreateAlertGrid(string Alert)
    {
        TextBox alertText = new() { Text = Alert, TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 136, 0)), IsReadOnly = true, FontSize = 26 };
        Grid alertGrid = new() { Background = new SolidColorBrush(Color.FromArgb(159, 0, 0, 0)), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Width = 400 };
        alertGrid.Children.Add(alertText);
        return alertGrid;
    }

    private void DuplicateProfile_Click(object sender, RoutedEventArgs e)
    {
        var profile = ExportSystem.ActiveProfile.Clone();
        ExportSystem.Profiles.Add(profile);
        ProfileComboBox.SelectedItem = profile;
    }

    readonly private static char[] InvalidNameChars = System.IO.Path.GetInvalidPathChars().Concat(System.IO.Path.GetInvalidFileNameChars()).ToArray();
    private void PlayerNameTextBox_PreviewInput(object sender, TextCompositionEventArgs e)
    {
        int index = e.Text.IndexOfAny(InvalidNameChars);
        if (index >= 0)
        { e.Handled = true; }
    }

    private Border lastLoadoutBorder= null;
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

            SetBorderColor(lastLoadoutBorder, DefaultBorderColor);
            switch (control.SelectedIndex)
            {
                case 0:
                    MainWindow.Profile.Loadout1.IsFemale = MainWindow.Profile.Loadout1.IsFemale;
                    lastLoadoutBorder = DetailsBorderLoadout1; 
                    break;
                case 1:
                    MainWindow.Profile.Loadout2.IsFemale = MainWindow.Profile.Loadout2.IsFemale;
                    lastLoadoutBorder = DetailsBorderLoadout2;
                    break;
                case 2:
                    MainWindow.Profile.Loadout3.IsFemale = MainWindow.Profile.Loadout3.IsFemale;
                    lastLoadoutBorder = DetailsBorderLoadout3;
                    break;
            }
            SetBorderColor(lastLoadoutBorder, ActiveBorderColor);
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
#if DEBUGWAIT
        MessageBox.Show("Waiting!");
#endif

        string BuildTag = "";

#if DEBUG
        BuildTag = "[Debug Build]:";
#elif RELEASE
        BuildTag = "[Release Build]:";
#elif PUBLISH
        BuildTag = "[Release Build]:";
#endif

        this.Title = $"{BuildTag}{App.CurrentRepo} - {App.CurrentVersion}";

        #region Backend Init
        var watch = Stopwatch.StartNew();
        App.CheckAppUpdate();
        LoggingSystem.Log($"[MainWindow]: Update Check took {watch.ElapsedMilliseconds}ms");

        watch.Restart();
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
        Self = this;
        IsPlayerProfileChanging = true;
        IsPlayerNameChanging = true;


        PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;
        ProfileComboBox.ItemsSource = ExportSystem.Profiles;
        ProfileComboBox.SelectedIndex = 0;

        UndoRedoSystem.BlockUpdate = true;
        UndoRedoSystem.BlockEvent = true;
        ActiveProfile = ExportSystem.ActiveProfile;
        UndoRedoSystem.BlockUpdate = false;
        UndoRedoSystem.BlockEvent = false;

        IsPlayerProfileChanging = false;
        IsPlayerNameChanging = false;

        LastSelectedBorder = ((WeaponControl)((Grid)((ScrollViewer)((TabItem)((TabControl)((Grid)((LoadoutControl)((TabItem)LoadoutTabs.Items[0]).Content).Content).Children[0]).Items[0]).Content).Content).Children[0]).Reciever;
        ItemFilters.Instance.WeaponFilter = Profile.Loadout1.Primary.Reciever;

        this.DataContext = Profile;
        #endregion Frontend Init

        SetItemList(ImportSystem.PRIMARY_CATEGORY);
        if (App.IsNewVersionAvailable && BLREditSettings.Settings.ShowUpdateNotice.Is)
        {
            System.Diagnostics.Process.Start($"https://github.com/{App.CurrentOwner}/{App.CurrentRepo}/releases");
        }
        if (BLREditSettings.Settings.DoRuntimeCheck.Is || BLREditSettings.Settings.ForceRuntimeCheck.Is)
        {
            if (App.IsBaseRuntimeMissing || App.IsUpdateRuntimeMissing || BLREditSettings.Settings.ForceRuntimeCheck.Is)
            {
                var info = new InfoPopups.DownloadRuntimes();
                if (!App.IsUpdateRuntimeMissing)
                {
                    info.Link2012Update4.IsEnabled = false;
                    info.Link2012Updatet4Content.Text = "Microsoft Visual C++ 2012 Update 4(x86/32bit) is already installed!";
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
                foreach (var client in GameClients)
                {
                    if (client.OriginalPath == GameInstance) { alreadyRegistered = true; continue; }
                }
                if (!alreadyRegistered)
                {
                    AddGameClient(new BLRClient() { OriginalPath = GameInstance });
                }
            }
        }

        if (MainWindow.GameClients.Count <= 0)
        {
            MessageBox.Show("You have to locate and add atleast one Client");
        }
        else
        {
            LoggingSystem.Log($"Validating Client List {GameClients.Count}");
            for (int i = 0; i < GameClients.Count; i++)
            {
                if (!GameClients[i].OriginalFileValidation())
                { GameClients.RemoveAt(i); i--; }
                else
                {
                    LoggingSystem.Log($"{GameClients[i]} has {GameClients[i].InstalledModules.Count} installed modules");
                    if (GameClients[i].InstalledModules.Count > 0)
                    {
                        GameClients[i].InstalledModules = new ObservableCollection<ProxyModule>(GameClients[i].InstalledModules.Distinct(new ProxyModuleComparer()));
                        LoggingSystem.Log($"{GameClients[i]} has {GameClients[i].InstalledModules.Count} installed modules");
                    }
                }
            }
        }

        AddDefaultServers();

        if (BLREditSettings.Settings.DefaultServer is null)
        {
            BLREditSettings.Settings.DefaultServer = ServerList[0];
        }
        BLREditSettings.SyncDefaultClient();

        Profile.Loadout1.IsFemale = Profile.Loadout1.IsFemale;

        BLREditPipe.ProcessArgs(Args);

        ClientListView.DataContext = GameClients;
        ServerListView.DataContext = ServerList;

        SetProfileSettings();

        ApplyLoadoutBorder.Background = SolidColorBrush;
        SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
        lastAnim = CalmAnim;
        Profile.PropertyChanged += LoadoutChanged;

        LoggingSystem.Log($"Window Init took {watch.ElapsedMilliseconds}ms");
    }

    private SolidColorBrush SolidColorBrush { get; } = new(Colors.Blue);
    private ColorAnimation AlertAnim { get; } = new()
    {
        From = Color.FromArgb(32, 0, 0, 0),
        To = Color.FromArgb(255, 255, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2)),
        AutoReverse = true,
        RepeatBehavior = RepeatBehavior.Forever
    };

    private ColorAnimation CalmAnim { get; } = new()
    {
        From = Color.FromArgb(255, 255, 0, 0),
        To = Color.FromArgb(32, 0, 0, 0),
        Duration = new Duration(TimeSpan.FromSeconds(2))
    };

    ColorAnimation lastAnim = null;

    public bool BlockChangeNotif { get; set; } = false;

    void LoadoutChanged(object sender, PropertyChangedEventArgs e)
    {
        if (BlockChangeNotif) return;
        if (Profile.IsChanged && lastAnim != AlertAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, AlertAnim, HandoffBehavior.Compose);
            lastAnim = AlertAnim;
        }
        else if (!Profile.IsChanged && lastAnim != CalmAnim)
        {
            SolidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, CalmAnim, HandoffBehavior.Compose);
            lastAnim = CalmAnim;
        }
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
    
    private void MainWindowTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Contains(LauncherTab))
        {
            RefreshPing();
        }
        if (e.RemovedItems.Contains(SettingsTab))
        {
            SetProfileSettings();
        }
        if (e.AddedItems.Contains(SettingsTab))
        {
            BLREditSettings.Settings.LastPlayerName = BLREditSettings.Settings.PlayerName;
        }
    }

    public void SetProfileSettings()
    {
        foreach (var item in ((TabControl)ProfileSettingsTab.Content).Items)
        {
            if (item is FrameworkElement element)
            {
                element.DataContext = ExportSystem.GetOrAddProfileSettings(BLREditSettings.Settings.PlayerName);
            }
        }
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        BringWindowIntoBounds();
        if (BLREditSettings.Settings.PlayerName == "BLREdit-Player")
        {
            SettingsTab.IsSelected= true;
            MessageBox.Show(Properties.Resources.msg_ChangePlayerName);
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        UIKeys.SetAll(false);
    }
}
