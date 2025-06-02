using BLREdit.Import;
using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class ProxyLoadoutManagerLoadout
{
    public ProxyLoadoutManagerWeapon? Primary { get; set; }
    public ProxyLoadoutManagerWeapon? Secondary { get; set; }
    public ProxyLoadoutManagerGear? Gear { get; set; }

    public int[] Depot { get; set; } = new int[5];
    public int[] Taunts { get; set; } = new int[8];

    public ProxyLoadoutManagerLoadout()
    {}

    public ProxyLoadoutManagerLoadout(BLREditLoadout loadout)
    {
        if (loadout is null) { LoggingSystem.FatalLog("loadout was null in ProxyLoadoutManagerLoadout constructor"); return; }
        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
        Gear = new(loadout);

        Taunts[0] = BLREditItem.GetLMID(loadout.Taunt1);
        Taunts[1] = BLREditItem.GetLMID(loadout.Taunt2);
        Taunts[2] = BLREditItem.GetLMID(loadout.Taunt3);
        Taunts[3] = BLREditItem.GetLMID(loadout.Taunt4);
        Taunts[4] = BLREditItem.GetLMID(loadout.Taunt5);
        Taunts[5] = BLREditItem.GetLMID(loadout.Taunt6);
        Taunts[6] = BLREditItem.GetLMID(loadout.Taunt7);
        Taunts[7] = BLREditItem.GetLMID(loadout.Taunt8);

        Depot[0] = BLREditItem.GetLMID(loadout.Depot1);
        Depot[1] = BLREditItem.GetLMID(loadout.Depot2);
        Depot[2] = BLREditItem.GetLMID(loadout.Depot3);
        Depot[3] = BLREditItem.GetLMID(loadout.Depot4);
        Depot[4] = BLREditItem.GetLMID(loadout.Depot5);
    }

    public BLREditLoadout GetLoadout()
    {
        return new BLREditLoadout(null)
        {
            Primary = Primary?.GetWeapon(true) ?? new(true),
            Secondary = Secondary?.GetWeapon(false) ?? new(false),

            Helmet = ImportSystem.GetItemByLMIDAndType(ImportSystem.HELMETS_CATEGORY, Gear?.Helmet ?? -1),
            UpperBody = ImportSystem.GetItemByLMIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, Gear?.UpperBody ?? -1),
            LowerBody = ImportSystem.GetItemByLMIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, Gear?.LowerBody ?? -1),
            Tactical = ImportSystem.GetItemByLMIDAndType(ImportSystem.TACTICAL_CATEGORY, Gear?.Tactical ?? -1),
            Trophy = ImportSystem.GetItemByLMIDAndType(ImportSystem.BADGES_CATEGORY, Gear?.Badge ?? -1),
            BodyCamo = ImportSystem.GetItemByLMIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, Gear?.BodyCamo ?? -1),
            Avatar = ImportSystem.GetItemByLMIDAndType(ImportSystem.AVATARS_CATEGORY, Gear?.Avatar ?? -1),

            Gear1 = ImportSystem.GetItemByLMIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear?.Gear_R1 ?? -1),
            Gear2 = ImportSystem.GetItemByLMIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear?.Gear_R2 ?? -1),
            Gear3 = ImportSystem.GetItemByLMIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear?.Gear_L1 ?? -1),
            Gear4 = ImportSystem.GetItemByLMIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear?.Gear_L2 ?? -1),

            Depot1 = ImportSystem.GetItemByLMIDAndType(ImportSystem.SHOP_CATEGORY, Depot[0]),
            Depot2 = ImportSystem.GetItemByLMIDAndType(ImportSystem.SHOP_CATEGORY, Depot[1]),
            Depot3 = ImportSystem.GetItemByLMIDAndType(ImportSystem.SHOP_CATEGORY, Depot[2]),
            Depot4 = ImportSystem.GetItemByLMIDAndType(ImportSystem.SHOP_CATEGORY, Depot[3]),
            Depot5 = ImportSystem.GetItemByLMIDAndType(ImportSystem.SHOP_CATEGORY, Depot[4]),

            Taunt1 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[0]),
            Taunt2 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[1]),
            Taunt3 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[2]),
            Taunt4 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[3]),
            Taunt5 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[4]),
            Taunt6 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[5]),
            Taunt7 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[6]),
            Taunt8 = ImportSystem.GetItemByLMIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[7])
        };
    }
}
