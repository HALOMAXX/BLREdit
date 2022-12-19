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

        Taunts[0] = GetLMID(loadout.Taunt1);
        Taunts[1] = GetLMID(loadout.Taunt2);
        Taunts[2] = GetLMID(loadout.Taunt3);
        Taunts[3] = GetLMID(loadout.Taunt4);
        Taunts[4] = GetLMID(loadout.Taunt5);
        Taunts[5] = GetLMID(loadout.Taunt6);
        Taunts[6] = GetLMID(loadout.Taunt7);
        Taunts[7] = GetLMID(loadout.Taunt8);

        Depot[0] = GetLMID(loadout.Depot1);
        Depot[1] = GetLMID(loadout.Depot2);
        Depot[2] = GetLMID(loadout.Depot3);
        Depot[3] = GetLMID(loadout.Depot4);
        Depot[4] = GetLMID(loadout.Depot5);
    }

    public static int GetLMID(BLRItem item)
    {
        if (item is null) return -1;
        if (item.LMID != -1) return item.LMID;
        return ImportSystem.GetIDOfItem(item);
    }
}
