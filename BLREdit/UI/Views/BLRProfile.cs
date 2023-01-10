using BLREdit.Export;

namespace BLREdit.UI.Views;

public sealed class BLRProfile
{
    public BLRLoadout Loadout1 { get; } = new();
    public BLRLoadout Loadout2 { get; } = new();
    public BLRLoadout Loadout3 { get; } = new();

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
    public void LoadMagicCowsProfile(MagiCowsProfile profile)
    {
        internalProfile = profile;
        Loadout1.LoadMagicCowsLoadout(internalProfile.Loadout1);
        Loadout2.LoadMagicCowsLoadout(internalProfile.Loadout2);
        Loadout3.LoadMagicCowsLoadout(internalProfile.Loadout3);
    }

    public void UpdateMagicCowsProfile()
    {
        Loadout1.UpdateMagicCowsLoadout();
        Loadout2.UpdateMagicCowsLoadout();
        Loadout3.UpdateMagicCowsLoadout();
    }
}
