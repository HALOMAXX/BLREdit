using BLREdit.UI.Views;

namespace BLREdit.API.Export;

//TODO: Change defaults

public class LMLoadout
{
    public string Name { get; set; } = string.Empty;
    public string PrimaryFriendlyName { get; set; } = string.Empty;
    public string SecondaryFriendlyName { get; set; } = string.Empty;
    public int Title { get; set; } = -1;
    public LMWeapon Primary { get; set; } = new();
    public LMWeapon Secondary { get; set; } = new();
    public int WeaponHanger { get; set; } = -1;
    public LMGear Gear { get; set; } = new();
    public LMBody Body { get; set; } = new();
    public int[] Depot { get; set; } = new int[5];
    public int[] Taunts { get; set; } = new int[8];
    public LMEmblem Emblem { get; set; } = new();
    public int DialogAnnouncer { get; set; } = -1;
    public int DialogPlayer { get; set; } = -1;

    public LMLoadout() { }
    public LMLoadout(BLRLoadout loadout, string name)
    {
        Name = name;
        PrimaryFriendlyName = loadout.Primary.WeaponDescriptor;
        SecondaryFriendlyName = loadout.Secondary.WeaponDescriptor;
        Title = loadout.Title?.UID ?? -1;
        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
        WeaponHanger = loadout.Primary?.Tag?.UID ?? -1;
        Gear = new(loadout);
        Body = new(loadout);
        Depot = new int[] { 
            loadout.Depot1?.UID ?? -1,
            loadout.Depot2?.UID ?? -1,
            loadout.Depot3?.UID ?? -1,
            loadout.Depot4?.UID ?? -1,
            loadout.Depot5?.UID ?? -1
        };

        Taunts = new int[] {
            loadout.Taunt1?.UID ?? -1,
            loadout.Taunt2?.UID ?? -1,
            loadout.Taunt3?.UID ?? -1,
            loadout.Taunt4?.UID ?? -1,
            loadout.Taunt5?.UID ?? -1,
            loadout.Taunt6?.UID ?? -1,
            loadout.Taunt7?.UID ?? -1,
            loadout.Taunt8?.UID ?? -1
        };

        Emblem = new(loadout);

        DialogAnnouncer = loadout.AnnouncerVoice?.UID ?? -1;
        DialogPlayer = loadout.PlayerVoice?.UID ?? -1;
    }
}

public class LMWeapon
{
    public int Receiver { get; set; } = -1;
    public int Barrel { get; set; } = -1;
    public int Scope { get; set; } = -1;
    public int Grip { get; set; } = -1;
    public int Stock { get; set; } = -1;
    public int Ammo { get; set; } = -1;
    public int Muzzle { get; set; } = -1;
    public int Magazine { get; set; } = -1;
    public int Skin { get; set; } = -1;
    public int Camo { get; set; } = -1;

    public LMWeapon() { }
    public LMWeapon(BLRWeapon weapon)
    {
        Receiver = weapon?.Receiver?.UID ?? -1;
        Barrel = weapon?.Barrel?.UID ?? -1;
        Scope = weapon?.Scope?.UID ?? -1;
        Grip = weapon?.Grip?.UID ?? -1;
        Stock = weapon?.Stock?.UID ?? -1;
        Ammo = weapon?.Ammo?.UID ?? -1;
        Muzzle = weapon?.Muzzle?.UID ?? -1;
        Magazine = weapon?.Magazine?.UID ?? -1;
        Skin = weapon?.Skin?.UID ?? -1;
        Camo = weapon?.Camo?.UID ?? -1;
    }
}

public class LMGear
{
    public int R1 { get; set; } = -1;
    public int R2 { get; set; } = -1;
    public int L1 { get; set; } = -1;
    public int L2 { get; set; } = -1;
    public int Tactical { get; set; } = -1;
    public int Hanger { get; set; } = -1;
    public LMGear() { }
    public LMGear(BLRLoadout loadout)
    {
        R1 = loadout?.Gear1?.UID ?? -1;
        R2 = loadout?.Gear2?.UID ?? -1;
        L1 = loadout?.Gear3?.UID ?? -1;
        L2 = loadout?.Gear4?.UID ?? -1;
        Tactical = loadout?.Tactical?.UID ?? -1;
        Hanger = -1;
    }
}

public class LMBody
{
    public bool Female { get; set; } = false;
    public int Camo { get; set; } = -1;
    public int UpperBody { get; set; } = -1;
    public int LowerBody { get; set; } = -1;
    public int Helmet { get; set; } = -1;
    public int Badge { get; set; } = -1;
    public int Avatar { get; set; } = -1;
    public int ButtPack { get; set; } = -1;
    public LMBody() { }
    public LMBody(BLRLoadout loadout)
    {
        Female = loadout.IsFemale;
        Camo = loadout?.BodyCamo?.UID ?? -1;
        UpperBody = loadout?.UpperBody?.UID ?? -1;
        LowerBody = loadout?.LowerBody?.UID ?? -1;
        Helmet = loadout?.Helmet?.UID ?? -1;
        Badge = loadout?.Trophy?.UID ?? -1;
        Avatar = loadout?.Avatar?.UID ?? -1;
        ButtPack = 12015;
    }
}

public class LMEmblem
{
    public int TopIcon { get; set; } = -1; //Icon
    public int TopColor { get; set; } = -1;
    public int MiddleIcon { get; set; } = -1; //Shape
    public int MiddleColor { get; set; } = -1;
    public int BottomIcon { get; set; } = -1; //Background
    public int BottomColor { get; set; } = -1;
    public LMEmblem() { }
    public LMEmblem(BLRLoadout loadout)
    {
        TopIcon = loadout?.EmblemIcon?.UID ?? -1;
        MiddleIcon = loadout?.EmblemShape?.UID ?? -1;
        BottomIcon = loadout?.EmblemBackground?.UID ?? -1;

        TopColor = loadout?.EmblemIconColor?.UID ?? -1;
        MiddleColor = loadout?.EmblemShapeColor?.UID ?? -1;
        BottomColor = loadout?.EmblemBackgroundColor?.UID ?? -1;
    }
}

//TODO: Change defaults