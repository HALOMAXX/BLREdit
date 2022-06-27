using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Drawing;

namespace BLREdit;

public class BLRItem
{
    public string Category { get; set; }
    public string Class { get; set; }
    public string DescriptorName { get; set; } = "";
    public string Icon { get; set; }
    public string Name { get; set; }

    public BLRPawnModifiers PawnModifiers { get; set; }
    public List<string> SupportedMods { get; set; }
    public string Tooltip { get; set; }
    public int UID { get; set; }
    public List<int> ValidFor { get; set; }
    public BLRWeaponModifiers WeaponModifiers { get; set; }
    public BLRWeaponStats WeaponStats { get; set; }
    public BLRWikiStats WikiStats { get; set; }

    [JsonIgnore]
    public BitmapSource WideImage { get { return GetWideImage(); } }
    [JsonIgnore]
    public BitmapSource LargeSquareImage { get { return GetLargeSquareImage(); } }
    [JsonIgnore]
    public BitmapSource SmallSquareImage { get { return GetSmallSquareImage(); } }
    [JsonIgnore]
    public BitmapSource wideImageMale = null;
    [JsonIgnore]
    public BitmapSource largeSquareImageMale = null;
    [JsonIgnore]
    public BitmapSource smallSquareImageMale = null;
    [JsonIgnore]
    public BitmapSource wideImageFemale = null;
    [JsonIgnore]
    public BitmapSource largeSquareImageFemale = null;
    [JsonIgnore]
    public BitmapSource smallSquareImageFemale = null;


    [JsonIgnore]
    public BitmapSource Crosshair { get; private set; }
    [JsonIgnore]
    public BitmapSource MiniPrimaryCrosshair { get { return GetBitmapCrosshair(Name); } }
    [JsonIgnore]
    public BitmapSource MiniSecondaryCrosshair { get { return GetBitmapCrosshair(GetSecondaryScope()); } }


    public BLRItem() { }
    public BLRItem(ImportItem item)
    {
        Category = item.Category;
        Class = item._class;
        DescriptorName = item.descriptorName;
        Icon = item.icon;
        Name = item.name;

        if (item.pawnModifiers != null)
            PawnModifiers = new BLRPawnModifiers(item.pawnModifiers);

        SupportedMods = new();
        if (item.supportedMods != null)
        {
            SupportedMods.AddRange(item.supportedMods);
        }
        Tooltip = item.tooltip;
        UID = item.uid;
        ValidFor = new();
        if (item.validFor != null)
        {
            ValidFor.AddRange(item.validFor);
        }
        if (item.weaponModifiers != null)
            WeaponModifiers = new BLRWeaponModifiers(item.weaponModifiers);

        if (item.stats != null && item.IniStats != null)
            WeaponStats = new BLRWeaponStats(item.stats, item.IniStats);

        if (item.WikiStats != null)
            WikiStats = new BLRWikiStats(item.WikiStats);
    }

    public BitmapSource GetWideImage()
    {
        return GetImage(wideImageMale, wideImageFemale);
    }
    public BitmapSource GetLargeSquareImage()
    {
        return GetImage(largeSquareImageMale, largeSquareImageFemale);
    }
    public BitmapSource GetSmallSquareImage()
    {
        return GetImage(smallSquareImageMale, smallSquareImageFemale);
    }

    public static BitmapSource GetImage(BitmapSource male, BitmapSource female)
    {
        if (UI.MainWindow.ActiveLoadout.IsFemale)
        {
            if (female == null)
            { return male; }
            return female;
        }
        else
        {
            return male;
        }
    }

    public string GetDescriptorName(double points)
    {
        string currentbest = "";
        foreach (StatDecriptor st in WeaponStats.StatDecriptors)
        {
            if (points >= st.Points)
            {
                currentbest = st.Name;
            }
        }
        return currentbest;
    }


    public bool IsValidForItemIDS(params int[] uids)
    {
        foreach (int valid in ValidFor)
        {
            foreach (int uid in uids)
            {
                if (valid == uid)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsValidFor(BLRItem item)
    {
        if (item is null) return false;
        if (Category != ImportSystem.MAGAZINES_CATEGORY && Category != ImportSystem.MUZZELS_CATEGORY && Category != ImportSystem.SCOPES_CATEGORY && Category != ImportSystem.STOCKS_CATEGORY && Category != ImportSystem.BARRELS_CATEGORY) return true;

        if (ValidFor == null) { return true; }

        foreach (int id in ValidFor)
        {
            if (id == item.UID)
            { return true; }
        }
        return false;
    }


    internal bool IsValidModType(string modType)
    {
        foreach (string supportedModType in SupportedMods)
        {
            if (modType == supportedModType)
            {
                return true;
            }
        }
        return false;
    }

    public void LoadImage()
    {
        bool male = false;
        if (!string.IsNullOrEmpty(Icon))
        {
            foreach (FoxIcon foxicon in ImportSystem.Icons)
            {
                if (foxicon.Name == Icon)
                {
                    wideImageMale = foxicon.GetWideImage();
                    largeSquareImageMale = foxicon.GetLargeSquareImage();
                    smallSquareImageMale = foxicon.GetSmallSquareImage();
                    male = true;
                }
                if (foxicon.Name == GetFemaleIconName())
                {
                    wideImageFemale = foxicon.GetWideImage();
                    largeSquareImageFemale = foxicon.GetLargeSquareImage();
                    smallSquareImageFemale = foxicon.GetSmallSquareImage();
                }
            }
        }
        if (!male)
        {
            wideImageMale = FoxIcon.CreateEmptyBitmap(256, 128);
            largeSquareImageMale = FoxIcon.CreateEmptyBitmap(128, 128);
            smallSquareImageMale = FoxIcon.CreateEmptyBitmap(64, 64);
        }
    }

    private string GetFemaleIconName()
    {
        string[] parts = Icon.Split('_');
        string female = "";
        for (int i = 0; i < parts.Length; i++)
        {
            if (i == parts.Length - 1)
            {
                female += "_Female";
            }
            if (i == 0)
            {
                female += parts[i];
            }
            else
            {
                female += "_" + parts[i];
            }
        }
        return female;
    }

    public void LoadCrosshair(bool isPrimary)
    {
        if (isPrimary)
        {
            Crosshair = GetBitmapCrosshair(Name);
        }
        else
        {
            Crosshair = GetBitmapCrosshair(GetSecondaryScope());
        }
    }

    private string GetSecondaryScope()
    {
        var name = (UI.MainWindow.Self.SecondaryRecieverImage.DataContext as BLRItem)?.Name ?? "";
        switch (Name)
        {
            case "No Optic Mod":

                if (name.Contains("Prestige"))
                {
                    return Name + " Light Pistol";
                }
                else
                {
                    return Name + " " + name;
                }

            //Pistols Only
            case "OPRL Holo Sight":
            case "Lightsky Reflex Sight":
            case "Krane Tactical Scope":
            case "EON Electric Scope":
            case "EMI Electric Scope":
            case "ArmCom CQC Scope":
            case "Aim Point Ammo Counter":
                return Name + GetSecondayScopePistol(name);

            //Pistols and shotguns
            case "Titan Rail Sight":
            case "MMRS Flip-Up Rail Sight":
            case "Lightsky Red Dot Sight":
            case "Krane Holo Sight":
                return Name + GetSecondayScopePistol(name) + GetSecondayScopeShotgun(name);

            default:
                return Name;
        }
    }

    private static string GetSecondayScopePistol(string secondaryName)
    {
        switch (secondaryName)
        {
            case "Breech Loaded Pistol":
            case "Snub 260":
            case "Heavy Pistol":
            case "Light Pistol":
            case "Burstfire Pistol":
            case "Prestige Light Pistol":
            case "Machine Pistol":
            case "Revolver":
                return " Pistol";
            default:
                return "";
        }
    }

    private static string GetSecondayScopeShotgun(string secondaryName)
    {
        switch (secondaryName)
        {
            case "Shotgun":
            case "Shotgun AR-k":
                return " Shotgun";
            default:
                return "";
        }
    }

    public void RemoveCrosshair()
    {
        Crosshair = null;
    }

    public static BitmapSource GetBitmapCrosshair(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            foreach (FoxIcon icon in ImportSystem.Crosshairs)
            {
                if (icon.Name.Equals(name))
                {
                    return new BitmapImage(icon.Icon);
                }
            }
        }
        return FoxIcon.CreateEmptyBitmap(1, 1);
    }

    [JsonIgnore]
    public DisplayStatDiscriptor DisplayStat1 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor DisplayStat2 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor DisplayStat3 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor DisplayStat4 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor DisplayStat5 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor DisplayStat6 { get; set; }

    [JsonIgnore]
    public double None
    {
        get
        {
            if (Tooltip == "Depot Item!")
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }

    [JsonIgnore]
    public double Accuracy
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.SECONDARY_CATEGORY:
                case ImportSystem.PRIMARY_CATEGORY:
                    return WeaponStats.accuracy;
                default:
                    return WeaponModifiers?.accuracy ?? 0;
            }
        }
    }
    [JsonIgnore]
    public double Aim
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.SECONDARY_CATEGORY:
                case ImportSystem.PRIMARY_CATEGORY:
                    return UI.MainWindow.CalculateSpread(this, 0, 0)[0];
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Ammo
    {
        get 
        {
            switch (Category)
            {
                case ImportSystem.SECONDARY_CATEGORY:
                case ImportSystem.PRIMARY_CATEGORY:
                    return WikiStats.ammoMag;
                case ImportSystem.MAGAZINES_CATEGORY:
                    return WeaponModifiers.ammo;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Damage
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    return UI.MainWindow.CalculateDamage(this, 0)[0];
                default:
                    return WeaponModifiers?.damage ?? 0;
            }
        }
    }

    [JsonIgnore]
    public double ElectroProtection
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.ATTACHMENTS_CATEGORY:
                    return PawnModifiers?.ElectroProtection ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double ExplosiveProtection 
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.ATTACHMENTS_CATEGORY:
                    return PawnModifiers?.ExplosiveProtection ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double GearSlots 
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.UPPER_BODIES_CATEGORY:
                case ImportSystem.LOWER_BODIES_CATEGORY:
                    return PawnModifiers?.GearSlots ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Health 
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.UPPER_BODIES_CATEGORY:
                case ImportSystem.LOWER_BODIES_CATEGORY:
                    return PawnModifiers?.Health ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double HeadProtection
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                    return PawnModifiers?.HelmetDamageReduction ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Hip
    {
        get 
        {
            switch (Category)
            {
                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    return UI.MainWindow.CalculateSpread(this, 0, 0)[1];
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double HRVDuration
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                    return PawnModifiers?.HRVDuration ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double HRVRecharge
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                    return PawnModifiers?.HRVRechargeRate ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double IncendiaryProtection
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.ATTACHMENTS_CATEGORY:
                    return PawnModifiers?.IncendiaryProtection ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double InfraredProtection 
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.ATTACHMENTS_CATEGORY:
                    return PawnModifiers?.InfraredProtection ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Infrared
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.SCOPES_CATEGORY:
                    if (UID != 45019 && UID != 45020 && UID != 45021)
                    {
                        return WikiStats?.zoom ?? 0;
                    }
                    else
                    {
                        return (WikiStats?.zoom ?? 0) + 10;
                    }
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double MeleeProtection
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.ATTACHMENTS_CATEGORY:
                    return PawnModifiers?.MeleeProtection ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Move
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    return UI.MainWindow.CalculateSpread(this, 0, 0)[2];
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Range
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    return WeaponStats?.range ?? 0;
                default:
                    return WeaponModifiers?.range ?? 0;
            }
        }
    }
    [JsonIgnore]
    public double Recoil
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    return WeaponStats?.recoil ?? 0;
                default:
                    return WeaponModifiers?.recoil ?? 0;
            }
        }
    }
    [JsonIgnore]
    public double Run
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.UPPER_BODIES_CATEGORY:
                case ImportSystem.LOWER_BODIES_CATEGORY:
                    return PawnModifiers?.MovementSpeed ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double ScopeInTime
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.SCOPES_CATEGORY:
                    return WikiStats?.scopeInTime ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double ToxicProtection
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                case ImportSystem.ATTACHMENTS_CATEGORY:
                    return PawnModifiers?.ToxicProtection ?? 0;
                default:
                    return 0;
            }
        }
    }
    [JsonIgnore]
    public double Zoom
    {
        get
        {
            switch (Category)
            {
                case ImportSystem.SCOPES_CATEGORY:
                    return WikiStats?.zoom ?? 0;
                default:
                    return 0;
            }
        }
    }

}

public class BLRPawnModifiers
{
    public double BodyDamageReduction { get; set; } = 0;
    public double ElectroProtection { get; set; } = 0;
    public double ExplosiveProtection { get; set; } = 0;
    public double GearSlots { get; set; } = 0;
    public double HRVDuration { get; set; } = 0;
    public double HRVRechargeRate { get; set; } = 0;
    public double Health { get; set; } = 0;
    public double HealthRecharge { get; set; } = 0;
    public double HelmetDamageReduction { get; set; } = 0;
    public double IncendiaryProtection { get; set; } = 0;
    public double InfraredProtection { get; set; } = 0;
    public double LegsDamageReduction { get; set; } = 0;
    public double MeleeProtection { get; set; } = 0;
    public double MeleeRange { get; set; } = 0;
    public double MovementSpeed { get; set; } = 0;
    public double PermanentHealthProtection { get; set; } = 0;
    public double SprintMultiplier { get; set; } = 1;
    public double Stamina { get; set; } = 0;
    public double SwitchWeaponSpeed { get; set; } = 0;
    public double ToxicProtection { get; set; } = 0;

    public BLRPawnModifiers(PawnModifiers pawnModifiers)
    {
        if (pawnModifiers == null)
        { return; }
        BodyDamageReduction = pawnModifiers.BodyDamageReduction;
        ElectroProtection = pawnModifiers.ElectroProtection;
        ExplosiveProtection = pawnModifiers.ExplosiveProtection;
        GearSlots = pawnModifiers.GearSlots;
        HRVDuration = pawnModifiers.HRVDuration;
        HRVRechargeRate = pawnModifiers.HRVRechargeRate;
        Health = pawnModifiers.Health;
        HealthRecharge = pawnModifiers.HealthRecharge;
        HelmetDamageReduction = pawnModifiers.HelmetDamageReduction;
        IncendiaryProtection = pawnModifiers.IncendiaryProtection;
        InfraredProtection = pawnModifiers.InfraredProtection;
        LegsDamageReduction = pawnModifiers.LegsDamageReduction;
        MeleeProtection = pawnModifiers.MeleeProtection;
        MeleeRange = pawnModifiers.MeleeRange;
        MovementSpeed = pawnModifiers.MovementSpeed;
        PermanentHealthProtection = pawnModifiers.PermanentHealthProtection;
        SprintMultiplier = pawnModifiers.SprintMultiplier;
        Stamina = pawnModifiers.Stamina;
        SwitchWeaponSpeed = pawnModifiers.SwitchWeaponSpeed;
        ToxicProtection = pawnModifiers.ToxicProtection;
    }
    public BLRPawnModifiers() { }
}

public class BLRWeaponModifiers
{
    public double accuracy { get; set; } = 0;
    public double ammo { get; set; } = 0;
    public double damage { get; set; } = 0;
    public double movementSpeed { get; set; } = 0;
    public double range { get; set; } = 0;
    public double rateOfFire { get; set; } = 0;
    public double rating { get; set; } = 0;
    public double recoil { get; set; } = 0;
    public double reloadSpeed { get; set; } = 0;
    public double switchWeaponSpeed { get; set; } = 0;
    public double weaponWeight { get; set; } = 0;

    public BLRWeaponModifiers(WeaponModifiers weaponModifiers)
    {
        if (weaponModifiers == null)
        { return; }
        accuracy = weaponModifiers.accuracy;
        ammo = weaponModifiers.ammo;
        damage = weaponModifiers.damage;
        movementSpeed = weaponModifiers.movementSpeed;
        range = weaponModifiers.range;
        rateOfFire = weaponModifiers.rateOfFire;
        rating = weaponModifiers.rating;
        recoil = weaponModifiers.recoil;
        reloadSpeed = weaponModifiers.reloadSpeed;
        switchWeaponSpeed = weaponModifiers.switchWeaponSpeed;
        weaponWeight = weaponModifiers.weaponWeight;
    }
    public BLRWeaponModifiers() { }
}

public class BLRWeaponStats
{
    public double accuracy { get; set; }
    public double damage { get; set; }
    public double movementSpeed { get; set; }
    public double range { get; set; }
    public double rateOfFire { get; set; }
    public double recoil { get; set; }
    public double reloadSpeed { get; set; }
    public double weaponWeight { get; set; }

    public double ApplyTime { get; set; } = 0;
    public double BaseSpread { get; set; } = 0.04f;
    public double Burst { get; set; } = 0;
    public double CrouchSpreadMultiplier { get; set; } = 0.5f;
    public double InitialMagazines { get; set; } = 4;
    public double IdealDistance { get; set; } = 8000;
    public double JumpSpreadMultiplier { get; set; } = 4.0f;
    public double MagSize { get; set; } = 30;
    public double MaxDistance { get; set; } = 16384;
    public double MaxRangeDamageMultiplier { get; set; } = 0.1f;
    public double MaxTraceDistance { get; set; } = 15000;
    public Vector3 ModificationRangeBaseSpread { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeCockRate { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeDamage { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeIdealDistance { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeMaxDistance { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeMoveSpeed { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeRecoil { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeReloadRate { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeRecoilReloadRate { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeTABaseSpread { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeWeightMultiplier { get; set; } = Vector3.Zero;
    public double MovementSpreadConstant { get; set; } = 0.0f;
    public double MovementSpreadMultiplier { get; set; } = 2.5f;
    public double RecoilAccumulation { get; set; } = 0;
    public double RecoilAccumulationMultiplier { get; set; } = 0.95f;
    public double RecoilSize { get; set; } = 0;
    public Vector3 RecoilVector { get; set; } = Vector3.Zero;
    public Vector3 RecoilVectorMultiplier { get; set; } = Vector3.Zero;
    public double RecoilZoomMultiplier { get; set; } = 0.5f;
    public double ROF { get; set; } = 0;
    public StatDecriptor[] StatDecriptors { get; set; } = new StatDecriptor[] { new StatDecriptor() };
    public double TABaseSpread { get; set; } = 0;
    public double TightAimTime { get; set; } = 0.0f;
    public bool UseTABaseSpread { get; set; } = false;
    public double Weight { get; set; } = 150.0f;
    public double ZoomSpreadMultiplier { get; set; } = 0.4f;

    public BLRWeaponStats(ImportStats stats, IniStats iniStats)
    {
        if (stats != null)
        {
            accuracy = stats.accuracy;
            damage = stats.damage;
            movementSpeed = stats.movementSpeed;
            range = stats.range;
            rateOfFire = stats.rateOfFire;
            recoil = stats.recoil;
            reloadSpeed = stats.reloadSpeed;
            weaponWeight = stats.weaponWeight;
        }

        if (iniStats != null)
        {
            ApplyTime = iniStats.ApplyTime;
            Burst = iniStats.Burst;
            BaseSpread = iniStats.BaseSpread;
            CrouchSpreadMultiplier = iniStats.CrouchSpreadMultiplier;
            InitialMagazines = iniStats.InitialMagazines;
            IdealDistance = iniStats.IdealDistance;
            JumpSpreadMultiplier = iniStats.JumpSpreadMultiplier;
            MagSize = iniStats.MagSize;
            MaxDistance = iniStats.MaxDistance;
            MaxRangeDamageMultiplier = iniStats.MaxRangeDamageMultiplier;
            MaxTraceDistance = iniStats.MaxTraceDistance;
            ModificationRangeBaseSpread = iniStats.ModificationRangeBaseSpread;
            ModificationRangeCockRate = iniStats.ModificationRangeCockRate;
            ModificationRangeDamage = iniStats.ModificationRangeDamage;
            ModificationRangeIdealDistance = iniStats.ModificationRangeIdealDistance;
            ModificationRangeMaxDistance = iniStats.ModificationRangeMaxDistance;
            ModificationRangeMoveSpeed = iniStats.ModificationRangeMoveSpeed;
            ModificationRangeRecoil = iniStats.ModificationRangeRecoil;
            ModificationRangeRecoilReloadRate = iniStats.ModificationRangeRecoilReloadRate;
            ModificationRangeReloadRate = iniStats.ModificationRangeReloadRate;
            ModificationRangeTABaseSpread = iniStats.ModificationRangeTABaseSpread;
            ModificationRangeWeightMultiplier = iniStats.ModificationRangeWeightMultiplier;
            MovementSpreadConstant = iniStats.MovementSpreadConstant;
            MovementSpreadMultiplier = iniStats.MovementSpreadMultiplier;
            RecoilAccumulation = iniStats.RecoilAccumulation;
            RecoilAccumulationMultiplier = iniStats.RecoilAccumulationMultiplier;
            RecoilSize = iniStats.RecoilSize;
            RecoilVector = iniStats.RecoilVector;
            RecoilVectorMultiplier = iniStats.RecoilVectorMultiplier;
            RecoilZoomMultiplier = iniStats.RecoilZoomMultiplier;
            ROF = iniStats.ROF;
            StatDecriptors = iniStats.StatDecriptors;
            TABaseSpread = iniStats.TABaseSpread;
            TightAimTime = iniStats.TightAimTime;
            UseTABaseSpread = iniStats.UseTABaseSpread;
            Weight = iniStats.Weight;
            ZoomSpreadMultiplier = iniStats.ZoomSpreadMultiplier;
        }
    }
    public BLRWeaponStats() { }
}

public class BLRWikiStats
{
    public BLRWikiStats(WikiStats wikiStats)
    {
        if (wikiStats == null)
        { return; }
        aimSpread = wikiStats.aimSpread;
        ammoMag = wikiStats.ammoMag;
        ammoReserve = wikiStats.ammoReserve;
        damage = wikiStats.damage;
        firerate = wikiStats.firerate;
        hipSpread = wikiStats.hipSpread;
        moveSpread = wikiStats.moveSpread;
        rangeClose = wikiStats.rangeClose;
        rangeFar = wikiStats.rangeFar;
        recoil = wikiStats.recoil;
        reload = wikiStats.reload;
        run = wikiStats.run;
        scopeInTime = wikiStats.scopeInTime;
        swaprate = wikiStats.swaprate;
        zoom = wikiStats.zoom;
    }

    public BLRWikiStats() { }

    public double aimSpread { get; set; }
    public double ammoMag { get; set; }
    public double ammoReserve { get; set; }
    public double damage { get; set; }
    public double firerate { get; set; }
    public double hipSpread { get; set; }
    public double moveSpread { get; set; }
    public double rangeClose { get; set; }
    public double rangeFar { get; set; }
    public double recoil { get; set; }
    public double reload { get; set; }
    public double run { get; set; }
    public double scopeInTime { get; set; }
    public double swaprate { get; set; }
    public double zoom { get; set; }
}
