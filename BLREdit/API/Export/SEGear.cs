using BLREdit.Import;
using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class SEGear
{
    public bool Female { get; set; } = false;
    public bool Bot { get; set; } = false;
    public int BodyCamo { get; set; } = 0;
    public int UpperBody { get; set; } = 0;
    public int LowerBody { get; set; } = 0;
    public int Helmet { get; set; } = 0;
    public int Badge { get; set; } = 0;
    public int Gear_R1 { get; set; } = 0;
    public int Gear_R2 { get; set; } = 0;
    public int Gear_L1 { get; set; } = 0;
    public int Gear_L2 { get; set; } = 0;
    public int Tactical { get; set; } = 0;
    public int ButtPack { get; set; } = 0;
    public int Avatar { get; set; } = -1;
    public int PatchIcon { get; set; } = 0;
    public int PatchIconColor { get; set; } = 0;
    public int PatchShape { get; set; } = 0;
    public int PatchShapeColor { get; set; } = 0;
    public int Hanger { get; set; } = 0;

    public SEGear()
    { }

    public SEGear(BLRLoadout loadout)
    {
        Female = loadout.IsFemale;
        BodyCamo = GetLMID(loadout.BodyCamo);
        UpperBody = GetLMID(loadout.UpperBody);
        LowerBody = GetLMID(loadout.LowerBody);
        Helmet = GetLMID(loadout.Helmet);
        Tactical = GetLMID(loadout.Tactical);
        int avatar = GetLMID(loadout.Avatar);
        if (avatar > 34) { Avatar = -1; } else { Avatar = avatar; }
        Badge = GetLMID(loadout.Trophy);
        if(loadout.GearSlot1Bool.Is) Gear_R1 = GetLMID(loadout.Gear1);
        if (loadout.GearSlot2Bool.Is) Gear_R2 = GetLMID(loadout.Gear2);
        if (loadout.GearSlot3Bool.Is) Gear_L1 = GetLMID(loadout.Gear3);
        if (loadout.GearSlot4Bool.Is) Gear_L2 = GetLMID(loadout.Gear4);

        //TODO Hanger, Icon, IconColor, PatchShape, PatchColor, ButtPack   
    }

    public static int GetLMID(BLRItem item)
    {
        if (item is null) return -1;
        if (item.LMID != -1) return item.LMID;
        return ImportSystem.GetIDOfItem(item);
    }
}
