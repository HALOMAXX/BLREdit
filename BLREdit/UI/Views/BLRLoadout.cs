using BLREdit.Export;
using BLREdit.Import;

using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Controls;

namespace BLREdit.UI.Views;

public sealed class BLRLoadout : INotifyPropertyChanged
{
    #region Event
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    private void ItemChanged([CallerMemberName] string propertyName = null)
    {
        if (!UndoRedoSystem.BlockUpdate) UpdateMagicCowsLoadout();
        CalculateStats();
        OnPropertyChanged(propertyName);
    }
    #endregion Event

    public BLRWeapon Primary { get; set; } = new(true);
    public BLRWeapon Secondary { get; set; } = new(false);
    private BLRItem helmet = null;
    public BLRItem Helmet { get { return helmet; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { helmet = value; ItemChanged(); return; } if (value is null || helmet != value && value.Category == ImportSystem.HELMETS_CATEGORY) { if (value is null) { helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, 0); } else { helmet = value; } ItemChanged(); } } }
    private BLRItem upperBody = null;
    public BLRItem UpperBody { get { return upperBody; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { upperBody = value; ItemChanged(); return; } if (value is null || upperBody != value && value.Category == ImportSystem.UPPER_BODIES_CATEGORY) { if (value is null) { upperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, 0); } else { upperBody = value; } ItemChanged(); } } }
    private BLRItem lowerBody = null;
    public BLRItem LowerBody { get { return lowerBody; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { lowerBody = value; ItemChanged(); return; } if (value is null || lowerBody != value && value.Category == ImportSystem.LOWER_BODIES_CATEGORY) { if (value is null) { lowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, 0); } else { lowerBody = value; } ItemChanged(); } } }
    private BLRItem tactical = null;
    public BLRItem Tactical { get { return tactical; } set { if (BLREditSettings.Settings.AdvancedModding.Is) { tactical = value; ItemChanged(); return; } if (value is null || tactical != value && value.Category == ImportSystem.TACTICAL_CATEGORY) { if (value is null) { tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, 0); } else { tactical = value; } ItemChanged(); } } }
    private BLRItem gear1 = null;
    public BLRItem Gear1 { get { return gear1; } set { if (value is null || gear1 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 1); } else { gear1 = value; } ItemChanged(); } } }
    private BLRItem gear2 = null;
    public BLRItem Gear2 { get { return gear2; } set { if (value is null || gear2 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 2); } else { gear2 = value; } ItemChanged(); } } }
    private BLRItem gear3 = null;
    public BLRItem Gear3 { get { return gear3; } set { if (value is null || gear3 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 0); } else { gear3 = value; } ItemChanged(); } } }
    private BLRItem gear4 = null;
    public BLRItem Gear4 { get { return gear4; } set { if (value is null || gear4 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { if (value is null) { gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, 0); } else { gear4 = value; } ItemChanged(); } } }
    private BLRItem camo = null;
    public BLRItem BodyCamo { get { return camo; } set { if (value is null || camo != value && value.Category == ImportSystem.CAMOS_BODIES_CATEGORY) { if (value is null) { camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, 0); } else camo = value; ItemChanged(); } } }
    private BLRItem avatar = null;
    public BLRItem Avatar { get { return avatar; } set { if (IsAvatarOK(value)) { avatar = value; ItemChanged(); } } }
    private BLRItem trophy = null;
    public BLRItem Trophy { get { return trophy; } set { if (value is null || trophy != value && value.Category == ImportSystem.BADGES_CATEGORY) { if (value is null) { trophy = ImportSystem.GetItemByIDAndType(ImportSystem.BADGES_CATEGORY, 0); } else { trophy = value; } ItemChanged(); } } }
    private bool isFemale;
    public bool IsFemale { get { return isFemale; } set { isFemale = value; ImportSystem.UpdateArmorImages(); ; ItemChanged(); } }
    private bool isBot;
    public bool IsBot { get { return isBot; } set { isBot = value; ItemChanged(); } }

    #region Depot
    private BLRItem depot1;
    public BLRItem Depot1 { get { return depot1; } set { if (value is null || depot1 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 0); } else { depot1 = value; } ItemChanged(); } } }
    private BLRItem depot2;
    public BLRItem Depot2 { get { return depot2; } set { if (value is null || depot2 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 1); } else { depot2 = value; } ItemChanged(); } } }
    private BLRItem depot3;
    public BLRItem Depot3 { get { return depot3; } set { if (value is null || depot3 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 2); } else { depot3 = value; } ItemChanged(); } } }
    private BLRItem depot4;
    public BLRItem Depot4 { get { return depot4; } set { if (value is null || depot4 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 3); } else { depot4 = value; } ItemChanged(); } } }
    private BLRItem depot5;
    public BLRItem Depot5 { get { return depot5; } set { if (value is null || depot5 != value && value.Category == ImportSystem.SHOP_CATEGORY) { if (value is null) { depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 4); } else { depot5 = value; } ItemChanged(); } } }
    #endregion Depot

    #region Taunts
    private BLRItem taunt1;
    public BLRItem Taunt1 { get { return taunt1; } set { if (value is null || taunt1 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 0); } else { taunt1 = value; } ItemChanged(); } } }
    private BLRItem taunt2;
    public BLRItem Taunt2 { get { return taunt2; } set { if (value is null || taunt2 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 1); } else { taunt2 = value; } ItemChanged(); } } }
    private BLRItem taunt3;
    public BLRItem Taunt3 { get { return taunt3; } set { if (value is null || taunt3 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 2); } else { taunt3 = value; } ItemChanged(); } } }
    private BLRItem taunt4;
    public BLRItem Taunt4 { get { return taunt4; } set { if (value is null || taunt4 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 3); } else { taunt4 = value; } ItemChanged(); } } }
    private BLRItem taunt5;
    public BLRItem Taunt5 { get { return taunt5; } set { if (value is null || taunt5 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 4); } else { taunt5 = value; } ItemChanged(); } } }
    private BLRItem taunt6;
    public BLRItem Taunt6 { get { return taunt6; } set { if (value is null || taunt6 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 5); } else { taunt6 = value; } ItemChanged(); } } }
    private BLRItem taunt7;
    public BLRItem Taunt7 { get { return taunt7; } set { if (value is null || taunt7 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 6); } else { taunt7 = value; } ItemChanged(); } } }
    private BLRItem taunt8;
    public BLRItem Taunt8 { get { return taunt8; } set { if (value is null || taunt8 != value && value.Category == ImportSystem.EMOTES_CATEGORY) { if (value is null) { taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 7); } else { taunt8 = value; } ItemChanged(); } } }
    #endregion Taunts



    private bool IsAvatarOK(BLRItem inAvatar)
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
    private string healthDisplay;
    public string HealthDisplay { get { return healthDisplay; } private set { healthDisplay = value; OnPropertyChanged(); } }

    private string headArmorDisplay;
    public string HeadArmorDisplay { get { return headArmorDisplay; } private set { headArmorDisplay = value; OnPropertyChanged(); } }

    private string runDisplay;
    public string RunDisplay { get { return runDisplay; } private set { runDisplay = value; OnPropertyChanged(); } }

    private string hrvDurationDisplay;
    public string HRVDurationDisplay { get { return hrvDurationDisplay; } private set { hrvDurationDisplay = value; OnPropertyChanged(); } }

    private string hrvRechargeDisplay;
    public string HRVRechargeDisplay { get { return hrvRechargeDisplay; } private set { hrvRechargeDisplay = value; OnPropertyChanged(); } }

    private string gearSlotsDsiplay;
    public string GearSlotsDsiplay { get { return gearSlotsDsiplay; } private set { gearSlotsDsiplay = value; OnPropertyChanged(); } }


    private string electroProtectionDisplay;
    public string ElectroProtectionDisplay { get { return electroProtectionDisplay; } private set { electroProtectionDisplay = value; OnPropertyChanged(); } }

    private string explosionProtectionDisplay;
    public string ExplosionProtectionDisplay { get { return explosionProtectionDisplay; } private set { explosionProtectionDisplay = value; OnPropertyChanged(); } }

    private string incendiaryProtectionDisplay;
    public string IncendiaryProtectionDisplay { get { return incendiaryProtectionDisplay; } private set { incendiaryProtectionDisplay = value; OnPropertyChanged(); } }
    private string infraredProtectionDisplay;
    public string InfraredProtectionDisplay { get { return infraredProtectionDisplay; } private set { infraredProtectionDisplay = value; OnPropertyChanged(); } }

    private string meleeProtectionDisplay;
    public string MeleeProtectionDisplay { get { return meleeProtectionDisplay; } private set { meleeProtectionDisplay = value; OnPropertyChanged(); } }
    private string toxicProtectionDisplay;
    public string ToxicProtectionDisplay { get { return toxicProtectionDisplay; } private set { toxicProtectionDisplay = value; OnPropertyChanged(); } }
    private string healthPercentageDisplay;
    public string HealthPercentageDisplay { get { return healthPercentageDisplay; } private set { healthPercentageDisplay = value; OnPropertyChanged(); } }
    private string headArmorPercentageDisplay;
    public string HeadArmorPercentageDisplay { get { return headArmorPercentageDisplay; } private set { headArmorPercentageDisplay = value; OnPropertyChanged(); } }
    private string runPercentageDisplay;
    public string RunPercentageDisplay { get { return runPercentageDisplay; } private set { runPercentageDisplay = value; OnPropertyChanged(); } }
    private string hrvDurationPercentageDisplay;
    public string HRVDurationPercentageDisplay { get { return hrvDurationPercentageDisplay; } private set { hrvDurationPercentageDisplay = value; OnPropertyChanged(); } }

    private string hrvRechargePercentageDisplay;
    public string HRVRechargePercentageDisplay { get { return hrvRechargePercentageDisplay; } private set { hrvRechargePercentageDisplay = value; OnPropertyChanged(); } }
    private string gearSlotsPercentageDisplay;
    public string GearSlotsPercentageDisplay { get { return gearSlotsPercentageDisplay; } private set { gearSlotsPercentageDisplay = value; OnPropertyChanged(); } }

    #endregion DisplayProperties

    #region GerSlots
    public UIBool GearSlot1Bool { get; private set; } = new UIBool();
    public UIBool GearSlot2Bool { get; private set; } = new UIBool();
    public UIBool GearSlot3Bool { get; private set; } = new UIBool();
    public UIBool GearSlot4Bool { get; private set; } = new UIBool();
    #endregion GearSlots

    private void CalculateStats()
    {
        UpdateGearSlots();



        CreateDisplay();
    }

    private void UpdateGearSlots()
    {
        GearSlot1Bool.SetBool(GearSlots > 0);
        GearSlot2Bool.SetBool(GearSlots > 1);
        GearSlot3Bool.SetBool(GearSlots > 2);
        GearSlot4Bool.SetBool(GearSlots > 3);
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

    public void UpdateMagicCowsLoadout()
    {
        if (UndoRedoSystem.BlockUpdate) return;
        internalLoadout.Tactical = Tactical?.GetMagicCowsID() ?? 0;
        internalLoadout.Helmet = Helmet?.GetMagicCowsID() ?? 0;
        internalLoadout.UpperBody = UpperBody?.GetMagicCowsID() ?? 0;
        internalLoadout.LowerBody = LowerBody?.GetMagicCowsID() ?? 0;

        internalLoadout.Camo = BodyCamo?.GetMagicCowsID() ?? 0;
        internalLoadout.Skin = Avatar?.GetMagicCowsID() ?? 99;
        internalLoadout.Trophy = Trophy?.GetMagicCowsID() ?? 0;

        internalLoadout.IsFemale = IsFemale;

        if (GearSlots > 0) { internalLoadout.Gear1 = Gear1?.GetMagicCowsID() ?? 0; }
        if (GearSlots > 1) { internalLoadout.Gear2 = Gear2?.GetMagicCowsID() ?? 0; }
        if (GearSlots > 2) { internalLoadout.Gear3 = Gear3?.GetMagicCowsID() ?? 0; }
        if (GearSlots > 3) { internalLoadout.Gear4 = Gear4?.GetMagicCowsID() ?? 0; }

        internalLoadout.Taunts = new int[] { Taunt1?.GetMagicCowsID() ?? 0, Taunt2?.GetMagicCowsID() ?? 1, Taunt3?.GetMagicCowsID() ?? 2, Taunt4?.GetMagicCowsID() ?? 3, Taunt5?.GetMagicCowsID() ?? 4, Taunt6?.GetMagicCowsID() ?? 5, Taunt7?.GetMagicCowsID() ?? 6, Taunt8?.GetMagicCowsID() ?? 7 };
        internalLoadout.Depot = new int[] { Depot1?.GetMagicCowsID() ?? 0, Depot2?.GetMagicCowsID() ?? 1, Depot3?.GetMagicCowsID() ?? 2, Depot4?.GetMagicCowsID() ?? 3, Depot5?.GetMagicCowsID() ?? 3 };
    }
    private MagiCowsLoadout internalLoadout;
    public void LoadMagicCowsLoadout(MagiCowsLoadout loadout)
    {
        internalLoadout = loadout;
        Primary.LoadMagicCowsWeapon(loadout.Primary);
        Primary.IsPrimary = true;

        Secondary.LoadMagicCowsWeapon(loadout.Secondary);

        helmet = loadout.GetHelmet();
        upperBody = loadout.GetUpperBody();
        lowerBody = loadout.GetLowerBody();

        tactical = loadout.GetTactical();

        gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear1);
        gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear2);
        gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear3);
        gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear4);

        if (loadout.Taunts.Length > 0) taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[0]); else taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 0);
        if (loadout.Taunts.Length > 0) taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[1]); else taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 1);
        if (loadout.Taunts.Length > 0) taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[2]); else taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 2);
        if (loadout.Taunts.Length > 0) taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[3]); else taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 3);
        if (loadout.Taunts.Length > 0) taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[4]); else taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 4);
        if (loadout.Taunts.Length > 0) taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[5]); else taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 5);
        if (loadout.Taunts.Length > 0) taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[6]); else taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 6);
        if (loadout.Taunts.Length > 0) taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, loadout.Taunts[7]); else taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 7);

        if (loadout.Depot.Length > 0) depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[0]); else depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 0);
        if (loadout.Depot.Length > 1) depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[1]); else depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 1);
        if (loadout.Depot.Length > 2) depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[2]); else depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 2);
        if (loadout.Depot.Length > 3) depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[3]); else depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 3);
        if (loadout.Depot.Length > 4) depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, loadout.Depot[4]); else depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 4);

        trophy = loadout.GetTrophy();
        avatar = loadout.GetSkin();
        camo = loadout.GetCamo();

        IsFemale = loadout.IsFemale;

        ItemChanged(nameof(Helmet));
        ItemChanged(nameof(UpperBody));
        ItemChanged(nameof(LowerBody));
        ItemChanged(nameof(Tactical));
        ItemChanged(nameof(Gear1));
        ItemChanged(nameof(Gear2));
        ItemChanged(nameof(Gear3));
        ItemChanged(nameof(Gear4));
        ItemChanged(nameof(Trophy));
        ItemChanged(nameof(Avatar));
        ItemChanged(nameof(BodyCamo));

        ItemChanged(nameof(Depot1));
        ItemChanged(nameof(Depot2));
        ItemChanged(nameof(Depot3));
        ItemChanged(nameof(Depot4));
        ItemChanged(nameof(Depot5));

        ItemChanged(nameof(Taunt1));
        ItemChanged(nameof(Taunt2));
        ItemChanged(nameof(Taunt3));
        ItemChanged(nameof(Taunt4));
        ItemChanged(nameof(Taunt5));
        ItemChanged(nameof(Taunt6));
        ItemChanged(nameof(Taunt7));
        ItemChanged(nameof(Taunt8));
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