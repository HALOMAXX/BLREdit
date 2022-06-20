using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Numerics;

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

    public BitmapSource WideImage { get { return GetWideImage(); } }
    public BitmapSource LargeSquareImage { get { return GetLargeSquareImage(); } }
    public BitmapSource SmallSquareImage { get { return GetSmallSquareImage(); } }

    public BitmapSource wideImageMale = null;
    public BitmapSource largeSquareImageMale = null;
    public BitmapSource smallSquareImageMale = null;

    public BitmapSource wideImageFemale = null;
    public BitmapSource largeSquareImageFemale = null;
    public BitmapSource smallSquareImageFemale = null;

    public BitmapSource Crosshair { get; private set; }
    public BitmapSource MiniCrosshair { get { return GetBitmapCrosshair(Name); } }

    //public BLRItem(ImportItem item)
    //{ 
    //    Category = item.Category;
    //    Class = item._class;
    //    DescriptorName = item.descriptorName;
    //    Icon = item.icon;
    //    Name = item.name;

    //    if(item.pawnModifiers != null)
    //    PawnModifiers = new BLRPawnModifiers(item.pawnModifiers);

    //    SupportedMods = new();
    //    if (item.supportedMods != null)
    //    {
    //        SupportedMods.AddRange(item.supportedMods);
    //    }
    //    Tooltip = item.tooltip;
    //    UID = item.uid;
    //    ValidFor = new();
    //    if (item.validFor != null)
    //    { 
    //        ValidFor.AddRange(item.validFor);
    //    }
    //    if(item.weaponModifiers != null)
    //    WeaponModifiers = new BLRWeaponModifiers(item.weaponModifiers);

    //    if(item.stats != null && item.IniStats != null)
    //    WeaponStats = new BLRWeaponStats(item.stats, item.IniStats);

    //    if(item.WikiStats != null)
    //    WikiStats = new BLRWikiStats(item.WikiStats);
    //}

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

    //public string GetDescriptorName()
    //{
    //    if (!string.IsNullOrEmpty(DescriptorName))
    //    { return DescriptorName + ":" + weaponModifiers.rating; }
    //    if (IniStats == null)
    //    { return ""; }

    //    string desc = "";
    //    int i = 0;
    //    foreach (StatDecriptor st in IniStats.StatDecriptors)
    //    {
    //        if (i <= 0)
    //        {
    //            desc += st.Name;
    //            i++;
    //        }
    //        else
    //        {
    //            desc += "/" + st.Name;
    //        }
    //    }
    //    return desc;
    //}
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
        if (Category != "magazines" && Category != "muzzles" && Category != "scopes" && Category != "stocks" && Category != "barrels") return true;
        
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

    public void LoadCrosshair()
{
        Crosshair = GetBitmapCrosshair(Name);
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

    public string DisplayStatDesc1
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                return "Damage:";
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return "Damage:";
            }
            else if (Category == "scopes")
            {
                return "Zoom:";
            }
            else if (Category == "magazines")
            {
                return "Ammo:";
            }
            else if (Category == "helmets" || Category == "upperBodies" || Category == "lowerBodies")
            {
                return "Health:";
            }
            return "";
        }
    }
    public string DisplayStat1
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] damage = UI.MainWindow.CalculateDamage(this, 0);
                return damage[0].ToString("0") + "/" + damage[1].ToString("0");
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return WeaponModifiers.damage + "%";
            }
            else if (Category == "scopes")
            {
                return (1.3 + (WikiStats?.zoom ?? 0)).ToString("0.00") + "x";
            }
            else if (Category == "magazines")
            {
                return WeaponModifiers.ammo.ToString("0");
            }
            else if (Category == "helmets" || Category == "upperBodies" || Category == "lowerBodies")
            {
                return PawnModifiers.Health.ToString("0");
            }
            return "";
        }
    }
    public bool DisplayStat1Gray
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] damage = UI.MainWindow.CalculateDamage(this, 0);
                if (damage[0] == 0)
                { return true; }
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                if (WeaponModifiers.damage == 0)
                { return true; }
            }
            else if (Category == "scopes")
            {
                if ((WikiStats?.zoom ?? 0) == 0)
                { return true; }
            }
            else if (Category == "magazines")
            {
                if (WeaponModifiers.ammo == 0)
                { return true; }
            }
            else if (Category == "helmets" || Category == "upperBodies" || Category == "lowerBodies")
            {
                if (PawnModifiers.Health == 0)
                { return true; }
            }
            return false;
        }
    }

    public string DisplayStatDesc2
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                return "Aim:";
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return "Accuracy:";
            }
            else if (Category == "scopes")
            {
                return "Scope In:";
            }
            else if (Category == "magazines")
            {
                if (IsValidForItemIDS(40021, 40002))
                {
                    return "Range:";
                }
                else
                {
                    return "Reload:";
                }
            }
            else if (Category == "helmets")
            {
                return "Head Armor:";
            }
            else if (Category == "upperBodies" || Category == "lowerBodies")
            {
                return "Run:";
            }
            return "";
        }
    }
    public string DisplayStat2
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                return spread[0].ToString("0.00") + '°';
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return WeaponModifiers.accuracy + "%";
            }
            else if (Category == "scopes")
            {
                //return (0.240 + (WikiStats?.scopeInTime ?? 0)).ToString("0.000") + "s";
                return "+" + (0.0 + (WikiStats?.scopeInTime ?? 0)).ToString("0.00") + "s";
            }
            else if (Category == "magazines")
            {
                if (IsValidForItemIDS(40021, 40002))
                {
                    return WeaponModifiers.range.ToString("0") + "%";
                }
                else
                {
                    return WikiStats.reload.ToString("0.00") + 's';
                }
            }
            else if (Category == "helmets")
            {
                return PawnModifiers.HelmetDamageReduction.ToString("0") + '%';
            }
            else if (Category == "upperBodies" || Category == "lowerBodies")
            {
                return PawnModifiers.MovementSpeed.ToString("0.00");
            }
            return "";
        }
    }
    public bool DisplayStat2Gray
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                if (spread[0] == 0)
                { return true; }
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                if (WeaponModifiers.accuracy == 0)
                { return true; }
            }
            else if (Category == "scopes")
            {
                if ((WikiStats?.scopeInTime ?? 0) == 0)
                { return true; } // the "invalid scope" for some reason doesn't follow this, so currently disabling it for consistency's sake
            }
            else if (Category == "magazines")
            {
                if (IsValidForItemIDS(40021, 40002))
                {
                    if (WeaponModifiers.range == 0)
                    { return true; }
                }
                else
                {
                    if (WikiStats.reload == 0)
                    { return true; }
                }
            }
            else if (Category == "helmets")
            {
                if (PawnModifiers.HelmetDamageReduction == 0)
                { return true; }
            }
            else if (Category == "upperBodies" || Category == "lowerBodies")
            {
                if (PawnModifiers.MovementSpeed == 0)
                { return true; }
            }
            return false;
        }
    }

    public string DisplayStatDesc3
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                return "Hip:";
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return "Recoil:";
            }
            else if (Category == "helmets")
            {
                return "Run:";
            }
            else if (Category == "scopes")
            {
                return "Is Infrared:";
            }
            else if (Category == "magazines")
            {
                return "Run:";
            }
            else if (Category == "upperBodies" || Category == "lowerBodies")
            {
                return "Gear:";
            }
            return "";
        }
    }
    public string DisplayStat3
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                return spread[1].ToString("0.00") + '°';
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return WeaponModifiers.recoil + "%";
            }
            else if (Category == "helmets")
            {
                return PawnModifiers.MovementSpeed.ToString("0.00");
            }
            else if (Category == "scopes")
            {
                if (UID == 45019 || UID == 45020 || UID == 45021)
                { return "True"; }
                else
                { return "False"; }
            }
            else if (Category == "magazines")
            {
                return WeaponModifiers.movementSpeed.ToString("0") + "%";
            }
            else if (Category == "upperBodies" || Category == "lowerBodies")
            {
                return PawnModifiers.GearSlots.ToString("0");
            }
            return "";
        }
    }
    public bool DisplayStat3Gray
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                if (spread[1] == 0)
                { return true; }
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                if (WeaponModifiers.recoil == 0)
                { return true; }
            }
            else if (Category == "helmets")
            {
                if (PawnModifiers.MovementSpeed == 0)
                { return true; }
            }
            else if (Category == "scopes")
            {
                if (UID != 45019 && UID != 45020 && UID != 45021)
                { return true; }
            }
            else if (Category == "magazines")
            {
                if (WeaponModifiers.movementSpeed == 0)
                { return true; }
            }
            else if (Category == "upperBodies" || Category == "lowerBodies")
            {
                if (PawnModifiers.GearSlots == 0)
                { return true; }
            }
            return false;
        }
    }

    public string DisplayStatDesc4
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                return "Move:";
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return "Range:";
            }
            else if (Category == "magazines")
            {
                return "Damage:";
            }
            else if (Category == "helmets")
            {
                return "HRV:";
            }
            return "";
        }
    }
    public string DisplayStat4
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                return spread[2].ToString("0.00") + '°';
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return WeaponModifiers.range + "%";
            }
            else if (Category == "magazines")
            {
                return WeaponModifiers.damage.ToString("0") + '%';
            }
            else if (Category == "helmets")
            {
                return PawnModifiers.HRVDuration.ToString("0.0");
            }
            return "";
        }
    }
    public bool DisplayStat4Gray
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                if (spread[2] == 0)
                { return true; }
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                if (WeaponModifiers.range == 0)
                { return true; }
            }
            else if (Category == "magazines")
            {
                if (WeaponModifiers.damage == 0)
                { return true; }
            }
            else if (Category == "helmets")
            {
                if (PawnModifiers.HRVDuration == 0)
                { return true; }
            }
            return false;
        }
    }

    public string DisplayStatDesc5
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                return "Recoil:";
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return "Run:";
            }
            else if (Category == "magazines")
            {
                return "Recoil:";
            }
            else if (Category == "helmets")
            {
                return "Recharge:";
            }
            return "";
        }
    }
    public string DisplayStat5
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double recoil = UI.MainWindow.CalculateRecoil(this, 0);
                return recoil.ToString("0.00") + '°';
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                return WeaponModifiers.movementSpeed + "%";
            }
            else if (Category == "magazines")
            {
                return WeaponModifiers.recoil + "%"; ;
            }
            else if (Category == "helmets")
            {
                return PawnModifiers.HRVRechargeRate.ToString("0.0") + "u/s";
            }
            return "";
        }
    }
    public bool DisplayStat5Gray
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double recoil = UI.MainWindow.CalculateRecoil(this, 0);
                if (recoil == 0)
                { return true; }
            }
            else if (Category == "barrels" || Category == "muzzles" || Category == "stocks")
            {
                if (WeaponModifiers.movementSpeed == 0)
                { return true; }
            }
            else if (Category == "magazines")
            {
                if (WeaponModifiers.recoil == 0)
                { return true; }
            }
            else if (Category == "helmets")
            {
                if (PawnModifiers.HRVRechargeRate == 0)
                { return true; }
            }
            return false;
        }
    }

    public string DisplayStatDesc6
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                return "Range:";
            }
            else if (Name == "Prex Chem/Hazmat Respirator-TOX")
            {
                return "Toxic:";
            }
            else if (Name == "Prex Chem/Hazmat Respirator-INC")
            {
                return "Fire:";
            }
            else if (Name == "Prex Chem/Hazmat Respirator-XPL")
            {
                return "Explo:";
            }
            else if (Category == "magazines")
            {
                if (IsValidForItemIDS(40021, 40002))
                {
                    return "Accuracy:";
                }
                else
                {
                    return "Range:";
                }
            }
            return "";
        }
    }
    public string DisplayStat6
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] range = UI.MainWindow.CalculateRange(this, 0);
                return range[0].ToString("0") + '/' + range[1].ToString("0");
            }
            else if (Name == "Prex Chem/Hazmat Respirator-TOX")
            {
                return PawnModifiers.ToxicProtection.ToString("0") + '%';
            }
            else if (Name == "Prex Chem/Hazmat Respirator-INC")
            {
                return PawnModifiers.IncendiaryProtection.ToString("0") + '%';
            }
            else if (Name == "Prex Chem/Hazmat Respirator-XPL")
            {
                return PawnModifiers.ExplosiveProtection.ToString("0") + '%';
            }
            else if (Category == "magazines")
            {
                if (IsValidForItemIDS(40021, 40002))
                {
                    return WeaponModifiers.accuracy.ToString("0") + '%';
                }
                else
                {
                    return WeaponModifiers.range.ToString("0") + '%';
                }
            }
            return "";
        }
    }
    public bool DisplayStat6Gray
    {
        get
        {
            if (Category == "primary" || Category == "secondary")
            {
                double[] range = UI.MainWindow.CalculateRange(this, 0);
                if (range[0] == 0)
                { return true; }
            }
            else if (Category == "magazines")
            {
                if (IsValidForItemIDS(40021, 40002))
                {
                    if (WeaponModifiers.accuracy == 0)
                    { return true; }
                }
                else
                {
                    if (WeaponModifiers.range == 0)
                    { return true; }
                }
            }
            return false;
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

    //public BLRPawnModifiers(PawnModifiers pawnModifiers)
    //{
    //    if (pawnModifiers == null)
    //    { return; }
    //    BodyDamageReduction = pawnModifiers.BodyDamageReduction;
    //    ElectroProtection = pawnModifiers.ElectroProtection;
    //    ExplosiveProtection = pawnModifiers.ExplosiveProtection;
    //    GearSlots = pawnModifiers.GearSlots;
    //    HRVDuration = pawnModifiers.HRVDuration;
    //    HRVRechargeRate = pawnModifiers.HRVRechargeRate;
    //    Health = pawnModifiers.Health;
    //    HealthRecharge = pawnModifiers.HealthRecharge;
    //    HelmetDamageReduction = pawnModifiers.HelmetDamageReduction;
    //    IncendiaryProtection = pawnModifiers.IncendiaryProtection;
    //    InfraredProtection = pawnModifiers.InfraredProtection;
    //    LegsDamageReduction = pawnModifiers.LegsDamageReduction;
    //    MeleeProtection = pawnModifiers.MeleeProtection;
    //    MeleeRange = pawnModifiers.MeleeRange;
    //    MovementSpeed = pawnModifiers.MovementSpeed;
    //    PermanentHealthProtection = pawnModifiers.PermanentHealthProtection;
    //    SprintMultiplier = pawnModifiers.SprintMultiplier;
    //    Stamina = pawnModifiers.Stamina;
    //    SwitchWeaponSpeed = pawnModifiers.SwitchWeaponSpeed;
    //    ToxicProtection = pawnModifiers.ToxicProtection;
    //}
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

    //public BLRWeaponModifiers(WeaponModifiers weaponModifiers)
    //{
    //    if (weaponModifiers == null)
    //    { return; }
    //    accuracy = weaponModifiers.accuracy;
    //    ammo = weaponModifiers.ammo;
    //    damage = weaponModifiers.damage;
    //    movementSpeed = weaponModifiers.movementSpeed;
    //    range = weaponModifiers.range;
    //    rateOfFire = weaponModifiers.rateOfFire;
    //    rating = weaponModifiers.rating;
    //    recoil = weaponModifiers.recoil;
    //    reloadSpeed = weaponModifiers.reloadSpeed;
    //    switchWeaponSpeed = weaponModifiers.switchWeaponSpeed;
    //    weaponWeight = weaponModifiers.weaponWeight;
    //}
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

    //public BLRWeaponStats(ImportStats stats, IniStats iniStats)
    //{
    //    if (stats != null)
    //    {
    //        accuracy = stats.accuracy;
    //        damage = stats.damage;
    //        movementSpeed = stats.movementSpeed;
    //        range = stats.range;
    //        rateOfFire = stats.rateOfFire;
    //        recoil = stats.recoil;
    //        reloadSpeed = stats.reloadSpeed;
    //        weaponWeight = stats.weaponWeight;
    //    }

    //    if (iniStats != null)
    //    {
    //        ApplyTime = iniStats.ApplyTime;
    //        Burst = iniStats.Burst;
    //        BaseSpread = iniStats.BaseSpread;
    //        CrouchSpreadMultiplier = iniStats.CrouchSpreadMultiplier;
    //        InitialMagazines = iniStats.InitialMagazines;
    //        IdealDistance = iniStats.IdealDistance;
    //        JumpSpreadMultiplier = iniStats.JumpSpreadMultiplier;
    //        MagSize = iniStats.MagSize;
    //        MaxDistance = iniStats.MaxDistance;
    //        MaxRangeDamageMultiplier = iniStats.MaxRangeDamageMultiplier;
    //        MaxTraceDistance = iniStats.MaxTraceDistance;
    //        ModificationRangeBaseSpread = iniStats.ModificationRangeBaseSpread;
    //        ModificationRangeCockRate = iniStats.ModificationRangeCockRate;
    //        ModificationRangeDamage = iniStats.ModificationRangeDamage;
    //        ModificationRangeIdealDistance = iniStats.ModificationRangeIdealDistance;
    //        ModificationRangeMaxDistance = iniStats.ModificationRangeMaxDistance;
    //        ModificationRangeMoveSpeed = iniStats.ModificationRangeMoveSpeed;
    //        ModificationRangeRecoil = iniStats.ModificationRangeRecoil;
    //        ModificationRangeRecoilReloadRate = iniStats.ModificationRangeRecoilReloadRate;
    //        ModificationRangeReloadRate = iniStats.ModificationRangeReloadRate;
    //        ModificationRangeTABaseSpread = iniStats.ModificationRangeTABaseSpread;
    //        ModificationRangeWeightMultiplier = iniStats.ModificationRangeWeightMultiplier;
    //        MovementSpreadConstant = iniStats.MovementSpreadConstant;
    //        MovementSpreadMultiplier = iniStats.MovementSpreadMultiplier;
    //        RecoilAccumulation = iniStats.RecoilAccumulation;
    //        RecoilAccumulationMultiplier = iniStats.RecoilAccumulationMultiplier;
    //        RecoilSize = iniStats.RecoilSize;
    //        RecoilVector = iniStats.RecoilVector;
    //        RecoilVectorMultiplier = iniStats.RecoilVectorMultiplier;
    //        RecoilZoomMultiplier = iniStats.RecoilZoomMultiplier;
    //        ROF = iniStats.ROF;
    //        StatDecriptors = iniStats.StatDecriptors;
    //        TABaseSpread = iniStats.TABaseSpread;
    //        TightAimTime = iniStats.TightAimTime;
    //        UseTABaseSpread = iniStats.UseTABaseSpread;
    //        Weight = iniStats.Weight;
    //        ZoomSpreadMultiplier = iniStats.ZoomSpreadMultiplier;
    //    }
    //}
}

public class BLRWikiStats
{
    //public BLRWikiStats(WikiStats wikiStats)
    //{
    //    if (wikiStats == null) 
    //    { return; }
    //    aimSpread = wikiStats.aimSpread;
    //    ammoMag = wikiStats.ammoMag;
    //    ammoReserve = wikiStats.ammoReserve;
    //    damage = wikiStats.damage;
    //    firerate = wikiStats.firerate;
    //    hipSpread = wikiStats.hipSpread;
    //    moveSpread = wikiStats.moveSpread;
    //    rangeClose = wikiStats.rangeClose;
    //    rangeFar = wikiStats.rangeFar;
    //    recoil = wikiStats.recoil;
    //    reload = wikiStats.reload;
    //    run = wikiStats.run;
    //    scopeInTime = wikiStats.scopeInTime;
    //    swaprate = wikiStats.swaprate;
    //    zoom = wikiStats.zoom;
    //}

    public float aimSpread { get; set; }
    public float ammoMag { get; set; }
    public float ammoReserve { get; set; }
    public float damage { get; set; }
    public float firerate { get; set; }
    public float hipSpread { get; set; }
    public float moveSpread { get; set; }
    public float rangeClose { get; set; }
    public float rangeFar { get; set; }
    public float recoil { get; set; }
    public float reload { get; set; }
    public float run { get; set; }
    public float scopeInTime { get; set; }
    public float swaprate { get; set; }
    public float zoom { get; set; }
}
