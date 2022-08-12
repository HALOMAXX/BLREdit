using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BLREdit;

public class BLRLoadoutSetup : INotifyPropertyChanged
{
    public BLRWeaponSetup Primary { get; set; } = new BLRWeaponSetup(true);
    public BLRWeaponSetup Secondary { get; set; } = new BLRWeaponSetup(false);
    private BLRItem helmet = null;
    public BLRItem Helmet { get { return helmet; } set { if (BLREditSettings.Settings.AdvancedModding) { helmet = value; ItemChanged(); return; } if (value is null || helmet != value && value.Category == ImportSystem.HELMETS_CATEGORY) { helmet = value; ItemChanged(); } } }
    private BLRItem upperBody = null;
    public BLRItem UpperBody { get { return upperBody; } set { if (BLREditSettings.Settings.AdvancedModding ) { upperBody = value; ItemChanged(); return; } if (value is null || upperBody != value && value.Category == ImportSystem.UPPER_BODIES_CATEGORY) { upperBody = value; ItemChanged(); } } }
    private BLRItem lowerBody = null;
    public BLRItem LowerBody { get { return lowerBody; } set { if (BLREditSettings.Settings.AdvancedModding) { lowerBody = value; ItemChanged(); return; } if (value is null || lowerBody != value && value.Category == ImportSystem.LOWER_BODIES_CATEGORY) { lowerBody = value; ItemChanged(); } } }
    private BLRItem tactical = null;
    public BLRItem Tactical { get { return tactical; } set { if (BLREditSettings.Settings.AdvancedModding) { tactical = value; ItemChanged(); return; } if (value is null || tactical != value && value.Category == ImportSystem.TACTICAL_CATEGORY) { tactical = value; ItemChanged(); } } }
    private BLRItem gear1 = null;
    public BLRItem Gear1 { get { return gear1; } set { if (BLREditSettings.Settings.AdvancedModding) { gear1 = value; ItemChanged(); return; } if (value is null || gear1 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear1 = value; ItemChanged(); } } }
    private BLRItem gear2 = null;
    public BLRItem Gear2 { get { return gear2; } set { if (BLREditSettings.Settings.AdvancedModding) { gear2 = value; ItemChanged(); return; } if (value is null || gear2 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear2 = value; ItemChanged(); } } }
    private BLRItem gear3 = null;
    public BLRItem Gear3 { get { return gear3; } set { if (BLREditSettings.Settings.AdvancedModding) { gear3 = value; ItemChanged(); return; } if (value is null || gear3 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear3 = value; ItemChanged(); } } }
    private BLRItem gear4 = null;
    public BLRItem Gear4 { get { return gear4; } set { if (BLREditSettings.Settings.AdvancedModding) { gear4 = value; ItemChanged(); return; } if (value is null || gear4 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear4 = value; ItemChanged(); } } }
    private BLRItem camo = null;
    public BLRItem Camo { get { return camo; } set { if (BLREditSettings.Settings.AdvancedModding) { camo = value; ItemChanged(); return; } if (value is null || camo != value && value.Category == ImportSystem.CAMOS_BODIES_CATEGORY) { camo = value; ItemChanged(); } } }
    private BLRItem avatar = null;
    public BLRItem Avatar { get { return avatar; } set { if (BLREditSettings.Settings.AdvancedModding) { avatar = value; ItemChanged(); return; } if (IsAvatarOK(value)) { avatar = value; ItemChanged(); } } }
    private BLRItem trophy = null;
    public BLRItem Trophy { get { return trophy; } set { if (BLREditSettings.Settings.AdvancedModding) { trophy = value; ItemChanged(); return; } if (value is null || trophy != value && value.Category == ImportSystem.BADGES_CATEGORY) { trophy = value; ItemChanged(); } } }
    private bool isFemale;
    public bool IsFemale { get { return isFemale; } set { isFemale = value; ImportSystem.UpdateArmorImages(); ; OnPropertyChanged(); } }


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

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    private void ItemChanged([CallerMemberName] string propertyName = null)
    {
        CalculateStats();
        OnPropertyChanged(propertyName);
    }
    public void RemoveItem(string mod)
    {
        switch (mod)
        {
            case nameof(Helmet):
            case nameof(helmet):
                helmet = null;
                ItemChanged(nameof(Helmet));
                break;
            case nameof(UpperBody):
            case nameof(upperBody):
                upperBody = null;
                ItemChanged(nameof(UpperBody));
                break;
            case nameof(LowerBody):
            case nameof(lowerBody):
                lowerBody = null;
                ItemChanged(nameof(LowerBody));
                break;
            case nameof(Tactical):
            case nameof(tactical):
                tactical = null;
                ItemChanged(nameof(Tactical));
                break;
            case nameof(Gear1):
            case nameof(gear1):
                gear1 = null;
                ItemChanged(nameof(Gear1));
                break;
            case nameof(Gear2):
            case nameof(gear2):
                gear2 = null;
                ItemChanged(nameof(Gear2));
                break;
            case nameof(Gear3):
            case nameof(gear3):
                gear3 = null;
                ItemChanged(nameof(Gear3));
                break;
            case nameof(Gear4):
            case nameof(gear4):
                gear4 = null;
                ItemChanged(nameof(Gear4));
                break;
            case nameof(Camo):
            case nameof(camo):
                camo = null;
                ItemChanged(nameof(Camo));
                break;
            case nameof(Avatar):
            case nameof(avatar):
                avatar = null;
                ItemChanged(nameof(Avatar));
                break;
            case nameof(Trophy):
            case nameof(trophy):
                trophy = null;
                ItemChanged(nameof(Trophy));
                break;
        }
        //RemoveIncompatibleMods();
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
                currentHealth = BLRWeaponSetup.Lerp(basehealth, 250, health_alpha);
            }
            else
            {
                currentHealth = BLRWeaponSetup.Lerp(basehealth, 150, health_alpha);
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

            double baserun = 765;
            double currentRun;

            if (allRun > 0)
            {
                currentRun = BLRWeaponSetup.Lerp(baserun, 900, run_alpha);
            }
            else
            {
                currentRun = BLRWeaponSetup.Lerp(baserun, 630, run_alpha);
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
            if (GearSlot1Enabled) total += Gear1?.PawnModifiers?.ElectroProtection ?? 0;
            if (GearSlot2Enabled) total += Gear2?.PawnModifiers?.ElectroProtection ?? 0;
            if (GearSlot3Enabled) total += Gear3?.PawnModifiers?.ElectroProtection ?? 0;
            if (GearSlot4Enabled) total += Gear4?.PawnModifiers?.ElectroProtection ?? 0;
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
            if (GearSlot1Enabled) total += Gear1?.PawnModifiers?.ExplosiveProtection ?? 0;
            if (GearSlot2Enabled) total += Gear2?.PawnModifiers?.ExplosiveProtection ?? 0;
            if (GearSlot3Enabled) total += Gear3?.PawnModifiers?.ExplosiveProtection ?? 0;
            if (GearSlot4Enabled) total += Gear4?.PawnModifiers?.ExplosiveProtection ?? 0;
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
            if (GearSlot1Enabled) total += Gear1?.PawnModifiers?.IncendiaryProtection ?? 0;
            if (GearSlot2Enabled) total += Gear2?.PawnModifiers?.IncendiaryProtection ?? 0;
            if (GearSlot3Enabled) total += Gear3?.PawnModifiers?.IncendiaryProtection ?? 0;
            if (GearSlot4Enabled) total += Gear4?.PawnModifiers?.IncendiaryProtection ?? 0;
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
            if (GearSlot1Enabled) total += Gear1?.PawnModifiers?.InfraredProtection ?? 0;
            if (GearSlot2Enabled) total += Gear2?.PawnModifiers?.InfraredProtection ?? 0;
            if (GearSlot3Enabled) total += Gear3?.PawnModifiers?.InfraredProtection ?? 0;
            if (GearSlot4Enabled) total += Gear4?.PawnModifiers?.InfraredProtection ?? 0;
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
            if (GearSlot1Enabled) total += Gear1?.PawnModifiers?.MeleeProtection ?? 0;
            if (GearSlot2Enabled) total += Gear2?.PawnModifiers?.MeleeProtection ?? 0;
            if (GearSlot3Enabled) total += Gear3?.PawnModifiers?.MeleeProtection ?? 0;
            if (GearSlot4Enabled) total += Gear4?.PawnModifiers?.MeleeProtection ?? 0;
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
            if (GearSlot1Enabled) total += Gear1?.PawnModifiers?.PermanentHealthProtection ?? 0;
            if (GearSlot2Enabled) total += Gear2?.PawnModifiers?.PermanentHealthProtection ?? 0;
            if (GearSlot3Enabled) total += Gear3?.PawnModifiers?.PermanentHealthProtection ?? 0;
            if (GearSlot4Enabled) total += Gear4?.PawnModifiers?.PermanentHealthProtection ?? 0;
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
            if (GearSlot1Enabled) total += Gear1?.PawnModifiers?.ToxicProtection ?? 0;
            if (GearSlot2Enabled) total += Gear2?.PawnModifiers?.ToxicProtection ?? 0;
            if (GearSlot3Enabled) total += Gear3?.PawnModifiers?.ToxicProtection ?? 0;
            if (GearSlot4Enabled) total += Gear4?.PawnModifiers?.ToxicProtection ?? 0;
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
    public string GearSlotsDsiplay { get { return gearSlotsDsiplay; } private  set { gearSlotsDsiplay = value; OnPropertyChanged(); } }


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
    private bool gearSlot1Enabled;
    public bool GearSlot1Enabled { get { return gearSlot1Enabled; } private set { gearSlot1Enabled = value; GearSlot1Disabled = value; GearSlot1Visibility = Visibility.Visible; OnPropertyChanged(); } }
    public bool GearSlot1Disabled { get { return !gearSlot1Enabled; } private set { OnPropertyChanged(); } }
    public Visibility GearSlot1Visibility { get { if (gearSlot1Enabled) { return Visibility.Collapsed; } else { return Visibility.Visible; } } private set { OnPropertyChanged(); } }

    private bool gearSlot2Enabled;
    public bool GearSlot2Enabled { get { return gearSlot2Enabled; } private set { gearSlot2Enabled = value; GearSlot2Disabled = value; GearSlot2Visibility = Visibility.Visible; OnPropertyChanged(); } }
    public bool GearSlot2Disabled { get { return !gearSlot2Enabled; } private set { OnPropertyChanged(); } }
    public Visibility GearSlot2Visibility { get { if (gearSlot2Enabled) { return Visibility.Collapsed; } else { return Visibility.Visible; } } private set { OnPropertyChanged(); } }

    private bool gearSlot3Enabled;
    public bool GearSlot3Enabled { get { return gearSlot3Enabled; } private set { gearSlot3Enabled = value; GearSlot3Disabled = value; GearSlot3Visibility = Visibility.Visible; OnPropertyChanged(); } }
    public bool GearSlot3Disabled { get { return !gearSlot3Enabled; } private set { OnPropertyChanged(); } }
    public Visibility GearSlot3Visibility { get { if (gearSlot3Enabled) { return Visibility.Collapsed; } else { return Visibility.Visible; } } private set { OnPropertyChanged(); } }

    private bool gearSlot4Enabled;
    public bool GearSlot4Enabled { get { return gearSlot4Enabled; } private set { gearSlot4Enabled = value; GearSlot4Disabled = value; GearSlot4Visibility = Visibility.Visible; OnPropertyChanged(); } }
    public bool GearSlot4Disabled { get { return !gearSlot4Enabled; } private set { OnPropertyChanged(); } }
    public Visibility GearSlot4Visibility { get { if (gearSlot4Enabled) { return Visibility.Collapsed; } else { return Visibility.Visible; } } private set { OnPropertyChanged(); } }
    #endregion GearSlots

    private void CalculateStats()
    {
        UpdateGearSlots();



        CreateDisplay();
    }

    private void UpdateGearSlots()
    {
        GearSlot1Enabled = GearSlots > 0;
        GearSlot2Enabled = GearSlots > 1;
        GearSlot3Enabled = GearSlots > 2;
        GearSlot4Enabled = GearSlots > 3;
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

    public void UpdateMagicCowsLoadout(MagiCowsLoadout loadout)
    {
        Primary.UpdateMagiCowsWeapon(loadout.Primary);
        Primary.IsPrimary = true;
        Secondary.UpdateMagiCowsWeapon(loadout.Secondary);

        loadout.Tactical = Tactical?.GetMagicCowsID() ?? 0;
        loadout.Helmet = Helmet?.GetMagicCowsID() ?? 0;
        loadout.UpperBody = UpperBody?.GetMagicCowsID() ?? 0;
        loadout.LowerBody = LowerBody?.GetMagicCowsID() ?? 0;

        loadout.Camo = Camo?.GetMagicCowsID() ?? 0;
        loadout.Skin = Avatar?.GetMagicCowsID() ?? 99;
        loadout.Trophy = Trophy?.GetMagicCowsID() ?? 0;

        loadout.IsFemale = IsFemale;

        if (GearSlots > 0) { loadout.Gear1 = Gear1?.GetMagicCowsID() ?? -1; }
        if (GearSlots > 1) { loadout.Gear2 = Gear2?.GetMagicCowsID() ?? -1; }
        if (GearSlots > 2) { loadout.Gear3 = Gear3?.GetMagicCowsID() ?? -1; }
        if (GearSlots > 3) { loadout.Gear4 = Gear4?.GetMagicCowsID() ?? -1; }
    }
    public void LoadMagicCowsLoadout(MagiCowsLoadout loadout)
    {
        Primary.LoadMagicCowsWeapon(loadout.Primary);
        Primary.IsPrimary = true;
        Secondary.LoadMagicCowsWeapon(loadout.Secondary);

        Helmet = loadout.GetHelmet();
        UpperBody = loadout.GetUpperBody();
        LowerBody = loadout.GetLowerBody();

        Tactical = loadout.GetTactical();

        Gear1 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear1);
        Gear2 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear2);
        Gear3 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear3);
        Gear4 = ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, loadout.Gear4);

        Trophy = loadout.GetTrophy();
        Avatar = loadout.GetSkin();
        Camo = loadout.GetCamo();

        IsFemale = loadout.IsFemale;
    }
}