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
        BodyCamo = loadout.BodyCamo.GetLMID();
        UpperBody = loadout.UpperBody.GetLMID();
        LowerBody = loadout.LowerBody.GetLMID();
        Helmet = loadout.Helmet.GetLMID();
        Tactical = loadout.Tactical.GetLMID();
        Badge = loadout.Trophy.GetLMID();

        int avatar = loadout.Avatar.GetLMID();

        if (avatar > 34) 
        { Avatar = -1; } 
        else 
        { Avatar = avatar; }

        if (loadout.GearSlot1Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_R1 = loadout.Gear1.GetLMID();
        if (loadout.GearSlot2Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_R2 = loadout.Gear2.GetLMID();
        if (loadout.GearSlot3Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_L1 = loadout.Gear3.GetLMID();
        if (loadout.GearSlot4Bool.Is || BLREditSettings.Settings.AdvancedModding.Is) Gear_L2 = loadout.Gear4.GetLMID();

        //TODO Hanger, Icon, IconColor, PatchShape, PatchColor, ButtPack   
    }
}
