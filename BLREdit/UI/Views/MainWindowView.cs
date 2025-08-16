using BLREdit.API.Export;
using BLREdit.Export;
using BLREdit.Game;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;

namespace BLREdit.UI.Views;

public sealed class MainWindowView : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    private string windowTitle = "";
    public string WindowTitle { get { return windowTitle; } set { windowTitle = value; OnPropertyChanged(); } }

    private BLRLoadoutStorage profile = GetLoadout();
    public BLRLoadoutStorage Profile { get { return profile; } set { profile.BLR.PropertyChanged -= LoadoutChangedRelay; profile = value; UpdateProfileBorders(); DataStorage.Settings.CurrentlyAppliedLoadout = DataStorage.Loadouts.IndexOf(value); profile.Shareable.LastViewed = DateTime.Now; profile.BLR.PropertyChanged += LoadoutChangedRelay; OnPropertyChanged(); } }

#pragma warning disable CA1822 // Mark members as static
    public BLREditSettings BLRESettings => DataStorage.Settings;
    public ObservableCollection<BLRClient> GameClients => DataStorage.GameClients;
    public ObservableCollection<BLRServer> ServerList => DataStorage.ServerList;
    public ObservableCollection<BLRLoadoutStorage> Loadouts => DataStorage.Loadouts;
#pragma warning restore CA1822 // Mark members as static

    public static readonly Color DefaultBorderColor = Color.FromArgb(14, 158, 158, 158);
    public static readonly Color ActiveBorderColor = Color.FromArgb(255, 255, 136, 0);

    public Color LastSelectedBorderColor { get; set; } = DefaultBorderColor;


    public Border? LastSelectedItemBorder { get { return lastSelectedItemBorder; } set { ResetLastBorder(); SetNewBorder(value); } }
    private Border? lastSelectedItemBorder;
    private BindingExpression? lastBindingExpression;
    private Color? lastSelectedBorderColor;

    private ListSortDirection itemListSortingDirection = ListSortDirection.Descending;
    private ListSortDirection profileListSortingDirection = ListSortDirection.Descending;
    public ListSortDirection ItemListSortingDirection { get { return itemListSortingDirection; } set { itemListSortingDirection = value; OnPropertyChanged(); OnPropertyChanged(nameof(ItemListSortingDirectionString)); } } 
    public ListSortDirection ProfileListSortingDirection { get { return profileListSortingDirection; } set { profileListSortingDirection = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProfileListSortingDirectionString)); } }
    public string ItemListSortingDirectionString { get { return ItemListSortingDirection == ListSortDirection.Ascending ? Properties.Resources.btn_Ascending : Properties.Resources.btn_Descending; } }
    public string ProfileListSortingDirectionString { get { return ProfileListSortingDirection == ListSortDirection.Ascending ? Properties.Resources.btn_Ascending : Properties.Resources.btn_Descending; } }
    public string CurrentProfileSortingPropertyName { get; set; } = "None";
    public Type? CurrentSortingEnumType { get; set; }
    public Type? CurrentProfileSortingEnumType { get; set; }
    public string LastSortingPropertyName { get; set; } = "None";
    public string CurrentSortingPropertyName { get; set; } = "None";
    public bool IsPlayerNameChanging { get; set; }
    public bool IsPlayerProfileChanging { get; set; }
    public bool IsCheckingGameClient { get; set; }
    public UIBool IsScopePreviewVisible { get; } = new(false);
    private readonly UIBool BackupIsFemale = new(false);
    public UIBool IsFemale { get { return Profile?.BLR?.IsFemale ?? BackupIsFemale; } }

    public BLREditWeapon? PrimaryWeaponCopy { get; set; }
    public BLREditWeapon? SecondaryWeaponCopy { get; set; }
    public BLRGear? GearCopy { get; set; }
    public BLRExtra? ExtraCopy { get; set; }

    public MainWindowView()
    {
        if (profile is null || profile.BLR is null) return;
        profile.BLR.IsChanged = false;
        profile.BLR.PropertyChanged += LoadoutChangedRelay;
    }

    static BLRLoadoutStorage GetLoadout()
    {
        if (DataStorage.Loadouts.Count <= 0)
        {
            BLRLoadoutStorage.AddNewLoadoutSet("Default Loadout 1", null, MagiCowsLoadout.DefaultLoadout1.ConvertToShareable());
            BLRLoadoutStorage.AddNewLoadoutSet("Default Loadout 2", null, MagiCowsLoadout.DefaultLoadout2.ConvertToShareable());
            BLRLoadoutStorage.AddNewLoadoutSet("Default Loadout 3", null, MagiCowsLoadout.DefaultLoadout3.ConvertToShareable());
        }

        var loadout = DataStorage.Loadouts[DataStorage.Loadouts.Count > DataStorage.Settings.CurrentlyAppliedLoadout ? DataStorage.Settings.CurrentlyAppliedLoadout : 0];
        loadout.BLR.PropertyChanged += LoadoutChangedRelay;
        return loadout;
    }

    void UpdateProfileBorders()
    {
        try
        {
            foreach (var l in DataStorage.Loadouts)
            {
                l.TriggerChangeNotify();
            }
        }
        catch { }
        OnPropertyChanged(nameof(IsFemale));
    }

    static void LoadoutChangedRelay(object sender, PropertyChangedEventArgs e)
    {
        if (MainWindow.Instance is null) return;
        MainWindow.Instance.LoadoutChanged(sender, e);
    }

    public void UpdateWindowTitle()
    {
        string BuildTag = "";

#if DEBUG
        BuildTag = $"[Debug]:";
#elif RELEASE
        BuildTag = $"[Release]:";
#endif

        var PlayerProfile = ExportSystem.GetOrAddProfileSettings(DataStorage.Settings?.PlayerName ?? "");

        WindowTitle = $"{BuildTag}{App.CurrentRepo}-{App.CurrentVersion}+{ThisAssembly.Git.Branch}, {DataStorage.Settings?.PlayerName} Playtime:[{new TimeSpan(0,0, PlayerProfile.PlayTime)}], SDK:[{DataStorage.Settings?.SDKVersionDate:yyyy.MM.dd(HH:mm:ss)}]";
    }

    public void ResetLastBorder()
    {
        if (lastSelectedItemBorder is null) return;
        if (lastBindingExpression is not null)
        {
            lastSelectedItemBorder.SetBinding(Border.BorderBrushProperty, lastBindingExpression.ParentBindingBase);
        }
        else
        {
            lastSelectedItemBorder.BorderBrush = new SolidColorBrush(lastSelectedBorderColor ?? DefaultBorderColor);
        }
    }

    public void SetNewBorder(Border? border)
    {
        if (border is not null)
        {
            if (border.GetBindingExpression(Border.BorderBrushProperty) is BindingExpression bindingExp)
            {
                lastBindingExpression = bindingExp;
                lastSelectedBorderColor = null;
            }
            else
            {
                if (border.BorderBrush is SolidColorBrush brush)
                { lastSelectedBorderColor = brush.Color; }
                else 
                { lastSelectedBorderColor = null; }

                lastBindingExpression = null;
            }

            border.BorderBrush = new SolidColorBrush(ActiveBorderColor);
            lastSelectedItemBorder = border;
        }
    }
}
