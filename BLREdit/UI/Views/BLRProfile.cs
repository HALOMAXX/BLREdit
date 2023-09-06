using BLREdit.Export;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.UI.Views;

public sealed class BLRProfile
{
    private IBLRProfile? _profile;

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Event

    public BLRLoadout Loadout1 { get; set; } = new();
    public BLRLoadout Loadout2 { get; set; } = new();
    public BLRLoadout Loadout3 { get; set; } = new();


    private bool isChanged = false;
    [JsonIgnore] public bool IsChanged { get { return isChanged; } set { isChanged = value; OnPropertyChanged(); } }

    public BLRProfile() {
        Loadout1.PropertyChanged += LoadoutChanged;
        Loadout2.PropertyChanged += LoadoutChanged;
        Loadout3.PropertyChanged += LoadoutChanged;

        Loadout1.Primary.PropertyChanged += LoadoutChanged;
        Loadout1.Secondary.PropertyChanged += LoadoutChanged;
        Loadout2.Primary.PropertyChanged += LoadoutChanged;
        Loadout2.Secondary.PropertyChanged += LoadoutChanged;
        Loadout3.Primary.PropertyChanged += LoadoutChanged;
        Loadout3.Secondary.PropertyChanged += LoadoutChanged;
    }

    void LoadoutChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsChanged)) IsChanged = true;
    }

    public static void UpdateSearchAndFilter()
    {
        MainWindow.Instance?.ApplySearchAndFilter();
    }

    public void CalculateStats()
    {
        Loadout1.CalculateStats();
        Loadout2.CalculateStats();
        Loadout3.CalculateStats();
    }

    public void SetProfile(IBLRProfile profile, bool registerReadBackEvent = false)
    { 
        _profile = profile;
        Loadout1.SetLoadout(profile.GetLoadout(0), registerReadBackEvent);
        Loadout2.SetLoadout(profile.GetLoadout(1), registerReadBackEvent);
        Loadout3.SetLoadout(profile.GetLoadout(2), registerReadBackEvent);
    }

    public void Write()
    {
        _profile?.Write(this);
    }

    public void Write(IBLRProfile profile)
    {
        profile.Write(this);
    }

    public void Read()
    {
        _profile?.Read(this);
    }

    public void Read(IBLRProfile profile)
    {
        profile.Read(this);
    }
}

public interface IBLRProfile
{
    public IBLRLoadout GetLoadout(int index);
    public void Read(BLRProfile profile);
    public void Write(BLRProfile profile);
}

public interface IBLRLoadout
{
    public IBLRWeapon GetPrimary();
    public IBLRWeapon GetSecondary();
    public void Read(BLRLoadout loadout);
    public void Write(BLRLoadout loadout);
}

public interface IBLRWeapon
{
    public void Read(BLRWeapon weapon);
    public void Write(BLRWeapon weapon);
}