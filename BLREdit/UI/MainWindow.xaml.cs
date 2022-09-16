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
using BLREdit.API.ImportSystem;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using BLREdit.Game.Proxy;

namespace BLREdit.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private static readonly Random rng = new();

    /// <summary>
    /// Contains the last selected Image for setting the ItemList
    /// </summary>
    public static Image LastSelectedImage { get { return lastSelectedImage; } private set { if (lastSelectedImage is not null) { SetBorderColor(lastSelectedImage.Parent as Border, Color.FromArgb(14, 158, 158, 158)); } lastSelectedImage = value; if (lastSelectedImage is not null) { SetBorderColor(lastSelectedImage.Parent as Border, Color.FromArgb(255, 255, 136, 0)); } } }
    private static Image lastSelectedImage = null;
    /// <summary>
    /// Contains the weapon to filter out Items From the ItemList
    /// </summary>
    private BLRItem FilterWeapon = null;

    /// <summary>
    /// Contains the current active loadout
    /// </summary>
    public static MagiCowsLoadout ActiveLoadout { get { return activeLoadout; } set { activeLoadout = value; Loadout.LoadMagicCowsLoadout(value); } }
    private static MagiCowsLoadout activeLoadout = null;

    public static BLRLoadoutSetup Loadout { get; } = new();
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

    public static List<BLRClient> GameClients { get; set; }
    public static List<BLRServer> ServerList { get; set; }

    public static MainWindow Self { get; private set; } = null;

    private int columns = 4;
    public int Columns { get { return columns; } set { columns = value; OnPropertyChanged(); } }

    public BLREditSettings Settings { get { return BLREditSettings.Settings; } }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindow()
    {
        Self = this;
        IsPlayerProfileChanging = true;
        IsPlayerNameChanging = true;

        InitializeComponent();

        ChangeSortingDirection(this, null);
        ChangeSortingDirection(this, null);

        ItemList.Items.Filter += new Predicate<object>(o =>
        {
            if (o is BLRItem item)
            {
                if (FilterWeapon is null && (item.Category == ImportSystem.PRIMARY_CATEGORY || item.Category == ImportSystem.SECONDARY_CATEGORY)) { return true; }
                if (BLREditSettings.Settings.AdvancedModding.Is)
                {
                    return AdvancedFilter(item, FilterWeapon);
                }
                else
                {
                    return item.IsValidFor(FilterWeapon);
                }
            }
            return false;
        });

        PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;
        ProfileComboBox.ItemsSource = ExportSystem.Profiles;
        ProfileComboBox.SelectedIndex = 0;

        UndoRedoSystem.BlockUpdate = true;
        UndoRedoSystem.BlockEvent = true;
        ActiveLoadout = ExportSystem.ActiveProfile.Loadout1;
        UndoRedoSystem.BlockUpdate = false;
        UndoRedoSystem.BlockEvent = false;

        EnabledLoadout = 1;

        IsPlayerProfileChanging = false;
        IsPlayerNameChanging = false;

        LastSelectedImage = PrimaryRecieverImage;
        FilterWeapon = Loadout.Primary.Reciever;
        SetItemList(ImportSystem.PRIMARY_CATEGORY);

        LoadoutGrid.DataContext = Loadout;
        AdvancedInfo.DataContext = Loadout;
    }

    private static void SetBorderColor(Border border, Color color)
    {
        if (border is not null)
        {
            border.BorderBrush = new SolidColorBrush(color);
        }
    }

    private static bool AdvancedFilter(BLRItem item, BLRItem filter)
    {
        switch (item.Category)
        {
            case ImportSystem.MAGAZINES_CATEGORY:
                if (item.Name.Contains("Standard") || (item.Name.Contains("Light") && !item.Name.Contains("Arrow")) || item.Name.Contains("Quick") || item.Name.Contains("Extended") || item.Name.Contains("Express") || item.Name.Contains("Quick") || item.Name.Contains("Electro") || item.Name.Contains("Explosive") || item.Name.Contains("Incendiary") || item.Name.Contains("Toxic") || item.Name.Contains("Magnum"))
                {
                    return item.IsValidFor(filter);
                }
                return true;

            default: return true;  
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {

        SetItemList(ImportSystem.PRIMARY_CATEGORY);
        if (App.IsNewVersionAvailable && BLREditSettings.Settings.ShowUpdateNotice)
        {
            System.Diagnostics.Process.Start($"https://github.com/{App.CurrentOwner}/{App.CurrentRepo}/releases");
        }
        if (BLREditSettings.Settings.DoRuntimeCheck || BLREditSettings.Settings.ForceRuntimeCheck)
        {
            if(App.IsBaseRuntimeMissing || App.IsUpdateRuntimeMissing || BLREditSettings.Settings.ForceRuntimeCheck)
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
            AddGameClient(new BLRClient() { OriginalPath = $"{folder}{IOResources.GAME_DEFAULT_EXE}" });
        }

        AddDefaultServers();

        RefreshPing();

        if (BLREditSettings.Settings.DefaultServer is null)
        {
            BLREditSettings.Settings.DefaultServer = ServerList[0];
        }

        CheckGameClients();
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

    public void RefreshPing()
    {
        foreach (BLRServer server in ServerList)
        {
            server.PingServer();
        }
    }

    private void AddDefaultServers()
    {
        List<BLRServer> defaultServers = new() {
        new() { ServerAddress = "majikau.ddns.net", Port = 7777, ServerName = "MagiCow's Server" }, //mooserver.ddns.net : 7777 //majikau.ddns.net
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

    private void ChangeCurrentGameClient_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is BLRClient client)
            {
                LoggingSystem.Log($"Setting Current Client:{client}");
                if (client.Patched.Is)
                {
                    BLREditSettings.Settings.DefaultClient = client;
                    foreach (BLRClient c in GameClients)
                    {
                        c.CurrentClient.SetBool(false);
                    }
                    client.CurrentClient.SetBool(true);
                }
            }
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
        IOResources.SerializeFile("GameClients.json", GameClients);
        IOResources.SerializeFile("ServerList.json", ServerList);
        BLREditSettings.Save();
        LanguageSet.Save();
        ProxyModule.Save();
    }

    private void ItemList_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if(element.DataContext is BLRItem item)
            {
                if (e.ClickCount >= 2)
                {
                    LoggingSystem.Log($"Double Clicking:{item.Name}");
                    SetItemToImage(LastSelectedImage, item);
                }
                else
                {
                    LoggingSystem.Log($"Dragging:{item.Name}");
                    DragDrop.DoDragDrop(element, item, DragDropEffects.Copy);
                }
            }
        }
    }

    private void Image_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Serializable))
        { e.Effects = DragDropEffects.Copy; }
        else
        { e.Effects = DragDropEffects.None; }
    }

    private void Image_Drop(object sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            if (e.Data.GetData(typeof(BLRItem)) is BLRItem item)
            {
                LoggingSystem.Log($"Recieving:{item.Name}");
                if (border.Child is Grid grid)
                {
                    if (grid.Children[0] is Image img)
                    {
                        SetItemToImage(img, item);
                    }
                }
                if (border.Child is Image image)
                {
                    SetItemToImage(image, item);
                }
            }
        }
        
    }

    private static bool CheckForPistolAndBarrel(BLRItem item)
    {
        return item.Name == "Light Pistol" || item.Name == "Heavy Pistol" || item.Name == "Prestige Light Pistol";
    }

    private void UpdatePrimaryImages(Image image)
    {
        FilterWeapon = PrimaryRecieverImage.Tag as BLRItem;

        if (image.Name.Contains("Reciever"))
        {
            SetItemList(ImportSystem.PRIMARY_CATEGORY);
            LastSelectedImage = PrimaryRecieverImage;
            return;
        }

        if (image.Name.Contains("Muzzle"))
        { 
            SetItemList(ImportSystem.MUZZELS_CATEGORY);
            LastSelectedImage = image;
            return;
        }
        if (image.Name.Contains("Grip"))
        {
            SetItemList(ImportSystem.GRIPS_CATEGORY);
            LastSelectedImage = image;
            return;
        }
        if (image.Name.Contains("Crosshair"))
        {
            var item = (PrimaryScopeImage.Tag as BLRItem);
            item.LoadCrosshair(true);
            ItemList.ItemsSource = new BLRItem[] { item };
            LastSelectedImage = PrimaryScopeImage;
            return;
        }
        UpdateImages(image);
    }

    private void UpdateSecondaryImages(Image image)
    {
        FilterWeapon = SecondaryRecieverImage.Tag as BLRItem;
        
        if (image.Name.Contains("Reciever"))
        {
            SetItemList(ImportSystem.SECONDARY_CATEGORY);
            LastSelectedImage = SecondaryRecieverImage;
            return;
        }
        if (image.Name.Contains("Muzzle"))
        { 
            SetItemList(ImportSystem.MUZZELS_CATEGORY);
            LastSelectedImage = image;
            return;
        }
        if (image.Name.Contains("Grip"))
        {
            SetItemList(ImportSystem.GRIPS_CATEGORY);
            LastSelectedImage = image;
            return;
        }
        if (image.Name.Contains("Crosshair"))
        {
            var item = (SecondaryScopeImage.Tag as BLRItem);
            if (item != null)
            {
                item.LoadCrosshair(false);
                ItemList.ItemsSource = new BLRItem[] { item };
                LastSelectedImage = SecondaryScopeImage;
            }
            return;
        }
        UpdateImages(image);
    }

    private void UpdateImages(Image image)
    {
        LastSelectedImage = image;
        
        if (image.Name.Contains("Reciever"))
        {
            SetItemList(ImportSystem.PRIMARY_CATEGORY);
            
            return;
        }
        if (image.Name.Contains("Magazine"))
        {
            SetItemList(ImportSystem.MAGAZINES_CATEGORY);
            return;
        }
        if (image.Name.Contains("Stock"))
        {
            SetItemList(ImportSystem.STOCKS_CATEGORY);
            return;
        }
        if (image.Name.Contains("Scope"))
        {
            foreach (var item in ImportSystem.GetItemListOfType(ImportSystem.SCOPES_CATEGORY))
            {
                item.RemoveCrosshair();
            }
            SetItemList(ImportSystem.SCOPES_CATEGORY);
            return;
        }
        if (image.Name.Contains("Barrel"))
        {
            SetItemList(ImportSystem.BARRELS_CATEGORY);
            return;
        }
        if (image.Name.Contains("Tag"))
        {
            SetItemList(ImportSystem.HANGERS_CATEGORY);
            return;
        }
        if (image.Name.Contains("CamoWeapon"))
        {
            SetItemList(ImportSystem.CAMOS_WEAPONS_CATEGORY);
            return;
        }
    }

    public void SetItemList(string Type)
    {
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
                    Columns = 4;
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
                    Columns = 4;
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
        if(!ItemListTab.IsFocused) ItemListTab.Focus();
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
        SortComboBox1.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = API.ImportSystem.LanguageSet.GetWords(SortingEnumType) });
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            if (sender is Border border)
            {
                if (border.Child is Grid grid)
                {
                    if (grid.Children[0] is Image img)
                    {
                        if (img.Name.Contains("Gear"))
                        {
                            SetItemList(ImportSystem.ATTACHMENTS_CATEGORY);
                            LastSelectedImage = img;
                            return;
                        }
                    }
                }
                if (border.Child is Image image)
                {
                    ItemList.ItemsSource = null;
                    if (image.Name.Contains("Primary"))
                    {
                        UpdatePrimaryImages(image);
                        return;
                    }
                    if (image.Name.Contains("Secondary"))
                    {
                        UpdateSecondaryImages(image);
                        return;
                    }
                    if (image.Name.Contains("Tactical"))
                    {
                        SetItemList(ImportSystem.TACTICAL_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }
                    if (image.Name.Contains("CamoBody"))
                    {
                        SetItemList(ImportSystem.CAMOS_BODIES_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }
                    if (image.Name.Contains("Helmet"))
                    {
                        SetItemList(ImportSystem.HELMETS_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }
                    if (image.Name.Contains("UpperBody"))
                    {
                        SetItemList(ImportSystem.UPPER_BODIES_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }

                    if (image.Name.Contains("Avatar"))
                    {
                        SetItemList(ImportSystem.AVATARS_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }

                    if (image.Name.Contains("LowerBody"))
                    {
                        SetItemList(ImportSystem.LOWER_BODIES_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }

                    if (image.Name.Contains("Trophy"))
                    {
                        SetItemList(ImportSystem.BADGES_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }

                    if (image.Name.Contains("Gear"))
                    {
                        SetItemList(ImportSystem.ATTACHMENTS_CATEGORY);
                        LastSelectedImage = image;
                        return;
                    }
                    LoggingSystem.Log("ItemList Didn't get set");
                }
            }
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            if (sender is Border border)
            {
                if (border.Child is Grid grid)
                {
                    if (grid.Children[0] is Image img)
                    {
                        RemoveItemFromImage(img);
                    }
                }
                if (border.Child is Image image)
                {
                    RemoveItemFromImage(image);
                }
            }
        }
    }

    private void RemoveItemFromImage(Image image)
    {
        SetItemToImage(image, null, true, false);
    }

    public void SetItemToImage(Image image, BLRItem item, bool blockEvent = false, bool blockUpdate = false)
    {
        switch (image.Name)
        {
            #region Weapons
            #region Primary
            case nameof(PrimaryRecieverImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Reciever)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryBarrelImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Barrel)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryMuzzleImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Muzzle)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryMagazineImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Magazine)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryStockImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Stock)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryScopeImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Scope)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryGripImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Grip)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryTagImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Tag)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PrimaryCamoWeaponImage):
                UndoRedoSystem.DoAction(item, Loadout.Primary.GetType().GetProperty(nameof(Loadout.Primary.Camo)), Loadout.Primary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            #endregion Primary

            #region Secondary
            case nameof(SecondaryRecieverImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Reciever)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryBarrelImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Barrel)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryMuzzleImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Muzzle)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryMagazineImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Magazine)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryStockImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Stock)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryScopeImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Scope)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryGripImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Grip)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryTagImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Tag)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(SecondaryCamoWeaponImage):
                UndoRedoSystem.DoAction(item, Loadout.Secondary.GetType().GetProperty(nameof(Loadout.Secondary.Camo)), Loadout.Secondary, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            #endregion Seconday
            #endregion Weapons

            case nameof(HelmetImage):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Helmet)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(UpperBodyImage):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.UpperBody)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(LowerBodyImage):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.LowerBody)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(TacticalImage):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Tactical)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(GearImage1):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Gear1)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(GearImage2):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Gear2)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(GearImage3):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Gear3)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(GearImage4):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Gear4)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(PlayerCamoBodyImage):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Camo)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(AvatarImage):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Avatar)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
            case nameof(TrophyImage):
                UndoRedoSystem.DoAction(item, Loadout.GetType().GetProperty(nameof(Loadout.Trophy)), Loadout, blockEvent, blockUpdate);
                UndoRedoSystem.EndAction();
                break;
        }
    }

    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsPlayerNameChanging && !UndoRedoSystem.BlockEvent)
        {
            IsPlayerProfileChanging = true;
            if (ProfileComboBox.SelectedValue is ExportSystemProfile profile)
            {
                UndoRedoSystem.CreateAction(e.RemovedItems[0], ProfileComboBox.SelectedValue, ProfileComboBox.GetType().GetProperty(nameof(ProfileComboBox.SelectedValue)), ProfileComboBox, true);
                UndoRedoSystem.DoAction(profile, typeof(ExportSystem).GetProperty(nameof(ExportSystem.ActiveProfile)), null);
                UndoRedoSystem.DoAction(profile.PlayerName, PlayerNameTextBox.GetType().GetProperty(nameof(PlayerNameTextBox.Text)), PlayerNameTextBox);
                UndoRedoSystem.DoAction(profile.GetLoadout(EnabledLoadout), typeof(MainWindow).GetProperty(nameof(ActiveLoadout)), null, true, true);
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
        this.ProfileComboBox.DataContext = null;
        this.ProfileComboBox.DataContext = ExportSystem.Profiles;
    }

    public void EnableLoadoutButton()
    {
        Loadout1Button.IsEnabled = true;
        Loadout2Button.IsEnabled = true;
        Loadout3Button.IsEnabled = true;
        switch (enabledLoadout)
        {
            case 1:
                Loadout1Button.IsEnabled = false;
                break;
            case 2:
                Loadout2Button.IsEnabled = false;
                break;
            case 3:
                Loadout3Button.IsEnabled = false;
                break;
            default: 
                Loadout1Button.IsEnabled = false;
                break;
        }
    }

    private int enabledLoadout = 1;
    public int EnabledLoadout { get { return enabledLoadout; } set { enabledLoadout = value; EnableLoadoutButton(); } }

    private void Loadout1Button_Click(object sender, RoutedEventArgs e)
    {
        UndoRedoSystem.DoAction(1, typeof(MainWindow).GetProperty(nameof(EnabledLoadout)), Self);
        UndoRedoSystem.DoAction(ExportSystem.ActiveProfile.Loadout1, typeof(MainWindow).GetProperty(nameof(ActiveLoadout)), null, true, true);
        UndoRedoSystem.EndAction();
    }

    private void Loadout2Button_Click(object sender, RoutedEventArgs e)
    {
        UndoRedoSystem.DoAction(2, typeof(MainWindow).GetProperty(nameof(EnabledLoadout)), Self);
        UndoRedoSystem.DoAction(ExportSystem.ActiveProfile.Loadout2, typeof(MainWindow).GetProperty(nameof(ActiveLoadout)), null, true, true);
        UndoRedoSystem.EndAction();
    }

    private void Loadout3Button_Click(object sender, RoutedEventArgs e)
    {
        UndoRedoSystem.DoAction(3, typeof(MainWindow).GetProperty(nameof(EnabledLoadout)), Self);
        UndoRedoSystem.DoAction(ExportSystem.ActiveProfile.Loadout3, typeof(MainWindow).GetProperty(nameof(ActiveLoadout)), null, true, true);
        UndoRedoSystem.EndAction();
    }

    private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        ExportSystem.CopyToClipBoard(ExportSystem.ActiveProfile);
        var grid = CreateAlertGrid($"{ExportSystem.ActiveProfile.Name} got Copied to Clipboard");
        AlertList.Items.Add(grid);
        new TripleAnimationDouble(0, 400, 1, 3, 1, grid, Grid.WidthProperty, AlertList.Items).Begin(AlertList);
        //ExportSystem.CreateSEProfile(ExportSystem.ActiveProfile); //Not really in use currently only makes waste on the Hard Drive
    }

    private void IsFemaleCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (UndoRedoSystem.BlockEvent) return;
        UndoRedoSystem.CreateAction(false, true, Loadout.GetType().GetProperty(nameof(Loadout.IsFemale)), Loadout, true, false);
        UndoRedoSystem.EndAction();
    }

    private void IsFemaleCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (UndoRedoSystem.BlockEvent) return;
        UndoRedoSystem.CreateAction(true,false, Loadout.GetType().GetProperty(nameof(Loadout.IsFemale)), Loadout, true, false);
        UndoRedoSystem.EndAction();
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
        ExportSystemProfile rngProfile = ExportSystem.AddProfile("Random");

        rngProfile.Loadout1 = RandomizeLoadout();
        rngProfile.Loadout2 = RandomizeLoadout();
        rngProfile.Loadout3 = RandomizeLoadout();

        IsPlayerProfileChanging = true;
        IsPlayerNameChanging = true;

        ExportSystem.ActiveProfile = rngProfile;

        PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;

        ProfileComboBox.SelectedIndex = ExportSystem.Profiles.IndexOf(ExportSystem.ActiveProfile);

        ActiveLoadout = ExportSystem.ActiveProfile.Loadout1;

        Loadout1Button.IsEnabled = false;
        Loadout2Button.IsEnabled = true;
        Loadout3Button.IsEnabled = true;

        IsPlayerProfileChanging = false;
        IsPlayerNameChanging = false;


        LastSelectedImage = PrimaryRecieverImage;
    }


    static readonly List<BLRItem> Primaries = ImportSystem.GetItemListOfType(ImportSystem.PRIMARY_CATEGORY);
    static readonly List<BLRItem> Secondaries = ImportSystem.GetItemListOfType(ImportSystem.SECONDARY_CATEGORY);

    static readonly List<BLRItem> Barrels = ImportSystem.GetItemListOfType(ImportSystem.BARRELS_CATEGORY);
    static readonly List<BLRItem> Scopes = ImportSystem.GetItemListOfType(ImportSystem.SCOPES_CATEGORY);
    static readonly List<BLRItem> Magazines = ImportSystem.GetItemListOfType(ImportSystem.MAGAZINES_CATEGORY);
    static readonly List<BLRItem> Muzzles = ImportSystem.GetItemListOfType(ImportSystem.MUZZELS_CATEGORY);
    static readonly List<BLRItem> Stocks = ImportSystem.GetItemListOfType(ImportSystem.STOCKS_CATEGORY);
    static readonly List<BLRItem> Hangers = ImportSystem.GetItemListOfType(ImportSystem.HANGERS_CATEGORY);
    static readonly List<BLRItem> CamosWeapon = ImportSystem.GetItemListOfType(ImportSystem.CAMOS_WEAPONS_CATEGORY);
    static readonly List<BLRItem> Grips = ImportSystem.GetItemListOfType(ImportSystem.GRIPS_CATEGORY);


    static readonly List<BLRItem> Tacticals = ImportSystem.GetItemListOfType(ImportSystem.TACTICAL_CATEGORY);
    static readonly List<BLRItem> Attachments = ImportSystem.GetItemListOfType(ImportSystem.ATTACHMENTS_CATEGORY);
    static readonly List<BLRItem> CamosBody = ImportSystem.GetItemListOfType(ImportSystem.CAMOS_BODIES_CATEGORY);
    static readonly List<BLRItem> Helmets = ImportSystem.GetItemListOfType(ImportSystem.HELMETS_CATEGORY);
    static readonly List<BLRItem> UpperBodies = ImportSystem.GetItemListOfType(ImportSystem.UPPER_BODIES_CATEGORY);
    static readonly List<BLRItem> LowerBodies = ImportSystem.GetItemListOfType(ImportSystem.LOWER_BODIES_CATEGORY);
    static readonly List<BLRItem> Avatars = ImportSystem.GetItemListOfType(ImportSystem.AVATARS_CATEGORY);
    static readonly List<BLRItem> Badges = ImportSystem.GetItemListOfType(ImportSystem.BADGES_CATEGORY);

    private static MagiCowsLoadout RandomizeLoadout()
    {
        MagiCowsLoadout loadout = new()
        {
            Primary = RandomizeWeapon(Primaries[rng.Next(0, Primaries.Count)]),
            Secondary = RandomizeWeapon(Secondaries[rng.Next(0, Secondaries.Count)], true),
            Tactical = rng.Next(0, Tacticals.Count),

            Gear1 = rng.Next(0, Attachments.Count),
            Gear2 = rng.Next(0, Attachments.Count),
            Gear3 = rng.Next(0, Attachments.Count),
            Gear4 = rng.Next(0, Attachments.Count),

            Camo = rng.Next(0, CamosBody.Count),

            Helmet = rng.Next(0, Helmets.Count),
            UpperBody = rng.Next(0, UpperBodies.Count),
            LowerBody = rng.Next(0, LowerBodies.Count),
            Skin = rng.Next(0, Avatars.Count),
            Trophy = rng.Next(0, Badges.Count),

            IsFemale = NextBoolean()
        };


        double gearSlots = 0;
        gearSlots += UpperBodies[loadout.UpperBody].PawnModifiers.GearSlots;
        gearSlots += LowerBodies[loadout.LowerBody].PawnModifiers.GearSlots;

        if (gearSlots < 4)
        {
            loadout.Gear4 = 0;
        }
        if (gearSlots < 3)
        {
            loadout.Gear3 = 0;
        }
        if (gearSlots < 2)
        {
            loadout.Gear2 = 0;
        }
        if (gearSlots < 1)
        {
            loadout.Gear1 = 0;
        }


        return loadout;
    }

    public static bool NextBoolean()
    {
        return rng.Next() > (Int32.MaxValue / 2);
        // Next() returns an int in the range [0..Int32.MaxValue]
    }

    private static MagiCowsWeapon RandomizeWeapon(BLRItem Weapon, bool IsSecondary = false)
    {
        MagiCowsWeapon weapon = MagiCowsWeapon.GetDefaultSetupOfReciever(Weapon);

        if (weapon == null)
        {
            return new()
            {
                Receiver = Weapon.Name
            };
        }

        var FilteredBarrels = Barrels.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredScopes = Scopes.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredMagazines = Magazines.Where(o => o.IsValidFor(Weapon)).ToArray();
        //Dependant of Barrel on secondarioes
        var FilteredMuzzles = Muzzles.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredStocks = Stocks.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredCamos = CamosWeapon.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredHangers = Hangers.Where(o => o.IsValidFor(Weapon)).ToArray();

        if (FilteredBarrels.Length > 0)
        {
            weapon.Barrel = FilteredBarrels[rng.Next(0, FilteredBarrels.Length)].Name;
        }

        if (FilteredScopes.Length > 0)
        {
            weapon.Scope = FilteredScopes[rng.Next(0, FilteredScopes.Length)].Name;
        }

        if (FilteredMagazines.Length > 0)
        {
            weapon.Magazine = ImportSystem.GetIDOfItem(FilteredMagazines[rng.Next(0, FilteredMagazines.Length)]);
        }

        if (CheckForPistolAndBarrel(Weapon) && weapon.Barrel != "" && weapon.Barrel != MagiCowsWeapon.NoBarrel || !IsSecondary)
        {
            if (FilteredStocks.Length > 0)
            { 
                weapon.Stock = FilteredStocks[rng.Next(0, FilteredStocks.Length)].Name;
            }
        }

        if (FilteredMuzzles.Length > 0)
        {
            weapon.Muzzle = ImportSystem.GetIDOfItem(FilteredMuzzles[rng.Next(0, FilteredMuzzles.Length)]);
        }

        if (FilteredHangers.Length > 0)
        { 
            weapon.Tag = ImportSystem.GetIDOfItem(FilteredHangers[rng.Next(0, FilteredHangers.Length)]);
        }

        if (FilteredCamos.Length > 0 && Weapon.Tooltip != "Depot Item!")
        {
            weapon.Camo = ImportSystem.GetIDOfItem(FilteredCamos[rng.Next(0, FilteredCamos.Length)]);
        }
        else
        {
            weapon.Camo = -1;
        }

        if (Weapon.Name == "Shotgun")
        {
            weapon.Grip = Grips[rng.Next(0, Grips.Count)].Name;
        }
        else
        {
            weapon.Grip = "";
        }

        weapon.IsHealthOkAndRepair();

        return weapon;
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

    private void RemoveGameClient_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            GameClients.Remove(button.DataContext as BLRClient);

            GameClientList.ItemsSource = null;
            GameClientList.ItemsSource = GameClients;
        }
    }

    private void StartServer_Click(object sender, RoutedEventArgs e)
    {

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

    private void PortValidation(object sender, TextCompositionEventArgs e)
    {
        TextBox box = (sender as TextBox);
        string selectedText = box.SelectedText;
        bool ok = false;
        Regex regex = new("[^0-9]");
        if (regex.IsMatch(e.Text))
        {
            ok = true;
        }
        else
        {
            string text = box.Text.Remove(box.Text.IndexOf(selectedText), selectedText.Length);
            long l = long.Parse(text + e.Text);
            if (l > ushort.MaxValue)
            {
                ok = true;
                box.Text = ushort.MaxValue.ToString();
            }
            if (l < ushort.MinValue)
            {
                ok = true;
                box.Text = ushort.MinValue.ToString();
            }
        }
        e.Handled = ok;
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
                    new TripleAnimationDouble(0,400,1,3,1,grid,Grid.WidthProperty, AlertList.Items).Begin(AlertList);
                    LoggingSystem.Log($"AdvancedModding:{BLREditSettings.Settings.AdvancedModding.Is}");
                }
                break;
            case Key.Z:
                if(ctrlDown) UndoRedoSystem.Undo();
                break;
            case Key.Y:
                if (ctrlDown) UndoRedoSystem.Redo();
                break;
        }
    }

    private Grid CreateAlertGrid(string Alert)
    {
        TextBox alertText = new() { Text=Alert, TextAlignment=TextAlignment.Center, Foreground=new SolidColorBrush(Color.FromArgb(255,255,136,0)), IsReadOnly=true, FontSize=26};
        Grid alertGrid = new() { Background = new SolidColorBrush(Color.FromArgb(159,0,0,0)), HorizontalAlignment=HorizontalAlignment.Right, VerticalAlignment=VerticalAlignment.Center, Width=400 };
        alertGrid.Children.Add(alertText);
        return alertGrid;
    }

    private void DuplicateProfile_Click(object sender, RoutedEventArgs e)
    {
        var profile = ExportSystem.ActiveProfile.Clone();
        ExportSystem.Profiles.Add(profile);
    }

    readonly private static char[] InvalidNameChars = System.IO.Path.GetInvalidPathChars().Concat(System.IO.Path.GetInvalidFileNameChars()).ToArray();
    private void PlayerNameTextBox_PreviewInput(object sender, TextCompositionEventArgs e)
    {
        int index = e.Text.IndexOfAny(InvalidNameChars);
        if (index >= 0)
        { e.Handled = true; }
    }

    private void ClientModifyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var clientWindow = new BLRClientWindow(button.DataContext);
            clientWindow.ShowDialog();
        }
    }
}
