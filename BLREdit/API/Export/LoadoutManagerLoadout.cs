using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class LoadoutManagerLoadout
{
    public LoadoutManagerWeapon Primary { get; set; }
    public LoadoutManagerWeapon Secondary { get; set; }
    public LoadoutManagerGear Gear { get; set; }

    public int[] Depot { get; set; } = new int[5];
    public int[] Taunts { get; set; } = new int[8];

    public LoadoutManagerLoadout()
    {}

    public LoadoutManagerLoadout(BLRLoadout loadout)
    {
        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
        Gear = new(loadout);

        Taunts[0] = loadout.Taunt1.GetLMID();
        Taunts[1] = loadout.Taunt2.GetLMID();
        Taunts[2] = loadout.Taunt3.GetLMID();
        Taunts[3] = loadout.Taunt4.GetLMID();
        Taunts[4] = loadout.Taunt5.GetLMID();
        Taunts[5] = loadout.Taunt6.GetLMID();
        Taunts[6] = loadout.Taunt7.GetLMID();
        Taunts[7] = loadout.Taunt8.GetLMID();

        Depot[0] = loadout.Depot1.GetLMID();
        Depot[1] = loadout.Depot2.GetLMID();
        Depot[2] = loadout.Depot3.GetLMID();
        Depot[3] = loadout.Depot4.GetLMID();
        Depot[4] = loadout.Depot5.GetLMID();
    }
}
