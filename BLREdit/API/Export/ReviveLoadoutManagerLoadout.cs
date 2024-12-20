﻿using BLREdit.Import;
using BLREdit.UI.Views;
using System.Collections.Generic;

namespace BLREdit.API.Export;

//TODO: Change defaults

public class ReviveLoadoutManagerLoadout
{
    public string Name { get; set; } = string.Empty;
    public string PrimaryFriendlyName { get; set; } = string.Empty;
    public string SecondaryFriendlyName { get; set; } = string.Empty;
    public int Title { get; set; } = -1;
    public ReviveLoadoutManagerWeapon Primary { get; set; } = new();
    public ReviveLoadoutManagerWeapon Secondary { get; set; } = new();
    public int WeaponHanger { get; set; } = -1;
    public ReviveLoadoutManagerGear Gear { get; set; } = new();
    public ReviveLoadoutManagerBody Body { get; set; } = new();
    public int[] Depot { get; set; } = new int[5];
    public int[] Taunts { get; set; } = new int[8];
    public ReviveLoadoutManagerEmblem Emblem { get; set; } = new();
    public int DialogAnnouncer { get; set; } = -1;
    public int DialogPlayer { get; set; } = -1;

    public ReviveLoadoutManagerLoadout() { }
    public ReviveLoadoutManagerLoadout(BLREditLoadout loadout, string name)
    {
        Name = name;
        PrimaryFriendlyName = loadout?.Primary?.WeaponDescriptor ?? "Null";
        SecondaryFriendlyName = loadout?.Secondary?.WeaponDescriptor ?? "Null";
        Title = loadout?.Title?.UID ?? -1;
        Primary = new(loadout?.Primary);
        Secondary = new(loadout?.Secondary);
        WeaponHanger = loadout?.Primary?.Tag?.UID ?? -1;
        Gear = new(loadout);
        Body = new(loadout);
        Depot = [ 
            loadout?.Depot1?.UID ?? -1,
            loadout?.Depot2?.UID ?? -1,
            loadout?.Depot3?.UID ?? -1,
            loadout?.Depot4?.UID ?? -1,
            loadout?.Depot5?.UID ?? -1
        ];

        Taunts = [
            loadout?.Taunt1?.UID ?? -1,
            loadout?.Taunt2?.UID ?? -1,
            loadout?.Taunt3?.UID ?? -1,
            loadout?.Taunt4?.UID ?? -1,
            loadout?.Taunt5?.UID ?? -1,
            loadout?.Taunt6?.UID ?? -1,
            loadout?.Taunt7?.UID ?? -1,
            loadout?.Taunt8?.UID ?? -1
        ];

        Emblem = new(loadout);

        DialogAnnouncer = loadout?.AnnouncerVoice?.UID ?? -1;
        DialogPlayer = loadout?.PlayerVoice?.UID ?? -1;
    }

    public BLREditLoadout GetLoadout()
    {
        var loadout = new BLREditLoadout(null)
        {
            Primary = this.Primary.GetWeapon(true),
            Secondary = this.Secondary.GetWeapon(false),

            Helmet = ImportSystem.GetItemByUIDAndType(ImportSystem.HELMETS_CATEGORY, Body.Helmet),
            UpperBody = ImportSystem.GetItemByUIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, Body.UpperBody),
            LowerBody = ImportSystem.GetItemByUIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, Body.LowerBody),

            Trophy = ImportSystem.GetItemByUIDAndType(ImportSystem.BADGES_CATEGORY, Body.Badge),
            BodyCamo = ImportSystem.GetItemByUIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, Body.Camo),
            Avatar = ImportSystem.GetItemByUIDAndType(ImportSystem.AVATARS_CATEGORY, Body.Avatar),

            Tactical = ImportSystem.GetItemByUIDAndType(ImportSystem.TACTICAL_CATEGORY, Gear.Tactical),
            Gear1 = ImportSystem.GetItemByUIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear.R1),
            Gear2 = ImportSystem.GetItemByUIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear.L1),
            Gear3 = ImportSystem.GetItemByUIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear.R2),
            Gear4 = ImportSystem.GetItemByUIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear.L2),

            Depot1 = ImportSystem.GetItemByUIDAndType(ImportSystem.SHOP_CATEGORY, Depot[0]),
            Depot2 = ImportSystem.GetItemByUIDAndType(ImportSystem.SHOP_CATEGORY, Depot[1]),
            Depot3 = ImportSystem.GetItemByUIDAndType(ImportSystem.SHOP_CATEGORY, Depot[2]),
            Depot4 = ImportSystem.GetItemByUIDAndType(ImportSystem.SHOP_CATEGORY, Depot[3]),
            Depot5 = ImportSystem.GetItemByUIDAndType(ImportSystem.SHOP_CATEGORY, Depot[4]),

            Taunt1 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[0]),
            Taunt2 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[1]),
            Taunt3 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[2]),
            Taunt4 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[3]),
            Taunt5 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[4]),
            Taunt6 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[5]),
            Taunt7 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[6]),
            Taunt8 = ImportSystem.GetItemByUIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[7]),

            Title = ImportSystem.GetItemByUIDAndType(ImportSystem.TITLES_CATEGORY, Title),

            AnnouncerVoice = ImportSystem.GetItemByUIDAndType(ImportSystem.ANNOUNCER_VOICE_CATEGORY, DialogAnnouncer),
            PlayerVoice = ImportSystem.GetItemByUIDAndType(ImportSystem.PLAYER_VOICE_CATEGORY, DialogPlayer),

            EmblemIcon = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_ICON_CATEGORY, Emblem.TopIcon),
            EmblemShape = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_SHAPE_CATEGORY, Emblem.MiddleIcon),
            EmblemBackground = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_BACKGROUND_CATEGORY, Emblem.BottomIcon),

            EmblemIconColor = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, Emblem.TopColor),
            EmblemShapeColor = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, Emblem.MiddleColor),
            EmblemBackgroundColor = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, Emblem.BottomColor)
        };
        loadout.IsFemale.Is = Body.Female;
        loadout.Primary.Tag = ImportSystem.GetItemByUIDAndType(ImportSystem.HANGERS_CATEGORY, Gear.Hanger);

        return loadout;
    }
}

public class ReviveLoadoutManagerWeapon
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

    public ReviveLoadoutManagerWeapon() { }
    public ReviveLoadoutManagerWeapon(BLREditWeapon? weapon)
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

    public BLREditWeapon GetWeapon(bool isPrimary)
    {
        return new BLREditWeapon(isPrimary)
        {
            Receiver = ImportSystem.GetItemByUIDAndType(isPrimary ? ImportSystem.PRIMARY_CATEGORY : ImportSystem.SECONDARY_CATEGORY, Receiver),
            Barrel = ImportSystem.GetItemByUIDAndType(ImportSystem.BARRELS_CATEGORY, Barrel),
            Muzzle = ImportSystem.GetItemByUIDAndType(ImportSystem.MUZZELS_CATEGORY, Muzzle),
            Grip = ImportSystem.GetItemByUIDAndType(ImportSystem.GRIPS_CATEGORY, Grip),
            Magazine = ImportSystem.GetItemByUIDAndType(ImportSystem.MAGAZINES_CATEGORY, Magazine),
            Ammo = ImportSystem.GetItemByUIDAndType(ImportSystem.AMMO_CATEGORY, Ammo),
            Stock = ImportSystem.GetItemByUIDAndType(ImportSystem.STOCKS_CATEGORY, Stock),
            Scope = ImportSystem.GetItemByUIDAndType(ImportSystem.SCOPES_CATEGORY, Scope),
            Skin = ImportSystem.GetItemByUIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, Skin)
        };
    }
}

public class ReviveLoadoutManagerGear
{

    //1 Is Upper, 2 Is Lower, L is first slot, R is second slot.
    public int R1 { get; set; } = 255;
    public int R2 { get; set; } = 255;
    public int L1 { get; set; } = 255;
    public int L2 { get; set; } = 255;
    public int Tactical { get; set; } = -1;
    public int Hanger { get; set; } = -1;
    public ReviveLoadoutManagerGear() { }
    public ReviveLoadoutManagerGear(BLREditLoadout? loadout)
    {
        if (loadout is null) return;

        Queue<int> GearUIDList = new();

        GearUIDList.Enqueue(loadout.Gear1?.UID ?? 255);
        GearUIDList.Enqueue(loadout.Gear2?.UID ?? 255);
        GearUIDList.Enqueue(loadout.Gear3?.UID ?? 255);
        GearUIDList.Enqueue(loadout.Gear4?.UID ?? 255);

        if (loadout.UpperBody is not null)
        {
            if (loadout.UpperBody.GearSlots > 0)
            {
                L1 = GearUIDList.Dequeue();
            }
            if (loadout.UpperBody.GearSlots > 1)
            {
                R1 = GearUIDList.Dequeue();
            }
        }
        if(loadout.LowerBody is not null)
        {
            if (loadout.LowerBody.GearSlots > 0)
            {
                L2 = GearUIDList.Dequeue();
            }
            if (loadout.LowerBody.GearSlots > 1)
            {
                R2 = GearUIDList.Dequeue();
            }
        }
        Tactical = loadout.Tactical?.UID ?? -1;
        Hanger = loadout.Primary?.Tag?.UID ?? -1;
    }
}

public class ReviveLoadoutManagerBody
{
    public bool Female { get; set; }
    public int Camo { get; set; } = -1;
    public int UpperBody { get; set; } = -1;
    public int LowerBody { get; set; } = -1;
    public int Helmet { get; set; } = -1;
    public int Badge { get; set; } = -1;
    public int Avatar { get; set; } = -1;
    public int ButtPack { get; set; } = -1;
    public ReviveLoadoutManagerBody() { }
    public ReviveLoadoutManagerBody(BLREditLoadout? loadout)
    {
        Female = loadout?.IsFemale?.Is ?? false;
        Camo = loadout?.BodyCamo?.UID ?? -1;
        UpperBody = loadout?.UpperBody?.UID ?? -1;
        LowerBody = loadout?.LowerBody?.UID ?? -1;
        Helmet = loadout?.Helmet?.UID ?? -1;
        Badge = loadout?.Trophy?.UID ?? -1;
        Avatar = loadout?.Avatar?.UID ?? -1;
        ButtPack = 12015;
    }
}

public class ReviveLoadoutManagerEmblem
{
    public int TopIcon { get; set; } = -1; //Icon
    public int TopColor { get; set; } = -1;
    public int MiddleIcon { get; set; } = -1; //Shape
    public int MiddleColor { get; set; } = -1;
    public int BottomIcon { get; set; } = -1; //Background
    public int BottomColor { get; set; } = -1;
    public ReviveLoadoutManagerEmblem() { }
    public ReviveLoadoutManagerEmblem(BLREditLoadout? loadout)
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