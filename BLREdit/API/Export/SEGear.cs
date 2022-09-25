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
        BodyCamo = ImportSystem.GetIDOfItem(loadout.BodyCamo); ;// loadout?.BodyCamo?.LMID ?? 0;
        UpperBody = ImportSystem.GetIDOfItem(loadout.UpperBody); ;// loadout?.UpperBody?.LMID ?? 0;
        LowerBody = ImportSystem.GetIDOfItem(loadout.LowerBody); ;// loadout?.LowerBody?.LMID ?? 0;
        Helmet = ImportSystem.GetIDOfItem(loadout.Helmet); ;// loadout?.Helmet?.LMID ?? 0;
        Tactical = ImportSystem.GetIDOfItem(loadout.Tactical); ;// loadout?.Tactical?.LMID ?? 0;
        int avatar = ImportSystem.GetIDOfItem(loadout.Avatar);
        if (avatar > 34) { Avatar = -1; } else { Avatar = avatar; }
        Badge = ImportSystem.GetIDOfItem(loadout.Trophy); ;// loadout?.Trophy?.LMID ?? 0;
        Gear_R1 = ImportSystem.GetIDOfItem(loadout.Gear1); ;// loadout?.Gear1?.LMID ?? 0;
        Gear_R2 = ImportSystem.GetIDOfItem(loadout.Gear2); ;// loadout?.Gear2?.LMID ?? 1;
        Gear_L1 = ImportSystem.GetIDOfItem(loadout.Gear3); ;// loadout?.Gear3?.LMID ?? 2;
        Gear_L2 = ImportSystem.GetIDOfItem(loadout.Gear4); ;// loadout?.Gear4?.LMID ?? 3;

        //TODO Hanger, Icon, IconColor, PatchShape, PatchColor, ButtPack   
    }
}
