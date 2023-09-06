using BLREdit.Import;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.UI.Views;

public sealed class BLRLoadout : INotifyPropertyChanged
{
    private IBLRLoadout? _loadout;

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
        if (!UndoRedoSystem.BlockUpdate) Write();
        CalculateStats();
        Primary.CalculateStats();
        Secondary.CalculateStats();
        OnPropertyChanged(propertyName);
        IsChanged = true;
    }
    #endregion Event

    static readonly Type loadoutType = typeof(BLRLoadout);

    private BLRItem? helmet = null;
    public BLRItem? Helmet { get { return helmet; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { helmet = value; ItemChanged(); return; } if (value is null || helmet != value && value.Category == ImportSystem.HELMETS_CATEGORY) { if (value is null) { helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, 0); } else { helmet = value; } ItemChanged(); } } }
    private BLRItem? upperBody = null;
    public BLRItem? UpperBody { get { return upperBody; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { upperBody = value; ItemChanged(); return; } if (value is null || upperBody != value && value.Category == ImportSystem.UPPER_BODIES_CATEGORY) { if (value is null) { upperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, 0); } else { upperBody = value; } ItemChanged(); } } }
    private BLRItem? lowerBody = null;
    public BLRItem? LowerBody { get { return lowerBody; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { lowerBody = value; ItemChanged(); return; } if (value is null || lowerBody != value && value.Category == ImportSystem.LOWER_BODIES_CATEGORY) { if (value is null) { lowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, 0); } else { lowerBody = value; } ItemChanged(); } } }
    private BLRItem? tactical = null;
    public BLRItem? Tactical { get { return tactical; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { tactical = value; ItemChanged(); return; } if (value is null || tactical != value && value.Category == ImportSystem.TACTICAL_CATEGORY) { if (value is null) { tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, 0); } else { tactical = value; } ItemChanged(); } } }
    private BLRItem? gear1 = null;
    public BLRItem? Gear1 { get { return gear1; } set { if (value is null || gear1 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 1); } else { gear1 = value; } ItemChanged(); } } }
    private BLRItem? gear2 = null;
    public BLRItem? Gear2 { get { return gear2; } set { if (value is null || gear2 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 2); } else { gear2 = value; } ItemChanged(); } } }
    private BLRItem? gear3 = null;
    public BLRItem? Gear3 { get { return gear3; } set { if (value is null || gear3 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 0); } else { gear3 = value; } ItemChanged(); } } }
    private BLRItem? gear4 = null;
    public BLRItem? Gear4 { get { return gear4; } set { if (value is null || gear4 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 0); } else { gear4 = value; } ItemChanged(); } } }
    private BLRItem? camo = null;
    public BLRItem? BodyCamo { get { return camo; } set { if (value is null || camo != value && value.Category == ImportSystem.CAMOS_BODIES_CATEGORY) { if (value is null) { camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, 0); } else camo = value; ItemChanged(); } } }
    private BLRItem? avatar = null;
    public BLRItem? Avatar { get { return avatar; } set { if (IsAvatarOK(value)) { avatar = value; ItemChanged(); } } }
    private BLRItem? trophy = null;
    public BLRItem? Trophy { get { return trophy; } set { if (value is null || trophy != value && value.Category == ImportSystem.BADGES_CATEGORY) { if (value is null) { trophy = ImportSystem.GetItemByIDAndType(ImportSystem.BADGES_CATEGORY, 0); } else { trophy = value; } ItemChanged(); } } }
    private bool isFemale;
    public bool IsFemale { get { return isFemale; } set { isFemale = value; ImportSystem.UpdateArmorImages(value); ItemChanged(); } }
    private bool isBot;
    public bool IsBot { get { return isBot; } set { isBot = value; ItemChanged(); } }
    
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
    public void ApplyGearCopy(BLRGear? gear)
    {
        if (gear is null) return;

        UndoRedoSystem.DoAction(gear.Helmet, loadoutType.GetProperty(nameof(Helmet)), this);
        UndoRedoSystem.DoAction(gear.UpperBody, loadoutType.GetProperty(nameof(UpperBody)), this);
        UndoRedoSystem.DoAction(gear.LowerBody, loadoutType.GetProperty(nameof(LowerBody)), this);
        UndoRedoSystem.DoAction(gear.Tactical, loadoutType.GetProperty(nameof(Tactical)), this);
        UndoRedoSystem.DoAction(gear.Gear1, loadoutType.GetProperty(nameof(Gear1)), this);
        UndoRedoSystem.DoAction(gear.Gear2, loadoutType.GetProperty(nameof(Gear2)), this);
        UndoRedoSystem.DoAction(gear.Gear3, loadoutType.GetProperty(nameof(Gear3)), this);
        UndoRedoSystem.DoAction(gear.Gear4, loadoutType.GetProperty(nameof(Gear4)), this);
        UndoRedoSystem.DoAction(gear.BodyCamo, loadoutType.GetProperty(nameof(BodyCamo)), this);
        UndoRedoSystem.DoAction(gear.Avatar, loadoutType.GetProperty(nameof(Avatar)), this);
        UndoRedoSystem.DoAction(gear.Trophy, loadoutType.GetProperty(nameof(Trophy)), this);
        UndoRedoSystem.DoAction(gear.IsFemale, loadoutType.GetProperty(nameof(IsFemale)), this);
        UndoRedoSystem.DoAction(gear.IsBot, loadoutType.GetProperty(nameof(IsBot)), this);

        UndoRedoSystem.EndAction();
        MainWindow.ShowAlert($"Pasted Gear!");
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
    public void ApplyExtraCopy(BLRExtra? extra)
    {
        if (extra is null) return;

        UndoRedoSystem.DoAction(extra.Depot1, loadoutType.GetProperty(nameof(Depot1)), this);
        UndoRedoSystem.DoAction(extra.Depot2, loadoutType.GetProperty(nameof(Depot2)), this);
        UndoRedoSystem.DoAction(extra.Depot3, loadoutType.GetProperty(nameof(Depot3)), this);
        UndoRedoSystem.DoAction(extra.Depot4, loadoutType.GetProperty(nameof(Depot4)), this);
        UndoRedoSystem.DoAction(extra.Depot5, loadoutType.GetProperty(nameof(Depot5)), this);

        UndoRedoSystem.DoAction(extra.Taunt1, loadoutType.GetProperty(nameof(Taunt1)), this);
        UndoRedoSystem.DoAction(extra.Taunt2, loadoutType.GetProperty(nameof(Taunt2)), this);
        UndoRedoSystem.DoAction(extra.Taunt3, loadoutType.GetProperty(nameof(Taunt3)), this);
        UndoRedoSystem.DoAction(extra.Taunt4, loadoutType.GetProperty(nameof(Taunt4)), this);
        UndoRedoSystem.DoAction(extra.Taunt5, loadoutType.GetProperty(nameof(Taunt5)), this);
        UndoRedoSystem.DoAction(extra.Taunt6, loadoutType.GetProperty(nameof(Taunt6)), this);
        UndoRedoSystem.DoAction(extra.Taunt7, loadoutType.GetProperty(nameof(Taunt7)), this);
        UndoRedoSystem.DoAction(extra.Taunt8, loadoutType.GetProperty(nameof(Taunt8)), this);

        UndoRedoSystem.EndAction();
        MainWindow.ShowAlert($"Pasted Extra!");
    }

    public BLRLoadout() 
    {
        Primary = new(true, this);
        Secondary = new(false, this);
    }

    #region Depot
    private BLRItem? depot1;
    public BLRItem? Depot1 { get { return depot1; } set { if (value is null || depot1 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 0); } else { depot1 = value; } ItemChanged(); } } }
    private BLRItem? depot2;
    public BLRItem? Depot2 { get { return depot2; } set { if (value is null || depot2 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 1); } else { depot2 = value; } ItemChanged(); } } }
    private BLRItem? depot3;
    public BLRItem? Depot3 { get { return depot3; } set { if (value is null || depot3 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 2); } else { depot3 = value; } ItemChanged(); } } }
    private BLRItem? depot4;
    public BLRItem? Depot4 { get { return depot4; } set { if (value is null || depot4 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 3); } else { depot4 = value; } ItemChanged(); } } }
    private BLRItem? depot5;
    public BLRItem? Depot5 { get { return depot5; } set { if (value is null || depot5 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 4); } else { depot5 = value; } ItemChanged(); } } }
    #endregion Depot

    #region Taunts
    private BLRItem? taunt1;
    public BLRItem? Taunt1 { get { return taunt1; } set { if (value is null || taunt1 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 0); } else { taunt1 = value; } ItemChanged(); } } }
    private BLRItem? taunt2;
    public BLRItem? Taunt2 { get { return taunt2; } set { if (value is null || taunt2 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 1); } else { taunt2 = value; } ItemChanged(); } } }
    private BLRItem? taunt3;
    public BLRItem? Taunt3 { get { return taunt3; } set { if (value is null || taunt3 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 2); } else { taunt3 = value; } ItemChanged(); } } }
    private BLRItem? taunt4;
    public BLRItem? Taunt4 { get { return taunt4; } set { if (value is null || taunt4 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 3); } else { taunt4 = value; } ItemChanged(); } } }
    private BLRItem? taunt5;
    public BLRItem? Taunt5 { get { return taunt5; } set { if (value is null || taunt5 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 4); } else { taunt5 = value; } ItemChanged(); } } }
    private BLRItem? taunt6;
    public BLRItem? Taunt6 { get { return taunt6; } set { if (value is null || taunt6 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 5); } else { taunt6 = value; } ItemChanged(); } } }
    private BLRItem? taunt7;
    public BLRItem? Taunt7 { get { return taunt7; } set { if (value is null || taunt7 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 6); } else { taunt7 = value; } ItemChanged(); } } }
    private BLRItem? taunt8;
    public BLRItem? Taunt8 { get { return taunt8; } set { if (value is null || taunt8 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 7); } else { taunt8 = value; } ItemChanged(); } } }
    #endregion Taunts

    private bool IsAvatarOK(BLRItem? inAvatar)
    {
        if (inAvatar is null) { return true; }
        if (inAvatar != avatar && inAvatar.Category == ImportSystem.AVATARS_CATEGORY)
        {
            return true;
        }
        else
        {
            return false;
        }
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

    private string? gearSlotsDsiplay;
    public string GearSlotsDsiplay { get { return gearSlotsDsiplay ?? string.Empty; } private set { gearSlotsDsiplay = value; OnPropertyChanged(); } }


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
        GearSlot1Bool.Set(GearSlots > 0);
        GearSlot2Bool.Set(GearSlots > 1);
        GearSlot3Bool.Set(GearSlots > 2);
        GearSlot4Bool.Set(GearSlots > 3);
    }

    private void CreateDisplay()
    {
        HealthDisplay = Health.ToString("0.0");
        HeadArmorDisplay = HeadProtection.ToString("0.0") + '%';
        RunDisplay = (Run / 100.0D).ToString("0.00");

        HRVDurationDisplay = HRVDuration.ToString("0.0") + 'u';
        HRVRechargeDisplay = HRVRechargeRate.ToString("0.0") + "u/s";
        GearSlotsDsiplay = GearSlots.ToString("0");

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

    public void SetLoadout(IBLRLoadout loadout, bool registerReadBackEvent = false)
    { 
        _loadout = loadout;
        Primary.SetWeapon(loadout.GetPrimary(), registerReadBackEvent);
        Secondary.SetWeapon(loadout.GetSecondary(), registerReadBackEvent);
    }

    public void Read()
    { 
        _loadout?.Read(this);
    }

    public void Write() 
    { 
        _loadout?.Write(this);
    }

    //public void WriteMagiCowsLoadout(MagiCowsLoadout loadout, bool overwriteLimits = false)
    //{
    //    if (UndoRedoSystem.BlockUpdate) return;

    //    Primary.WriteMagiCowsWeapon(loadout.Primary);
    //    Secondary.WriteMagiCowsWeapon(loadout.Secondary);

    //    loadout.Tactical = BLRItem.GetMagicCowsID(Tactical);
    //    loadout.Helmet = BLRItem.GetMagicCowsID(Helmet);
    //    loadout.UpperBody = BLRItem.GetMagicCowsID(UpperBody);
    //    loadout.LowerBody = BLRItem.GetMagicCowsID(LowerBody);

    //    loadout.Camo = BLRItem.GetMagicCowsID(BodyCamo);
    //    loadout.Skin = BLRItem.GetMagicCowsID(Avatar, 99);
    //    loadout.Trophy = BLRItem.GetMagicCowsID(Trophy);

    //    loadout.IsFemale = IsFemale;

    //    if (GearSlots > 0 || overwriteLimits) loadout.Gear1 = BLRItem.GetMagicCowsID(Gear1);
    //    if (GearSlots > 1 || overwriteLimits) loadout.Gear2 = BLRItem.GetMagicCowsID(Gear2);
    //    if (GearSlots > 2 || overwriteLimits) loadout.Gear3 = BLRItem.GetMagicCowsID(Gear3);
    //    if (GearSlots > 3 || overwriteLimits) loadout.Gear4 = BLRItem.GetMagicCowsID(Gear4); 

    //    loadout.Taunts = new int[] { BLRItem.GetMagicCowsID(Taunt1,0), BLRItem.GetMagicCowsID(Taunt2,1), BLRItem.GetMagicCowsID(Taunt3, 2), BLRItem.GetMagicCowsID(Taunt4, 3), BLRItem.GetMagicCowsID(Taunt5, 4), BLRItem.GetMagicCowsID(Taunt6, 5), BLRItem.GetMagicCowsID(Taunt7, 6), BLRItem.GetMagicCowsID(Taunt8, 7) };
    //    loadout.Depot = new int[] { BLRItem.GetMagicCowsID(Depot1), BLRItem.GetMagicCowsID(Depot2, 1), BLRItem.GetMagicCowsID(Depot3, 2), BLRItem.GetMagicCowsID(Depot4,3), BLRItem.GetMagicCowsID(Depot5, 3) };
    //}
    //public void LoadMagicCowsLoadout(MagiCowsLoadout loadout)
    //{
    //    _internalLoadout = loadout;
    //    Primary.LoadMagicCowsWeapon(loadout.Primary);
    //    Primary.IsPrimary = true;

    //    Secondary.LoadMagicCowsWeapon(loadout.Secondary);

    //    helmet = loadout.GetHelmet();
    //    upperBody = loadout.GetUpperBody();
    //    lowerBody = loadout.GetLowerBody();

    //    tactical = loadout.GetTactical();

    //    gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear1);
    //    gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear2);
    //    gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear3);
    //    gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear4);

    //    if (loadout.Taunts.Length > 0) taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[0]); else taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 0);
    //    if (loadout.Taunts.Length > 1) taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[1]); else taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 1);
    //    if (loadout.Taunts.Length > 2) taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[2]); else taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 2);
    //    if (loadout.Taunts.Length > 3) taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[3]); else taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 3);
    //    if (loadout.Taunts.Length > 4) taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[4]); else taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 4);
    //    if (loadout.Taunts.Length > 5) taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[5]); else taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 5);
    //    if (loadout.Taunts.Length > 6) taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[6]); else taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 6);
    //    if (loadout.Taunts.Length > 7) taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[7]); else taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 7);

    //    if (loadout.Depot.Length > 0) depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[0]); else depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 0);
    //    if (loadout.Depot.Length > 1) depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[1]); else depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 1);
    //    if (loadout.Depot.Length > 2) depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[2]); else depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 2);
    //    if (loadout.Depot.Length > 3) depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[3]); else depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 3);
    //    if (loadout.Depot.Length > 4) depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[4]); else depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 4);

    //    trophy = loadout.GetTrophy();
    //    avatar = loadout.GetSkin();
    //    camo = loadout.GetCamo();

    //    IsFemale = loadout.IsFemale;

    //    ItemChanged(nameof(Helmet));
    //    ItemChanged(nameof(UpperBody));
    //    ItemChanged(nameof(LowerBody));
    //    ItemChanged(nameof(Tactical));
    //    ItemChanged(nameof(Gear1));
    //    ItemChanged(nameof(Gear2));
    //    ItemChanged(nameof(Gear3));
    //    ItemChanged(nameof(Gear4));
    //    ItemChanged(nameof(Trophy));
    //    ItemChanged(nameof(Avatar));
    //    ItemChanged(nameof(BodyCamo));

    //    ItemChanged(nameof(Depot1));
    //    ItemChanged(nameof(Depot2));
    //    ItemChanged(nameof(Depot3));
    //    ItemChanged(nameof(Depot4));
    //    ItemChanged(nameof(Depot5));

    //    ItemChanged(nameof(Taunt1));
    //    ItemChanged(nameof(Taunt2));
    //    ItemChanged(nameof(Taunt3));
    //    ItemChanged(nameof(Taunt4));
    //    ItemChanged(nameof(Taunt5));
    //    ItemChanged(nameof(Taunt6));
    //    ItemChanged(nameof(Taunt7));
    //    ItemChanged(nameof(Taunt8));
    //}
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

        //UndoRedoSystem.DoAction(profile.PlayerName, PlayerNameTextBox.GetType().GetProperty(nameof(PlayerNameTextBox.Text)), PlayerNameTextBox);

        UndoRedoSystem.DoAction(helmet, this.GetType().GetProperty(nameof(Helmet)), this);
        UndoRedoSystem.DoAction(upperBody, this.GetType().GetProperty(nameof(UpperBody)), this);
        UndoRedoSystem.DoAction(lowerBody, this.GetType().GetProperty(nameof(LowerBody)), this);
        UndoRedoSystem.DoAction(avatar, this.GetType().GetProperty(nameof(Avatar)), this);
        UndoRedoSystem.DoAction(trophy, this.GetType().GetProperty(nameof(Trophy)), this);
        UndoRedoSystem.DoAction(camo, this.GetType().GetProperty(nameof(BodyCamo)), this);
        UndoRedoSystem.DoAction(tactical, this.GetType().GetProperty(nameof(Tactical)), this);
        UndoRedoSystem.DoAction(NextBoolean(), this.GetType().GetProperty(nameof(IsFemale)), this);

        if (GearSlots > 0)
        { 
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoAction(gear, this.GetType().GetProperty(nameof(Gear1)), this);
        }
        if (GearSlots > 1)
        {
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoAction(gear, this.GetType().GetProperty(nameof(Gear2)), this);
        }
        if (GearSlots > 2)
        {
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoAction(gear, this.GetType().GetProperty(nameof(Gear3)), this);
        }
        if (GearSlots > 3)
        {
            var gear = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.ATTACHMENTS_CATEGORY)?.Length ?? 0));
            UndoRedoSystem.DoAction(gear, this.GetType().GetProperty(nameof(Gear4)), this);
        }
        UndoRedoSystem.EndAction();
    }

    public static bool NextBoolean()
    {
        return rng.Next() > (Int32.MaxValue / 2);
        // Next() returns an int in the range [0..Int32.MaxValue]
    }
}