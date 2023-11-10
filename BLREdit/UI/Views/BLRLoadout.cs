using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.UI.Views;

public sealed class BLRLoadout : INotifyPropertyChanged
{
    private IBLRLoadout? _loadout;
    public IBLRLoadout? InternalLoadout { get { return _loadout; } }

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    public BLRWeapon Primary { get; set; }
    public BLRWeapon Secondary { get; set; }

    private bool isChanged = false;
    [JsonIgnore] public bool IsChanged { get { return isChanged; } set { isChanged = value; OnPropertyChanged(); } }

    private void ItemChanged([CallerMemberName] string? propertyName = null)
    {
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteLoadout)) Write();
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Calculate)) CalculateStats();
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) OnPropertyChanged(propertyName);
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) IsChanged = true;
    }
    #endregion Event

    static readonly Type loadoutType = typeof(BLRLoadout);

    public BLRProfile? Profile { get; private set; }

    public static PropertyInfo[] LoadoutPartInfo { get; } = ((from property in loadoutType.GetProperties() where Attribute.IsDefined(property, typeof(BLRItemAttribute)) orderby ((BLRItemAttribute)property.GetCustomAttributes(typeof(BLRItemAttribute), false).Single()).PropertyOrder select property).ToArray());
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

    private readonly Dictionary<int, BLRItem?> LoadoutParts = new();

    private BLRItem? GetValueOf([CallerMemberName] string? name = null)
    {
        if (string.IsNullOrEmpty(name)) return null;

        var property = LoadoutPartInfoDictonary[name];
        var attribute = property.GetCustomAttribute<BLRItemAttribute>();
        if (LoadoutParts.TryGetValue(attribute.PropertyOrder, out var value))
        { return value; }
        else
        { return null; }
    }

    private void SetValueOf(BLRItem? value, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? name = null)
    {
        if (string.IsNullOrEmpty(name)) return;
        var property = LoadoutPartInfoDictonary[name];
        var attribute = property.GetCustomAttribute<BLRItemAttribute>();
        if (value is not null && !attribute.ItemType.Contains(value.Category))
        { return; }
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
    [BLRItem(ImportSystem.HELMETS_CATEGORY)] public BLRItem? Helmet { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.UPPER_BODIES_CATEGORY)] public BLRItem? UpperBody { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.LOWER_BODIES_CATEGORY)] public BLRItem? LowerBody { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.TACTICAL_CATEGORY)] public BLRItem? Tactical { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLRItem? Gear1 { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLRItem? Gear2 { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLRItem? Gear3 { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.ATTACHMENTS_CATEGORY)] public BLRItem? Gear4 { get { return GetValueOf(); } set { SetValueOf(value); } }
    [BLRItem(ImportSystem.CAMOS_BODIES_CATEGORY)] public BLRItem? BodyCamo { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.AVATARS_CATEGORY)] public BLRItem? Avatar { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); OnPropertyChanged(nameof(HasAvatar)); } }
    [BLRItem(ImportSystem.BADGES_CATEGORY)] public BLRItem? Trophy { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    #endregion Gear

    #region Depot
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLRItem? Depot1 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLRItem? Depot2 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLRItem? Depot3 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLRItem? Depot4 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.SHOP_CATEGORY)] public BLRItem? Depot5 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    #endregion Depot

    #region Taunts
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt1 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt2 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt3 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt4 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt5 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt6 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt7 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    [BLRItem(ImportSystem.EMOTES_CATEGORY)] public BLRItem? Taunt8 { get { return GetValueOf(); } set { SetValueOf(value, BlockEvents.Calculate); } }
    #endregion Taunts

    private bool isFemale;
    public bool IsFemale { get { return isFemale; } set { isFemale = value; ImportSystem.UpdateArmorImages(value); ItemChanged(); } }
    private bool isBot;
    public bool IsBot { get { return isBot; } set { isBot = value; ItemChanged(); } }

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
            IsFemale = this.IsFemale,
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
            UndoRedoSystem.DoValueChange(gear.IsFemale, loadoutType.GetProperty(nameof(IsFemale)), this, BlockEvents.AllExceptUpdate);
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
            if (gear is not null) { message += " & Extra"; }
            else { message += " Extra"; }
        }

        UndoRedoSystem.DoValueChange(Taunt8, loadoutType.GetProperty(nameof(Taunt8)), this, BlockEvents.All & ~BlockEvents.ReadAll & ~BlockEvents.WriteLoadout);

        UndoRedoSystem.EndUndoRecord(true);
        MainWindow.ShowAlert($"{message}!");
    }

    public BLRLoadout(BLRProfile? profile) 
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
                currentHealth = BLRWeapon.Lerp(basehealth, 250, health_alpha);
            }
            else
            {
                currentHealth = BLRWeapon.Lerp(basehealth, 150, health_alpha);
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
                currentRun = BLRWeapon.Lerp(baserun, 900, run_alpha);
            }
            else
            {
                currentRun = BLRWeapon.Lerp(baserun, 630, run_alpha);
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
        GearSlot1Bool.Set(GearSlots > 0 || (Profile?.IsAdvanced.Is ?? false));
        GearSlot2Bool.Set(GearSlots > 1 || (Profile?.IsAdvanced.Is ?? false));
        GearSlot3Bool.Set(GearSlots > 2 || (Profile?.IsAdvanced.Is ?? false));
        GearSlot4Bool.Set(GearSlots > 3 || (Profile?.IsAdvanced.Is ?? false));
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
            Primary.SetWeapon(_loadout.GetPrimary(), registerReadBackEvent);
            Secondary.SetWeapon(_loadout.GetSecondary(), registerReadBackEvent);
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

    public void Write() 
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteLoadout)) return;
        _loadout?.Write(this);
    }

    static readonly Random rng = new();
    public void Randomize()
    {
        Primary.Randomize();
        Secondary.Randomize();

        var helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.HELMETS_CATEGORY)?.Length ?? 0));
        var upperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.UPPER_BODIES_CATEGORY)?.Length ?? 0));
        var lowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.LOWER_BODIES_CATEGORY)?.Length ?? 0));
        var avatar = ImportSystem.GetItemByIDAndType(ImportSystem.AVATARS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.AVATARS_CATEGORY)?.Length ?? 0));
        var trophy = ImportSystem.GetItemByIDAndType(ImportSystem.BADGES_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.BADGES_CATEGORY)?.Length ?? 0));
        var camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.CAMOS_BODIES_CATEGORY)?.Length ?? 0));
        var tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.TACTICAL_CATEGORY)?.Length ?? 0));

        UndoRedoSystem.DoValueChange(helmet, this.GetType().GetProperty(nameof(Helmet)), this, BlockEvents.All);
        UndoRedoSystem.DoValueChange(upperBody, this.GetType().GetProperty(nameof(UpperBody)), this, BlockEvents.All);
        UndoRedoSystem.DoValueChange(lowerBody, this.GetType().GetProperty(nameof(LowerBody)), this, BlockEvents.All);
        UndoRedoSystem.DoValueChange(avatar, this.GetType().GetProperty(nameof(Avatar)), this, BlockEvents.All);
        UndoRedoSystem.DoValueChange(trophy, this.GetType().GetProperty(nameof(Trophy)), this, BlockEvents.All);
        UndoRedoSystem.DoValueChange(camo, this.GetType().GetProperty(nameof(BodyCamo)), this, BlockEvents.All);
        UndoRedoSystem.DoValueChange(tactical, this.GetType().GetProperty(nameof(Tactical)), this, BlockEvents.All);
        

        if (GearSlots > 0)
        { 
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoValueChange(gear, this.GetType().GetProperty(nameof(Gear1)), this, BlockEvents.All);
        }
        if (GearSlots > 1)
        {
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoValueChange(gear, this.GetType().GetProperty(nameof(Gear2)), this, BlockEvents.All);
        }
        if (GearSlots > 2)
        {
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoValueChange(gear, this.GetType().GetProperty(nameof(Gear3)), this, BlockEvents.All);
        }
        if (GearSlots > 3)
        {
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoValueChange(gear, this.GetType().GetProperty(nameof(Gear4)), this, BlockEvents.All);
        }

        UndoRedoSystem.DoValueChange(NextBoolean(), this.GetType().GetProperty(nameof(IsFemale)), this);

        UndoRedoSystem.EndUndoRecord();
    }

    public static bool NextBoolean()
    {
        return rng.Next() > (Int32.MaxValue / 2);
        // Next() returns an int in the range [0..Int32.MaxValue]
    }
}