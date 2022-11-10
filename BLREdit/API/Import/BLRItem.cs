using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BLREdit.UI.Views;
using BLREdit.UI;
using BLREdit.Export;

namespace BLREdit.Import;

public sealed class BLRItem : INotifyPropertyChanged
{
    public int LMID { get; set; } = -1;
    public string Category { get; set; }
    public string DescriptorName { get; set; } = "";
    public string Icon { get; set; }
    public string Name { get; set; }
    public double CP { get; set; } = 0;

    [JsonIgnore] public string DisplayName { get { return LanguageSet.GetWord(UID.ToString() + ".Name", Name); } }

    public BLRPawnModifiers PawnModifiers { get; set; }
    public List<string> SupportedMods { get; set; }
    public string Tooltip { get; set; }
    [JsonIgnore] public string DisplayTooltip { get { return LanguageSet.GetWord(UID.ToString() + ".Tooltip", Tooltip); } }
    public int UID { get; set; }
    public List<int> ValidFor { get; set; }
    public BLRWeaponModifiers WeaponModifiers { get; set; }
    public BLRWeaponStats WeaponStats { get; set; }
    public BLRWikiStats WikiStats { get; set; }

    private bool female = false;
    [JsonIgnore] public BitmapSource WideImage { get { return GetWideImage(female); } }
    [JsonIgnore] public BitmapSource LargeSquareImage { get { return GetLargeSquareImage(female); } }
    [JsonIgnore] public BitmapSource SmallSquareImage { get { return GetSmallSquareImage(female); } }

    [JsonIgnore] public FoxIcon MaleIcon;
    [JsonIgnore] public FoxIcon FemaleIcon;

    [JsonIgnore] public BitmapSource Crosshair { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ExternalOnPropertyChanged(params string[] properties)
    {
        foreach (var prop in properties)
        {
            OnPropertyChanged(prop);
        }
    }

    public void UpdateImage(bool female)
    {
        this.female = female;
        OnPropertyChanged(nameof(WideImage));
        OnPropertyChanged(nameof(LargeSquareImage));
        OnPropertyChanged(nameof(SmallSquareImage));
    }

    public BitmapSource GetWideImage(bool female)
    {
        if (female)
        {
            if (FemaleIcon == null)
            { return MaleIcon.WideImage.Value; }
            return FemaleIcon.WideImage.Value;
        }
        else
        {
            if (MaleIcon is null)
            { return FoxIcon.WideEmpty; }
            return MaleIcon.WideImage.Value;
        }
    }
    public BitmapSource GetLargeSquareImage(bool female)
    {
        if (female)
        {
            if (FemaleIcon == null)
            { return MaleIcon.LargeImage.Value; }
            return FemaleIcon.LargeImage.Value;
        }
        else
        {
            if (MaleIcon is null)
            { return FoxIcon.LargeEmpty; }
            return MaleIcon.LargeImage.Value;
        }
    }
    public BitmapSource GetSmallSquareImage(bool female)
    {
        if (female)
        {
            if (FemaleIcon == null)
            { return MaleIcon.SmallImage.Value; }
            return FemaleIcon.SmallImage.Value;
        }
        else
        {
            if (MaleIcon is null)
            { return FoxIcon.SmallEmpty; }
            return MaleIcon.SmallImage.Value;
        }
    }

    public static BitmapSource GetImage(BitmapSource male, BitmapSource female)
    {
        if (UI.MainWindow.Profile.Loadout1.IsFemale)
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
        if (WeaponStats != null && WeaponStats.StatDecriptors != null)
        {
            foreach (StatDecriptor st in WeaponStats.StatDecriptors)
            {
                if (points >= st.Points)
                {
                    currentbest = st.DisplayName;
                }
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

        if (BLREditSettings.Settings.AdvancedModding.Is)
        {
            return AdvancedFilter(this, item);
        }

        return ValidForTest(item);
    }

    public bool ValidForTest(BLRItem filter)
    {
        if (Category != ImportSystem.AMMO_CATEGORY && Category != ImportSystem.MAGAZINES_CATEGORY && Category != ImportSystem.MUZZELS_CATEGORY && Category != ImportSystem.SCOPES_CATEGORY && Category != ImportSystem.STOCKS_CATEGORY && Category != ImportSystem.BARRELS_CATEGORY && Category != ImportSystem.GRIPS_CATEGORY) return true;

        if (ValidFor == null) { return true; }

        foreach (int id in ValidFor)
        {
            if (id == filter.UID)
            { return true; }
        }
        return false;
    }

    private static bool AdvancedFilter(BLRItem item, BLRItem filter)
    {
        switch (item.Category)
        {
            case ImportSystem.MAGAZINES_CATEGORY:
                if (item.Name.Contains("Standard") || (item.Name.Contains("Light") && !item.Name.Contains("Arrow")) || item.Name.Contains("Quick") || item.Name.Contains("Extended") || item.Name.Contains("Express") || item.Name.Contains("Quick") || item.Name.Contains("Electro") || item.Name.Contains("Explosive") || item.Name.Contains("Incendiary") || item.Name.Contains("Toxic") || item.Name.Contains("Magnum"))
                {
                    return item.ValidForTest(filter);
                }
                return true;

            default: return true;
        }
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
        if (!string.IsNullOrEmpty(Icon))
        {
            var femaleIconName = GetFemaleIconName();
            foreach (FoxIcon foxicon in ImportSystem.Icons)
            {
                if (foxicon.IconName == Icon)
                {
                    MaleIcon = foxicon;
                }
                if (foxicon.IconName == femaleIconName)
                {
                    FemaleIcon = foxicon;
                }
            }
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

    public void LoadCrosshair(BLRWeapon weapon)
    {
        Crosshair = GetBitmapCrosshair(GetSecondaryScope(weapon));
    }

    public string GetSecondaryScope(BLRWeapon weapon)
    {
        var name = weapon?.Reciever?.Name ?? "";
        switch (Name ?? "")
        {
            case "No Optic Mod":

                if (name.Contains("Prestige"))
                {
                    return Name + " Light Pistol";
                }
                else if (name.Contains("Rocket"))
                {
                    //TODO differenciate between stinger and swarm and add Swarm preview image
                    return "AV Rocket Launcher Scope";
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

    public int GetMagicCowsID()
    {
        return ImportSystem.GetIDOfItem(this);
    }

    public static BitmapSource GetBitmapCrosshair(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            foreach (FoxIcon icon in ImportSystem.ScopePreviews)
            {
                if (icon.IconName.Equals(name))
                {
                    return new BitmapImage(new System.Uri(icon.IconFileInfo.FullName, System.UriKind.Absolute));
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
                    return BLRWeapon.CalculateSpread(this, 0, 0).ZoomSpread;
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
                    return WikiStats?.ammoMag ?? 0;
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
                    return BLRWeapon.CalculateDamage(this, 0).DamageIdeal;
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
                    return BLRWeapon.CalculateSpread(this, 0, 0).HipSpread;
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
                    return BLRWeapon.CalculateSpread(this, 0, 0).MovmentSpread;
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
                    return BLRWeapon.CalculateRecoil(this, 0).RecoilHip;
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
                case ImportSystem.BARRELS_CATEGORY:
                case ImportSystem.STOCKS_CATEGORY:
                case ImportSystem.MUZZELS_CATEGORY:
                    return WeaponModifiers?.movementSpeed ?? 0;
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

public sealed class BLRPawnModifiers
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
}

public sealed class BLRWeaponModifiers
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
}

public sealed class BLRWeaponStats
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
    public double RecoveryTime { get; set; } = 0;
    public double BaseSpread { get; set; } = 0.04f;
    public double Burst { get; set; } = 0;
    public double FragmentsPerShell { get; set; } = 1;
    public double ZoomRateOfFire { get; set; } = 0;
    public double CrouchSpreadMultiplier { get; set; } = 0.5f;
    public double InitialMagazines { get; set; } = 4;
    public double IdealDistance { get; set; } = 8000;
    public double JumpSpreadMultiplier { get; set; } = 4.0f;
    public double SpreadCenterWeight { get; set; } = 0.2f;
    public double SpreadCenter { get; set; } = 0.4f;
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
    public double ReloadShortMultiplier { get; set; } = 1.0f; // not actually a thing, but this is currently the easiest way with how we do the reload numbers
    public double ROF { get; set; } = 0;
    public StatDecriptor[] StatDecriptors { get; set; } = new StatDecriptor[] { new StatDecriptor() };
    public double TABaseSpread { get; set; } = 0;
    public double TightAimTime { get; set; } = 0.0f;
    public bool UseTABaseSpread { get; set; } = false;
    public double Weight { get; set; } = 150.0f;
    public double ZoomSpreadMultiplier { get; set; } = 0.4f;
}

public sealed class BLRWikiStats
{
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
