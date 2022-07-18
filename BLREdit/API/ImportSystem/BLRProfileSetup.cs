using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit;

public class BLRLoadoutSetup : INotifyPropertyChanged
{
    public BLRWeaponSetup Primary { get; set; } = new BLRWeaponSetup(true);
    public BLRWeaponSetup Secondary { get; set; } = new BLRWeaponSetup(false);
    private BLRItem helmet = null;
    public BLRItem Helmet { get { return helmet; } set { if (value != null && helmet != value && value.Category == ImportSystem.HELMETS_CATEGORY) { helmet = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem upperBody = null;
    public BLRItem UpperBody { get { return upperBody; } set { if (value != null && upperBody != value && value.Category == ImportSystem.UPPER_BODIES_CATEGORY) { upperBody = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem lowerBody = null;
    public BLRItem LowerBody { get { return lowerBody; } set { if (value != null && lowerBody != value && value.Category == ImportSystem.LOWER_BODIES_CATEGORY) { lowerBody = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem tactical = null;
    public BLRItem Tactical { get { return tactical; } set { if (value != null && tactical != value && value.Category == ImportSystem.TACTICAL_CATEGORY) { tactical = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem gear1 = null;
    public BLRItem Gear1 { get { return gear1; } set { if (value != null && gear1 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear1 = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem gear2 = null;
    public BLRItem Gear2 { get { return gear2; } set { if (value != null && gear2 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear2 = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem gear3 = null;
    public BLRItem Gear3 { get { return gear3; } set { if (value != null && gear3 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear3 = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem gear4 = null;
    public BLRItem Gear4 { get { return gear4; } set { if (value != null && gear4 != value && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear4 = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem camo = null;
    public BLRItem Camo { get { return camo; } set { if (value != null && camo != value && value.Category == ImportSystem.CAMOS_BODIES_CATEGORY) { camo = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem avatar = null;
    public BLRItem Avatar { get { return avatar; } set { if (IsAvatarOK(value)) { avatar = value; CalculateStats(); OnPropertyChanged(); } } }
    private BLRItem trophy = null;
    public BLRItem Trophy { get { return trophy; } set { if (value != null && trophy != value && value.Category == ImportSystem.BADGES_CATEGORY) { trophy = value; CalculateStats();  OnPropertyChanged(); } } }
    private bool isFemale;
    public bool IsFemale { get { return isFemale; } set { isFemale = value; UpdateImages(); OnPropertyChanged(); } }


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
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Properties
    public double GearSlots
    {
        get
        {
            double total = 0;
            total += UpperBody?.PawnModifiers?.GearSlots ?? 0;
            total += LowerBody?.PawnModifiers?.GearSlots ?? 0;
            if (total < 4)
            {
                gear4 = null;
            }
            if (total < 3)
            {
                gear3 = null;
            }
            if (total < 2)
            {
                gear2 = null;
            }
            if (total < 1)
            {
                gear1 = null;
            }
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

            return currentRun / 100.0D;
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
            total += Helmet?.PawnModifiers.HRVDuration ?? 0;
            total += Tactical?.PawnModifiers.HRVDuration ?? 0;
            return Math.Min(Math.Max(total, 40.0), 100.0);
        }
    }

    public double RawElectroProtection
    {
        get
        {
            double total = Helmet?.PawnModifiers?.ElectroProtection ?? 0;
            total += Gear1?.PawnModifiers?.ElectroProtection ?? 0;
            total += Gear2?.PawnModifiers?.ElectroProtection ?? 0;
            total += Gear3?.PawnModifiers?.ElectroProtection ?? 0;
            total += Gear4?.PawnModifiers?.ElectroProtection ?? 0;
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
            total += Gear1?.PawnModifiers?.ExplosiveProtection ?? 0;
            total += Gear2?.PawnModifiers?.ExplosiveProtection ?? 0;
            total += Gear3?.PawnModifiers?.ExplosiveProtection ?? 0;
            total += Gear4?.PawnModifiers?.ExplosiveProtection ?? 0;
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
            total += Gear1?.PawnModifiers?.IncendiaryProtection ?? 0;
            total += Gear2?.PawnModifiers?.IncendiaryProtection ?? 0;
            total += Gear3?.PawnModifiers?.IncendiaryProtection ?? 0;
            total += Gear4?.PawnModifiers?.IncendiaryProtection ?? 0;
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
            total += Gear1?.PawnModifiers?.InfraredProtection ?? 0;
            total += Gear2?.PawnModifiers?.InfraredProtection ?? 0;
            total += Gear3?.PawnModifiers?.InfraredProtection ?? 0;
            total += Gear4?.PawnModifiers?.InfraredProtection ?? 0;
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
            total += Gear1?.PawnModifiers?.MeleeProtection ?? 0;
            total += Gear2?.PawnModifiers?.MeleeProtection ?? 0;
            total += Gear3?.PawnModifiers?.MeleeProtection ?? 0;
            total += Gear4?.PawnModifiers?.MeleeProtection ?? 0;
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
            total += Gear1?.PawnModifiers?.PermanentHealthProtection ?? 0;
            total += Gear2?.PawnModifiers?.PermanentHealthProtection ?? 0;
            total += Gear3?.PawnModifiers?.PermanentHealthProtection ?? 0;
            total += Gear4?.PawnModifiers?.PermanentHealthProtection ?? 0;
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
            total += Gear1?.PawnModifiers?.ToxicProtection ?? 0;
            total += Gear2?.PawnModifiers?.ToxicProtection ?? 0;
            total += Gear3?.PawnModifiers?.ToxicProtection ?? 0;
            total += Gear4?.PawnModifiers?.ToxicProtection ?? 0;
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
    public string HealthDisplay { get { return healthDisplay; } set { healthDisplay = value; OnPropertyChanged(); } }

    private string headArmor;
    public string HeadArmorDisplay { get { return headArmor; } set { headArmor = value; OnPropertyChanged(); } }

    private string runDisplay;
    public string RunDisplay { get { return runDisplay; } set { runDisplay = value; OnPropertyChanged(); } }

    private string hrvDurationDisplay;
    public string HRVDurationDisplay { get { return hrvDurationDisplay; } set { hrvDurationDisplay = value; OnPropertyChanged(); } }

    private string hrvRechargeDisplay;
    public string HRVRechargeDisplay { get { return hrvRechargeDisplay; } set { hrvRechargeDisplay = value; OnPropertyChanged(); } }

    private string gearSlotsDsiplay;
    public string GearSlotsDsiplay { get { return gearSlotsDsiplay; } set { gearSlotsDsiplay = value; OnPropertyChanged(); } }
    #endregion DisplayProperties

    private void UpdateImages()
    {
        UpperBody?.TriggerImageUpdate();
        LowerBody?.TriggerImageUpdate();
    }

    private void CalculateStats()
    {




        CreateDisplay();
    }

    private void CreateDisplay()
    {
        HealthDisplay = Health.ToString("0");
        HeadArmorDisplay = HeadProtection.ToString("0.00") + '%';
        RunDisplay = Run.ToString("0.00");

        HRVDurationDisplay = HRVDuration.ToString("0.0") + 's';
        HRVRechargeDisplay = HRVRechargeRate.ToString("0.0") + "U/s";
        GearSlotsDsiplay = GearSlots.ToString("0");
    }

    public void UpdateMagicCowsLoadout(MagiCowsLoadout loadout)
    {
        Primary.UpdateMagiCowsWeapon(loadout.Primary);
        Secondary.UpdateMagiCowsWeapon(loadout.Secondary);

        loadout.Tactical = Tactical.GetMagicCowsID();
        loadout.Helmet = Helmet.GetMagicCowsID();
        loadout.UpperBody = UpperBody.GetMagicCowsID();
        loadout.LowerBody = LowerBody.GetMagicCowsID();

        loadout.Camo = Camo.GetMagicCowsID();
        loadout.Skin = Avatar?.GetMagicCowsID() ?? 99;
        loadout.Trophy = Trophy.GetMagicCowsID();

        loadout.IsFemale = IsFemale;

        if (GearSlots > 0) { loadout.Gear1 = Gear1?.GetMagicCowsID() ?? -1; }
        if (GearSlots > 1) { loadout.Gear2 = Gear2?.GetMagicCowsID() ?? -1; }
        if (GearSlots > 2) { loadout.Gear3 = Gear3?.GetMagicCowsID() ?? -1; }
        if (GearSlots > 3) { loadout.Gear4 = Gear4?.GetMagicCowsID() ?? -1; }
    }
    public void LoadMagicCowsLoadout(MagiCowsLoadout loadout)
    {
        Primary.LoadMagicCowsWeapon(loadout.Primary);
        Secondary.LoadMagicCowsWeapon(loadout.Secondary);

        IsFemale = loadout.IsFemale;

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
    }
}