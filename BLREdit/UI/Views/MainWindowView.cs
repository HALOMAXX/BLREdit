using BLREdit.Export;
using BLREdit.Game;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
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

    private BLRLoadoutStorage profile = DataStorage.Loadouts.FirstOrDefault();
    public BLRLoadoutStorage Profile { get { return profile; } set { profile.BLR.PropertyChanged -= LoadoutChangedRelay; profile = value; profile.Shareable.LastViewed = DateTime.Now; profile.BLR.PropertyChanged += LoadoutChangedRelay; OnPropertyChanged(); } }

#pragma warning disable CA1822 // Mark members as static
    public BLREditSettings BLRESettings => DataStorage.Settings;
    public ObservableCollection<BLRClient> GameClients => DataStorage.GameClients;
    public ObservableCollection<BLRServer> ServerList => DataStorage.ServerList;
    public ObservableCollectionExtended<BLRLoadoutStorage> Loadouts => DataStorage.Loadouts;
#pragma warning restore CA1822 // Mark members as static

    public readonly Color DefaultBorderColor = Color.FromArgb(14, 158, 158, 158);
    public readonly Color ActiveBorderColor = Color.FromArgb(255, 255, 136, 0);
    public Border? LastSelectedItemBorder { get { return lastSelectedItemBorder; } set { SetBorderColor(lastSelectedItemBorder, DefaultBorderColor); lastSelectedItemBorder = value; SetBorderColor(lastSelectedItemBorder, ActiveBorderColor); } }
    private Border? lastSelectedItemBorder = null;

    public ListSortDirection ItemListSortingDirection { get; set; } = ListSortDirection.Descending;
    public ListSortDirection ProfileListSortingDirection { get; set; } = ListSortDirection.Descending;
    public string CurrentProfileSortingPropertyName { get; set; } = "None";
    public Type? CurrentSortingEnumType { get; set; }
    public Type? CurrentProfileSortingEnumType { get; set; }
    public string LastSortingPropertyName { get; set; } = "None";
    public string CurrentSortingPropertyName { get; set; } = "None";
    public bool IsPlayerNameChanging { get; set; } = false;
    public bool IsPlayerProfileChanging { get; set; } = false;
    public bool IsCheckingGameClient { get; set; } = false;

    public BLRWeapon? PrimaryWeaponCopy { get; set; } = null;
    public BLRWeapon? SecondaryWeaponCopy { get; set; } = null;
    public BLRGear? GearCopy { get; set; } = null;
    public BLRExtra? ExtraCopy { get; set; } = null;

    public MainWindowView()
    {
        if (profile is null || profile.BLR is null) return;
        profile.BLR.IsChanged = false;
        profile.BLR.PropertyChanged += LoadoutChangedRelay;
    }

    void LoadoutChangedRelay(object sender, PropertyChangedEventArgs e)
    {
        if (MainWindow.Instance is null) return;
        MainWindow.Instance.LoadoutChanged(sender, e);
    }

    public void UpdateWindowTitle()
    {
        string BuildTag = "";

#if DEBUG
        BuildTag = "[Debug Build]:";
#elif RELEASE
        BuildTag = "[Release Build]:";
#elif PUBLISH
        BuildTag = "[Release Build]:";
#endif

        var PlayerProfile = ExportSystem.GetOrAddProfileSettings(DataStorage.Settings?.PlayerName ?? "");

        WindowTitle = $"{BuildTag}{App.CurrentRepo} - {App.CurrentVersion}, {DataStorage.Settings?.PlayerName} Playtime:[{new TimeSpan(0,0, PlayerProfile.PlayTime)}]";
    }

    public static void SetBorderColor(Border? border, Color color)
    {
        if (border is not null) border.BorderBrush = new SolidColorBrush(color);
    }
}
