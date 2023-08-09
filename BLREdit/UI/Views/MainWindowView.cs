using BLREdit.Export;
using BLREdit.Game;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
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

    public BLRProfile Profile { get; } = new();

    public BLREditSettings BLRESettings { get { return BLREditSettings.Settings; } }

    private ObservableCollection<BLRClient>? _gameClients;
    private ObservableCollection<BLRServer>? _servers;
    public ObservableCollection<BLRClient> GameClients { get { _gameClients ??= IOResources.DeserializeFile<ObservableCollection<BLRClient>>($"GameClients.json") ?? new(); return _gameClients; } }
    public ObservableCollection<BLRServer> ServerList { get { _servers ??= IOResources.DeserializeFile<ObservableCollection<BLRServer>>($"ServerList.json") ?? new(); return _servers; } }

    public readonly Color DefaultBorderColor = Color.FromArgb(14, 158, 158, 158);
    public readonly Color ActiveBorderColor = Color.FromArgb(255, 255, 136, 0);
    public  Border? LastSelectedItemBorder { get { return lastSelectedItemBorder; } set { SetBorderColor(lastSelectedItemBorder, DefaultBorderColor); lastSelectedItemBorder = value; SetBorderColor(lastSelectedItemBorder, ActiveBorderColor); } }
    private Border? lastSelectedItemBorder = null;
    public  MagiCowsProfile? ActiveLoadoutSet { get { return activeLoadoutSet; } set { activeLoadoutSet = value; Profile.LoadMagiCowsProfile(value); } }
    private MagiCowsProfile? activeLoadoutSet = null;

    public ListSortDirection ItemListSortingDirection { get; set; } = ListSortDirection.Descending;
    public Type CurrentSortingEnumType { get; set; }
    public string CurrentSortingPropertyName { get; set; } = "None";
    public bool IsPlayerNameChanging { get; set; } = false;
    public bool IsPlayerProfileChanging { get; set; } = false;
    public bool IsCheckingGameClient { get; set; } = false;

    public BLRWeapon? Copy { get; set; } = null;


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

        var PlayerProfile = ExportSystem.GetOrAddProfileSettings(BLREditSettings.Settings?.PlayerName ?? "");

        WindowTitle = $"{BuildTag}{App.CurrentRepo} - {App.CurrentVersion}, {BLREditSettings.Settings?.PlayerName} Playtime:[{new TimeSpan(0,0, PlayerProfile.PlayTime)}]";
    }

    public static void SetBorderColor(Border? border, Color color)
    {
        if (border is not null) border.BorderBrush = new SolidColorBrush(color);
    }
}
