using BLREdit.Import;
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

        Taunts[0] = ImportSystem.GetIDOfItem(loadout.Taunt1);
        Taunts[1] = ImportSystem.GetIDOfItem(loadout.Taunt2);
        Taunts[2] = ImportSystem.GetIDOfItem(loadout.Taunt3);
        Taunts[3] = ImportSystem.GetIDOfItem(loadout.Taunt4);
        Taunts[4] = ImportSystem.GetIDOfItem(loadout.Taunt5);
        Taunts[5] = ImportSystem.GetIDOfItem(loadout.Taunt6);
        Taunts[6] = ImportSystem.GetIDOfItem(loadout.Taunt7);
        Taunts[7] = ImportSystem.GetIDOfItem(loadout.Taunt8);

        Depot[0] = ImportSystem.GetIDOfItem(loadout.Depot1);
        Depot[1] = ImportSystem.GetIDOfItem(loadout.Depot2);
        Depot[2] = ImportSystem.GetIDOfItem(loadout.Depot3);
        Depot[3] = ImportSystem.GetIDOfItem(loadout.Depot4);
        Depot[4] = ImportSystem.GetIDOfItem(loadout.Depot5);
        //TODO Depot, Taunts
    }
}
