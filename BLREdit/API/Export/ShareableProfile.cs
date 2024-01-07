using BLREdit.Export;
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BLREdit.API.Export;

public sealed class ShareableProfile(string name = "New Profile") : INotifyPropertyChanged, IBLRProfile
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? WasWrittenTo;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    [JsonIgnore] private string name = name;
    public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }
    public UIBool IsAdvanced { get; set; } = new(false);
    [JsonIgnore] public int TimeOfCreation { get; set; }
    public DateTime LastApplied { get; set; } = DateTime.MinValue;
    public DateTime LastModified { get; set; } = DateTime.MinValue;
    public DateTime LastViewed { get; set; } = DateTime.MinValue;
    public ObservableCollection<ShareableLoadout> Loadouts { get; set; } = new() { MagiCowsLoadout.DefaultLoadout1.ConvertToShareable(), MagiCowsLoadout.DefaultLoadout2.ConvertToShareable(), MagiCowsLoadout.DefaultLoadout3.ConvertToShareable() };
    

    private static readonly Regex CopyWithCount = new(@".* - Copy \([0-9]*\)$");
    private static readonly Regex CopyWithoutCount = new(@".* - Copy$");

    public void RegisterWithChildren()
    {
        foreach (var loadout in Loadouts)
        {
            loadout.RegisterWithChildren(this);
        }
    }

    public void RefreshInfo()
    {
        OnPropertyChanged(nameof(Name));
    }

    public BLRProfile ToBLRProfile()
    {
        var profile = new BLRProfile();
        profile.SetProfile(this, true);
        profile.Read();
        profile.CalculateStats();
        return profile;
    }

    public ShareableProfile Clone()
    {
        string name = Name;

        if (CopyWithCount.IsMatch(Name))
        {
            var lastOpeningBracketIndex = Name.LastIndexOf('(');
            var lastClosingBracketIndex = Name.LastIndexOf(')');
            var numberPartOfName = Name.Substring(lastOpeningBracketIndex+1, lastClosingBracketIndex - (lastOpeningBracketIndex+1));
            var copyNumber = int.Parse(numberPartOfName);
            var cutName = Name.Substring(0, lastOpeningBracketIndex);
            name = cutName + $"({++copyNumber})";
        }
        else if (CopyWithoutCount.IsMatch(Name))
        {
            name += " (1)";
        }
        else
        {
            name += " - Copy";
        }
        

        var dup = new ShareableProfile()
        {
            Name = name,
        };
        dup.Loadouts.Clear();
        foreach (var loadout in Loadouts)
        {
            dup.Loadouts.Add(loadout.Clone());
        }
        return dup;
    }

    public ShareableProfile Duplicate()
    {
        var dup = this.Clone();
        //BLRLoadoutStorage.AddNewLoadoutSet("", null, dup);
        return dup;
    }

    public IBLRLoadout GetLoadout(int index)
    {
        if (Loadouts.Count > index)
        { return Loadouts[index]; }
        else
        { return Loadouts[0]; }
    }

    public void Read(BLRProfile profile)
    {
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadProfile)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All & ~BlockEvents.ReadAll;
        profile.IsAdvanced.Set(IsAdvanced.Is);
        Loadouts[0].Read(profile.Loadout1);
        Loadouts[1].Read(profile.Loadout2);
        Loadouts[2].Read(profile.Loadout3);
        UndoRedoSystem.RestoreBlockedEvents();
        LoggingSystem.PrintElapsedTime($"Profile Read took {"{0}"}ms ({Name})");
    }

    public void Write(BLRProfile profile)
    {
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteProfile)) return;
        LastModified = DateTime.Now;
        IsAdvanced.Set(profile.IsAdvanced.Is);
        Loadouts[0].Write(profile.Loadout1);
        Loadouts[1].Write(profile.Loadout2);
        Loadouts[2].Write(profile.Loadout3);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(profile, EventArgs.Empty); }
        LoggingSystem.PrintElapsedTime($"Profile Write took {"{0}"}ms ({Name})");
    }
}

public sealed class Shareable3LoadoutSet : IBLRProfile
{
    [JsonPropertyName("A")] public UIBool IsAdvanced { get; set; } = new(false);
    [JsonPropertyName("L1")] public ShareableLoadout Loadout1 { get; set; } = new(null);
    [JsonPropertyName("L2")] public ShareableLoadout Loadout2 { get; set; } = new(null);
    [JsonPropertyName("L3")] public ShareableLoadout Loadout3 { get; set; } = new(null);

    public Shareable3LoadoutSet() { }
    public Shareable3LoadoutSet(BLRProfile profile)
    {
        IsAdvanced.Set(profile.IsAdvanced.Is);
        Loadout1 = new ShareableLoadout(profile.Loadout1, null);
        Loadout2 = new ShareableLoadout(profile.Loadout2, null);
        Loadout3 = new ShareableLoadout(profile.Loadout3, null);
    }

    public event EventHandler? WasWrittenTo;

    public BLRProfile ToBLRProfile()
    {
        var profile = new BLRProfile();
        profile.IsAdvanced.Set(IsAdvanced.Is);
        profile.Loadout1 = Loadout1.ToBLRLoadout(profile);
        profile.Loadout2 = Loadout2.ToBLRLoadout(profile);
        profile.Loadout3 = Loadout3.ToBLRLoadout(profile);
        return profile;
    }

    public IBLRLoadout GetLoadout(int index)
    {
        return index switch
        {
            1 => Loadout2,
            2 => Loadout3,
            _ => Loadout1,
        };
    }

    public void Read(BLRProfile profile)
    {
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadProfile)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;
        profile.IsAdvanced.Set(IsAdvanced.Is);
        Loadout1.Read(profile.Loadout1);
        Loadout2.Read(profile.Loadout2);
        Loadout3.Read(profile.Loadout3);
        UndoRedoSystem.RestoreBlockedEvents();
        LoggingSystem.PrintElapsedTime("Profile Read took {0}ms (3L)");
    }

    public void Write(BLRProfile profile)
    {
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteProfile)) return;
        IsAdvanced.Set(profile.IsAdvanced.Is);
        Loadout1.Write(profile.Loadout1);
        Loadout2.Write(profile.Loadout2);
        Loadout3.Write(profile.Loadout3);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(profile, EventArgs.Empty); }
        LoggingSystem.PrintElapsedTime("Profile Write took {0}ms (3L)");
    }
}

public sealed class ShareableLoadout : IBLRLoadout
{
    public DateTime TimeOfCreation { get; set; } = DateTime.Now;
    public DateTime LastApplied { get; set; } = DateTime.MinValue;
    public DateTime LastModified { get; set; } = DateTime.MinValue;
    public DateTime LastViewed { get; set; } = DateTime.MinValue;
    [JsonIgnore] public ShareableProfile? Profile { get; set; } = null;
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

    [JsonPropertyName("G1")] public int Gear_R1 { get; set; } = 0;
    [JsonPropertyName("G2")] public int Gear_R2 { get; set; } = 0;
    [JsonPropertyName("G3")] public int Gear_L1 { get; set; } = 0;
    [JsonPropertyName("G4")] public int Gear_L2 { get; set; } = 0;

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

    public ShareableLoadout(ShareableProfile? profile)
    { Profile = profile; }

    public ShareableLoadout(BLRLoadout loadout, ShareableProfile? profile)
    {
        Name = loadout.Name;
        Profile = profile;
        Female = loadout.IsFemale;
        Apply = loadout.Apply;
        BodyCamo = BLRItem.GetMagicCowsID(loadout.BodyCamo);
        UpperBody = BLRItem.GetMagicCowsID(loadout.UpperBody);
        LowerBody = BLRItem.GetMagicCowsID(loadout.LowerBody);
        Helmet = BLRItem.GetMagicCowsID(loadout.Helmet);
        Tactical = BLRItem.GetMagicCowsID(loadout.Tactical);
        Badge = BLRItem.GetMagicCowsID(loadout.Trophy);

        Avatar = BLRItem.GetMagicCowsID(loadout.Avatar, 99);

        Gear_R1 = BLRItem.GetMagicCowsID(loadout.Gear1);
        Gear_R2 = BLRItem.GetMagicCowsID(loadout.Gear2);
        Gear_L1 = BLRItem.GetMagicCowsID(loadout.Gear3);
        Gear_L2 = BLRItem.GetMagicCowsID(loadout.Gear4);

        Taunts[0] = BLRItem.GetMagicCowsID(loadout.Taunt1);
        Taunts[1] = BLRItem.GetMagicCowsID(loadout.Taunt2);
        Taunts[2] = BLRItem.GetMagicCowsID(loadout.Taunt3);
        Taunts[3] = BLRItem.GetMagicCowsID(loadout.Taunt4);
        Taunts[4] = BLRItem.GetMagicCowsID(loadout.Taunt5);
        Taunts[5] = BLRItem.GetMagicCowsID(loadout.Taunt6);
        Taunts[6] = BLRItem.GetMagicCowsID(loadout.Taunt7);
        Taunts[7] = BLRItem.GetMagicCowsID(loadout.Taunt8);

        Depot[0] = BLRItem.GetMagicCowsID(loadout.Depot1);
        Depot[1] = BLRItem.GetMagicCowsID(loadout.Depot2);
        Depot[2] = BLRItem.GetMagicCowsID(loadout.Depot3);
        Depot[3] = BLRItem.GetMagicCowsID(loadout.Depot4);
        Depot[4] = BLRItem.GetMagicCowsID(loadout.Depot5);

        PatchIcon = BLRItem.GetUID(loadout.EmblemIcon);
        PatchIconColor = BLRItem.GetUID(loadout.EmblemIconColor);
        PatchShape = BLRItem.GetUID(loadout.EmblemShape);
        PatchShapeColor = BLRItem.GetUID(loadout.EmblemShapeColor);
        PatchBackground = BLRItem.GetUID(loadout.EmblemBackground);
        PatchBackgroundColor = BLRItem.GetUID(loadout.EmblemBackgroundColor);

        AnnouncerVoice = BLRItem.GetUID(loadout.AnnouncerVoice);
        PlayerVoice = BLRItem.GetUID(loadout.PlayerVoice);
        Title = BLRItem.GetUID(loadout.Title);

        Primary = new(loadout.Primary);
        Secondary = new(loadout.Secondary);
    }

    public event EventHandler? WasWrittenTo;

    public BLRLoadout ToBLRLoadout(BLRProfile? profile = null)
    {
        var loadout = new BLRLoadout(profile);
        loadout.SetLoadout(this, true);
        loadout.Read();
        loadout.CalculateStats();
        return loadout;
    }

    public ShareableLoadout Duplicate()
    {
        var dup = this.Clone();
        BLRLoadoutStorage.AddNewLoadoutSet($"Duplicate of {dup.Name}", null, dup);
        return dup;
    }

    public ShareableLoadout Clone()
    {
        var clone = new ShareableLoadout(null)
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
        Primary.Read(loadout.Primary);
        Secondary.Read(loadout.Secondary);
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadLoadout)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;

        loadout.Name = Name.IsNullOrEmpty() ? $"Loadout {Profile?.Loadouts.IndexOf(this) + 1}" : Name;
        loadout.IsFemale = Female;
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

    public void Write(BLRLoadout loadout)
    {
        Primary.Write(loadout.Primary);
        Secondary.Write(loadout.Secondary);
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteLoadout)) return;
        if (Profile is not null) { Profile.LastModified = DateTime.Now; }

        Name = loadout.Name;
        Female = loadout.IsFemale;
        Bot = loadout.IsBot;

        Apply = loadout.Apply;

        Avatar = BLRItem.GetMagicCowsID(loadout.Avatar, -1);
        BodyCamo = BLRItem.GetMagicCowsID(loadout.BodyCamo);

        Depot[0] = BLRItem.GetMagicCowsID(loadout.Depot1);
        Depot[1] = BLRItem.GetMagicCowsID(loadout.Depot2);
        Depot[2] = BLRItem.GetMagicCowsID(loadout.Depot3);
        Depot[3] = BLRItem.GetMagicCowsID(loadout.Depot4);
        Depot[4] = BLRItem.GetMagicCowsID(loadout.Depot5);

        Gear_R1 = BLRItem.GetMagicCowsID(loadout.Gear1);
        Gear_R2 = BLRItem.GetMagicCowsID(loadout.Gear2);
        Gear_L1 = BLRItem.GetMagicCowsID(loadout.Gear3);
        Gear_L2 = BLRItem.GetMagicCowsID(loadout.Gear4);

        Helmet = BLRItem.GetMagicCowsID(loadout.Helmet);
        UpperBody = BLRItem.GetMagicCowsID(loadout.UpperBody);
        LowerBody = BLRItem.GetMagicCowsID(loadout.LowerBody);

        Tactical = BLRItem.GetMagicCowsID(loadout.Tactical);

        Taunts[0] = BLRItem.GetMagicCowsID(loadout.Taunt1);
        Taunts[1] = BLRItem.GetMagicCowsID(loadout.Taunt2);
        Taunts[2] = BLRItem.GetMagicCowsID(loadout.Taunt3);
        Taunts[3] = BLRItem.GetMagicCowsID(loadout.Taunt4);
        Taunts[4] = BLRItem.GetMagicCowsID(loadout.Taunt5);
        Taunts[5] = BLRItem.GetMagicCowsID(loadout.Taunt6);
        Taunts[6] = BLRItem.GetMagicCowsID(loadout.Taunt7);
        Taunts[7] = BLRItem.GetMagicCowsID(loadout.Taunt8);

        PatchIcon = BLRItem.GetUID(loadout.EmblemIcon);
        PatchIconColor = BLRItem.GetUID(loadout.EmblemIconColor);
        PatchShape = BLRItem.GetUID(loadout.EmblemShape);
        PatchShapeColor = BLRItem.GetUID(loadout.EmblemShapeColor);
        PatchBackground = BLRItem.GetUID(loadout.EmblemBackground);
        PatchBackgroundColor = BLRItem.GetUID(loadout.EmblemBackgroundColor);

        AnnouncerVoice = BLRItem.GetUID(loadout.AnnouncerVoice);
        PlayerVoice = BLRItem.GetUID(loadout.PlayerVoice);
        Title = BLRItem.GetUID(loadout.Title);

        Badge = BLRItem.GetMagicCowsID(loadout.Trophy);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(loadout, EventArgs.Empty); }
    }

    public void RegisterWithChildren(ShareableProfile? shareableProfile)
    {
        Profile = shareableProfile;
        Primary.Profile = shareableProfile;
        Secondary.Profile = shareableProfile;
        Primary.Loadout = this;
        Secondary.Loadout = this;
    }
}

public sealed class ShareableWeapon : IBLRWeapon
{
    [JsonIgnore] public ShareableProfile? Profile { get; set; } = null;
    [JsonIgnore] public ShareableLoadout? Loadout { get; set; } = null;
    [JsonPropertyName("A1")] public int Ammo { get; set; } = 0;
    [JsonPropertyName("B1")] public int Barrel { get; set; } = 0;
    [JsonPropertyName("C1")] public int Camo { get; set; } = 0;
    [JsonPropertyName("G1")] public int Grip { get; set; } = 0;
    [JsonPropertyName("M1")] public int Muzzle { get; set; } = 0;
    [JsonPropertyName("M2")] public int Magazine { get; set; } = 0;
    [JsonPropertyName("R1")] public int Receiver { get; set; } = 1;
    [JsonPropertyName("S1")] public int Scope { get; set; } = 0;
    [JsonPropertyName("S2")] public int Stock { get; set; } = 0;
    [JsonPropertyName("S3")] public int Skin { get; set; } = -1;
    [JsonPropertyName("T1")] public int Tag { get; set; } = 0;

    private static Dictionary<string, PropertyInfo> Properties { get; } = GetAllProperties();

    public event EventHandler? WasWrittenTo;

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
        foreach (var part in BLRWeapon.WeaponPartInfo)
        {
            if (Properties.TryGetValue(part.Name, out PropertyInfo info))
            {
                info.SetValue(this, BLRItem.GetMagicCowsID((BLRItem)part.GetValue(weapon)));
            }
        }
    }

    public BLRWeapon ToBLRWeapon(bool isPrimary, BLRLoadout? loadout = null) 
    {
        return new BLRWeapon(isPrimary, loadout, this, true);
    }

    public ShareableWeapon Clone()
    {
        return new()
        { 
            Ammo = Ammo,
            Barrel = Barrel,
            Camo = Camo,
            Grip = Grip,
            Magazine = Magazine,
            Muzzle = Muzzle,
            Receiver = Receiver,
            Scope = Scope,
            Skin = Skin,
            Stock = Stock,
            Tag = Tag
        };
    }

    public void Read(BLRWeapon weapon)
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadWeapon)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;
        weapon.Receiver = ImportSystem.GetItemByIDAndType(weapon.IsPrimary ? ImportSystem.PRIMARY_CATEGORY : ImportSystem.SECONDARY_CATEGORY, Receiver);
        weapon.Barrel = ImportSystem.GetItemByIDAndType(ImportSystem.BARRELS_CATEGORY, Barrel);
        weapon.Muzzle = ImportSystem.GetItemByIDAndType(ImportSystem.MUZZELS_CATEGORY, Muzzle);
        weapon.Magazine = ImportSystem.GetItemByIDAndType(ImportSystem.MAGAZINES_CATEGORY, Magazine);
        weapon.Stock = ImportSystem.GetItemByIDAndType(ImportSystem.STOCKS_CATEGORY, Stock);
        weapon.Scope = ImportSystem.GetItemByIDAndType(ImportSystem.SCOPES_CATEGORY, Scope);
        weapon.Grip = ImportSystem.GetItemByIDAndType(ImportSystem.GRIPS_CATEGORY, Grip);
        weapon.Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, Ammo);
        weapon.Tag = ImportSystem.GetItemByIDAndType(ImportSystem.HANGERS_CATEGORY, Tag);
        weapon.Camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, Camo);
        weapon.Skin = ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, Skin);
        UndoRedoSystem.RestoreBlockedEvents();
    }

    public void Write(BLRWeapon weapon)
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteWeapon)) return;
        if (Profile is not null) { Profile.LastModified = DateTime.Now; }
        Receiver = BLRItem.GetMagicCowsID(weapon.Receiver, -1);
        Barrel = BLRItem.GetMagicCowsID(weapon.Barrel, -1);
        Muzzle = BLRItem.GetMagicCowsID(weapon.Muzzle, -1);
        Magazine = BLRItem.GetMagicCowsID(weapon.Magazine, -1);
        Stock = BLRItem.GetMagicCowsID(weapon.Stock, -1);
        Scope = BLRItem.GetMagicCowsID(weapon.Scope, -1);
        Grip = BLRItem.GetMagicCowsID(weapon.Grip, -1);
        Ammo = BLRItem.GetMagicCowsID(weapon.Ammo, -1);
        Tag = BLRItem.GetMagicCowsID(weapon.Tag, -1);
        Camo = BLRItem.GetMagicCowsID(weapon.Camo, -1);
        Skin = BLRItem.GetMagicCowsID(weapon.Skin, -1);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(weapon, EventArgs.Empty); }
    }
}