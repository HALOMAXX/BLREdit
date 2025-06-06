﻿using BLREdit.Export;
using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media;

using static BLREdit.API.Utils.HelperFunctions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace BLREdit.UI.Views;

public sealed class BLREditLoadout : INotifyPropertyChanged
{
    private IBLRLoadout? _loadout;
    public IBLRLoadout? InternalLoadout { get { return _loadout; } }

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    public BLREditWeapon Primary { get; set; }
    public BLREditWeapon Secondary { get; set; }

    private bool isChanged;
    
    private string name = "";
    public string Name { get { return name; } set { name = value; WriteToBackingStructure(); } }
    [JsonIgnore] public bool IsChanged { get { return isChanged; } set { isChanged = value; OnPropertyChanged(); } }

    private void ItemChanged([CallerMemberName] string? propertyName = null)
    {
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteLoadout)) WriteToBackingStructure();
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Calculate)) CalculateStats();
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) OnPropertyChanged(propertyName);
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) IsChanged = true;
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) OnPropertyChanged(nameof(LoadoutReport));
    }
    #endregion Event

    static readonly Type loadoutType = typeof(BLREditLoadout);

    public BLRProfile? Profile { get; private set; }

    public UIBool IsAdvanced { get; set; } = new(false);

    public static PropertyInfo[] LoadoutPartInfo { get; } = ([.. (from property in loadoutType.GetProperties() where Attribute.IsDefined(property, typeof(BLRItemAttribute)) orderby ((BLRItemAttribute)property.GetCustomAttributes(typeof(BLRItemAttribute), false).Single()).PropertyOrder select property)]);
    private static readonly Dictionary<string?, PropertyInfo> LoadoutPartInfoDictonary = GetLoadoutPartPropertyInfo();
    private static Dictionary<string?, PropertyInfo> GetLoadoutPartPropertyInfo()
    {
        var dict = new Dictionary<string?, PropertyInfo>();
        foreach (var sett in LoadoutPartInfo)
        {
            dict.Add(sett.Name, sett);
        }
        return dict;
    }

    private readonly Dictionary<int, BLREditItem?> LoadoutParts = [];

    private BLREditItem? GetValueOf([CallerMemberName] string? name = null)
    {
        if (string.IsNullOrEmpty(name)) return null;

        var property = LoadoutPartInfoDictonary[name];
        var attribute = property.GetCustomAttribute<BLRItemAttribute>();
        if (LoadoutParts.TryGetValue(attribute.PropertyOrder, out var value))
        { return value; }
        else
        { return null; }
    }

    private LoadoutErrorReport? loadoutReport;
    public LoadoutErrorReport LoadoutReport { get { if (loadoutReport is null || isChanged) { loadoutReport = GenerateLoadoutReport(); } return (LoadoutErrorReport)loadoutReport; } }

    /// <summary>
    /// Check if the loadout is vanilla conform
    /// </summary>
    /// <returns>true if its valid and flase if its invalid</returns>
    public bool ValidateLoadout(ref string message)
    {
        bool valid = true;

        message += $"\n{Name}:";
        message += $"\n\tPrimary:";
        if (!Primary.ValidateWeapon(ref message)) valid = false;
        message += $"\n\tSecondary:";
        if (!Secondary.ValidateWeapon(ref message)) valid = false;


        bool gear = true;
        message += $"\n\tGear:";
        string attach = "";
        if (!(Helmet?.IsValidFor(null) ?? false)) { gear = false; attach += $"\n\t\tHelmet is invalid"; }
        if (!(UpperBody?.IsValidFor(null) ?? false)) { gear = false; attach += $"\n\t\tUpperBody is invalid"; }
        if (!(LowerBody?.IsValidFor(null) ?? false)) { gear = false; attach += $"\n\t\tLowerBody is invalid"; }

        if (!(Tactical?.IsValidFor(null) ?? true)) { gear = false; attach += $"\n\t\tTactical is invalid"; }
        if (!(Trophy?.IsValidFor(null) ?? true)) { gear = false; attach += $"\n\t\tTrophy is invalid"; }
        if (!(Avatar?.IsValidFor(null) ?? true)) { gear = false; attach += $"\n\t\tAvatar is invalid"; }
        if (!(BodyCamo?.IsValidFor(null) ?? false)) { gear = false; attach += $"\n\t\tBodyCamo is invalid"; }

        if (!(Gear1?.IsValidFor(null) ?? true)) { gear = false; attach += $"\n\t\tGear1 is invalid"; }
        if (!(Gear2?.IsValidFor(null) ?? true)) { gear = false; attach += $"\n\t\tGear2 is invalid"; }
        if (!(Gear3?.IsValidFor(null) ?? true)) { gear = false; attach += $"\n\t\tGear3 is invalid"; }
        if (!(Gear4?.IsValidFor(null) ?? true)) { gear = false; attach += $"\n\t\tGear4 is invalid"; }

        if (HasDuplicatedGear) { gear = false; attach += $"\n\t\tGear duplicates are not allowed!"; }

        if (gear)
        {
            message += " ✔️" + attach;
        }
        else
        {
            message += " ❌" + attach;
            valid = false;
        }

        bool extra = true;
        message += $"\n\tExtra:";
        attach = "";
        if (!(Depot1?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tDepot1 is invalid"; }
        if (!(Depot2?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tDepot2 is invalid"; }
        if (!(Depot3?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tDepot3 is invalid"; }
        if (!(Depot4?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tDepot4 is invalid"; }
        if (!(Depot5?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tDepot5 is invalid"; }

        if (!(Taunt1?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt1 is invalid"; }
        if (!(Taunt2?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt2 is invalid"; }
        if (!(Taunt3?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt3 is invalid"; }
        if (!(Taunt4?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt4 is invalid"; }
        if (!(Taunt5?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt5 is invalid"; }
        if (!(Taunt6?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt6 is invalid"; }
        if (!(Taunt7?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt7 is invalid"; }
        if (!(Taunt8?.IsValidFor(null) ?? true)) { extra = false; attach += $"\n\t\tTaunt8 is invalid"; }

        if (extra)
        {
            message += " ✔️" + attach;
        }
        else
        {
            message += " ❌" + attach;
            valid = false;
        }

        return valid;
    }

    public bool HasDuplicatedGear { get { return LoadoutReport.GearReport.HasDuplicates; } }

    private void SetValueOf(BLREditItem? value, BLREditItem? defaultItem = null, [CallerMemberName] string? name = null)
    {
        if (string.IsNullOrEmpty(name)) return;
        var property = LoadoutPartInfoDictonary[name];
        var attribute = property.GetCustomAttribute<BLRItemAttribute>();
        if (value is not null && !attribute.ItemType.Contains(value.Category)) { return; }
        if (value is null && (name == nameof(Helmet) || name == nameof(UpperBody) || name == nameof(LowerBody) || name == nameof(BodyCamo))) { value = defaultItem; }
        if (IsAdvanced.IsNot && value is null && defaultItem is not null) { value = defaultItem; }
        BLREditItem? gear1 = Gear1, gear2 = Gear2, gear3 = Gear3, gear4 = Gear4;
        switch ($"IsValid{name}")
        {
            case nameof(IsValidGear1):
                gear1 = value;
                break;
            case nameof(IsValidGear2):
                gear2 = value;
                break;
            case nameof(IsValidGear3):
                gear3 = value;
                break;
            case nameof(IsValidGear4):
                gear4 = value;
                break;
        }
        bool isValid = BLREditItem.IsValidFor(value, null);
        switch ($"IsValid{name}")
        {
            case nameof(IsValidHelmet):
                IsValidHelmet.Set(isValid);
                break;
            case nameof(IsValidUpperBody):
                IsValidUpperBody.Set(isValid);
                break;
            case nameof(IsValidLowerBody):
                IsValidLowerBody.Set(isValid);
                break;
            case nameof(IsValidTactical):
                IsValidTactical.Set(isValid);
                break;
            case nameof(IsValidGear1):
            case nameof(IsValidGear2):
            case nameof(IsValidGear3):
            case nameof(IsValidGear4):
                bool gear1valid = BLREditItem.IsValidFor(gear1, null), gear2valid = BLREditItem.IsValidFor(gear2, null), gear3valid = BLREditItem.IsValidFor(gear3, null), gear4valid = BLREditItem.IsValidFor(gear4, null);
                if (gear4 is not null && gear4.UID != 12015)
                {
                    if (gear1 is not null && gear1.UID == gear4.UID) { gear4valid = false; }
                    if (gear2 is not null && gear2.UID == gear4.UID) { gear4valid = false; }
                    if (gear3 is not null && gear3.UID == gear4.UID) { gear4valid = false; }
                }

                if (gear3 is not null && gear3.UID != 12015)
                {
                    if (gear1 is not null && gear1.UID == gear3.UID) { gear3valid = false; }
                    if (gear2 is not null && gear2.UID == gear3.UID) { gear3valid = false; }
                    if (gear4 is not null && gear4.UID == gear3.UID) { gear3valid = false; }
                }

                if (gear2 is not null && gear2.UID != 12015)
                {
                    if (gear1 is not null && gear1.UID == gear2.UID) { gear2valid = false; }
                    if (gear3 is not null && gear3.UID == gear2.UID) { gear2valid = false; }
                    if (gear4 is not null && gear4.UID == gear2.UID) { gear2valid = false; }
                }

                if (gear1 is not null && gear1.UID != 12015)
                {
                    if (gear2 is not null && gear2.UID == gear1.UID) { gear1valid = false; }
                    if (gear3 is not null && gear3.UID == gear1.UID) { gear1valid = false; }
                    if (gear4 is not null && gear4.UID == gear1.UID) { gear1valid = false; }
                }

                if (IsAdvanced.IsNot && (!gear1valid || !gear2valid || !gear3valid || !gear4valid))
                {
                    MainWindow.ShowAlert("Duplicate Gears are not Allowed!");
                    return;
                }

                IsValidGear1.Set(gear1valid);
                IsValidGear2.Set(gear2valid);
                IsValidGear3.Set(gear3valid);
                IsValidGear4.Set(gear4valid);
                break;
            case nameof(IsValidBodyCamo):
                IsValidBodyCamo.Set(isValid);
                break;
            case nameof(IsValidAvatar):
                IsValidAvatar.Set(isValid);
                break;
            case nameof(IsValidTrophy):
                IsValidTrophy.Set(isValid);
                break;
        }
        if (LoadoutParts.ContainsKey(attribute.PropertyOrder))
        {
            LoadoutParts[attribute.PropertyOrder] = value;
        }
        else
        {
            LoadoutParts.Add(attribute.PropertyOrder, value);
        }
        ItemChanged(name);
    }
    #region Gear
    [BLRItem(ImportSystem.HELMETS_CATEGORY)] public BLREditItem? Helmet { get { return GetValueOf(); } set { SetValueOf(value, MagiCowsLoadout.DefaultLoadout1.GetHelmetBLRItem()); } }
    public UIBool IsValidHelmet { get; } = new(true);
    [BLRItem(ImportSystem.UPPER_BODIES_CATEGORY)] public BLREditItem? UpperBody { get { return GetValueOf(); } set { SetValueOf(value, MagiCowsLoadout.DefaultLoadout1.GetUpperBodyBLRItem()); } }
    public UIBool IsValidUpperBody { get; } = new(true);
    [BLRItem(ImportSystem.LOWER_BODIES_CATEGORY)] public BLREditItem? LowerBody { get { return GetValueOf(); } set { SetValueOf(value, MagiCowsLoadout.DefaultLoadout1.GetLowerBodyBLRItem()); } }
    public UIBool IsValidLowerBody { get; } = new(true);
    [BLRItem(ImportSystem.TACTICAL_CATEGORY)] public BLREditItem? Tactical { get { return GetValueOf(); } set { SetValueOf(value); } }
    public UIBool IsValidTactical { get; } = new(true);
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLREditItem? Gear1 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, MagiCowsLoadout.DefaultLoadout1.Gear1)); } }
    public UIBool IsValidGear1 { get; } = new(true);
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLREditItem? Gear2 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, MagiCowsLoadout.DefaultLoadout1.Gear2)); } }
    public UIBool IsValidGear2 { get; } = new(true);
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLREditItem? Gear3 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, MagiCowsLoadout.DefaultLoadout1.Gear3)); } }
    public UIBool IsValidGear3 { get; } = new(true);
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLREditItem? Gear4 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, MagiCowsLoadout.DefaultLoadout1.Gear4)); } }
    public UIBool IsValidGear4 { get; } = new(true);
    [BLRItem(ImportSystem.CAMOS_BODIES_CATEGORY)] public BLREditItem? BodyCamo { get { return GetValueOf(); } set { SetValueOf(value, MagiCowsLoadout.DefaultLoadout1.GetCamoBLRItem()); } }
    public UIBool IsValidBodyCamo { get; } = new(true);
    [BLRItem(ImportSystem.AVATARS_CATEGORY)] public BLREditItem? Avatar { get { return GetValueOf(); } set { SetValueOf(value); OnPropertyChanged(nameof(HasAvatar)); } }
    public UIBool IsValidAvatar { get; } = new(true);
    [BLRItem(ImportSystem.BADGES_CATEGORY)] public BLREditItem? Trophy { get { return GetValueOf(); } set { SetValueOf(value); } }
    public UIBool IsValidTrophy { get; } = new(true);
    #endregion Gear

    [BLRItem(ImportSystem.EMBLEM_BACKGROUND_CATEGORY)] public BLREditItem? EmblemBackground { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.EMBLEM_COLOR_CATEGORY)] public BLREditItem? EmblemBackgroundColor { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.EMBLEM_SHAPE_CATEGORY)] public BLREditItem? EmblemShape { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.EMBLEM_COLOR_CATEGORY)] public BLREditItem? EmblemShapeColor { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.EMBLEM_ICON_CATEGORY)] public BLREditItem? EmblemIcon { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.EMBLEM_COLOR_CATEGORY)] public BLREditItem? EmblemIconColor { get { return GetValueOf(); } set { SetValueOf(value); } }


    [BLRItem(ImportSystem.ANNOUNCER_VOICE_CATEGORY)] public BLREditItem? AnnouncerVoice { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.PLAYER_VOICE_CATEGORY)] public BLREditItem? PlayerVoice { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.TITLES_CATEGORY)] public BLREditItem? Title { get { return GetValueOf(); } set { SetValueOf(value); } }

    #region Depot
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLREditItem? Depot1 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 0)); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLREditItem? Depot2 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 1)); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLREditItem? Depot3 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 2)); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLREditItem? Depot4 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 3)); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLREditItem? Depot5 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 4)); } }
    #endregion Depot

    #region Taunts
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt1 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 0)); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt2 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 1)); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt3 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 2)); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt4 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 3)); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt5 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 4)); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt6 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 5)); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt7 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 6)); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLREditItem? Taunt8 { get { return GetValueOf(); } set { SetValueOf(value, ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 7)); } }
    #endregion Taunts

    public UIBool IsFemale { get; } = new();
    public bool IsFemaleSetter { get { return IsFemale.Is; } set { IsFemale.Set(value); } }
    private bool isBot;
    public bool IsBot { get { return isBot; } set { isBot = value; ItemChanged(); } }

    private bool apply;
    public bool Apply { get { return apply; } set { apply = value; WriteToBackingStructure(); OnPropertyChanged(); } }

    [JsonIgnore] public bool HasAvatar { get { return Avatar is not null && Avatar.Name != "No Avatar"; } }

    public BLRGear CopyGear()
    {
        return new BLRGear
        {
            Helmet = this.Helmet,
            UpperBody = this.UpperBody,
            LowerBody = this.LowerBody,
            Tactical = this.Tactical,
            Gear1 = this.Gear1,
            Gear2 = this.Gear2,
            Gear3 = this.Gear3,
            Gear4 = this.Gear4,
            BodyCamo = this.BodyCamo,
            Avatar = this.Avatar,
            Trophy = this.Trophy,
            IsFemale = this.IsFemale.Is,
            IsBot = this.IsBot,
        };
    }

    public BLRExtra CopyExtra()
    {
        return new BLRExtra
        {
            Depot1 = this.Depot1,
            Depot2 = this.Depot2,
            Depot3 = this.Depot3,
            Depot4 = this.Depot4,
            Depot5 = this.Depot5,

            Taunt1 = this.Taunt1,
            Taunt2 = this.Taunt2,
            Taunt3 = this.Taunt3,
            Taunt4 = this.Taunt4,
            Taunt5 = this.Taunt5,
            Taunt6 = this.Taunt6,
            Taunt7 = this.Taunt7,
            Taunt8 = this.Taunt8,

            TopIcon = this.EmblemIcon,
            TopColor = this.EmblemIconColor,
            MiddleIcon = this.EmblemShape,
            MiddleColor = this.EmblemShapeColor,
            BottomIcon = this.EmblemBackground,
            BottomColor = this.EmblemBackgroundColor,

            AnnouncerVoice = this.AnnouncerVoice,
            PlayerVoice = this.PlayerVoice,
            Title = this.Title,
        };
    }

    public void ApplyExtraGearCopy(BLRExtra? extra = null, BLRGear? gear = null)
    {
        if (gear is null && extra is null) return;

        var message = "Pasted";

        if (gear is not null)
        {
            UndoRedoSystem.DoValueChange(gear.Helmet, loadoutType.GetProperty(nameof(Helmet)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.UpperBody, loadoutType.GetProperty(nameof(UpperBody)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.LowerBody, loadoutType.GetProperty(nameof(LowerBody)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.Tactical, loadoutType.GetProperty(nameof(Tactical)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.Gear1, loadoutType.GetProperty(nameof(Gear1)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.Gear2, loadoutType.GetProperty(nameof(Gear2)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.Gear3, loadoutType.GetProperty(nameof(Gear3)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.Gear4, loadoutType.GetProperty(nameof(Gear4)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.BodyCamo, loadoutType.GetProperty(nameof(BodyCamo)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.Avatar, loadoutType.GetProperty(nameof(Avatar)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.Trophy, loadoutType.GetProperty(nameof(Trophy)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.IsFemale, loadoutType.GetProperty(nameof(IsFemaleSetter)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(gear.IsBot, loadoutType.GetProperty(nameof(IsBot)), this, BlockEvents.AllExceptUpdate);
            message += " Gear";
            
        }
        if (extra is not null)
        {
            UndoRedoSystem.DoValueChange(extra.Depot1, loadoutType.GetProperty(nameof(Depot1)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Depot2, loadoutType.GetProperty(nameof(Depot2)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Depot3, loadoutType.GetProperty(nameof(Depot3)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Depot4, loadoutType.GetProperty(nameof(Depot4)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Depot5, loadoutType.GetProperty(nameof(Depot5)), this, BlockEvents.AllExceptUpdate);

            UndoRedoSystem.DoValueChange(extra.Taunt1, loadoutType.GetProperty(nameof(Taunt1)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Taunt2, loadoutType.GetProperty(nameof(Taunt2)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Taunt3, loadoutType.GetProperty(nameof(Taunt3)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Taunt4, loadoutType.GetProperty(nameof(Taunt4)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Taunt5, loadoutType.GetProperty(nameof(Taunt5)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Taunt6, loadoutType.GetProperty(nameof(Taunt6)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Taunt7, loadoutType.GetProperty(nameof(Taunt7)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Taunt8, loadoutType.GetProperty(nameof(Taunt8)), this, BlockEvents.AllExceptUpdate);

            UndoRedoSystem.DoValueChange(extra.TopIcon, loadoutType.GetProperty(nameof(EmblemIcon)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.TopColor, loadoutType.GetProperty(nameof(EmblemIconColor)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.MiddleIcon, loadoutType.GetProperty(nameof(EmblemShape)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.MiddleColor, loadoutType.GetProperty(nameof(EmblemShapeColor)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.BottomIcon, loadoutType.GetProperty(nameof(EmblemBackground)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.BottomColor, loadoutType.GetProperty(nameof(EmblemBackgroundColor)), this, BlockEvents.AllExceptUpdate);

            UndoRedoSystem.DoValueChange(extra.AnnouncerVoice, loadoutType.GetProperty(nameof(AnnouncerVoice)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.PlayerVoice, loadoutType.GetProperty(nameof(PlayerVoice)), this, BlockEvents.AllExceptUpdate);
            UndoRedoSystem.DoValueChange(extra.Title, loadoutType.GetProperty(nameof(Title)), this, BlockEvents.AllExceptUpdate);
            if (gear is not null) { message += " & Extra"; }
            else { message += " Extra"; }
        }

        UndoRedoSystem.DoValueChange(Taunt8, loadoutType.GetProperty(nameof(Taunt8)), this, BlockEvents.All & ~BlockEvents.ReadAll & ~BlockEvents.WriteLoadout);

        UndoRedoSystem.EndUndoRecord(true);
        MainWindow.ShowAlert($"{message}!");
    }

    public BLREditLoadout(BLRProfile? profile = null) 
    {
        Profile = profile;
        Primary = new(true, this);
        Secondary = new(false, this);
    }

    #region Properties
    public double GearSlots
    {
        get
        {
            double total = 0;
            total += UpperBody?.PawnModifiers?.GearSlots ?? 0;
            total += LowerBody?.PawnModifiers?.GearSlots ?? 0;
            return total;
        }
    }

    public double RawHealth
    {
        get
        {
            double total = 0;
            total += Helmet?.PawnModifiers?.Health ?? 0;
            total += UpperBody?.PawnModifiers?.Health ?? 0;
            total += LowerBody?.PawnModifiers?.Health ?? 0;
            return Math.Min(Math.Max((int)total, -100), 100);
        }
    }

    public double Health
    {
        get
        {
            double health_alpha = Math.Abs(RawHealth) / 100;
            double basehealth = 200;
            double currentHealth;

            if (RawHealth > 0)
            {
                currentHealth = BLREditWeapon.Lerp(basehealth, 250, health_alpha);
            }
            else
            {
                currentHealth = BLREditWeapon.Lerp(basehealth, 150, health_alpha);
            }
            return currentHealth;
        }
    }

    public double HeadProtection
    {
        get
        {
            return Helmet?.PawnModifiers?.HelmetDamageReduction ?? 0;
        }
    }

    public double RawMoveSpeed
    {
        get
        {
            double total = 0;
            total += Helmet?.PawnModifiers?.MovementSpeed ?? 0;
            total += UpperBody?.PawnModifiers?.MovementSpeed ?? 0;
            total += LowerBody?.PawnModifiers?.MovementSpeed ?? 0;
            return total;
        }
    }

    public double Run
    {
        get
        {
            double allRun = Math.Min(Math.Max(RawMoveSpeed, -100), 100);

            double run_alpha = Math.Abs(allRun) / 100;
            run_alpha *= 0.9;

            double baserun = 765;
            double currentRun;

            if (allRun > 0)
            {
                currentRun = BLREditWeapon.Lerp(baserun, 900, run_alpha);
            }
            else
            {
                currentRun = BLREditWeapon.Lerp(baserun, 630, run_alpha);
            }

            return currentRun;
        }
    }

    public double HRVRechargeRate
    {
        get
        {
            double total = 0;
            total += Helmet?.PawnModifiers?.HRVRechargeRate ?? 0;
            total += Tactical?.PawnModifiers?.HRVRechargeRate ?? 0;
            return Math.Min(Math.Max(total, 5.0), 10.0); ;
        }
    }

    public double HRVDuration
    {
        get
        {
            double total = 0;
            total += Helmet?.PawnModifiers?.HRVDuration ?? 0;
            total += Tactical?.PawnModifiers?.HRVDuration ?? 0;
            return Math.Min(Math.Max(total, 40.0), 100.0);
        }
    }

    public double RawElectroProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.ElectroProtection ?? 0;
            if (GearSlot1Bool.Is) total += Gear1?.PawnModifiers?.ElectroProtection ?? 0;
            if (GearSlot2Bool.Is) total += Gear2?.PawnModifiers?.ElectroProtection ?? 0;
            if (GearSlot3Bool.Is) total += Gear3?.PawnModifiers?.ElectroProtection ?? 0;
            if (GearSlot4Bool.Is) total += Gear4?.PawnModifiers?.ElectroProtection ?? 0;
            return total;
        }
    }
    public double ElectroProtection
    {
        get
        {
            return Math.Min(RawElectroProtection, 100.0);
        }
    }

    public double RawExplosiveProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.ExplosiveProtection ?? 0;
            if (GearSlot1Bool.Is) total += Gear1?.PawnModifiers?.ExplosiveProtection ?? 0;
            if (GearSlot2Bool.Is) total += Gear2?.PawnModifiers?.ExplosiveProtection ?? 0;
            if (GearSlot3Bool.Is) total += Gear3?.PawnModifiers?.ExplosiveProtection ?? 0;
            if (GearSlot4Bool.Is) total += Gear4?.PawnModifiers?.ExplosiveProtection ?? 0;
            return total;
        }
    }
    public double ExplosiveProtection
    {
        get
        {
            return Math.Min(RawExplosiveProtection, 100.0);
        }
    }

    public double RawIncendiaryProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.IncendiaryProtection ?? 0;
            if (GearSlot1Bool.Is) total += Gear1?.PawnModifiers?.IncendiaryProtection ?? 0;
            if (GearSlot2Bool.Is) total += Gear2?.PawnModifiers?.IncendiaryProtection ?? 0;
            if (GearSlot3Bool.Is) total += Gear3?.PawnModifiers?.IncendiaryProtection ?? 0;
            if (GearSlot4Bool.Is) total += Gear4?.PawnModifiers?.IncendiaryProtection ?? 0;
            return total;
        }
    }
    public double IncendiaryProtection
    {
        get
        {
            return Math.Min(RawIncendiaryProtection, 100.0);
        }
    }

    public double RawInfraredProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.InfraredProtection ?? 0;
            if (GearSlot1Bool.Is) total += Gear1?.PawnModifiers?.InfraredProtection ?? 0;
            if (GearSlot2Bool.Is) total += Gear2?.PawnModifiers?.InfraredProtection ?? 0;
            if (GearSlot3Bool.Is) total += Gear3?.PawnModifiers?.InfraredProtection ?? 0;
            if (GearSlot4Bool.Is) total += Gear4?.PawnModifiers?.InfraredProtection ?? 0;
            return total;
        }
    }
    public double InfraredProtection
    {
        get
        {
            return Math.Min(RawInfraredProtection, 100.0);
        }
    }

    public double RawMeleeProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.MeleeProtection ?? 0;
            if (GearSlot1Bool.Is) total += Gear1?.PawnModifiers?.MeleeProtection ?? 0;
            if (GearSlot2Bool.Is) total += Gear2?.PawnModifiers?.MeleeProtection ?? 0;
            if (GearSlot3Bool.Is) total += Gear3?.PawnModifiers?.MeleeProtection ?? 0;
            if (GearSlot4Bool.Is) total += Gear4?.PawnModifiers?.MeleeProtection ?? 0;
            return total;
        }
    }
    public double MeleeProtection
    {
        get
        {
            return Math.Min(RawMeleeProtection, 100.0);
        }
    }

    public double RawPermanentHealthProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.PermanentHealthProtection ?? 0;
            if (GearSlot1Bool.Is) total += Gear1?.PawnModifiers?.PermanentHealthProtection ?? 0;
            if (GearSlot2Bool.Is) total += Gear2?.PawnModifiers?.PermanentHealthProtection ?? 0;
            if (GearSlot3Bool.Is) total += Gear3?.PawnModifiers?.PermanentHealthProtection ?? 0;
            if (GearSlot4Bool.Is) total += Gear4?.PawnModifiers?.PermanentHealthProtection ?? 0;
            return total;
        }
    }
    public double PermanentHealthProtection
    {
        get
        {
            return Math.Min(RawPermanentHealthProtection, 100.0);
        }
    }

    public double RawToxicProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.ToxicProtection ?? 0;
            if (GearSlot1Bool.Is) total += Gear1?.PawnModifiers?.ToxicProtection ?? 0;
            if (GearSlot2Bool.Is) total += Gear2?.PawnModifiers?.ToxicProtection ?? 0;
            if (GearSlot3Bool.Is) total += Gear3?.PawnModifiers?.ToxicProtection ?? 0;
            if (GearSlot4Bool.Is) total += Gear4?.PawnModifiers?.ToxicProtection ?? 0;
            return total;
        }
    }
    public double ToxicProtection
    {
        get
        {
            return Math.Min(RawToxicProtection, 100.0);
        }
    }
    #endregion Properties

    #region CalculatedProperties

    #endregion CalculatedProperties

    #region DisplayProperties
    private string? healthDisplay;
    public string HealthDisplay { get { return healthDisplay ?? string.Empty; } private set { healthDisplay = value; OnPropertyChanged(); } }

    private string? headArmorDisplay;
    public string HeadArmorDisplay { get { return headArmorDisplay ?? string.Empty; } private set { headArmorDisplay = value; OnPropertyChanged(); } }

    private string? runDisplay;
    public string RunDisplay { get { return runDisplay ?? string.Empty; } private set { runDisplay = value; OnPropertyChanged(); } }

    private string? hrvDurationDisplay;
    public string HRVDurationDisplay { get { return hrvDurationDisplay ?? string.Empty; } private set { hrvDurationDisplay = value; OnPropertyChanged(); } }

    private string? hrvRechargeDisplay;
    public string HRVRechargeDisplay { get { return hrvRechargeDisplay ?? string.Empty; } private set { hrvRechargeDisplay = value; OnPropertyChanged(); } }

    private string? gearSlotsDisplay;
    public string GearSlotsDisplay { get { return gearSlotsDisplay ?? string.Empty; } private set { gearSlotsDisplay = value; OnPropertyChanged(); } }


    private string? electroProtectionDisplay;
    public string ElectroProtectionDisplay { get { return electroProtectionDisplay ?? string.Empty; } private set { electroProtectionDisplay = value; OnPropertyChanged(); } }

    private string? explosionProtectionDisplay;
    public string ExplosionProtectionDisplay { get { return explosionProtectionDisplay ?? string.Empty; } private set { explosionProtectionDisplay = value; OnPropertyChanged(); } }

    private string? incendiaryProtectionDisplay;
    public string IncendiaryProtectionDisplay { get { return incendiaryProtectionDisplay ?? string.Empty; } private set { incendiaryProtectionDisplay = value; OnPropertyChanged(); } }
    private string? infraredProtectionDisplay;
    public string InfraredProtectionDisplay { get { return infraredProtectionDisplay ?? string.Empty; } private set { infraredProtectionDisplay = value; OnPropertyChanged(); } }

    private string? meleeProtectionDisplay;
    public string MeleeProtectionDisplay { get { return meleeProtectionDisplay ?? string.Empty; } private set { meleeProtectionDisplay = value; OnPropertyChanged(); } }
    private string? toxicProtectionDisplay;
    public string ToxicProtectionDisplay { get { return toxicProtectionDisplay ?? string.Empty; } private set { toxicProtectionDisplay = value; OnPropertyChanged(); } }
    private string? healthPercentageDisplay;
    public string HealthPercentageDisplay { get { return healthPercentageDisplay ?? string.Empty; } private set { healthPercentageDisplay = value; OnPropertyChanged(); } }
    private string? headArmorPercentageDisplay;
    public string HeadArmorPercentageDisplay { get { return headArmorPercentageDisplay ?? string.Empty; } private set { headArmorPercentageDisplay = value; OnPropertyChanged(); } }
    private string? runPercentageDisplay;
    public string RunPercentageDisplay { get { return runPercentageDisplay ?? string.Empty; } private set { runPercentageDisplay = value; OnPropertyChanged(); } }
    private string? hrvDurationPercentageDisplay;
    public string HRVDurationPercentageDisplay { get { return hrvDurationPercentageDisplay ?? string.Empty; } private set { hrvDurationPercentageDisplay = value; OnPropertyChanged(); } }

    private string? hrvRechargePercentageDisplay;
    public string HRVRechargePercentageDisplay { get { return hrvRechargePercentageDisplay ?? string.Empty; } private set { hrvRechargePercentageDisplay = value; OnPropertyChanged(); } }
    private string? gearSlotsPercentageDisplay;
    public string GearSlotsPercentageDisplay { get { return gearSlotsPercentageDisplay ?? string.Empty; } private set { gearSlotsPercentageDisplay = value; OnPropertyChanged(); } }

    #endregion DisplayProperties

    #region GerSlots
    public UIBool GearSlot1Bool { get; private set; } = new UIBool();
    public UIBool GearSlot2Bool { get; private set; } = new UIBool();
    public UIBool GearSlot3Bool { get; private set; } = new UIBool();
    public UIBool GearSlot4Bool { get; private set; } = new UIBool();
    #endregion GearSlots

    public void CalculateStats()
    {
        UpdateGearSlots();

        CreateDisplay();

        Primary.CalculateStats();
        Secondary.CalculateStats();
    }

    private void UpdateGearSlots()
    {
        GearSlot1Bool.Set(GearSlots > 0 || IsAdvanced.Is);
        GearSlot2Bool.Set(GearSlots > 1 || IsAdvanced.Is);
        GearSlot3Bool.Set(GearSlots > 2 || IsAdvanced.Is);
        GearSlot4Bool.Set(GearSlots > 3 || IsAdvanced.Is);
    }

    private void CreateDisplay()
    {
        HealthDisplay = Health.ToString("0.0");
        HeadArmorDisplay = HeadProtection.ToString("0.0") + '%';
        RunDisplay = (Run / 100.0D).ToString("0.00");

        HRVDurationDisplay = HRVDuration.ToString("0.0") + 'u';
        HRVRechargeDisplay = HRVRechargeRate.ToString("0.0") + "u/s";
        GearSlotsDisplay = GearSlots.ToString("0");

        ElectroProtectionDisplay = RawElectroProtection.ToString("0") + '%';
        ExplosionProtectionDisplay = RawExplosiveProtection.ToString("0") + '%';
        IncendiaryProtectionDisplay = RawIncendiaryProtection.ToString("0") + '%';
        InfraredProtectionDisplay = RawInfraredProtection.ToString("0") + '%';
        MeleeProtectionDisplay = RawMeleeProtection.ToString("0") + '%';
        ToxicProtectionDisplay = RawToxicProtection.ToString("0") + '%';
        HealthPercentageDisplay = RawHealth.ToString("0") + '%';
        HeadArmorPercentageDisplay = (HeadProtection - 12.5).ToString("0.0") + '%';
        RunPercentageDisplay = RawMoveSpeed.ToString("0") + '%';
        HRVDurationPercentageDisplay = (HRVDuration - 70).ToString("0.0") + "u";
        HRVRechargePercentageDisplay = (HRVRechargeRate - 6.6).ToString("0.0") + "u/s";
        GearSlotsPercentageDisplay = (GearSlots - 2).ToString("0");
    }

    public void SetLoadout(IBLRLoadout? loadout, bool registerReadBackEvent = false)
    {
        if (_loadout is not null) { _loadout.WasWrittenTo -= ReadCallback; }
        _loadout = loadout;
        if (_loadout is not null)
        {
            Primary.SetWeapon(_loadout.GetPrimaryWeaponInterface(), registerReadBackEvent);
            Secondary.SetWeapon(_loadout.GetSecondaryWeaponInterface(), registerReadBackEvent);
            if (registerReadBackEvent) { _loadout.WasWrittenTo += ReadCallback; }
        }
        else
        {
            Primary.SetWeapon(null, registerReadBackEvent);
            Secondary.SetWeapon(null, registerReadBackEvent);
        }
    }

    private void ReadCallback(object sender, EventArgs e)
    {
        if (sender != this)
        {
            Read();
        }
    }

    public void Read()
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadLoadout)) return;
        _loadout?.Read(this);
    }

    public void WriteToBackingStructure() 
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteLoadout)) return;
        _loadout?.Write(this);
    }

    static readonly Random rng = new();

    public void RandomizeGear()
    {
        var helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.HELMETS_CATEGORY)?.Length ?? 0));
        var upperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.UPPER_BODIES_CATEGORY)?.Length ?? 0));
        var lowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.LOWER_BODIES_CATEGORY)?.Length ?? 0));
        var avatar = ImportSystem.GetItemByIDAndType(ImportSystem.AVATARS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.AVATARS_CATEGORY)?.Length ?? 0));
        var camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.CAMOS_BODIES_CATEGORY)?.Length ?? 0));
        var tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.TACTICAL_CATEGORY)?.Length ?? 0));

        var trophys = ImportSystem.GetItemArrayOfType(ImportSystem.BADGES_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();

        var gears = ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();

        UndoRedoSystem.DoValueChange(helmet, this.GetType().GetProperty(nameof(Helmet)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(upperBody, this.GetType().GetProperty(nameof(UpperBody)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(lowerBody, this.GetType().GetProperty(nameof(LowerBody)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(avatar, this.GetType().GetProperty(nameof(Avatar)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(trophys[rng.Next(0, trophys.Count)], this.GetType().GetProperty(nameof(Trophy)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(camo, this.GetType().GetProperty(nameof(BodyCamo)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(tactical, this.GetType().GetProperty(nameof(Tactical)), this, BlockEvents.AllExceptUpdate);

        if (GearSlots > 0)
        {
            var gearIndex = rng.Next(0, gears.Count);
            UndoRedoSystem.DoValueChange(gears[gearIndex], this.GetType().GetProperty(nameof(Gear1)), this, BlockEvents.AllExceptUpdate);
            if (IsAdvanced.IsNot) gears.RemoveAt(gearIndex);
        }
        if (GearSlots > 1)
        {
            var gearIndex = rng.Next(0, gears.Count);
            UndoRedoSystem.DoValueChange(gears[gearIndex], this.GetType().GetProperty(nameof(Gear2)), this, BlockEvents.AllExceptUpdate);
            if (IsAdvanced.IsNot) gears.RemoveAt(gearIndex);
        }
        if (GearSlots > 2)
        {
            var gearIndex = rng.Next(0, gears.Count);
            UndoRedoSystem.DoValueChange(gears[gearIndex], this.GetType().GetProperty(nameof(Gear3)), this, BlockEvents.AllExceptUpdate);
            if (IsAdvanced.IsNot) gears.RemoveAt(gearIndex);
        }
        if (GearSlots > 3)
        {
            var gearIndex = rng.Next(0, gears.Count);
            UndoRedoSystem.DoValueChange(gears[gearIndex], this.GetType().GetProperty(nameof(Gear4)), this, BlockEvents.AllExceptUpdate);
            if (IsAdvanced.IsNot) gears.RemoveAt(gearIndex);
        }
        UndoRedoSystem.DoValueChange(NextBoolean(), this.GetType().GetProperty(nameof(IsFemaleSetter)), this);
        UndoRedoSystem.EndUndoRecord();
    }

    public void RandomizeExtra()
    {
        RandomizeTaunts();
        RandomizeDepot();
        RandomizeEmblem();
        RandomizeVoices();

        //TODO: Add Emblem, Voices and Title to Randomization
    }

    public void RandomizeTaunts()
    {
        var taunts = ImportSystem.GetItemArrayOfType(ImportSystem.EMOTES_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();

        //TODO: might want to change the anti duplicate behaviour for the emotes
        var tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt1)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt2)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt3)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt4)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt5)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt6)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt7)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        tauntIndex = rng.Next(0, taunts.Count);
        UndoRedoSystem.DoValueChange(taunts[tauntIndex], this.GetType().GetProperty(nameof(Taunt8)), this, BlockEvents.AllExceptUpdate);
        taunts.RemoveAt(tauntIndex);
        UndoRedoSystem.EndUndoRecord();
    }

    public void RandomizeDepot()
    {
        var depots = ImportSystem.GetItemArrayOfType(ImportSystem.SHOP_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();
        var depotIndex = rng.Next(0, depots.Count);
        UndoRedoSystem.DoValueChange(depots[depotIndex], this.GetType().GetProperty(nameof(Depot1)), this, BlockEvents.AllExceptUpdate);
        depots.RemoveAt(depotIndex);
        depotIndex = rng.Next(0, depots.Count);
        UndoRedoSystem.DoValueChange(depots[depotIndex], this.GetType().GetProperty(nameof(Depot2)), this, BlockEvents.AllExceptUpdate);
        depots.RemoveAt(depotIndex);
        depotIndex = rng.Next(0, depots.Count);
        UndoRedoSystem.DoValueChange(depots[depotIndex], this.GetType().GetProperty(nameof(Depot3)), this, BlockEvents.AllExceptUpdate);
        depots.RemoveAt(depotIndex);
        depotIndex = rng.Next(0, depots.Count);
        UndoRedoSystem.DoValueChange(depots[depotIndex], this.GetType().GetProperty(nameof(Depot4)), this, BlockEvents.AllExceptUpdate);
        depots.RemoveAt(depotIndex);
        depotIndex = rng.Next(0, depots.Count);
        UndoRedoSystem.DoValueChange(depots[depotIndex], this.GetType().GetProperty(nameof(Depot5)), this, BlockEvents.AllExceptUpdate);
        depots.RemoveAt(depotIndex);
        UndoRedoSystem.EndUndoRecord();
    }

    public void RandomizeEmblem()
    {
        var icons = ImportSystem.GetItemArrayOfType(ImportSystem.EMBLEM_ICON_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();
        var shapes = ImportSystem.GetItemArrayOfType(ImportSystem.EMBLEM_SHAPE_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();
        var backgrounds = ImportSystem.GetItemArrayOfType(ImportSystem.EMBLEM_BACKGROUND_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();
        var colors = ImportSystem.GetItemArrayOfType(ImportSystem.EMBLEM_COLOR_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();

        var inconIndex = rng.Next(0, icons.Count);
        var inconColor = rng.Next(0, colors.Count);
        UndoRedoSystem.DoValueChange(icons[inconIndex], this.GetType().GetProperty(nameof(EmblemIcon)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(colors[inconColor], this.GetType().GetProperty(nameof(EmblemIconColor)), this, BlockEvents.AllExceptUpdate);

        var shapeIndex = rng.Next(0, shapes.Count);
        var shapeColor = rng.Next(0, colors.Count);
        UndoRedoSystem.DoValueChange(shapes[shapeIndex], this.GetType().GetProperty(nameof(EmblemShape)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(colors[shapeColor], this.GetType().GetProperty(nameof(EmblemShapeColor)), this, BlockEvents.AllExceptUpdate);

        var backgroundIndex = rng.Next(0, backgrounds.Count);
        var backgroundColor = rng.Next(0, colors.Count);
        UndoRedoSystem.DoValueChange(backgrounds[backgroundIndex], this.GetType().GetProperty(nameof(EmblemBackground)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(colors[backgroundColor], this.GetType().GetProperty(nameof(EmblemBackgroundColor)), this);
        UndoRedoSystem.EndUndoRecord();
    }

    public void RandomizeVoices()
    { 
        var announcers = ImportSystem.GetItemArrayOfType(ImportSystem.ANNOUNCER_VOICE_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();
        var players = ImportSystem.GetItemArrayOfType(ImportSystem.PLAYER_VOICE_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();
        var titles = ImportSystem.GetItemArrayOfType(ImportSystem.TITLES_CATEGORY).Where(o => o.IsValidFor(null, IsAdvanced.Is)).ToList();

        var announcerIndex = rng.Next(0, announcers.Count);
        var playerIndex = rng.Next(0, players.Count);
        var titleIndex = rng.Next(0, titles.Count);

        UndoRedoSystem.DoValueChange(announcers[announcerIndex], this.GetType().GetProperty(nameof(AnnouncerVoice)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(players[playerIndex], this.GetType().GetProperty(nameof(PlayerVoice)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(titles[titleIndex], this.GetType().GetProperty(nameof(Title)), this);
        UndoRedoSystem.EndUndoRecord();
    }

    public void Randomize()
    {
        Primary.Randomize();
        Secondary.Randomize();
        RandomizeGear();
        RandomizeExtra();
    }

    public static bool NextBoolean()
    {
        return rng.Next() > (Int32.MaxValue / 2); // Next() returns an int in the range [0..Int32.MaxValue]
    }

    private GearErrorReport GenerateGearReport()
    {
        bool g1dup = false, g1frag = false, g1missing = false;
        bool g2frag = false, g2missing = false;
        bool g3frag = false, g3missing = false;
        bool g4frag = false, g4missing = false;

        bool g4dup;
        if (Gear4 is not null && Gear4.UID != 12015)
        {
            if (Gear4.Name is not null)
                g4frag = Gear4.Name.Contains("Frag") || Gear4.Name.Contains("I Heart U") || Gear4.Name.Contains("Snowpocalypse");
        }
        else
        {
            g4missing = Gear4 is null;
        }

        bool g3dup;
        if (Gear3 is not null && Gear3.UID != 12015)
        {
            if (Gear3.Name is not null) g3frag = Gear3.Name.Contains("Frag") || Gear3.Name.Contains("I Heart U") || Gear3.Name.Contains("Snowpocalypse");
        }
        else
        {
            g3missing = Gear3 is null;
        }

        bool g2dup;
        if (Gear2 is not null && Gear2.UID != 12015)
        {
            if (Gear2.Name is not null) g2frag = Gear2.Name.Contains("Frag") || Gear2.Name.Contains("I Heart U") || Gear2.Name.Contains("Snowpocalypse");
        }
        else
        {
            g2missing = Gear2 is null;
        }

        if (Gear1 is not null && Gear1.UID != 12015)
        {
            if (Gear1.Name is not null) g1frag = Gear1.Name.Contains("Frag") || Gear1.Name.Contains("I Heart U") || Gear1.Name.Contains("Snowpocalypse");
        }
        else
        {
            g1missing = Gear1 is null;
        }

        g4dup = (g4frag && (g3frag || g2frag || g1frag));
        g3dup = (g3frag && (g2frag || g1frag));
        g2dup = (g2frag && g1frag);

        return new GearErrorReport(
            ItemCheck(Helmet, null),
            ItemCheck(UpperBody, null),
            ItemCheck(LowerBody, null),
            ItemCheck(Tactical, null),
            ItemCheck(BodyCamo, null),
            ItemCheck(Avatar, null),
            ItemCheck(Trophy, null),
            ItemCheck(Gear1, g1dup, g1missing),
            ItemCheck(Gear2, g2dup, g2missing),
            ItemCheck(Gear3, g3dup, g3missing),
            ItemCheck(Gear4, g4dup, g4missing)
            );
    }

    private ExtraErrorReport GenerateExtraReport()
    {
        return new ExtraErrorReport(
            ItemCheck(Depot1, null),
            ItemCheck(Depot2, null),
            ItemCheck(Depot3, null),
            ItemCheck(Depot4, null),
            ItemCheck(Depot5, null),

            ItemCheck(Taunt1, null),
            ItemCheck(Taunt2, null),
            ItemCheck(Taunt3, null),
            ItemCheck(Taunt4, null),
            ItemCheck(Taunt5, null),
            ItemCheck(Taunt6, null),
            ItemCheck(Taunt7, null),
            ItemCheck(Taunt8, null),

            ItemCheck(EmblemIcon, null),
            ItemCheck(EmblemIconColor, null),

            ItemCheck(EmblemShape, null),
            ItemCheck(EmblemShapeColor, null),

            ItemCheck(EmblemBackground, null),
            ItemCheck(EmblemBackgroundColor, null),

            ItemCheck(AnnouncerVoice, null),
            ItemCheck(PlayerVoice, null),
            ItemCheck(Title, null)
            );
    }

    public LoadoutErrorReport GenerateLoadoutReport()
    {
        return new(
                Primary.GenerateReport(),
                Secondary.GenerateReport(),
                GenerateGearReport(),
                GenerateExtraReport()
            );
    }
}

public readonly struct LoadoutErrorReport(WeaponErrorReport primary, WeaponErrorReport secondary, GearErrorReport gear, ExtraErrorReport extra)
{
    public bool IsValid { get { return (PrimaryReport.IsValid && SecondaryReport.IsValid && GearReport.IsValid && ExtraReport.IsValid); } }
    public WeaponErrorReport PrimaryReport { get; } = primary;
    public WeaponErrorReport SecondaryReport { get; } = secondary;
    public GearErrorReport GearReport { get; } = gear;
    public ExtraErrorReport ExtraReport { get; } = extra;
}