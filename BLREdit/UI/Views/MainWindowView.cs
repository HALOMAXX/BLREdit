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
    public BLRLoadoutStorage Profile { get { return profile; } set { profile.BLR.PropertyChanged -= LoadoutChangedRelay; profile = value; DataStorage.Settings.CurrentlyAppliedLoadout = DataStorage.Loadouts.IndexOf(value); profile.Shareable.LastViewed = DateTime.Now; profile.BLR.PropertyChanged += LoadoutChangedRelay; OnPropertyChanged(); } }

#pragma warning disable CA1822 // Mark members as static
    public BLREditSettings BLRESettings => DataStorage.Settings;
    public ObservableCollection<BLRClient> GameClients => DataStorage.GameClients;
    public ObservableCollection<BLRServer> ServerList => DataStorage.ServerList;
    public ObservableCollectionExtended<BLRLoadoutStorage> Loadouts => DataStorage.Loadouts;
#pragma warning restore CA1822 // Mark members as static

    public static readonly Color DefaultBorderColor = Color.FromArgb(14, 158, 158, 158);
    public static readonly Color ActiveBorderColor = Color.FromArgb(255, 255, 136, 0);

    public Color LastSelectedBorderColor = Color.FromArgb(14, 158, 158, 158);


    public Border? LastSelectedItemBorder { get { return lastSelectedItemBorder; } set { ResetLastBorder(); SetNewBorder(value); } }
    private Border? lastSelectedItemBorder;
    private BindingExpression? lastBindingExpression;
    private Color? lastSelectedBorderColor;

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

    static BLRLoadoutStorage GetLoadout()
    {
        if (DataStorage.Loadouts.Count <= 0)
        {
            DataStorage.Loadouts.Add(new(MagiCowsLoadout.DefaultLoadout1.ConvertToShareable()));
            DataStorage.Loadouts.Add(new(MagiCowsLoadout.DefaultLoadout2.ConvertToShareable()));
            DataStorage.Loadouts.Add(new(MagiCowsLoadout.DefaultLoadout3.ConvertToShareable()));
            string message = string.Empty;
            DataStorage.Loadouts[0].BLR.Name = "Default Loadout 1";
            DataStorage.Loadouts[0].BLR.Apply = DataStorage.Loadouts[0].BLR.ValidateLoadout(ref message);
            DataStorage.Loadouts[1].BLR.Name = "Default Loadout 2";
            DataStorage.Loadouts[1].BLR.Apply = DataStorage.Loadouts[1].BLR.ValidateLoadout(ref message);
            DataStorage.Loadouts[2].BLR.Name = "Default Loadout 3";
            DataStorage.Loadouts[2].BLR.Apply = DataStorage.Loadouts[2].BLR.ValidateLoadout(ref message);
        }

        var loadout = DataStorage.Loadouts[DataStorage.Loadouts.Count > DataStorage.Settings.CurrentlyAppliedLoadout ? DataStorage.Settings.CurrentlyAppliedLoadout : 0];
        loadout.BLR.PropertyChanged += LoadoutChangedRelay;
        return loadout;
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
        BuildTag = "[Debug Build]:";
#elif RELEASE
        BuildTag = "[Release Build]:";
#elif PUBLISH
        BuildTag = "[Release Build]:";
#endif

        var PlayerProfile = ExportSystem.GetOrAddProfileSettings(DataStorage.Settings?.PlayerName ?? "");

        WindowTitle = $"{BuildTag}{App.CurrentRepo} - {App.CurrentVersion}, {DataStorage.Settings?.PlayerName} Playtime:[{new TimeSpan(0,0, PlayerProfile.PlayTime)}]";
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
