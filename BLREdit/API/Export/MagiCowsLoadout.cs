using BLREdit.Import;

using System;
using System.Text.Json.Serialization;

namespace BLREdit.Export;

public sealed class MagiCowsLoadout
{
    [JsonIgnore] private MagiCowsWeapon primary = MagiCowsWeapon.DefaultWeapons.AssaultRifle.Clone();
    public MagiCowsWeapon Primary { get { return primary; } set { if (primary != value) { primary = value; isDirty = true; } } }

    [JsonIgnore] private MagiCowsWeapon secondary = MagiCowsWeapon.DefaultWeapons.LightPistol.Clone();
    public MagiCowsWeapon Secondary { get { return secondary; } set { if (secondary != value) { secondary = value; isDirty = true; } } }

    [JsonIgnore] private int gear1 = 1;
    public int Gear1 { get { return gear1; } set { if (gear1 != value) { gear1 = value; isDirty = true; } } }

    [JsonIgnore] private int gear2 = 2;
    public int Gear2 { get { return gear2; } set { if (gear2 != value) { gear2 = value; isDirty = true; } } }

    [JsonIgnore] private int gear3 = 0;
    public int Gear3 { get { return gear3; } set { if (gear3 != value) { gear3 = value; isDirty = true; } } }

    [JsonIgnore] private int gear4 = 0;
    public int Gear4 { get { return gear4; } set { if (gear4 != value) { gear4 = value; isDirty = true; } } }

    [JsonIgnore] private int tactical = 0;
    public int Tactical { get { return tactical; } set { if (tactical != value) { tactical = value; isDirty = true; } } }

    [JsonIgnore] private bool isFemale = false;
    public bool IsFemale { get { return isFemale; } set { if (isFemale != value) { isFemale = value; isDirty = true; } } }

    [JsonIgnore] private int helmet = 0;
    public int Helmet { get { return helmet; } set { if (helmet != value) { helmet = value; isDirty = true; } } }

    [JsonIgnore] private int upperBody = 0;
    public int UpperBody { get { return upperBody; } set { if (upperBody != value) { upperBody = value; isDirty = true; } } }

    [JsonIgnore] private int lowerBody = 0;
    public int LowerBody { get { return lowerBody; } set { if (lowerBody != value) { lowerBody = value; isDirty = true; } } }

    [JsonIgnore] private int camo = 0;
    public int Camo { get { return camo; } set { if (camo != value) { camo = value; isDirty = true; } } }

    [JsonIgnore] private int skin = 99;
    public int Skin { get { return skin; } set { if (skin != value) { skin = value; isDirty = true; } } }

    [JsonIgnore] private int trophy = 0;
    public int Trophy { get { return trophy; } set { if (trophy != value) { trophy = value; isDirty = true; } } }

    [JsonIgnore] private int[] taunts = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
    public int[] Taunts { get { return taunts; } set { if (taunts != value) { taunts = value; isDirty = true; } } }

    [JsonIgnore] private int[] depot = new int[] { 0, 1, 2, 3, 4 };
    public int[] Depot { get { return depot; } set { if (depot != value) { depot = value; isDirty = true; } } }

    [JsonIgnore] private bool isDirty = true;
    [JsonIgnore] public bool IsDirty { get { return (isDirty || primary.IsDirty || secondary.IsDirty); } set { isDirty = value; primary.IsDirty = value; secondary.IsDirty = value; } }



    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }

    public MagiCowsLoadout Clone()
    {
        MagiCowsLoadout clone = this.MemberwiseClone() as MagiCowsLoadout ?? new();
        clone.Primary = this.Primary.Clone();
        clone.Secondary = this.Secondary.Clone();
        clone.isDirty = true;
        return clone;
    }

    public bool IsHealthOkAndRepair()
    {
        bool isHealthy = true;
        if (!Primary.IsHealthOkAndRepair())
        {
            isHealthy = false;
        }
        if (!Secondary.IsHealthOkAndRepair())
        {
            isHealthy = false;
        }
        return isHealthy;
    }

    public static MagiCowsLoadout DefaultLoadout1 { get; } = new MagiCowsLoadout() { Primary = MagiCowsWeapon.DefaultWeapons.AssaultRifle.Clone(), Secondary = MagiCowsWeapon.DefaultWeapons.LightPistol.Clone() };
    public static MagiCowsLoadout DefaultLoadout2 { get; } = new MagiCowsLoadout() { Primary = MagiCowsWeapon.DefaultWeapons.SubmachineGun.Clone(), Secondary = MagiCowsWeapon.DefaultWeapons.LightPistol.Clone() };
    public static MagiCowsLoadout DefaultLoadout3 { get; } = new MagiCowsLoadout() { Primary = MagiCowsWeapon.DefaultWeapons.BoltActionRifle.Clone(), Secondary = MagiCowsWeapon.DefaultWeapons.LightPistol.Clone() };

    public static BLRItem? GetGear(int GearID)
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, GearID);
    }
    public BLRItem? GetTactical()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, Tactical);
    }
    public BLRItem? GetHelmet()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, Helmet);
    }
    public BLRItem? GetUpperBody()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, UpperBody);
    }
    public BLRItem? GetLowerBody()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, LowerBody);
    }
    public BLRItem? GetCamo()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, Camo);
    }
    public BLRItem? GetSkin()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.AVATARS_CATEGORY, Skin);
    }
    public BLRItem? GetTrophy()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.BADGES_CATEGORY, Trophy);
    }
}