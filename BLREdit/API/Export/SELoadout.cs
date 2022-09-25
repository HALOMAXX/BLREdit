using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class SELoadout
{
    public SEWeapon Primary { get; set; }
    public SEWeapon Secondary { get; set; }
    public SEGear Gear { get; set; }

    public int[] Depot { get; set; } = new int[5];
    public int[] Taunts { get; set; } = new int[8];

    public SELoadout()
    {}

    public SELoadout(BLRLoadout loadout)
    {
        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
        Gear = new(loadout);

        //TODO Depot, Taunts
    }
}
