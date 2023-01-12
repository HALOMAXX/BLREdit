using BLREdit.Import;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.API.Export;

public sealed class ShareableProfile
{
    [JsonPropertyName("L1")] public ShareableLoadout Loadout1 { get; set; } = new();
    [JsonPropertyName("L2")] public ShareableLoadout Loadout2 { get; set; } = new();
    [JsonPropertyName("L3")] public ShareableLoadout Loadout3 { get; set; } = new();

    public ShareableProfile() { }
    public ShareableProfile(BLRProfile profile)
    { 
        Loadout1 = new ShareableLoadout(profile.Loadout1);
        Loadout2 = new ShareableLoadout(profile.Loadout2);
        Loadout3 = new ShareableLoadout(profile.Loadout3);
    }

    public BLRProfile ToBLRProfile()
    {
        var profile = new BLRProfile
        {
            Loadout1 = Loadout1.ToBLRLoadout(),
            Loadout2 = Loadout2.ToBLRLoadout(),
            Loadout3 = Loadout3.ToBLRLoadout()
        };
        return profile;
    }
}

public sealed class ShareableLoadout
{
    [JsonPropertyName("R1")] public ShareableWeapon Primary { get; set; } = new();
    [JsonPropertyName("R2")] public ShareableWeapon Secondary { get; set; } = new();
    [JsonPropertyName("F1")] public bool Female { get; set; } = false;
    [JsonPropertyName("B1")] public bool Bot { get; set; } = false;
    [JsonPropertyName("A1")] public int Avatar { get; set; } = -1;
    [JsonPropertyName("B2")] public int BodyCamo { get; set; } = 0;
    [JsonPropertyName("B3")] public int Badge { get; set; } = 0;
    [JsonPropertyName("B4")] public int ButtPack { get; set; } = 0;
    [JsonPropertyName("D1")] public int[] Depot { get; set; } = new int[5];
    [JsonPropertyName("U1")] public int UpperBody { get; set; } = 0;
    [JsonPropertyName("L1")] public int LowerBody { get; set; } = 0;
    [JsonPropertyName("H1")] public int Helmet { get; set; } = 0;
    [JsonPropertyName("H2")] public int Hanger { get; set; } = 0;

    [JsonPropertyName("G1")] public int Gear_R1 { get; set; } = 0;
    [JsonPropertyName("G2")] public int Gear_R2 { get; set; } = 0;
    [JsonPropertyName("G3")] public int Gear_L1 { get; set; } = 0;
    [JsonPropertyName("G4")] public int Gear_L2 { get; set; } = 0;

    [JsonPropertyName("P1")] public int PatchIcon { get; set; } = 0;
    [JsonPropertyName("P2")] public int PatchIconColor { get; set; } = 0;
    [JsonPropertyName("P3")] public int PatchShape { get; set; } = 0;
    [JsonPropertyName("P4")] public int PatchShapeColor { get; set; } = 0;
    [JsonPropertyName("T1")] public int Tactical { get; set; } = 0;
    [JsonPropertyName("T2")] public int[] Taunts { get; set; } = new int[8];

    public ShareableLoadout()
    { }

    public ShareableLoadout(BLRLoadout loadout)
    {
        Female = loadout.IsFemale;
        BodyCamo = loadout.BodyCamo.GetMagicCowsID();
        UpperBody = loadout.UpperBody.GetMagicCowsID();
        LowerBody = loadout.LowerBody.GetMagicCowsID();
        Helmet = loadout.Helmet.GetMagicCowsID();
        Tactical = loadout.Tactical.GetMagicCowsID();
        Badge = loadout.Trophy.GetMagicCowsID();

        Avatar = loadout.Avatar?.GetMagicCowsID() ?? -1;

        Gear_R1 = loadout.Gear1.GetMagicCowsID();
        Gear_R2 = loadout.Gear2.GetMagicCowsID();
        Gear_L1 = loadout.Gear3.GetMagicCowsID();
        Gear_L2 = loadout.Gear4.GetMagicCowsID();

        Taunts[0] = loadout.Taunt1.GetMagicCowsID();
        Taunts[1] = loadout.Taunt2.GetMagicCowsID();
        Taunts[2] = loadout.Taunt3.GetMagicCowsID();
        Taunts[3] = loadout.Taunt4.GetMagicCowsID();
        Taunts[4] = loadout.Taunt5.GetMagicCowsID();
        Taunts[5] = loadout.Taunt6.GetMagicCowsID();
        Taunts[6] = loadout.Taunt7.GetMagicCowsID();
        Taunts[7] = loadout.Taunt8.GetMagicCowsID();

        Depot[0] = loadout.Depot1.GetMagicCowsID();
        Depot[1] = loadout.Depot2.GetMagicCowsID();
        Depot[2] = loadout.Depot3.GetMagicCowsID();
        Depot[3] = loadout.Depot4.GetMagicCowsID();
        Depot[4] = loadout.Depot5.GetMagicCowsID();

        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
    }

    public BLRLoadout ToBLRLoadout()
    {
        var loadout = new BLRLoadout
        {
            IsFemale = Female,
            IsBot = Bot,

            Avatar = ImportSystem.GetItemByIDAndType(ImportSystem.AVATARS_CATEGORY, Avatar),

            BodyCamo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, BodyCamo),

            Depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[0]),
            Depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[1]),
            Depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[2]),
            Depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[3]),
            Depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[4]),

            Gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_R1),
            Gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_R2),
            Gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_L1),
            Gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_L2),

            Helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, Helmet),
            LowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, LowerBody),

            Tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, Tactical),

            Taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[0]),
            Taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[1]),
            Taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[2]),
            Taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[3]),
            Taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[4]),
            Taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[5]),
            Taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[6]),
            Taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[7]),

            Trophy = ImportSystem.GetItemByIDAndType(ImportSystem.BADGES_CATEGORY, Badge),
            UpperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, UpperBody)
        };

        loadout.Primary = Primary.ToBLRWeapon(true, loadout);
        loadout.Secondary = Secondary.ToBLRWeapon(false, loadout);

        return loadout;
    }
}

public sealed class ShareableWeapon
{
    [JsonPropertyName("A1")] public int Ammo { get; set; } = 0;
    [JsonPropertyName("B1")] public int Barrel { get; set; } = 0;
    [JsonPropertyName("C1")] public int Camo { get; set; } = 0;
    [JsonPropertyName("G1")] public int Grip { get; set; } = 0;
    [JsonPropertyName("M1")] public int Muzzle { get; set; } = 0;
    [JsonPropertyName("M2")] public int Magazine { get; set; } = 0;
    [JsonPropertyName("R1")] public int Reciever { get; set; } = 1;
    [JsonPropertyName("S1")] public int Scope { get; set; } = 0;
    [JsonPropertyName("S2")] public int Stock { get; set; } = 0;
    [JsonPropertyName("S3")] public int Skin { get; set; } = -1;
    [JsonPropertyName("T1")] public int Tag { get; set; } = 0;

    private static Dictionary<string, PropertyInfo> Properties { get; } = GetAllProperties();
    private static Dictionary<string, PropertyInfo> GetAllProperties()
    {
        var props = new Dictionary<string, PropertyInfo>();
        var properties = typeof(ShareableWeapon).GetProperties().ToArray();
        foreach (var prop in properties)
        {
            props.Add(prop.Name, prop);
        }
        return props;
    }
    public ShareableWeapon() { }

    /// <summary>
    /// Creates a Loadout-Manager readable Weapon
    /// </summary>
    /// <param name="weapon"></param>
    public ShareableWeapon(BLRWeapon weapon)
    {
        foreach (var part in BLRWeapon.WeaponParts)
        {
            if (Properties.TryGetValue(part.Name, out PropertyInfo info))
            {
                info.SetValue(this, ((BLRItem)part.GetValue(weapon))?.GetMagicCowsID() ?? -1);
            }
        }
    }

    public BLRWeapon ToBLRWeapon(bool isPrimary, BLRLoadout loadout) 
    {
        return new BLRWeapon(isPrimary, loadout, this);
    }
}