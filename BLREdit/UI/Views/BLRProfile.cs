using BLREdit.Export;

namespace BLREdit.UI.Views;

public sealed class BLRProfile
{
    public BLRLoadout Loadout1 { get; set; } = new();
    public BLRLoadout Loadout2 { get; set; } = new();
    public BLRLoadout Loadout3 { get; set; } = new();

    public static void UpdateSearchAndFilter()
    {
        MainWindow.Self.ApplySearchAndFilter();
    }

    public void CalculateStats()
    {
        Loadout1.CalculateStats();
        Loadout2.CalculateStats();
        Loadout3.CalculateStats();
    }

    private MagiCowsProfile internalProfile;
    public void LoadMagiCowsProfile(MagiCowsProfile profile)
    {
        internalProfile = profile;
        Loadout1.LoadMagicCowsLoadout(internalProfile.Loadout1);
        Loadout2.LoadMagicCowsLoadout(internalProfile.Loadout2);
        Loadout3.LoadMagicCowsLoadout(internalProfile.Loadout3);
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
