using BLREdit.Export;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.UI.Views;

public sealed class BLRProfile
{
    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Event


    public BLRLoadout Loadout1 { get; set; } = new();
    public BLRLoadout Loadout2 { get; set; } = new();
    public BLRLoadout Loadout3 { get; set; } = new();
    private MagiCowsProfile? internalProfile;

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
        if(e.PropertyName == nameof(IsChanged)) IsChanged = true;
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

    
    public void LoadMagiCowsProfile(MagiCowsProfile? profile)
    {
        if (profile is null) return;
        internalProfile = profile;
        Loadout1.LoadMagicCowsLoadout(internalProfile.Loadout1);
        Loadout2.LoadMagicCowsLoadout(internalProfile.Loadout2);
        Loadout3.LoadMagicCowsLoadout(internalProfile.Loadout3);
        IsChanged = false;
    }

    public void WriteMagiCowsProfile(MagiCowsProfile profile, bool overwriteLimits = false)
    {
        Loadout1.WriteMagiCowsLoadout(profile.Loadout1, overwriteLimits);
        Loadout2.WriteMagiCowsLoadout(profile.Loadout2, overwriteLimits);
        Loadout3.WriteMagiCowsLoadout(profile.Loadout3, overwriteLimits);
    }

    public void UpdateMagiCowsProfile()
    {
        Loadout1.UpdateMagicCowsLoadout();
        Loadout2.UpdateMagicCowsLoadout();
        Loadout3.UpdateMagicCowsLoadout();
    }
}
