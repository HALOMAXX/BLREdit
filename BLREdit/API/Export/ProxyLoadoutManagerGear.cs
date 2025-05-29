using BLREdit.Import;
using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class ProxyLoadoutManagerGear
{
    public bool Female { get; set; }
    public bool Bot { get; set; }
    public int BodyCamo { get; set; }
    public int UpperBody { get; set; }
    public int LowerBody { get; set; }
    public int Helmet { get; set; }
    public int Badge { get; set; }
    public int Gear_R1 { get; set; }
    public int Gear_R2 { get; set; }
    public int Gear_L1 { get; set; }
    public int Gear_L2 { get; set; }
    public int Tactical { get; set; }
    public int ButtPack { get; set; }
    public int Avatar { get; set; } = -1;
    public int PatchIcon { get; set; }
    public int PatchIconColor { get; set; }
    public int PatchShape { get; set; }
    public int PatchShapeColor { get; set; }
    public int Hanger { get; set; }

    public ProxyLoadoutManagerGear()
    { }

    public ProxyLoadoutManagerGear(BLREditLoadout? loadout)
    {
        Female = loadout?.IsFemale?.Is ?? false;
        BodyCamo = BLREditItem.GetLMID(loadout?.BodyCamo);
        UpperBody = BLREditItem.GetLMID(loadout?.UpperBody);
        LowerBody = BLREditItem.GetLMID(loadout?.LowerBody);
        Helmet = BLREditItem.GetLMID(loadout?.Helmet);
        Tactical = BLREditItem.GetLMID(loadout?.Tactical);
        Badge = BLREditItem.GetLMID(loadout?.Trophy);

        int avatar = BLREditItem.GetLMID(loadout?.Avatar);

        if (avatar > 34) 
        { Avatar = -1; } 
        else 
        { Avatar = avatar; }

        // first two gear slots are enabled because default loadout has a grenade and knife by default
        if (loadout?.GearSlot1Bool?.Is ?? true) Gear_R1 = BLREditItem.GetLMID(loadout?.Gear1); 
        if (loadout?.GearSlot2Bool?.Is ?? true) Gear_R2 = BLREditItem.GetLMID(loadout?.Gear2);
        if (loadout?.GearSlot3Bool?.Is ?? false) Gear_L1 = BLREditItem.GetLMID(loadout.Gear3);
        if (loadout?.GearSlot4Bool?.Is ?? false) Gear_L2 = BLREditItem.GetLMID(loadout.Gear4);
    }
}
