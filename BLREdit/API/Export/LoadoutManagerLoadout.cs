using BLREdit.Import;
using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class LoadoutManagerLoadout
{
    public bool IsAdvanced { get; set; } = false;
    public LoadoutManagerWeapon? Primary { get; set; }
    public LoadoutManagerWeapon? Secondary { get; set; }
    public LoadoutManagerGear? Gear { get; set; }

    public int[] Depot { get; set; } = new int[5];
    public int[] Taunts { get; set; } = new int[8];

    public LoadoutManagerLoadout()
    {}

    public LoadoutManagerLoadout(BLRLoadout loadout, bool isAdvanced)
    {
        IsAdvanced = isAdvanced;
        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
        Gear = new(loadout);

        Taunts[0] = BLRItem.GetLMID(loadout.Taunt1);
        Taunts[1] = BLRItem.GetLMID(loadout.Taunt2);
        Taunts[2] = BLRItem.GetLMID(loadout.Taunt3);
        Taunts[3] = BLRItem.GetLMID(loadout.Taunt4);
        Taunts[4] = BLRItem.GetLMID(loadout.Taunt5);
        Taunts[5] = BLRItem.GetLMID(loadout.Taunt6);
        Taunts[6] = BLRItem.GetLMID(loadout.Taunt7);
        Taunts[7] = BLRItem.GetLMID(loadout.Taunt8);

        Depot[0] = BLRItem.GetLMID(loadout.Depot1);
        Depot[1] = BLRItem.GetLMID(loadout.Depot2);
        Depot[2] = BLRItem.GetLMID(loadout.Depot3);
        Depot[3] = BLRItem.GetLMID(loadout.Depot4);
        Depot[4] = BLRItem.GetLMID(loadout.Depot5);
    }
}
