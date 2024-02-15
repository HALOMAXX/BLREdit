using BLREdit.API.Export;
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Text.Json.Serialization;

namespace BLREdit.Export;

public sealed class MagiCowsLoadout : IBLRLoadout
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

    [JsonIgnore] private int[] taunts = [0, 1, 2, 3, 4, 5, 6, 7];
    public int[] Taunts { get { return taunts; } set { if (taunts != value) { taunts = value; isDirty = true; } } }

    [JsonIgnore] private int[] depot = [0, 1, 2, 3, 4];
    public int[] Depot { get { return depot; } set { if (depot != value) { depot = value; isDirty = true; } } }

    [JsonIgnore] private bool isDirty = true;

    public event EventHandler? WasWrittenTo;

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

    public IBLRWeapon GetPrimary()
    {
        return Primary;
    }

    public IBLRWeapon GetSecondary()
    {
        return Secondary;
    }

    public void Read(BLRLoadout loadout)
    {
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;

        Primary.Read(loadout.Primary);
        Secondary.Read(loadout.Secondary);

        loadout.IsFemale = IsFemale;

        loadout.Helmet = GetHelmet();
        loadout.UpperBody = GetUpperBody();
        loadout.LowerBody = GetLowerBody();

        loadout.Tactical = GetTactical();

        loadout.Gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear1);
        loadout.Gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear2);
        loadout.Gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear3);
        loadout.Gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear4);

        loadout.Taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[0]);
        loadout.Taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[1]);
        loadout.Taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[2]);
        loadout.Taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[3]);
        loadout.Taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[4]);
        loadout.Taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[5]);
        loadout.Taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[6]);
        loadout.Taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[7]);

        loadout.Depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[0]);
        loadout.Depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[1]);
        loadout.Depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[2]);
        loadout.Depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[3]);
        loadout.Depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[4]);

        loadout.Trophy = GetTrophy();
        loadout.Avatar = GetSkin();
        loadout.BodyCamo = GetCamo();

        UndoRedoSystem.RestoreBlockedEvents();
    }

    public void Write(BLRLoadout loadout)
    {
        Primary.Write(loadout.Primary);
        Secondary.Write(loadout.Secondary);

        IsFemale = loadout.IsFemale;

        Tactical = BLRItem.GetMagicCowsID(loadout.Tactical);
        Helmet = BLRItem.GetMagicCowsID(loadout.Helmet);
        UpperBody = BLRItem.GetMagicCowsID(loadout.UpperBody);
        LowerBody = BLRItem.GetMagicCowsID(loadout.LowerBody);

        Camo = BLRItem.GetMagicCowsID(loadout.BodyCamo);
        Skin = BLRItem.GetMagicCowsID(loadout.Avatar, 99);
        Trophy = BLRItem.GetMagicCowsID(loadout.Trophy);


        Gear1 = BLRItem.GetMagicCowsID(loadout.Gear1);
        Gear2 = BLRItem.GetMagicCowsID(loadout.Gear2);
        Gear3 = BLRItem.GetMagicCowsID(loadout.Gear3);
        Gear4 = BLRItem.GetMagicCowsID(loadout.Gear4);

        Taunts = [BLRItem.GetMagicCowsID(loadout.Taunt1, 0), BLRItem.GetMagicCowsID(loadout.Taunt2, 1), BLRItem.GetMagicCowsID(loadout.Taunt3, 2), BLRItem.GetMagicCowsID(loadout.Taunt4, 3), BLRItem.GetMagicCowsID(loadout.Taunt5, 4), BLRItem.GetMagicCowsID(loadout.Taunt6, 5), BLRItem.GetMagicCowsID(loadout.Taunt7, 6), BLRItem.GetMagicCowsID(loadout.Taunt8, 7)];
        Depot = [BLRItem.GetMagicCowsID(loadout.Depot1), BLRItem.GetMagicCowsID(loadout.Depot2, 1), BLRItem.GetMagicCowsID(loadout.Depot3, 2), BLRItem.GetMagicCowsID(loadout.Depot4, 3), BLRItem.GetMagicCowsID(loadout.Depot5, 3)];
        if (WasWrittenTo is not null) { WasWrittenTo(loadout, EventArgs.Empty); }
    }

    public ShareableLoadout ConvertToShareable()
    {
        int[] taunts = new int[Taunts.Length];
        int[] depot = new int[Depot.Length];

        Array.Copy(Taunts, taunts, Taunts.Length);
        Array.Copy(Depot, depot, Depot.Length);

        return new ShareableLoadout() 
        {
            Primary = Primary.ConvertToShareable(),
            Secondary = Secondary.ConvertToShareable(),

            Avatar = Skin,
            Badge = Trophy,
            BodyCamo = Camo,
            
            Female = IsFemale,

            Helmet = Helmet,
            UpperBody = UpperBody,
            LowerBody = LowerBody,

            Tactical = Tactical,

            Gear_R1 = Gear1,
            Gear_R2 = Gear2,
            Gear_L1 = Gear3,
            Gear_L2 = Gear4,

            Taunts = taunts,
            Depot = depot,

            AnnouncerVoice = 63000,
            PlayerVoice = 63050,
            Title = 61000,
            PatchIcon = 64017,
            PatchIconColor = 64752,
            PatchShape = 64640,
            PatchShapeColor = 64762,
            PatchBackground = 64740,
            PatchBackgroundColor = 64762
        };
    }
}