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

namespace BLREdit.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private static readonly Random rng = new();

    public static UILanguageWrapper Lang { get; } = new UILanguageWrapper();
    /// <summary>
    /// Contains the last selected Image for setting the ItemList
    /// </summary>
    public static Image LastSelectedImage { get { return lastSelectedImage; } private set { if (lastSelectedImage is not null) { SetBorderColor(lastSelectedImage.Parent as Border, Color.FromArgb(14, 158, 158, 158)); } lastSelectedImage = value; if (value is not null) { SetBorderColor(lastSelectedImage.Parent as Border, Color.FromArgb(255, 255, 136, 0)); } } }
    private static Image lastSelectedImage = null;
    /// <summary>
    /// Contains the weapon to filter out Items From the ItemList
    /// </summary>
    private BLRItem FilterWeapon = null;

    /// <summary>
    /// Contains the current active loadout
    /// </summary>
    public static MagiCowsLoadout ActiveLoadout { get; set; } = null;
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

    public List<BLRClient> GameClients { get; set; } = new();
    public List<BLRServer> ServerList { get; set; } = new();

    public static MainWindow Self { get; private set; } = null;

    private int buttonIndex = 0;
    private List<FrameworkElement> ItemButtons = new();

    private int columns = 4;
    public int Columns { get { return columns; } set { columns = value; OnPropertyChanged(); } }

    public BLREditSettings Settings { get; set; } = BLREditSettings.Settings;

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

        ItemButtons.Add(ItemListButton);
        ItemButtons.Add(AdvancedInfoButton);
        ItemButtons.Add(LauncherButton);

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

        SetLoadout(ExportSystem.ActiveProfile.Loadout1);

        Loadout1Button.IsEnabled = false;

        IsPlayerProfileChanging = false;
        IsPlayerNameChanging = false;

        LastSelectedImage = PrimaryRecieverImage;
        FilterWeapon = Loadout.Primary.Reciever;
        SetItemList(ImportSystem.PRIMARY_CATEGORY);

        LoadoutGrid.DataContext = Loadout;
        AdvancedInfo.DataContext = Loadout;
        MenuGrid.DataContext = this;
        LauncherMenu.DataContext = this;

        ItemListButton_Click(ItemListButton, new RoutedEventArgs());
        ItemListButton_Click(AdvancedInfoButton, new RoutedEventArgs());
        ItemListButton_Click(LauncherButton, new RoutedEventArgs());
        LauncherMenuButton_Click(ServerListButton, new RoutedEventArgs());
        LauncherMenuButton_Click(GameClientButton, new RoutedEventArgs());
        ItemListButton_Click(ItemListButton, new RoutedEventArgs());
        LauncherMenuButton_Click(ServerListButton, new RoutedEventArgs());
    }

    private static void SetBorderColor(Border border, Color color)
    { 
        border.BorderBrush = new SolidColorBrush(color);
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

        var clients = IOResources.DeserializeFile<List<BLRClient>>("GameClients.json");
        if (clients is not null) { GameClients = clients; }

        var servers = IOResources.DeserializeFile<List<BLRServer>>("ServerList.json");
        if (servers is not null) { ServerList = servers; }

        GameClientList.ItemsSource = null;
        GameClientList.Items.Clear();
        GameClientList.ItemsSource = GameClients;

        ServerListView.ItemsSource = null;
        ServerListView.Items.Clear();
        ServerListView.ItemsSource = ServerList;

        IOResources.GetGameLocationsFromSteam();
        foreach (string folder in IOResources.GameFolders)
        {
            AddGameClient(new BLRClient() { OriginalPath = folder + IOResources.GAME_DEFAULT_EXE});
        }

        AddDefaultServers();

        RefreshPing();

        if (BLREditSettings.Settings.DefaultServer is null)
        {
            BLREditSettings.Settings.DefaultServer = ServerList[0];
        }
        CheckGameClientSetup();
    }

    public void CheckGameClientSetup()
    {
        if (!IsCheckingGameClient)
        {
            IsCheckingGameClient = true;
            CheckGameClients();
            IsCheckingGameClient = false;
        }
    }

    private void CheckGameClients()
    {
        if (GameClients.Count <= 0)
        {
            ItemListButton_Click(LauncherButton, new RoutedEventArgs());
            LauncherMenuButton_Click(GameClientButton, new RoutedEventArgs());
            MessageBox.Show("You have to locate and add atleast one Client");
        }
        else
        {
            List<BLRClient> patchedClients = new();
            bool isClientStillExistent = false;
            foreach (BLRClient client in GameClients)
            {
                if (client.Equals(BLREditSettings.Settings.DefaultClient))
                { isClientStillExistent = true; }
                if (client.Patched.Is)
                { patchedClients.Add(client); }
            }

            if (!isClientStillExistent) { BLREditSettings.Settings.DefaultClient = null; }

            if (patchedClients.Count > 0)
            {
                if (BLREditSettings.Settings.DefaultClient is null)
                {
                    if (patchedClients.Count > 1)
                    {
                        ItemListButton_Click(LauncherButton, new RoutedEventArgs());
                        LauncherMenuButton_Click(GameClientButton, new RoutedEventArgs());
                        MessageBox.Show("You have to select one of the Patched Clients as a Default Client");
                    }
                    else
                    { BLREditSettings.Settings.DefaultClient = patchedClients[0]; patchedClients[0].CurrentClient.SetBool(true); }
                }
            }
            else if (GameClients.Count == 1)
            {
                GameClients[0].PatchClient();
                BLREditSettings.Settings.DefaultClient = GameClients[0];
            }
            else
            {
                ItemListButton_Click(LauncherButton, new RoutedEventArgs());
                LauncherMenuButton_Click(GameClientButton, new RoutedEventArgs());
                MessageBox.Show("You have to Patch one of the Available Clients");
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
        new() { ServerAddress = "mooserver.ddns.net", Port = 7777, ServerName = "MagiCow's Server" }, //mooserver.ddns.net : 7777
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
                if (client.Patched.Is)
                {
                    BLREditSettings.Settings.DefaultClient = client;
                    foreach (BLRClient c in GameClients)
                    {
                        c.CurrentClient.SetBool(false);
                    }
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
    }

    private void ScanGameClients()
    { 
        
    }

    private void ItemList_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if(element.DataContext is BLRItem item)
            {
                if (e.ClickCount >= 2)
                {
                    LoggingSystem.LogInfo($"Double Clicking:{item.Name}");
                    SetItemToImage(LastSelectedImage, item);
                }
                else
                {
                    LoggingSystem.LogInfo($"Dragging:{item.Name}");
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
                LoggingSystem.LogInfo($"Recieving:{item.Name}");
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

    public void SetItemToImage(Image image, BLRItem item, bool updateLoadout = true)
    {
        switch (image.Name)
        {
            case nameof(PrimaryRecieverImage):
                Loadout.Primary.Reciever = item;
                break;
            case nameof(SecondaryRecieverImage):
                Loadout.Secondary.Reciever = item;
                break;

            case nameof(PrimaryBarrelImage):
                Loadout.Primary.Barrel = item;
                break;
            case nameof(SecondaryBarrelImage):
                Loadout.Secondary.Barrel = item;
                break;

            case nameof(PrimaryMuzzleImage):
                Loadout.Primary.Muzzle = item;
                break;
            case nameof(SecondaryMuzzleImage):
                Loadout.Secondary.Muzzle = item;
                break;

            case nameof(PrimaryMagazineImage):
                Loadout.Primary.Magazine = item;
                break;
            case nameof(SecondaryMagazineImage):
                Loadout.Secondary.Magazine = item;
                break;

            case nameof(PrimaryStockImage):
                Loadout.Primary.Stock = item;
                break;
            case nameof(SecondaryStockImage):
                Loadout.Secondary.Stock = item;
                break;

            case nameof(PrimaryScopeImage):
                Loadout.Primary.Scope = item;
                break;
            case nameof(SecondaryScopeImage):
                Loadout.Secondary.Scope = item;
                break;

            case nameof(PrimaryGripImage):
                Loadout.Primary.Grip = item;
                break;
            case nameof(SecondaryGripImage):
                Loadout.Secondary.Grip = item;
                break;

            case nameof(PrimaryTagImage):
                Loadout.Primary.Tag = item;
                break;
            case nameof(SecondaryTagImage):
                Loadout.Secondary.Tag = item;
                break;

            case nameof(PrimaryCamoWeaponImage):
                Loadout.Primary.Camo = item;
                break;
            case nameof(SecondaryCamoWeaponImage):
                Loadout.Secondary.Camo = item;
                break;


            case nameof(HelmetImage):
                Loadout.Helmet = item;
                break;
            case nameof(UpperBodyImage):
                Loadout.UpperBody = item;
                break;
            case nameof(LowerBodyImage):
                Loadout.LowerBody = item;
                break;
            case nameof(TacticalImage):
                Loadout.Tactical = item;
                break;
            case nameof(GearImage1):
                Loadout.Gear1 = item;
                break;
            case nameof(GearImage2):
                Loadout.Gear2 = item;
                break;
            case nameof(GearImage3):
                Loadout.Gear3 = item;
                break;
            case nameof(GearImage4):
                Loadout.Gear4 = item;
                break;
            case nameof(PlayerCamoBodyImage):
                Loadout.Camo = item;
                break;
            case nameof(AvatarImage):
                Loadout.Avatar = item;
                break;
            case nameof(TrophyImage):
                Loadout.Trophy = item;
                break;
        }

        if (updateLoadout)
        {
            UpdateActiveLoadout();
        }
    }

    private static void UpdateActiveLoadout()
    {
        Loadout.UpdateMagicCowsLoadout(ActiveLoadout);
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
        LoggingSystem.LogInfo($"ItemList Set for {Type}");
        ItemListButton_Click(ItemListButton, new RoutedEventArgs());
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
                    LoggingSystem.LogInfo("ItemList Din't get set");
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
        switch (image.Name)
        {
            case nameof(PrimaryRecieverImage):
                Loadout.Primary.RemoveItem("Reciever");
                break;
            case nameof(SecondaryRecieverImage):
                Loadout.Secondary.RemoveItem("Reciever");
                break;

            case nameof(PrimaryBarrelImage):
                Loadout.Primary.RemoveItem("Barrel");
                break;
            case nameof(SecondaryBarrelImage):
                Loadout.Secondary.RemoveItem("Barrel");
                break;

            case nameof(PrimaryMuzzleImage):
                Loadout.Primary.RemoveItem("Muzzle");
                break;
            case nameof(SecondaryMuzzleImage):
                Loadout.Secondary.RemoveItem("Muzzle");
                break;

            case nameof(PrimaryMagazineImage):
                Loadout.Primary.RemoveItem("Magazine");
                break;
            case nameof(SecondaryMagazineImage):
                Loadout.Secondary.RemoveItem("Magazine");
                break;

            case nameof(PrimaryStockImage):
                Loadout.Primary.RemoveItem("Stock");
                break;
            case nameof(SecondaryStockImage):
                Loadout.Secondary.RemoveItem("Stock");
                break;

            case nameof(PrimaryScopeImage):
                Loadout.Primary.RemoveItem("Scope");
                break;
            case nameof(SecondaryScopeImage):
                Loadout.Secondary.RemoveItem("Scope");
                break;

            case nameof(PrimaryGripImage):
                Loadout.Primary.RemoveItem("Grip");
                break;
            case nameof(SecondaryGripImage):
                Loadout.Secondary.RemoveItem("Grip");
                break;

            case nameof(PrimaryTagImage):
                Loadout.Primary.RemoveItem("Tag");
                break;
            case nameof(SecondaryTagImage):
                Loadout.Secondary.RemoveItem("Tag");
                break;

            case nameof(PrimaryCamoWeaponImage):
                Loadout.Primary.RemoveItem("Camo");
                break;
            case nameof(SecondaryCamoWeaponImage):
                Loadout.Secondary.RemoveItem("Camo");
                break;


            case nameof(HelmetImage):
                Loadout.RemoveItem("Helmet");
                break;
            case nameof(UpperBodyImage):
                Loadout.RemoveItem("UpperBody");
                break;
            case nameof(LowerBodyImage):
                Loadout.RemoveItem("LowerBody");
                break;
            case nameof(TacticalImage):
                Loadout.RemoveItem("Tactical");
                break;
            case nameof(GearImage1):
                Loadout.RemoveItem("Gear1");
                break;
            case nameof(GearImage2):
                Loadout.RemoveItem("Gear2");
                break;
            case nameof(GearImage3):
                Loadout.RemoveItem("Gear3");
                break;
            case nameof(GearImage4):
                Loadout.RemoveItem("Gear4");
                break;
            case nameof(PlayerCamoBodyImage):
                Loadout.RemoveItem("Camo");
                break;
            case nameof(AvatarImage):
                Loadout.RemoveItem("Avatar");
                break;
            case nameof(TrophyImage):
                Loadout.RemoveItem("Trophy");
                break;
        }
        UpdateActiveLoadout();
    }

    public static void SetLoadout(MagiCowsLoadout loadout)
    {
        ActiveLoadout = loadout;
        Loadout.LoadMagicCowsLoadout(loadout);
    }
    
    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsPlayerNameChanging)
        {
            IsPlayerProfileChanging = true;
            if (ProfileComboBox.SelectedValue is ExportSystemProfile profile)
            {
                ExportSystem.ActiveProfile = profile;
                PlayerNameTextBox.Text = profile.PlayerName;
                SetLoadout(profile.Loadout1);
            }
            IsPlayerProfileChanging = false;
        }
    }
    private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsPlayerProfileChanging)
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

    private void Loadout1Button_Click(object sender, RoutedEventArgs e)
    {
        SetLoadout(ExportSystem.ActiveProfile.Loadout1);
        Loadout1Button.IsEnabled = false;
        Loadout2Button.IsEnabled = true;
        Loadout3Button.IsEnabled = true;
    }

    private void Loadout2Button_Click(object sender, RoutedEventArgs e)
    {
        SetLoadout(ExportSystem.ActiveProfile.Loadout2);
        Loadout1Button.IsEnabled = true;
        Loadout2Button.IsEnabled = false;
        Loadout3Button.IsEnabled = true;
    }

    private void Loadout3Button_Click(object sender, RoutedEventArgs e)
    {
        SetLoadout(ExportSystem.ActiveProfile.Loadout3);
        Loadout1Button.IsEnabled = true;
        Loadout2Button.IsEnabled = true;
        Loadout3Button.IsEnabled = false;
    }

    private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        ExportSystem.CopyToClipBoard(ExportSystem.ActiveProfile);
        //ExportSystem.CreateSEProfile(ExportSystem.ActiveProfile); //Not really in use currently only makes waste on the Hard Drive
    }

    private void IsFemaleCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Loadout.IsFemale = true;
        UpdateActiveLoadout();
    }

    private void IsFemaleCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        Loadout.IsFemale = false;
        UpdateActiveLoadout();
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

        SetLoadout(ExportSystem.ActiveProfile.Loadout1);

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

    private void ItemListButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            buttonIndex = ItemButtons.IndexOf(button);
            foreach (FrameworkElement element in MenuGrid.Children)
            {
                if (element.Tag is UIElement otherElement)
                { 
                    otherElement.Visibility = Visibility.Collapsed;
                    otherElement.IsEnabled = false;
                }
                element.IsEnabled = true;
            }
            button.IsEnabled = false;
            if (button.Tag is FrameworkElement element2)
            { 
                element2.Visibility = Visibility.Visible;
                element2.IsEnabled = true;
            }
        }
    }

    private void LauncherMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            foreach (FrameworkElement element in LauncherMenu.Children)
            {
                if (element.Tag is UIElement otherElement)
                {
                    otherElement.Visibility = Visibility.Collapsed;
                    otherElement.IsEnabled = false;
                }
                element.IsEnabled = true;
            }
            button.IsEnabled = false;
            if (button.Tag is FrameworkElement element2)
            {
                element2.Visibility = Visibility.Visible;
                element2.IsEnabled = true;
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
        CheckGameClientSetup();
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
        }
    }

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
            case Key.Tab:
                int index = buttonIndex;
                if (shiftDown)
                {
                    index--;
                }
                else
                {
                    index++;
                }
                if (index < 0)
                {
                    index = ItemButtons.Count - 1;
                }
                if (index > ItemButtons.Count - 1)
                {
                    index = 0;
                }
                ItemListButton_Click(ItemButtons[index], new RoutedEventArgs());
                break;
            case Key.A:
                if (shiftDown)
                {
                    BLREditSettings.Settings.AdvancedModding.SetBool(!BLREditSettings.Settings.AdvancedModding.Is);
                    BLREditSettings.Save();
                    var grid = CreateAlertGrid($"AdvancedModding:{BLREditSettings.Settings.AdvancedModding.Is}");
                    AlertList.Items.Add(grid);
                    new TripleAnimationDouble(0,400,1,3,1,grid,Grid.WidthProperty, AlertList.Items).Begin(AlertList);
                    LoggingSystem.LogInfo($"AdvancedModding:{BLREditSettings.Settings.AdvancedModding.Is}");
                }
                break;
        }
    }

    private Grid CreateAlertGrid(string Alert)
    {
        TextBox alertText = new TextBox() { Text=Alert, TextAlignment=TextAlignment.Center, Foreground=new SolidColorBrush(Color.FromArgb(255,255,136,0)), IsReadOnly=true, FontSize=26};
        Grid alertGrid = new Grid() { Background = new SolidColorBrush(Color.FromArgb(159,0,0,0)), HorizontalAlignment=HorizontalAlignment.Right, VerticalAlignment=VerticalAlignment.Center, Width=400 };
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
}
