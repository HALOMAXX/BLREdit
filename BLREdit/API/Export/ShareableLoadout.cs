using BLREdit.Export;
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;

using Microsoft.IdentityModel.Tokens;

using PeNet;

using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.Export;

public sealed class ShareableLoadout : IBLRLoadout
{
    public DateTime TimeOfCreation { get; set; } = DateTime.Now;
    public DateTime LastApplied { get; set; } = DateTime.MinValue;
    public DateTime LastModified { get; set; } = DateTime.MinValue;
    public DateTime LastViewed { get; set; } = DateTime.MinValue;
    [JsonPropertyName("Name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("E")] public bool Apply { get; set; } = false;
    [JsonPropertyName("R1")] public ShareableWeapon Primary { get; set; } = new();
    [JsonPropertyName("R2")] public ShareableWeapon Secondary { get; set; } = new();
    [JsonPropertyName("F1")] public bool Female { get; set; } = false;
    [JsonPropertyName("B1")] public bool Bot { get; set; } = false;
    [JsonPropertyName("A1")] public int Avatar { get; set; } = 99;
    [JsonPropertyName("B2")] public int BodyCamo { get; set; } = 0;
    [JsonPropertyName("B3")] public int Badge { get; set; } = 0;
    [JsonPropertyName("B4")] public int ButtPack { get; set; } = 0;
    [JsonPropertyName("D1")] public int[] Depot { get; set; } = new int[5];
    [JsonPropertyName("U1")] public int UpperBody { get; set; } = 0;
    [JsonPropertyName("L1")] public int LowerBody { get; set; } = 0;
    [JsonPropertyName("H1")] public int Helmet { get; set; } = 0;
    [JsonPropertyName("H2")] public int Hanger { get; set; } = 0;

#pragma warning disable CA1707 // Identifiers should not contain underscores
    [JsonPropertyName("G1")] public int Gear_R1 { get; set; } = 0;
    [JsonPropertyName("G2")] public int Gear_R2 { get; set; } = 0;
    [JsonPropertyName("G3")] public int Gear_L1 { get; set; } = 0;
    [JsonPropertyName("G4")] public int Gear_L2 { get; set; } = 0;
#pragma warning restore CA1707 // Identifiers should not contain underscores

    [JsonPropertyName("P1")] public int PatchIcon { get; set; } = 0;
    [JsonPropertyName("P2")] public int PatchIconColor { get; set; } = 0;
    [JsonPropertyName("P3")] public int PatchShape { get; set; } = 0;
    [JsonPropertyName("P4")] public int PatchShapeColor { get; set; } = 0;
    [JsonPropertyName("P5")] public int PatchBackground { get; set; } = 0;
    [JsonPropertyName("P6")] public int PatchBackgroundColor { get; set; } = 0;

    [JsonPropertyName("A2")] public int AnnouncerVoice { get; set; } = 0;
    [JsonPropertyName("P7")] public int PlayerVoice { get; set; } = 0;
    [JsonPropertyName("T3")] public int Title { get; set; } = 0;

    [JsonPropertyName("T1")] public int Tactical { get; set; } = 0;
    [JsonPropertyName("T2")] public int[] Taunts { get; set; } = new int[8];

    public ShareableLoadout()
    { }

    public ShareableLoadout(BLREditLoadout loadout)
    {
        if (loadout is null) { LoggingSystem.FatalLog("loadout was null in ShareableLoadout constructor"); return; }
        Name = loadout.Name;
        Female = loadout.IsFemale.Is;
        Apply = loadout.Apply;
        BodyCamo = BLREditItem.GetMagicCowsID(loadout.BodyCamo);
        UpperBody = BLREditItem.GetMagicCowsID(loadout.UpperBody);
        LowerBody = BLREditItem.GetMagicCowsID(loadout.LowerBody);
        Helmet = BLREditItem.GetMagicCowsID(loadout.Helmet);
        Tactical = BLREditItem.GetMagicCowsID(loadout.Tactical);
        Badge = BLREditItem.GetMagicCowsID(loadout.Trophy);

        Avatar = BLREditItem.GetMagicCowsID(loadout.Avatar, 99);

        Gear_R1 = BLREditItem.GetMagicCowsID(loadout.Gear1);
        Gear_R2 = BLREditItem.GetMagicCowsID(loadout.Gear2);
        Gear_L1 = BLREditItem.GetMagicCowsID(loadout.Gear3);
        Gear_L2 = BLREditItem.GetMagicCowsID(loadout.Gear4);

        Taunts[0] = BLREditItem.GetMagicCowsID(loadout.Taunt1);
        Taunts[1] = BLREditItem.GetMagicCowsID(loadout.Taunt2);
        Taunts[2] = BLREditItem.GetMagicCowsID(loadout.Taunt3);
        Taunts[3] = BLREditItem.GetMagicCowsID(loadout.Taunt4);
        Taunts[4] = BLREditItem.GetMagicCowsID(loadout.Taunt5);
        Taunts[5] = BLREditItem.GetMagicCowsID(loadout.Taunt6);
        Taunts[6] = BLREditItem.GetMagicCowsID(loadout.Taunt7);
        Taunts[7] = BLREditItem.GetMagicCowsID(loadout.Taunt8);

        Depot[0] = BLREditItem.GetMagicCowsID(loadout.Depot1);
        Depot[1] = BLREditItem.GetMagicCowsID(loadout.Depot2);
        Depot[2] = BLREditItem.GetMagicCowsID(loadout.Depot3);
        Depot[3] = BLREditItem.GetMagicCowsID(loadout.Depot4);
        Depot[4] = BLREditItem.GetMagicCowsID(loadout.Depot5);

        PatchIcon = BLREditItem.GetUID(loadout.EmblemIcon);
        PatchIconColor = BLREditItem.GetUID(loadout.EmblemIconColor);
        PatchShape = BLREditItem.GetUID(loadout.EmblemShape);
        PatchShapeColor = BLREditItem.GetUID(loadout.EmblemShapeColor);
        PatchBackground = BLREditItem.GetUID(loadout.EmblemBackground);
        PatchBackgroundColor = BLREditItem.GetUID(loadout.EmblemBackgroundColor);

        AnnouncerVoice = BLREditItem.GetUID(loadout.AnnouncerVoice);
        PlayerVoice = BLREditItem.GetUID(loadout.PlayerVoice);
        Title = BLREditItem.GetUID(loadout.Title);

        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
    }

    public event EventHandler? WasWrittenTo;

    public BLREditLoadout ToBLRLoadout(BLRProfile? profile = null)
    {
        var loadout = new BLREditLoadout(profile);
        loadout.SetLoadout(this, true);
        loadout.Read();
        loadout.CalculateStats();
        return loadout;
    }

    public ShareableLoadout Duplicate()
    {
        var dup = this.Clone();
        //BLRLoadoutStorage.AddNewLoadoutSet($"Duplicate of {dup.Name}", null, dup);
        return dup;
    }

    public ShareableLoadout Clone()
    {
        var clone = new ShareableLoadout()
        {
            Name = Name,
            Apply = Apply,
            Primary = Primary.Clone(),
            Secondary = Secondary.Clone(),
            Avatar = Avatar,
            Badge = Badge,
            BodyCamo = BodyCamo,
            Bot = Bot,
            ButtPack = ButtPack,
            Female = Female,
            Gear_R1 = Gear_R1,
            Gear_R2 = Gear_R2,
            Gear_L1 = Gear_L1,
            Gear_L2 = Gear_L2,
            Hanger = Hanger,
            Helmet = Helmet,
            LowerBody = LowerBody,
            PatchIcon = PatchIcon,
            PatchIconColor = PatchIconColor,
            PatchShape = PatchShape,
            PatchShapeColor = PatchShapeColor,
            PatchBackground = PatchBackground,
            PatchBackgroundColor = PatchBackgroundColor,

            AnnouncerVoice = AnnouncerVoice,
            PlayerVoice = PlayerVoice,
            Title = Title,

            Tactical = Tactical,
            UpperBody = UpperBody,
            Depot = new int[Depot.Length],
            Taunts = new int[Taunts.Length]
        };
        Array.Copy(Depot, clone.Depot, Depot.Length);
        Array.Copy(Taunts, clone.Taunts, Taunts.Length);
        return clone;
    }

    public IBLRWeapon GetPrimaryWeaponInterface()
    {
        return Primary;
    }

    public IBLRWeapon GetSecondaryWeaponInterface()
    {
        return Secondary;
    }

    public void Read(BLREditLoadout loadout)
    {
        if (loadout is null) { LoggingSystem.FatalLog("loadout was null when reading"); return; }
        Primary.Read(loadout.Primary);
        Secondary.Read(loadout.Secondary);
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadLoadout)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;

        loadout.Name = Name;
        loadout.IsFemale.Is = Female;
        loadout.IsBot = Bot;

        loadout.Apply = Apply;

        loadout.Avatar = ImportSystem.GetItemByIDAndType(ImportSystem.AVATARS_CATEGORY, Avatar);

        loadout.BodyCamo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, BodyCamo);

        loadout.Depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[0]);
        loadout.Depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[1]);
        loadout.Depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[2]);
        loadout.Depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[3]);
        loadout.Depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, Depot[4]);

        loadout.Gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_R1);
        loadout.Gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_R2);
        loadout.Gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_L1);
        loadout.Gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, Gear_L2);

        loadout.Helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, Helmet);
        loadout.UpperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, UpperBody);
        loadout.LowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, LowerBody);

        loadout.Tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, Tactical);

        loadout.Taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[0]);
        loadout.Taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[1]);
        loadout.Taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[2]);
        loadout.Taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[3]);
        loadout.Taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[4]);
        loadout.Taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[5]);
        loadout.Taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[6]);
        loadout.Taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, Taunts[7]);

        loadout.EmblemIcon = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_ICON_CATEGORY, PatchIcon);
        loadout.EmblemIconColor = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, PatchIconColor);
        loadout.EmblemShape = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_SHAPE_CATEGORY, PatchShape);
        loadout.EmblemShapeColor = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, PatchShapeColor);
        loadout.EmblemBackground = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_BACKGROUND_CATEGORY, PatchBackground);
        loadout.EmblemBackgroundColor = ImportSystem.GetItemByUIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, PatchBackgroundColor);

        loadout.AnnouncerVoice = ImportSystem.GetItemByUIDAndType(ImportSystem.ANNOUNCER_VOICE_CATEGORY, AnnouncerVoice);
        loadout.PlayerVoice = ImportSystem.GetItemByUIDAndType(ImportSystem.PLAYER_VOICE_CATEGORY, PlayerVoice);
        loadout.Title = ImportSystem.GetItemByUIDAndType(ImportSystem.TITLES_CATEGORY, Title);

        loadout.Trophy = ImportSystem.GetItemByIDAndType(ImportSystem.BADGES_CATEGORY, Badge);
        UndoRedoSystem.RestoreBlockedEvents();
    }

    public void Write(BLREditLoadout loadout)
    {
        if (loadout is null) { LoggingSystem.FatalLog("loadout was null when writing"); return; }
        Primary.Write(loadout.Primary);
        Secondary.Write(loadout.Secondary);
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteLoadout)) return;
        LastModified = DateTime.Now;

        Name = loadout.Name;
        Female = loadout.IsFemale.Is;
        Bot = loadout.IsBot;

        Apply = loadout.Apply;

        Avatar = BLREditItem.GetMagicCowsID(loadout.Avatar, -1);
        BodyCamo = BLREditItem.GetMagicCowsID(loadout.BodyCamo);

        Depot[0] = BLREditItem.GetMagicCowsID(loadout.Depot1);
        Depot[1] = BLREditItem.GetMagicCowsID(loadout.Depot2);
        Depot[2] = BLREditItem.GetMagicCowsID(loadout.Depot3);
        Depot[3] = BLREditItem.GetMagicCowsID(loadout.Depot4);
        Depot[4] = BLREditItem.GetMagicCowsID(loadout.Depot5);

        Gear_R1 = BLREditItem.GetMagicCowsID(loadout.Gear1);
        Gear_R2 = BLREditItem.GetMagicCowsID(loadout.Gear2);
        Gear_L1 = BLREditItem.GetMagicCowsID(loadout.Gear3);
        Gear_L2 = BLREditItem.GetMagicCowsID(loadout.Gear4);

        Helmet = BLREditItem.GetMagicCowsID(loadout.Helmet);
        UpperBody = BLREditItem.GetMagicCowsID(loadout.UpperBody);
        LowerBody = BLREditItem.GetMagicCowsID(loadout.LowerBody);

        Tactical = BLREditItem.GetMagicCowsID(loadout.Tactical);

        Taunts[0] = BLREditItem.GetMagicCowsID(loadout.Taunt1);
        Taunts[1] = BLREditItem.GetMagicCowsID(loadout.Taunt2);
        Taunts[2] = BLREditItem.GetMagicCowsID(loadout.Taunt3);
        Taunts[3] = BLREditItem.GetMagicCowsID(loadout.Taunt4);
        Taunts[4] = BLREditItem.GetMagicCowsID(loadout.Taunt5);
        Taunts[5] = BLREditItem.GetMagicCowsID(loadout.Taunt6);
        Taunts[6] = BLREditItem.GetMagicCowsID(loadout.Taunt7);
        Taunts[7] = BLREditItem.GetMagicCowsID(loadout.Taunt8);

        PatchIcon = BLREditItem.GetUID(loadout.EmblemIcon);
        PatchIconColor = BLREditItem.GetUID(loadout.EmblemIconColor);
        PatchShape = BLREditItem.GetUID(loadout.EmblemShape);
        PatchShapeColor = BLREditItem.GetUID(loadout.EmblemShapeColor);
        PatchBackground = BLREditItem.GetUID(loadout.EmblemBackground);
        PatchBackgroundColor = BLREditItem.GetUID(loadout.EmblemBackgroundColor);

        AnnouncerVoice = BLREditItem.GetUID(loadout.AnnouncerVoice);
        PlayerVoice = BLREditItem.GetUID(loadout.PlayerVoice);
        Title = BLREditItem.GetUID(loadout.Title);

        Badge = BLREditItem.GetMagicCowsID(loadout.Trophy);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(loadout, EventArgs.Empty); }
    }

    public void RegisterWithChildren()
    {
        Primary.Loadout = this;
        Secondary.Loadout = this;
    }
}
