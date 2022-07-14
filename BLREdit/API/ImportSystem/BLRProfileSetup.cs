using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit;

public class BLRLoadoutSetup
{ 
    public BLRWeaponSetup Primary { get; } = new BLRWeaponSetup(true);
    public BLRWeaponSetup Secondary { get; } = new BLRWeaponSetup(false);
    private BLRItem helmet = null;
    public BLRItem Helmet { get { return helmet; } set { if (value != null && helmet != value && value.Category == ImportSystem.HELMETS_CATEGORY) { helmet = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem upperBody = null;
    public BLRItem UpperBody { get { return upperBody; } set { if (value != null && upperBody != value && value.Category == ImportSystem.UPPER_BODIES_CATEGORY) { upperBody = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem lowerBody = null;
    public BLRItem LowerBody { get { return lowerBody; } set { if (value != null && lowerBody != value && value.Category == ImportSystem.LOWER_BODIES_CATEGORY) { lowerBody = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem tactical = null;
    public BLRItem Tactical { get { return tactical; } set { if (value != null && tactical != value && value.Category == ImportSystem.TACTICAL_CATEGORY) { tactical = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem gear1 = null;
    public BLRItem Gear1 { get { return gear1; } set { if (value != null && gear1 != value && GearSlots > 0 && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear1 = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem gear2 = null;
    public BLRItem Gear2 { get { return gear2; } set { if (value != null && gear2 != value && GearSlots > 1 && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear2 = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem gear3 = null;
    public BLRItem Gear3 { get { return gear3; } set { if (value != null && gear3 != value && GearSlots > 2 && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear3 = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem gear4 = null;
    public BLRItem Gear4 { get { return gear4; } set { if (value != null && gear4 != value && GearSlots > 3 && value.Category == ImportSystem.ATTACHMENTS_CATEGORY) { gear4 = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem camo = null;
    public BLRItem Camo { get { return camo; } set { if (value != null && camo != value && value.Category == ImportSystem.CAMOS_BODIES_CATEGORY) { camo = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem avatar = null;
    public BLRItem Avatar { get { return avatar; } set { if (value != null && avatar != value && value.Category == ImportSystem.AVATARS_CATEGORY) { avatar = value; /* TODO: Calculate Armor/Gear Stats */ } } }
    private BLRItem trophy = null;
    public BLRItem Trophy { get { return trophy; } set { if (value != null && trophy != value && value.Category == ImportSystem.BADGES_CATEGORY) { trophy = value; /* TODO: Calculate Armor/Gear Stats */ } } }

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

}

public class BLRWeaponSetup
{
    private bool IsPrimary = false;
    private BLRItem reciever = null;
    public BLRItem Reciever { get { return reciever; } set { if (value != null && reciever != value && AllowReciever(value)) { reciever = value; RemoveIncompatibleMods(); CalculateStats(); } } }
    private BLRItem barrel = null;
    public BLRItem Barrel { get { return barrel; } set { if (value != null && barrel != value && value.IsValidFor(reciever) && value.Category == ImportSystem.BARRELS_CATEGORY) { barrel = value; AllowStock(); CalculateStats(); } } }
    private BLRItem magazine = null;
    public BLRItem Magazine { get { return magazine; } set { if (value != null && magazine != value && value.IsValidFor(reciever) && value.Category == ImportSystem.MAGAZINES_CATEGORY) { magazine = value; CalculateStats(); } } }
    private BLRItem muzzle = null;
    public BLRItem Muzzle { get { return muzzle; } set { if (value != null && muzzle != value && value.IsValidFor(reciever) && value.Category == ImportSystem.MUZZELS_CATEGORY) { muzzle = value; CalculateStats(); } } }
    private BLRItem stock = null;
    public BLRItem Stock { get { return stock; } set { if (value != null && stock != value && AllowStock() && value.IsValidFor(reciever) && value.Category == ImportSystem.STOCKS_CATEGORY) { stock = value; CalculateStats(); } } }
    private BLRItem scope = null;
    public BLRItem Scope { get { return scope; } set { if (value != null && scope != value && value.IsValidFor(reciever) && value.Category == ImportSystem.SCOPES_CATEGORY) { scope = value; CalculateStats(); } } }
    private BLRItem grip = null;
    public BLRItem Grip { get { return grip; } set { if (value != null && grip != value && value.IsValidFor(reciever) && value.Category == ImportSystem.GRIPS_CATEGORY) { grip = value; CalculateStats(); } } }

    private BLRItem tag = null;
    public BLRItem Tag { get { return tag; } set { if (value != null && tag != value && value.IsValidFor(reciever) && value.Category == ImportSystem.HANGERS_CATEGORY) { tag = value; } } }
    private BLRItem camo = null;
    public BLRItem Camo { get { return camo; } set { if (value != null && camo != value && value.IsValidFor(reciever) && value.Category == ImportSystem.CAMOS_WEAPONS_CATEGORY) { camo = value; } } }

    private bool IsPistol()
    {
        if (Reciever == null) return false;
        return Reciever.Name == "Light Pistol" || Reciever.Name == "Heavy Pistol" || Reciever.Name == "Prestige Light Pistol";
    }
    private bool AllowReciever(BLRItem item)
    {
        bool allow = true;
        if (IsPrimary)
        {
            if (item.Category != ImportSystem.PRIMARY_CATEGORY)
            {
                allow = false;
            }
        }
        else
        { 
            if(item.Category != ImportSystem.SECONDARY_CATEGORY)
            {
                allow = false;
            }
        }
        return allow;
    }

    private bool AllowStock()
    {
        bool allow = true;
        if (!IsPrimary)
        {
            if (IsPistol() && (Barrel?.Name ?? MagiCowsWeapon.NoBarrel) == MagiCowsWeapon.NoBarrel)
            {
                allow = false;
                stock = null; 
            }
        }
        return allow;
    }

    public BLRWeaponSetup(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }

    #region Properties
    public double AccuracyPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.accuracy ?? 0;
            total += Barrel?.WeaponModifiers?.accuracy ?? 0;
            total += Magazine?.WeaponModifiers?.accuracy ?? 0;
            total += Muzzle?.WeaponModifiers?.accuracy ?? 0;
            total += Stock?.WeaponModifiers?.accuracy ?? 0;
            total += Scope?.WeaponModifiers?.accuracy ?? 0;
            total += Grip?.WeaponModifiers?.accuracy ?? 0;
            return total;
        }
    }
    public double AdditionalAmmo
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.ammo ?? 0;
            total += Barrel?.WeaponModifiers?.ammo ?? 0;
            total += Magazine?.WeaponModifiers?.ammo ?? 0;
            total += Muzzle?.WeaponModifiers?.ammo ?? 0;
            total += Stock?.WeaponModifiers?.ammo ?? 0;
            total += Scope?.WeaponModifiers?.ammo ?? 0;
            total += Grip?.WeaponModifiers?.ammo ?? 0;
            return total;
        }
    }
    public double DamagePercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.damage ?? 0;
            total += Barrel?.WeaponModifiers?.damage ?? 0;
            total += Magazine?.WeaponModifiers?.damage ?? 0;
            total += Muzzle?.WeaponModifiers?.damage ?? 0;
            total += Stock?.WeaponModifiers?.damage ?? 0;
            total += Scope?.WeaponModifiers?.damage ?? 0;
            total += Grip?.WeaponModifiers?.damage ?? 0;
            return total;
        }
    }
    public double MovementSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.movementSpeed ?? 0;
            total += Barrel?.WeaponModifiers?.movementSpeed ?? 0;
            total += Magazine?.WeaponModifiers?.movementSpeed ?? 0;
            total += Muzzle?.WeaponModifiers?.movementSpeed ?? 0;
            total += Stock?.WeaponModifiers?.movementSpeed ?? 0;
            total += Scope?.WeaponModifiers?.movementSpeed ?? 0;
            total += Grip?.WeaponModifiers?.movementSpeed ?? 0;
            return total;
        }
    }
    public double RangePercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.range ?? 0;
            total += Barrel?.WeaponModifiers?.range ?? 0;
            total += Magazine?.WeaponModifiers?.range ?? 0;
            total += Muzzle?.WeaponModifiers?.range ?? 0;
            total += Stock?.WeaponModifiers?.range ?? 0;
            total += Scope?.WeaponModifiers?.range ?? 0;
            total += Grip?.WeaponModifiers?.range ?? 0;
            return total;
        }
    }
    public double RateOfFirePercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.rateOfFire ?? 0;
            total += Barrel?.WeaponModifiers?.rateOfFire ?? 0;
            total += Magazine?.WeaponModifiers?.rateOfFire ?? 0;
            total += Muzzle?.WeaponModifiers?.rateOfFire ?? 0;
            total += Stock?.WeaponModifiers?.rateOfFire ?? 0;
            total += Scope?.WeaponModifiers?.rateOfFire ?? 0;
            total += Grip?.WeaponModifiers?.rateOfFire ?? 0;
            return total;
        }
    }
    public double TotalRatingPoints
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.rating ?? 0;
            total += Barrel?.WeaponModifiers?.rating ?? 0;
            total += Magazine?.WeaponModifiers?.rating ?? 0;
            total += Muzzle?.WeaponModifiers?.rating ?? 0;
            total += Stock?.WeaponModifiers?.rating ?? 0;
            total += Scope?.WeaponModifiers?.rating ?? 0;
            total += Grip?.WeaponModifiers?.rating ?? 0;
            return total;
        }
    }
    public double RecoilPercentage 
    {
        get 
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.recoil ?? 0;
            total += Barrel?.WeaponModifiers?.recoil ?? 0;
            total += Magazine?.WeaponModifiers?.recoil ?? 0;
            total += Muzzle?.WeaponModifiers?.recoil ?? 0;
            total += Stock?.WeaponModifiers?.recoil ?? 0;
            total += Scope?.WeaponModifiers?.recoil ?? 0;
            total += Grip?.WeaponModifiers?.recoil ?? 0;
            return total;
        }
    }
    public double ReloadSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.reloadSpeed ?? 0;
            total += Barrel?.WeaponModifiers?.reloadSpeed ?? 0;
            total += Magazine?.WeaponModifiers?.reloadSpeed ?? 0;
            total += Muzzle?.WeaponModifiers?.reloadSpeed ?? 0;
            total += Stock?.WeaponModifiers?.reloadSpeed ?? 0;
            total += Scope?.WeaponModifiers?.reloadSpeed ?? 0;
            total += Grip?.WeaponModifiers?.reloadSpeed ?? 0;
            return total;
        }
    }
    public double SwitchWeaponSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.switchWeaponSpeed ?? 0;
            total += Barrel?.WeaponModifiers?.switchWeaponSpeed ?? 0;
            total += Magazine?.WeaponModifiers?.switchWeaponSpeed ?? 0;
            total += Muzzle?.WeaponModifiers?.switchWeaponSpeed ?? 0;
            total += Stock?.WeaponModifiers?.switchWeaponSpeed ?? 0;
            total += Scope?.WeaponModifiers?.switchWeaponSpeed ?? 0;
            total += Grip?.WeaponModifiers?.switchWeaponSpeed ?? 0;
            return total;
        }
    }
    public double WeaponWeightPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.weaponWeight ?? 0;
            total += Barrel?.WeaponModifiers?.weaponWeight ?? 0;
            total += Magazine?.WeaponModifiers?.weaponWeight ?? 0;
            total += Muzzle?.WeaponModifiers?.weaponWeight ?? 0;
            total += Stock?.WeaponModifiers?.weaponWeight ?? 0;
            total += Scope?.WeaponModifiers?.weaponWeight ?? 0;
            total += Grip?.WeaponModifiers?.weaponWeight ?? 0;
            return total;
        }
    }
    public double RawZoomMagnification
    {
        get
        {
            return Scope?.WikiStats?.zoom ?? 0;
        }
    }
    public double ZoomMagnification
    {
        get
        {
            return 1.3D + RawZoomMagnification;
        }
    }

    public double RawAmmoMagazine
    {
        get
        {
            if (Reciever?.UID == 40019)
            {
                return 1; // cheat because for some reason it isn't reading AMR's currently, might be due to lack of mag but am not sure
            }
            else
            { return (Reciever?.WeaponStats?.MagSize ?? 0); } }
    }
    public double ModifiedAmmoMagazine
    { get { return RawAmmoMagazine + AdditionalAmmo; } }
    public double RawAmmoReserve
    { get { return RawAmmoMagazine * (Reciever?.WeaponStats?.InitialMagazines ?? 0); } }
    public double ModifiedAmmoReserve
    { get { return ModifiedAmmoMagazine * (Reciever?.WeaponStats?.InitialMagazines ?? 0); } }
    public double RawRateOfFire
    { get { return Reciever?.WeaponStats?.rateOfFire ?? 0; } }
    public double ModifiedRateOfFire 
    {
        get 
        {
            return RawRateOfFire * CockRateMultiplier;
        }
    }
    public double RawReloadSpeed
    { 
        get
        {
            double total = 0;
            total += Reciever?.WikiStats?.reload ?? 0;
            total += Barrel?.WikiStats?.reload ?? 0;
            total += Magazine?.WikiStats?.reload ?? 0;
            total += Muzzle?.WikiStats?.reload ?? 0;
            total += Stock?.WikiStats?.reload ?? 0;
            total += Scope?.WikiStats?.reload ?? 0;
            total += Grip?.WikiStats?.reload ?? 0;
            return total;
        } 
    }
    public double ModifiedReloadSpeed { get { return RawReloadSpeed * ReloadMultiplier; } }
    public double ReloadMultiplier { get; private set; }
    public double CockRateMultiplier { get; private set; }
    public double RawSwapRate
    { get { return Reciever?.WikiStats?.swaprate ?? 0; } }

    #region Damage
    public double DamageClose { get; private set; }
    public double DamageFar { get; private set; }
    #endregion Damage

    #region Range
    public double RangeClose { get; private set; }
    public double RangeFar { get; private set; }
    public double RangeTracer { get; private set; }
    #endregion Range

    #region Recoil
    public double RecoilHip { get; private set; }
    public double RecoilZoom { get; private set; }
    #endregion Recoil

    #region Spread
    public double SpreadWhileMoving { get; private set; }
    public double SpreadWhileStanding { get; private set; }
    public double SpreadWhileADS { get; private set; }
    #endregion Spread

    public double VerticalRecoilRatio
    {
        get 
        {
            if (Reciever != null && Reciever.WeaponStats != null)
            {
                double vertical = Reciever.WeaponStats.RecoilVector.Y * Reciever.WeaponStats.RecoilVectorMultiplier.Y * 0.3535;
                double horizontal = Reciever.WeaponStats.RecoilVector.X * Reciever.WeaponStats.RecoilVectorMultiplier.X * 0.5;
                if ((vertical + horizontal) != 0)
                {
                    return vertical / (vertical + horizontal);
                }
                return 1;
            }
            return 1;
        }
    }

    public double RecoilRecoveryTime
    {
        get 
        {
            if (Reciever != null && Reciever.WeaponStats != null)
            {
                if (Reciever.WeaponStats.RecoveryTime > 0)
                {
                    return Reciever.WeaponStats.RecoveryTime;
                }
                return 60 / Reciever.WeaponStats.ROF;
            }
            return 0;
        }
    }

    public double ZoomRateOfFire
    {
        get
        {
            if (Reciever != null && Reciever.WeaponStats != null)
            {
                if (Reciever.WeaponStats.ZoomRateOfFire > 0)
                {
                    return Reciever.WeaponStats.ZoomRateOfFire * CockRateMultiplier;
                }
                return Reciever.WeaponStats.ROF * CockRateMultiplier;
            }
            return 0;
        }
    }

    public double SpreadCenterWeight
    {
        get
        {
            return Reciever?.WeaponStats?.SpreadCenterWeight ?? 0;
        }
    }

    public double SpreadCenter
    {
        get
        {
            return Reciever?.WeaponStats?.SpreadCenter ?? 0;
        }
    }

    public double FragmentsPerShell
    {
        get
        {
            return Reciever?.WeaponStats?.FragmentsPerShell ?? 0;
        }
    }

    public double SpreadCrouchMultiplier
    {
        get
        { 
            return Reciever?.WeaponStats?.CrouchSpreadMultiplier ?? 0;
        }
    }

    public double SpreadJumpMultiplier
    { 
        get
        {
            return Reciever?.WeaponStats?.JumpSpreadMultiplier ?? 0;
        }
    }

    public double RawScopeInTime 
    { 
        get 
        {
            double total = 0;
            total += Reciever?.WikiStats?.scopeInTime ?? 0;
            total += Barrel?.WikiStats?.scopeInTime ?? 0;
            total += Magazine?.WikiStats?.scopeInTime ?? 0;
            total += Muzzle?.WikiStats?.scopeInTime ?? 0;
            total += Stock?.WikiStats?.scopeInTime ?? 0;
            total += Scope?.WikiStats?.scopeInTime ?? 0;
            total += Grip?.WikiStats?.scopeInTime ?? 0;
            return total;
        }
    }
    public double ModifiedScopeInTime { get; private set; }
    public double ModifiedRunSpeed { get; private set; }

    public string WeaponDescriptorPart1 { get; private set; }
    public string WeaponDescriptorPart2 { get; private set; }
    public string WeaponDescriptorPart3 { get; private set; }
    public string WeaponDescriptor { get { return WeaponDescriptorPart1 + ' ' + WeaponDescriptorPart2 + ' ' + WeaponDescriptorPart3; } }
    #endregion Properties
    private void RemoveIncompatibleMods()
    {
        MagiCowsWeapon wpn = MagiCowsWeapon.GetDefaultSetupOfReciever(Reciever);
        if (Reciever.IsValidModType(ImportSystem.MUZZELS_CATEGORY))
        {
            if (muzzle is null || !muzzle.IsValidFor(Reciever))
            { 
                Muzzle = wpn.GetMuzzle();
            }
        }
        else
        {
            muzzle = null;
        }

        if (Reciever.IsValidModType(ImportSystem.BARRELS_CATEGORY))
        {
            if (barrel is null || !barrel.IsValidFor(Reciever))
            {
                Barrel = wpn.GetBarrel();
            }
        }
        else
        {
            barrel = null;
        }

        if (Reciever.IsValidModType(ImportSystem.STOCKS_CATEGORY))
        {
            if (stock is null || !stock.IsValidFor(Reciever))
            {
                Stock = wpn.GetStock();
            }
        }
        else
        {
            stock = null;
        }

        if (Reciever.IsValidModType(ImportSystem.SCOPES_CATEGORY))
        {
            if (scope is null || !scope.IsValidFor(Reciever))
            {
                Scope = wpn.GetScope();
            }
        }
        else
        {
            if ((Reciever?.Name ?? "") == "Snub 260")
            {
                //has to be lower case scope as it allows setting it with out compatability checks
                scope = ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, MagiCowsWeapon.NoScope);
            }
            else
            {
                scope = null;
            }
        }

        if (Reciever.IsValidModType(ImportSystem.MAGAZINES_CATEGORY))
        {
            if (magazine is null || !magazine.IsValidFor(Reciever))
            {
                Magazine = wpn.GetMagazine();
            }
        }
        else
        {
            magazine = null;
        }

        if (Reciever.IsValidModType(ImportSystem.GRIPS_CATEGORY))
        {
            if (grip is null || !grip.IsValidFor(Reciever))
            {
                Grip = wpn.GetGrip();
            }
        }
        else
        {
            grip = null;
        }

        if (Reciever.IsValidModType(ImportSystem.CAMOS_WEAPONS_CATEGORY))
        {
            if (Camo is null || !Camo.IsValidFor(Reciever))
            {
                Camo = wpn.GetCamo();
            }
        }
        else
        {
            camo = null;
        }

        if (Tag is null || Reciever.IsValidModType(ImportSystem.HANGERS_CATEGORY))
        {
            if (Tag is null || !Tag.IsValidFor(Reciever))
            {
                Tag = wpn.GetTag();
            }
        }
        else
        {
            tag = null;
        }
    }

    private void CalculateStats()
    {
        ResetStats();
        //if (Reciever is null || Barrel is null || Muzzle is null || Magazine is null || Stock is null || Reciever.Tooltip == "Depot Item!") { if (LoggingSystem.IsDebuggingEnabled) { LoggingSystem.LogWarning("Missing Reciever:" + Reciever + " Barrel:" + Barrel + " Muzzle:" + Muzzle + " Magazine:" + Magazine + " Stock:" + Stock); } }
        CalculateCockRate();
        CalculateDamage();
        CalculateMovementSpeed();
        CalculateRange();
        CalculateRecoil();
        CalculateReloadRate();
        double allMovementScopeIn = Barrel?.WeaponModifiers?.movementSpeed ?? 0;
        allMovementScopeIn += Stock?.WeaponModifiers?.movementSpeed ?? 0;
        ModifiedScopeInTime = CalculateScopeInTime(Clamp((allMovementScopeIn / 80.0D), -1.0D, 1.0D));
        CalculateSpread(Clamp((allMovementScopeIn / 100.0D), -1.0D, 1.0D));

        WeaponDescriptorPart1 = CompareItemDescriptor1(Barrel, Magazine);
        WeaponDescriptorPart2 = CompareItemDescriptor2(Stock, Muzzle, Scope);
        WeaponDescriptorPart3 = Reciever.GetDescriptorName(TotalRatingPoints);
    }

    private void ResetStats()
    {        
        ReloadMultiplier = double.NaN;
        CockRateMultiplier = double.NaN;
        
        DamageClose = double.NaN;
        DamageFar = double.NaN;

        RangeClose = double.NaN;
        RangeFar = double.NaN;
        RangeTracer = double.NaN;

        RecoilHip = double.NaN;
        RecoilZoom = double.NaN;

        SpreadWhileADS = double.NaN;
        SpreadWhileMoving = double.NaN;
        SpreadWhileStanding = double.NaN;

        ModifiedScopeInTime = double.NaN;
        ModifiedRunSpeed = double.NaN;
    }
    private void CalculateCockRate()
    {
        double allRecoil = Percentage(RecoilPercentage);
        double alpha = Math.Abs(allRecoil);
        double cockrate;
        if ((Reciever?.WeaponStats?.ModificationRangeCockRate.Z ?? 0) != 0)
        {
            if (allRecoil > 0)
            {
                cockrate = Lerp(Reciever.WeaponStats.ModificationRangeCockRate.Z, Reciever.WeaponStats.ModificationRangeCockRate.Y, alpha);
            }
            else
            {
                cockrate = Lerp(Reciever.WeaponStats.ModificationRangeCockRate.Z, Reciever.WeaponStats.ModificationRangeCockRate.X, alpha);
            }
            if (cockrate > 0)
            {
                cockrate = 1.0 / cockrate;
            }
            CockRateMultiplier = cockrate;
        }
        else
        {
            CockRateMultiplier = 1.0;
        }
    }
    private void CalculateReloadRate()
    {
        double allReloadSpeed = Percentage(ReloadSpeedPercentage);
        double allRecoil = Percentage(RecoilPercentage);
        double WeaponReloadRate = 1.0;
        double rate_alpha;

        if ((Reciever?.WeaponStats?.ModificationRangeReloadRate.Z ?? 0) > 0)
        {
            rate_alpha = Math.Abs(allReloadSpeed);
            if (allReloadSpeed > 0)
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeReloadRate.Z, Reciever.WeaponStats.ModificationRangeReloadRate.X, rate_alpha);
            }
            else
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeReloadRate.Z, Reciever.WeaponStats.ModificationRangeReloadRate.Y, rate_alpha);
            }
        }

        if ((Reciever?.WeaponStats?.ModificationRangeRecoilReloadRate.Z ?? 0 ) == 1)
        {
            rate_alpha = Math.Abs(allRecoil);
            if (allRecoil > 0)
            {
                WeaponReloadRate = Lerp(Reciever?.WeaponStats?.ModificationRangeRecoilReloadRate.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeRecoilReloadRate.X ?? 0, rate_alpha);
            }
            else
            {
                WeaponReloadRate = Lerp(Reciever?.WeaponStats?.ModificationRangeRecoilReloadRate.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeRecoilReloadRate.Y ?? 0, rate_alpha);
            }
        }
        ReloadMultiplier = WeaponReloadRate;
    }
    private void CalculateMovementSpeed()
    {
        double allMovementSpeed = Percentage(MovementSpeedPercentage);
        double move_alpha = Math.Abs(allMovementSpeed);
        double move_modifier;
        if (allMovementSpeed > 0)
        {
            move_modifier = Lerp(Reciever?.WeaponStats?.ModificationRangeMoveSpeed.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeMoveSpeed.Y ?? 0, move_alpha);
        }
        else
        {
            move_modifier = Lerp(Reciever?.WeaponStats?.ModificationRangeMoveSpeed.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeMoveSpeed.X ?? 0, move_alpha);
        }
        ModifiedRunSpeed = (765 + (move_modifier * 0.9)) / 100.0f; // Apparently percent of movement from gear is applied to weapons, and not percent of movement from weapons
    }
    private void CalculateSpread(double allMovementSpread)
    {
        double allAccuracy = Percentage(AccuracyPercentage);
        double accuracyBaseModifier;
        double accuracyTABaseModifier;
        double alpha = Math.Abs(allAccuracy);
        if (allAccuracy > 0)
        {
            accuracyBaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeBaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeBaseSpread.Y ?? 0, alpha);
            accuracyTABaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeTABaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeTABaseSpread.Y ?? 0, alpha);
        }
        else
        {
            accuracyBaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeBaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeBaseSpread.X ?? 0, alpha);
            accuracyTABaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeTABaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeTABaseSpread.X ?? 0, alpha);
        }

        double hip = accuracyBaseModifier * (180 / Math.PI);
        double aim = (accuracyBaseModifier * (Reciever?.WeaponStats?.ZoomSpreadMultiplier ?? 0)) * (180 / Math.PI);
        if (Reciever?.WeaponStats?.UseTABaseSpread ?? false)
        {
            aim = accuracyTABaseModifier * (float)(180 / Math.PI);
        }

        double weight_alpha = Math.Abs((Reciever?.WeaponStats?.Weight ?? 0) / 80.0);
        double weight_clampalpha = Math.Min(Math.Max(weight_alpha, -1.0), 1.0); // Don't ask me why they clamp the absolute value with a negative, I have no idea.
        double weight_multiplier;
        if ((Reciever?.WeaponStats?.Weight ?? 0) > 0)   // It was originally supposed to compare the total weight of equipped mods, but from what I can currently gather from the scripts, nothing modifies weapon weight so I'm just comparing base weight for now.
        {
            weight_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Y ?? 0, weight_clampalpha);  // Originally supposed to be a weapon specific range, but they all set the same values so it's not worth setting elsewhere.
        }
        else
        {
            weight_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.X ?? 0, weight_clampalpha);
        }

        double move_alpha = Math.Abs(allMovementSpread); // Combied movement speed modifiers from only barrel and stock, divided by 100
        double move_multiplier; // Applying movement to it like this isn't how it's done to my current knowledge, but seems to be consistently closer to how it should be in most cases so far.
        if (allMovementSpread > 0)
        {
            move_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Y ?? 0, move_alpha);
        }
        else
        {
            move_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.X ?? 0, move_alpha);
        }

        double movemultiplier_current = 1.0 + (((Reciever?.WeaponStats?.MovementSpreadMultiplier ?? 0) - 1.0) * (weight_multiplier * move_multiplier));
        double moveconstant_current = (Reciever?.WeaponStats?.MovementSpreadConstant ?? 0) * (weight_multiplier * move_multiplier);

        double move = ((accuracyBaseModifier + moveconstant_current) * (180 / Math.PI)) * movemultiplier_current;

        // Average spread over multiple shots to account for random center weight multiplier
        double[] averageSpread = { 0, 0, 0 };
        double magsize = Math.Min(Reciever?.WeaponStats?.MagSize ?? 0, 15.0f);
        if (magsize <= 1)
        {
            magsize = (Reciever?.WeaponStats?.InitialMagazines ?? 0) + 1.0;
        }
        if (magsize > 0)
        {
            double averageShotCount = Math.Max(magsize, 5.0f);
            for (int shot = 1; shot <= averageShotCount; shot++)
            {
                if (shot > (averageShotCount - (averageShotCount * (Reciever?.WeaponStats?.SpreadCenterWeight ?? 0))))
                {
                    averageSpread[0] += aim * (Reciever?.WeaponStats?.SpreadCenter ?? 0);
                    averageSpread[1] += hip * (Reciever?.WeaponStats?.SpreadCenter ?? 0);
                    averageSpread[2] += move * (Reciever?.WeaponStats?.SpreadCenter ?? 0);
                }
                else
                {
                    averageSpread[0] += aim;
                    averageSpread[1] += hip;
                    averageSpread[2] += move;
                }
            }
            averageSpread[0] /= averageShotCount;
            averageSpread[1] /= averageShotCount;
            averageSpread[2] /= averageShotCount;
        }

        SpreadWhileADS = averageSpread[0];
        SpreadWhileStanding = averageSpread[1];
        SpreadWhileMoving = averageSpread[2];
    }
    private double CalculateScopeInTime(double allMovementScopeIn)
    {
        double TTTA_alpha = Math.Abs(allMovementScopeIn);
        double TightAimTime, ComboScopeMod, FourXAmmoCounterMod, ArmComInfraredMod, EMITechScopeMod, EMIInfraredMod, EMIInfraredMK2Mod, ArmComSniperMod, KraneSniperScopeMod, SilverwoodHeavyMod, FrontierSniperMod;

        // giant cheat incoming, please lord forgive me for what i am about to do
        if (allMovementScopeIn > 0)
        {
            TightAimTime = Lerp(0.225, 0.15, TTTA_alpha);
            ComboScopeMod = Lerp(0.0, 0.03, TTTA_alpha);
            FourXAmmoCounterMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            ArmComInfraredMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            EMITechScopeMod = Lerp(RawScopeInTime, 0.215, TTTA_alpha);
            EMIInfraredMod = Lerp(RawScopeInTime, 0.215, TTTA_alpha);
            EMIInfraredMK2Mod = Lerp(RawScopeInTime, 0.36, TTTA_alpha);
            ArmComSniperMod = Lerp(RawScopeInTime, 0.275, TTTA_alpha);
            KraneSniperScopeMod = Lerp(RawScopeInTime, 0.275, TTTA_alpha);
            SilverwoodHeavyMod = Lerp(RawScopeInTime, 0.275, TTTA_alpha);
            FrontierSniperMod = Lerp(RawScopeInTime, 0.315, TTTA_alpha);
        }
        else
        {
            TightAimTime = Lerp(0.225, 0.30, TTTA_alpha);
            ComboScopeMod = Lerp(0.0, -0.05, TTTA_alpha);
            FourXAmmoCounterMod = Lerp(RawScopeInTime, 0.10, TTTA_alpha);
            ArmComInfraredMod = Lerp(RawScopeInTime, 0.10, TTTA_alpha);
            EMITechScopeMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            EMIInfraredMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            EMIInfraredMK2Mod = Lerp(RawScopeInTime, 0.30, TTTA_alpha);
            ArmComSniperMod = Lerp(RawScopeInTime, 0.20, TTTA_alpha);
            KraneSniperScopeMod = Lerp(RawScopeInTime, 0.20, TTTA_alpha);
            SilverwoodHeavyMod = Lerp(RawScopeInTime, 0.20, TTTA_alpha);
            FrontierSniperMod = Lerp(RawScopeInTime, 0.235, TTTA_alpha);
        }

        if ((Reciever?.WeaponStats?.TightAimTime ?? 0) > 0)
        {
            return Reciever?.WeaponStats?.TightAimTime ?? 0;
        }
        else
        {
            if (TightAimTime > 0)
            {
                return (Scope?.UID) switch
                {
                    45005 => TightAimTime + ComboScopeMod + RawScopeInTime,
                    45023 => TightAimTime + FourXAmmoCounterMod,
                    45021 => TightAimTime + ArmComInfraredMod,
                    45020 => TightAimTime + EMIInfraredMod,
                    45019 => TightAimTime + EMIInfraredMK2Mod,
                    45015 => TightAimTime + ArmComSniperMod,
                    45008 => TightAimTime + SilverwoodHeavyMod,
                    45007 => TightAimTime + KraneSniperScopeMod,
                    45004 => TightAimTime + EMITechScopeMod,
                    45001 => TightAimTime + FrontierSniperMod,
                    _ => TightAimTime + RawScopeInTime,
                };
            }
        }
        return 0.225;
    }
    private void CalculateRange()
    {
        double allRange = Percentage(RangePercentage);
        double idealRange;
        double maxRange;
        double alpha = Math.Abs(allRange);
        if (allRange > 0)
        {
            idealRange = (int)Lerp(Reciever?.WeaponStats?.ModificationRangeIdealDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeIdealDistance.Y ?? 0, alpha);
            maxRange = Lerp(Reciever?.WeaponStats?.ModificationRangeMaxDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeMaxDistance.Y ?? 0, alpha);
        }
        else
        {
            idealRange = (int)Lerp(Reciever?.WeaponStats?.ModificationRangeIdealDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeIdealDistance.X ?? 0, alpha);
            maxRange = Lerp(Reciever?.WeaponStats?.ModificationRangeMaxDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeMaxDistance.X ?? 0, alpha);
        }

        RangeClose = idealRange / 100;
        RangeFar = maxRange / 100;
        RangeTracer = (Reciever?.WeaponStats?.MaxTraceDistance ?? 0) / 100 ;
    }

    private void CalculateRecoil()
    {
        double allRecoil = Percentage(RecoilPercentage);
        double recoilModifier;
        double alpha = Math.Abs(allRecoil);
        if (allRecoil > 0)
        {
            recoilModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeRecoil.Y ?? 0, alpha);
        }
        else
        {
            recoilModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeRecoil.X ?? 0, alpha);
        }
        if ((Reciever?.WeaponStats?.MagSize ?? 0) > 0)
        {
            double averageShotCount = Math.Min(Reciever?.WeaponStats?.MagSize ?? 0, 15.0f);
            Vector3 averageRecoil = new(0, 0, 0);

            for (int shot = 1; shot <= averageShotCount; shot++)
            {
                Vector3 newRecoil = new(0, 0, 0)
                {
                    X = ((Reciever?.WeaponStats?.RecoilVector.X ?? 0) * (Reciever?.WeaponStats?.RecoilVectorMultiplier.X ?? 0) * 0.5f) / 2.0f, // in the recoil, recoil vector is actually a multiplier on a random X and Y value in the -0.5/0.5 and 0.0/0.3535 range respectively
                    Y = ((Reciever?.WeaponStats?.RecoilVector.Y ?? 0) * (Reciever?.WeaponStats?.RecoilVectorMultiplier.Y ?? 0) * 0.3535f)
                };

                double accumExponent = Reciever?.WeaponStats?.RecoilAccumulation ?? 0;
                if (accumExponent > 1)
                {
                    accumExponent = ((accumExponent - 1.0) * (Reciever?.WeaponStats?.RecoilAccumulationMultiplier ?? 0)) + 1.0; // Apparently this is how they apply the accumulation multiplier in the actual recoil
                }

                double previousMultiplier = (Reciever?.WeaponStats?.RecoilSize ?? 0) * Math.Pow(shot / (Reciever?.WeaponStats?.Burst ?? 0), accumExponent);
                double currentMultiplier = (Reciever?.WeaponStats?.RecoilSize ?? 0) * Math.Pow(shot / (Reciever?.WeaponStats?.Burst ?? 0) + 1.0f, accumExponent);
                double multiplier = currentMultiplier - previousMultiplier;
                newRecoil *= (float)multiplier;
                averageRecoil += newRecoil;
            }

            if (averageShotCount > 0)
            {
                averageRecoil /= (float)averageShotCount;
            }
            if ((Reciever?.WeaponStats?.ROF ?? 0) > 0 && (Reciever?.WeaponStats?.ApplyTime ?? 0 ) > 60 / (Reciever?.WeaponStats?.ROF ?? 999))
            {
                averageRecoil *= (float)(60 / ((Reciever?.WeaponStats?.ROF ?? 0) * (Reciever?.WeaponStats?.ApplyTime ?? 0)));
            }
            double recoil = averageRecoil.Length() * recoilModifier;
            recoil *= (180 / Math.PI);
            RecoilHip = recoil;
            RecoilZoom = recoil * (Reciever?.WeaponStats?.RecoilZoomMultiplier ?? 0) * 0.8;
        }
        else
        {
            RecoilHip = 0;
            RecoilZoom = 0;
        }
    }

    private void CalculateDamage()
    {
        double allDamage = Percentage(DamagePercentage);
        double damageModifier;
        double alpha = Math.Abs(allDamage);
        if (allDamage > 0)
        {
            damageModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeDamage.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeDamage.Y ?? 0, alpha);
        }
        else
        {
            damageModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeDamage.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeDamage.X ?? 0, alpha);
        }

        DamageClose = damageModifier;
        DamageFar = damageModifier * (Reciever?.WeaponStats?.MaxRangeDamageMultiplier ?? 0.1d);
    }

    private static string CompareItemDescriptor1(BLRItem itembarrel, BLRItem itemmag)
    {
        if (itembarrel == null && itemmag != null)
        {
            return itemmag.DescriptorName;
        }
        else if (itembarrel != null && itemmag == null)
        {
            return itembarrel.DescriptorName;
        }
        else if (itembarrel == null && itemmag == null)
        {
            return "Standard";
        }

        if (itembarrel.WeaponModifiers.rating > itemmag.WeaponModifiers.rating)
        {
            return itembarrel.DescriptorName;
        }
        else
        {
            return itemmag.DescriptorName;
        }
    }

    private static string CompareItemDescriptor2(BLRItem itemstock, BLRItem itemmuzzle, BLRItem itemscope)
    {
        if (itemstock == null && itemmuzzle == null && itemscope == null)
        {
            return "Basic";
        }

        if (itemstock == null && itemmuzzle == null && itemscope != null)
        {
            return itemscope.DescriptorName;
        }
        else if (itemstock == null && itemmuzzle != null && itemscope != null)
        {
            if (itemmuzzle.WeaponModifiers.rating >= itemscope.WeaponModifiers.rating)
            {
                return itemmuzzle.DescriptorName;
            }
            else
            {
                return itemscope.DescriptorName;
            }
        }
        else if (itemstock == null && itemmuzzle != null)
        {
            return itemmuzzle.DescriptorName;
        }
        else if (itemstock != null && itemmuzzle == null)
        {
            return itemstock.DescriptorName;
        }

        if ((itemstock.WeaponModifiers.rating >= itemmuzzle.WeaponModifiers.rating) && (itemstock.WeaponModifiers.rating >= itemscope?.WeaponModifiers.rating))
        {
            if (itemstock.WeaponModifiers.rating > 0)
            {
                return itemstock.DescriptorName;
            }
            return "Basic";
        }
        else if ((itemmuzzle.WeaponModifiers.rating >= itemstock.WeaponModifiers.rating) && (itemmuzzle.WeaponModifiers.rating >= itemscope?.WeaponModifiers.rating))
        {
            return itemmuzzle.DescriptorName;
        }
        else if ((itemscope?.WeaponModifiers.rating >= itemstock.WeaponModifiers.rating) && (itemscope?.WeaponModifiers.rating >= itemmuzzle.WeaponModifiers.rating))
        {
            return itemscope?.DescriptorName;
        }

        return itemstock.DescriptorName;
    }

    public static double Percentage(double input)
    { 
        return Clamp(input, -100.0D, 100.0D) / 100.0D;
    }

    public static double Lerp(double start, double target, double time)
    {
        return start * (1.0d - time) + target * time;
    }

    public static double Clamp(double input, double min, double max)
    { 
        return Math.Min(Math.Max(input, min), max);
    }

    public void UpdateMagiCowsWeapon(ref MagiCowsWeapon weapon)
    {
        weapon.Receiver = Reciever?.Name ?? "Assault Rifle";
        weapon.Muzzle = ImportSystem.GetIDOfItem(Muzzle);
        weapon.Barrel = Barrel?.Name ?? "No Barrel Mod";
        weapon.Magazine = ImportSystem.GetIDOfItem(Magazine);
        weapon.Scope = Scope?.Name ?? "No Optic Mod";
        weapon.Stock = Stock?.Name ?? "No Stock";
        weapon.Grip = Grip?.Name ?? "";
        weapon.Tag = ImportSystem.GetIDOfItem(Tag);
        weapon.Camo = ImportSystem.GetIDOfItem(Camo);
    }
}