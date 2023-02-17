using BLREdit.Import;
using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class LoadoutManagerGear
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

    public LoadoutManagerGear()
    { }

    public LoadoutManagerGear(BLRLoadout loadout)
    {
        Female = loadout.IsFemale;
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

        if (loadout.GearSlot1Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_R1 = BLRItem.GetLMID(loadout.Gear1);
        if (loadout.GearSlot2Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_R2 = BLRItem.GetLMID(loadout.Gear2);
        if (loadout.GearSlot3Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_L1 = BLRItem.GetLMID(loadout.Gear3);
        if (loadout.GearSlot4Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_L2 = BLRItem.GetLMID(loadout.Gear4);

        //TODO: Hanger, Icon, IconColor, PatchShape, PatchColor, ButtPack   
    }
}
