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

namespace BLREdit.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public static readonly BLRClientWindow ClientWindow = new();

    /// <summary>
    /// Contains the last selected Border for setting the ItemList
    /// </summary>
    public static Border LastSelectedBorder { get { return lastSelectedBorder; } private set { if (lastSelectedBorder is not null) { SetBorderColor(lastSelectedBorder, Color.FromArgb(14, 158, 158, 158)); } lastSelectedBorder = value; if (lastSelectedBorder is not null) { SetBorderColor(lastSelectedBorder, Color.FromArgb(255, 255, 136, 0)); } } }
    private static Border lastSelectedBorder = null;
    /// <summary>
    /// Contains the weapon to filter out Items From the ItemList
    /// </summary>
    public BLRItem FilterWeapon = null;

    /// <summary>
    /// Contains the current active loadout
    /// </summary>
    public static MagiCowsProfile ActiveProfile { get { return activeProfile; } set { activeProfile = value; Profile.LoadMagicCowsProfile(value); } }
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

    public static MainWindow Self { get; private set; } = null;

    private int columns = 4;
    public int Columns { get { return columns; } set { columns = value; OnPropertyChanged(); } }

    //public static BLREditSettings Settings { get { return BLREditSettings.Settings; } }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool IsSearch(BLRItem item)
    {
        string searchText = SearchFilter.Trim().ToLower();
        string itemName = item.Name.ToLower();
        if (string.IsNullOrEmpty(searchText)) { return true; }
        return itemName.Contains(searchText);
    }

    private string searchFilter = "";
    public string SearchFilter { get { return searchFilter; } set { if (value != searchFilter) { searchFilter = value; ApplySearchAndFilter(); OnPropertyChanged(); } } }

    //TODO Add Item Search
    public MainWindow()
    {
        Self = this;
        IsPlayerProfileChanging = true;
        IsPlayerNameChanging = true;

        InitializeComponent();

        ItemList.Items.IsLiveFiltering = true;
        ItemList.Items.LiveFilteringProperties.Add("SearchFilter");

        ChangeSortingDirection(this, null);
        ChangeSortingDirection(this, null);

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

        LastSelectedBorder = ((WeaponControl)((Grid)((TabItem)((TabControl)((Grid)((LoadoutControl)((TabItem)LoadoutTabs.Items[0]).Content).Content).Children[0]).Items[0]).Content).Children[0]).Reciever;
        FilterWeapon = Profile.Loadout1.Primary.Reciever;
        SetItemList(ImportSystem.PRIMARY_CATEGORY);

        this.DataContext = Profile;
    }

    public void ApplySearchAndFilter()
    {
        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ItemList.ItemsSource);
        if (view != null)
        {
            view.Refresh();
        }
    }

    private static void SetBorderColor(Border border, Color color)
    {
        if (border is not null)
        {
            border.BorderBrush = new SolidColorBrush(color);
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.Title = $"{App.CurrentRepo} - {App.CurrentVersion}";
        SetItemList(ImportSystem.PRIMARY_CATEGORY);
        if (App.IsNewVersionAvailable && BLREditSettings.Settings.ShowUpdateNotice)
        {
            System.Diagnostics.Process.Start($"https://github.com/{App.CurrentOwner}/{App.CurrentRepo}/releases");
        }
        if (BLREditSettings.Settings.DoRuntimeCheck || BLREditSettings.Settings.ForceRuntimeCheck)
        {
            if (App.IsBaseRuntimeMissing || App.IsUpdateRuntimeMissing || BLREditSettings.Settings.ForceRuntimeCheck)
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

        GameClientList.ItemsSource = null;
        GameClientList.Items.Clear();
        GameClientList.ItemsSource = GameClients;

        ServerListView.ItemsSource = null;
        ServerListView.Items.Clear();
        ServerListView.ItemsSource = ServerList;

        IOResources.GetGameLocationsFromSteam();
        foreach (string folder in IOResources.GameFolders)
        {
            var GameInstance = $"{folder}{IOResources.GAME_DEFAULT_EXE}";
            if (File.Exists(GameInstance))
            {
                LoggingSystem.Log($"Adding Steam Client: {GameInstance}");
                AddGameClient(new BLRClient() { OriginalPath = GameInstance });
            }
        }

        AddDefaultServers();

        RefreshPing();

        if (BLREditSettings.Settings.DefaultServer is null)
        {
            BLREditSettings.Settings.DefaultServer = ServerList[0];
        }

        CheckGameClients();
        BLREditSettings.SyncDefaultClient();

        Profile.Loadout1.IsFemale = Profile.Loadout1.IsFemale;
    }

    private static void CheckGameClients()
    {
        LoggingSystem.Log("Checking for patched clients");
        if (GameClients.Count <= 0)
        {
            MessageBox.Show("You have to locate and add atleast one Client");
        }
        else
        {
            bool isClientStillExistent = false;
            BLRClient patchedClient = null;
            foreach (BLRClient client in GameClients)
            {
                if (client.Equals(BLREditSettings.Settings.DefaultClient))
                {
                    isClientStillExistent = true;
                    client.CurrentClient.SetBool(true);
                }
                else
                {
                    client.CurrentClient.SetBool(false);
                    if (patchedClient is null && client.Patched.Is) patchedClient = client;
                }
            }

            if (!isClientStillExistent)
            {
                if (patchedClient is null)
                {
                    BLREditSettings.Settings.DefaultClient = null;
                    GameClients[0].PatchClient();
                    BLREditSettings.Settings.DefaultClient = GameClients[0];
                    GameClients[0].CurrentClient.SetBool(true);
                }
                else
                {
                    BLREditSettings.Settings.DefaultClient = patchedClient;
                    patchedClient.CurrentClient.SetBool(true);
                }
            }
        }
    }

    public static void RefreshPing()
    {
        foreach (BLRServer server in ServerList)
        {
            server.PingServer();
        }
    }

    private void AddDefaultServers()
    {
        List<BLRServer> defaultServers = new() {
        new() { ServerAddress = "mooserver.ddns.net", Port = 7777, ServerName = "MagiCow's Server" }, //mooserver.ddns.net : 7777 //majikau.ddns.net or mooserver.ddns.net
        new() { ServerAddress = "blr.akoot.me", Port = 7777, ServerName = "Akoot's Server" }, //blr.akoot.me : 7777
        new() { ServerAddress = "blr.753z.net", Port = 7777, ServerName = "IKE753Z's Server" }, //blr.753z.net : 7777
        new() { ServerAddress = "localhost", Port = 7777, ServerName = "Local Host"}
        //BLRServer subsonic = null;
        //new() { ServerAddress = "dozette.tplinkdns.com", Port = 7777, ServerName = "Dozette's Server" }, //dozette.tplinkdns.com : 7777
        };

        foreach (BLRServer defaultServer in defaultServers)
        {
            bool add = true;
            foreach (BLRServer server in ServerList)
            {
                if (server.ServerAddress == defaultServer.ServerAddress && server.Port == defaultServer.Port)
                {
                    add = false;
                }
            }
            if (add)
            {
                ServerList.Add(defaultServer);
            }
        }
        ServerListView.ItemsSource = null;
        ServerListView.ItemsSource = ServerList;
    }

    public void AddServer(BLRServer server, bool forceAdd = false)
    {
        bool add = true;
        foreach (BLRServer s in ServerList)
        {
            if (!forceAdd && s.ServerAddress == server.ServerAddress && s.Port == server.Port)
            {
                add = false;
            }
        }
        if (add)
        {
            ServerList.Add(server);
            ServerListView.ItemsSource = null;
            ServerListView.ItemsSource = ServerList;
        }
    }

    public void AddGameClient(BLRClient client)
    {
        bool add = true;
        foreach (BLRClient c in GameClients)
        {
            if (c.OriginalPath == client.OriginalPath)
            {
                add = false;
            }
        }
        if (add)
        {
            GameClients.Add(client);
            GameClientList.ItemsSource = null;
            GameClientList.ItemsSource = GameClients;
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
        LanguageSet.Save();
        ProxyModule.Save();
        ClientWindow.ForceClose();
    }

    private void Border_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Serializable))
        { e.Effects = DragDropEffects.Copy; }
        else
        { e.Effects = DragDropEffects.None; }
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
                if (mage.Parent is Border order)
                {
                    border = order;
                }
            }
            else if (e.OriginalSource is Border order)
            {
                border = order;
                image = (Image)order.Child;
            }

            if (image is null || border is null) { LoggingSystem.Log("Image or Border is null"); }

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
            if (mage.Parent is Border order)
            {
                border = order;
            }
        }
        else if (e.OriginalSource is Border order)
        {
            border = order;
            if (order.Child is Image mage2) { image = mage2; }
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
            if (weapon is not null) FilterWeapon = weapon.Reciever;
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
                    if (image.GetBindingExpression(Image.SourceProperty).ResolvedSourcePropertyName == nameof(weapon.Scope.LargeSquareImage))
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

    private string CurrentListType;
    public void SetItemList(string Type)
    {
        CurrentListType = Type;
        var list = ImportSystem.GetItemListOfType(Type);
        if (list.Count > 0)
        {
            int index = SortComboBox1.SelectedIndex;
            SortComboBox1.ItemsSource = null;
            SortComboBox1.Items.Clear();

            switch (list[0].Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                    Columns = 2;
                    SetSortingType(typeof(ImportHelmetSortingType));
                    break;

                case ImportSystem.UPPER_BODIES_CATEGORY:
                case ImportSystem.LOWER_BODIES_CATEGORY:
                    Columns = 3;
                    SetSortingType(typeof(ImportArmorSortingType));
                    break;

                case ImportSystem.ATTACHMENTS_CATEGORY:
                    Columns = 5;
                    SetSortingType(typeof(ImportGearSortingType));
                    break;

                case ImportSystem.SCOPES_CATEGORY:
                    Columns = 3;
                    SetSortingType(typeof(ImportScopeSortingType));
                    break;

                case ImportSystem.AVATARS_CATEGORY:
                case ImportSystem.CAMOS_BODIES_CATEGORY:
                case ImportSystem.CAMOS_WEAPONS_CATEGORY:
                case ImportSystem.HANGERS_CATEGORY:
                case ImportSystem.BADGES_CATEGORY:
                    Columns = 5;
                    SetSortingType(typeof(ImportNoStatsSortingType));
                    break;

                case ImportSystem.TACTICAL_CATEGORY:
                    Columns = 2;
                    SetSortingType(typeof(ImportNoStatsSortingType));
                    break;

                case ImportSystem.GRIPS_CATEGORY:
                    Columns = 4;
                    SetSortingType(typeof(ImportGripSortingType));
                    break;

                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    Columns = 4;
                    SetSortingType(typeof(ImportWeaponSortingType));
                    break;
                case ImportSystem.AMMO_CATEGORY:
                    Columns = 5;
                    SetSortingType(typeof(ImportModificationSortingType));
                    break;

                case ImportSystem.SHOP_CATEGORY:
                case ImportSystem.EMOTES_CATEGORY:
                    Columns = 5;
                    SetSortingType(typeof(ImportModificationSortingType));
                    break;

                default:
                    Columns = 4;
                    SetSortingType(typeof(ImportModificationSortingType));
                    break;
            }

            if (index > SortComboBox1.Items.Count)
            {
                index = SortComboBox1.Items.Count - 1;
            }
            if (index < 0)
            {
                index = 0;
            }
            SortComboBox1.SelectedIndex = index;

            ItemList.ItemsSource = list;
            ApplySorting();
        }
        LoggingSystem.Log($"ItemList Set for {Type}");
        if (!ItemListTab.IsFocused) ItemListTab.Focus();
    }

    public void ApplySorting()
    {
        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ItemList.ItemsSource);
        if (view != null)
        {
            //Clear old sorting descriptions
            view.SortDescriptions.Clear();

            if (SortComboBox1.Items.Count > 0 && SortComboBox1.SelectedItem != null)
            {
                CurrentSortingPropertyName = Enum.GetName(CurrentSortingEnumType, Enum.GetValues(CurrentSortingEnumType).GetValue(SortComboBox1.SelectedIndex));
                view.SortDescriptions.Add(new SortDescription(CurrentSortingPropertyName, SortDirection));
            }
        }
    }

    private void SetSortingType(Type SortingEnumType)
    {
        CurrentSortingEnumType = SortingEnumType;
        SortComboBox1.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = LanguageSet.GetWords(SortingEnumType) });
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
        LoggingSystem.Log("Changing Profile");
        if (!IsPlayerNameChanging && !UndoRedoSystem.BlockEvent)
        {
            IsPlayerProfileChanging = true;
            if (ProfileComboBox.SelectedValue is ExportSystemProfile profile)
            {
                UndoRedoSystem.CreateAction(e.RemovedItems[0], ProfileComboBox.SelectedValue, ProfileComboBox.GetType().GetProperty(nameof(ProfileComboBox.SelectedValue)), ProfileComboBox, true);
                UndoRedoSystem.DoAction(profile, typeof(ExportSystem).GetProperty(nameof(ExportSystem.ActiveProfile)), null);
                UndoRedoSystem.DoAction(profile.PlayerName, PlayerNameTextBox.GetType().GetProperty(nameof(PlayerNameTextBox.Text)), PlayerNameTextBox);
                UndoRedoSystem.EndAction();
            }
            IsPlayerProfileChanging = false;
        }
    }
    private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsPlayerProfileChanging && !UndoRedoSystem.BlockEvent)
        {
            IsPlayerNameChanging = true;

            int index = ProfileComboBox.SelectedIndex;
            ProfileComboBox.ItemsSource = null;
            ExportSystem.RemoveActiveProfileFromDisk();
            ExportSystem.ActiveProfile.PlayerName = PlayerNameTextBox.Text;
            ProfileComboBox.ItemsSource = ExportSystem.Profiles;
            ProfileComboBox.SelectedIndex = index;

            IsPlayerNameChanging = false;
        }
    }
    private void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ExportSystem.AddProfile();
    }

    private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        var directory = $"{BLREditSettings.Settings.DefaultClient.ConfigFolder}\\profiles\\";
        Directory.CreateDirectory(directory);
        IOResources.SerializeFile<SELoadout[]>($"{directory}{ExportSystem.ActiveProfile.PlayerName}.json", new[] { new SELoadout(Profile.Loadout1), new SELoadout(Profile.Loadout2), new SELoadout(Profile.Loadout3) });
        ExportSystem.CopyToClipBoard(ExportSystem.ActiveProfile);
        var grid = CreateAlertGrid($"{ExportSystem.ActiveProfile.Name} got Copied to Clipboard");
        AlertList.Items.Add(grid);
        new TripleAnimationDouble(0, 400, 1, 3, 1, grid, Grid.WidthProperty, AlertList.Items).Begin(AlertList);
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
            SortDirectionButton.Content = LanguageSet.GetWord("Descending", "Descending");
        }
        else
        {
            SortDirection = ListSortDirection.Ascending;
            SortDirectionButton.Content = LanguageSet.GetWord("Ascending", "Ascending");
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

    #region GameClient UI
    private void OpenNewGameClient_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = "Game Client|*.exe",
            InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}",
            Multiselect = false
        };
        dialog.ShowDialog();
        if (!string.IsNullOrEmpty(dialog.FileName))
        {
            AddGameClient(new BLRClient() { OriginalPath = dialog.FileName });
        }
        CheckGameClients();
    }
    #endregion GameClient UI


    #region Server UI
    private void AddNewServer_Click(object sender, RoutedEventArgs e)
    {
        AddServer(new BLRServer(), true);
    }

    private void RemoveServer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            ServerList.Remove(button.DataContext as BLRServer);

            ServerListView.ItemsSource = null;
            ServerListView.ItemsSource = ServerList;
        }
    }

    private void PingServers_Click(object sender, RoutedEventArgs e)
    {
        foreach (BLRServer server in ServerList)
        {
            server.PingServer();
        }
    }
    #endregion Server UI


    bool shiftDown = false;
    bool altDown = false;
    bool ctrlDown = false;

    /// <summary>
    /// Modifier Key Check
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PreviewKeyDownMainWindow(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.LeftShift:
            case Key.RightShift:
                shiftDown = true;
                break;
            case Key.LeftAlt:
            case Key.RightAlt:
                altDown = true;
                break;
            case Key.LeftCtrl:
            case Key.RightCtrl:
                ctrlDown = true;
                break;
        }
    }


    private int buttonIndex = 0;
    private void PreviewKeyUpMainWindow(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.LeftShift:
            case Key.RightShift:
                shiftDown = false;
                break;
            case Key.LeftAlt:
            case Key.RightAlt:
                altDown = false;
                break;
            case Key.LeftCtrl:
            case Key.RightCtrl:
                ctrlDown = false;
                break;
            case Key.Tab:
                if (shiftDown)
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
                if (shiftDown)
                {
                    BLREditSettings.Settings.AdvancedModding.SetBool(!BLREditSettings.Settings.AdvancedModding.Is);
                    BLREditSettings.Save();
                    var grid = CreateAlertGrid($"AdvancedModding:{BLREditSettings.Settings.AdvancedModding.Is}");
                    AlertList.Items.Add(grid);
                    new TripleAnimationDouble(0, 400, 1, 3, 1, grid, Grid.WidthProperty, AlertList.Items).Begin(AlertList);
                    LoggingSystem.Log($"AdvancedModding:{BLREditSettings.Settings.AdvancedModding.Is}");
                }
                break;
            case Key.Z:
                if (ctrlDown) UndoRedoSystem.Undo();
                break;
            case Key.Y:
                if (ctrlDown) UndoRedoSystem.Redo();
                break;
        }
    }

    private Grid CreateAlertGrid(string Alert)
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

    private void LoadoutTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BLRLoadout loadoutOld = null;
        BLRLoadout loadoutNew = null;
        if (e.RemovedItems.Count > 0 && ((TabItem)e.RemovedItems[0]).Content is LoadoutControl loadoutRemoved)
        {
            loadoutOld = (BLRLoadout)loadoutRemoved.DataContext;
        }
        if (e.AddedItems.Count > 0 && ((TabItem)e.AddedItems[0]).Content is LoadoutControl loadoutAdded)
        {
            loadoutNew = (BLRLoadout)loadoutAdded.DataContext;
        }

        if (loadoutOld is not null)
        {
            if (loadoutOld.IsFemale == loadoutNew.IsFemale)
            { return; }
        }
        if (loadoutNew is not null)
        { loadoutNew.IsFemale = loadoutNew.IsFemale; }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchFilter = SearchBox.Text;
    }
}
