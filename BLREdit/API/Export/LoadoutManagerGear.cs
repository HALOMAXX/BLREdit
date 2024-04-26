using BLREdit.Import;
using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class LoadoutManagerGear
{
    public bool Female { get; set; }
    public bool Bot { get; set; }
    public int BodyCamo { get; set; }
    public int UpperBody { get; set; }
    public int LowerBody { get; set; }
    public int Helmet { get; set; }
    public int Badge { get; set; }
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public int Gear_R1 { get; set; }
    public int Gear_R2 { get; set; }
    public int Gear_L1 { get; set; }
    public int Gear_L2 { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
    public int Tactical { get; set; }
    public int ButtPack { get; set; }
    public int Avatar { get; set; } = -1;
    public int PatchIcon { get; set; }
    public int PatchIconColor { get; set; }
    public int PatchShape { get; set; }
    public int PatchShapeColor { get; set; }
    public int Hanger { get; set; }

    public LoadoutManagerGear()
    { }

    public LoadoutManagerGear(BLRLoadout loadout)
    {
        Female = loadout.IsFemale.Is;
        BodyCamo = BLRItem.GetLMID(loadout.BodyCamo);
        UpperBody = BLRItem.GetLMID(loadout.UpperBody);
        LowerBody = BLRItem.GetLMID(loadout.LowerBody);
        Helmet = BLRItem.GetLMID(loadout.Helmet);
        Tactical = BLRItem.GetLMID(loadout.Tactical);
        Badge = BLRItem.GetLMID(loadout.Trophy);

        int avatar = BLRItem.GetLMID(loadout.Avatar);

        if (avatar > 34) 
        { Avatar = -1; } 
        else 
        { Avatar = avatar; }

        if (loadout.GearSlot1Bool.Is) Gear_R1 = BLRItem.GetLMID(loadout.Gear1);
        if (loadout.GearSlot2Bool.Is) Gear_R2 = BLRItem.GetLMID(loadout.Gear2);
        if (loadout.GearSlot3Bool.Is) Gear_L1 = BLRItem.GetLMID(loadout.Gear3);
        if (loadout.GearSlot4Bool.Is) Gear_L2 = BLRItem.GetLMID(loadout.Gear4);
        //if (loadout.GearSlot4Bool.Is || (loadout.Profile?.IsAdvanced.Is ?? false))

        //TODO: Hanger, Icon, IconColor, PatchShape, PatchColor, ButtPack   
    }
}
