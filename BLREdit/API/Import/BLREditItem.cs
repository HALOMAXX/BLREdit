using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BLREdit.UI.Views;
using BLREdit.UI;
using BLREdit.Properties;
using System;
using System.Collections.ObjectModel;

namespace BLREdit.Import;

[JsonConverter(typeof(JsonBLRItemConverter))]
public sealed class BLREditItem : INotifyPropertyChanged
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public const string UID_FORMAT = "000000";

    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
    #endregion Events

    #region Overrides
    public override string ToString()
    {
        return DisplayName;
    }
    #endregion Overrides
    public int LMID { get; set; } = -69;
    [JsonIgnore] public string? Category { get; set; }
    public string? DescriptorName { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string? Name { get; set; }
    public double CP { get; set; }
    public int AmmoType { get; set; } = -1;
    [JsonIgnore] public string DisplayName { get { return ItemNames.ResourceManager.GetString(UID.ToString(UID_FORMAT)); } }
    [JsonIgnore] public UIBool IsValid { get; set; } = new(true);

    public BLRPawnModifiers? PawnModifiers { get; set; }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
    public Collection<string>? SupportedMods { get; set; }
    [JsonIgnore] public string DisplayTooltip { 
        get { 
            var tt = ItemTooltips.ResourceManager.GetString(UID.ToString(UID_FORMAT)); 
            return string.IsNullOrEmpty(tt) ? DisplayName : tt; } }
    public int UID { get; set; }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
    public Collection<int>? ValidFor { get; set; }
    public BLRWeaponModifiers? WeaponModifiers { get; set; }
    public BLRWeaponStats? WeaponStats { get; set; }
    public BLRWikiStats? WikiStats { get; set; }

    private bool female;

    [JsonIgnore] public FoxIcon? Image { get { if (this.female) { return FemaleIcon; } else { return MaleIcon; } } }

    [JsonIgnore] public FoxIcon? MaleIcon { get; private set; }
    [JsonIgnore] public FoxIcon? FemaleIcon { get; private set; }

    [JsonIgnore] public FoxIcon? ScopePreview { get; private set; }

    /// <summary>
    /// Gets the Loadout-Manager ID for the item.
    /// </summary>
    /// <param name="item">The item to get the Loadout-Manager ID from</param>
    /// <returns>ID for Loadout-Manager</returns>
    public static int GetLMID(BLREditItem? item, int defaultLMID = -1)
    {
        return item?.GetLMID() ?? defaultLMID;
    }

    private int GetLMID()
    {
        if (this.LMID != -69) return this.LMID;
        return ImportSystem.GetIDOfItem(this);
    }

    public void UpdateImage(bool female)
    {
        this.female = female;
        OnPropertyChanged(nameof(Image));
    }

    public bool ItemNameContains(params string[] names)
    {
        if (Name is null || names is null || names.Length == 0) return false;
        foreach (var name in names)
        {
            if (Name.Contains(name)) return true;
        }
        return false;
    }

    public string SelectDescriptorName(double points)
    {
        string currentbest = "";
        if (WeaponStats != null && WeaponStats.StatDecriptors != null)
        {
            foreach (StatDecriptor st in WeaponStats.StatDecriptors)
            {
                if (points >= st.Points)
                {
                    currentbest = st.Name;
                }
            }
        }
        return currentbest;
    }


    public bool IsValidForItemIDS(params int[] uids)
    {
        if (ValidFor is null) return true;
        if (uids is null || uids.Length == 0) return false;
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

    public bool IsValidFor(BLREditItem? filter, bool advanced = false)
    {
        return IsValidFor(this, filter, advanced);
    }

    public static bool IsValidFor(BLREditItem? item, BLREditItem? filter, bool advanced = false)
    {
        if (item == null) return true;

        if ((item.Category == ImportSystem.PRIMARY_CATEGORY || item.Category == ImportSystem.SECONDARY_CATEGORY) && (item.UID < 20000 || item.UID > 20100)) { return true; }
        else if ((item.Category == ImportSystem.PRIMARY_CATEGORY || item.Category == ImportSystem.SECONDARY_CATEGORY) && (item.UID > 20000 || item.UID < 20100)) { return false; }

        if (advanced)
        {
            return AdvancedFilter(item, filter);
        }

        return item.ValidForTest(filter);
    }

    public bool ValidForTest(BLREditItem? filter)
    {
        switch (UID)
        {
            case 324:
            case 349:
            case 362:
            case 363:
            case 369:
            case 370:
            case 373:
            case 374:
            case 12096:
            case 12097:
            case 12098:
            case 12099:
            case 20016:
            case 46050:
                return false;
        }

        if (filter is null || ValidFor == null || ValidFor.Count <= 0) { return true; }

        foreach (int id in ValidFor)
        {
            if (id == filter.UID)
            { return true; }
        }
        return false;
    }

    private static bool AdvancedFilter(BLREditItem? item, BLREditItem? filter)
    {
        if (item is null || item.UID == 45012) return false;
        switch (item.Category)
        {
            case ImportSystem.MAGAZINES_CATEGORY:
                if(item.Name is null) return true;
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
        if (SupportedMods is null) return false;
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
        if (string.IsNullOrEmpty(Icon)) { return; }
        MaleIcon = new FoxIcon($"Assets\\textures\\{Icon}.png");
        if(Icon.Length > 8) FemaleIcon = new FoxIcon($"Assets\\textures\\{Icon.Insert(Icon.Length - 8, "_Female")}.png");
        if (!FemaleIcon?.IconFileInfo?.Exists ?? true) { FemaleIcon = MaleIcon; }
    }

    public void LoadCrosshair(BLREditWeapon weapon)
    {
        ScopePreview = GetBitmapCrosshair(GetSecondaryScope(weapon));
    }

    static readonly UIBool scopePreviewDefault = new();
    public UIBool ScopePreviewBool { get { return MainWindow.MainView?.IsScopePreviewVisible ?? scopePreviewDefault; } }

    public string GetSecondaryScope(BLREditWeapon weapon)
    {
        var receiverName = weapon?.Receiver?.Name ?? "";
        switch (Name)
        {
            case "No Optic Mod":

                if (receiverName.Contains("Prestige"))
                {
                    return Name + " Light Pistol";
                }
                else if (receiverName.Contains("Rocket"))
                {
                    return "AV Rocket Launcher Scope"; //Not in use anymore as Rocketlaunchers are not selectable anymore
                }
                else
                {
                    return Name + " " + receiverName;
                }

            //Pistols Only
            case "OPRL Holo Sight":
            case "Lightsky Reflex Sight":
            case "Krane Tactical Scope":
            case "EON Electric Scope":
            case "EMI Electric Scope":
            case "ArmCom CQC Scope":
            case "Aim Point Ammo Counter":
                return Name + GetSecondayScopePistol(receiverName);

            //Pistols and shotguns
            case "Titan Rail Sight":
            case "MMRS Flip-Up Rail Sight":
            case "Lightsky Red Dot Sight":
            case "Krane Holo Sight":
                return Name + GetSecondayScopePistol(receiverName) + GetSecondayScopeShotgun(receiverName);

            default:
                return Name ?? string.Empty;
        }
    }

    private static string GetSecondayScopePistol(string secondaryName)
    {
        return secondaryName switch
        {
            "Breech Loaded Pistol" or "Snub 260" or "Heavy Pistol" or "Light Pistol" or "Burstfire Pistol" or "Prestige Light Pistol" or "Machine Pistol" or "Revolver" => " Pistol",
            _ => "",
        };
    }

    private static string GetSecondayScopeShotgun(string secondaryName)
    {
        return secondaryName switch
        {
            "Shotgun" or "Shotgun AR-k" => " Shotgun",
            _ => "",
        };
    }

    public void RemoveCrosshair()
    {
        ScopePreview = null;
    }

    public static int GetMagicCowsID(BLREditItem? item, int defaultID = 0)
    {
        if(item is null) return defaultID;
        return ImportSystem.GetIDOfItem(item);
    }

    public static int GetUID(BLREditItem? item, int defaultUID = 0)
    {
        if (item is null) return defaultUID;
        return item.UID;
    }

    public static FoxIcon GetBitmapCrosshair(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            foreach (FoxIcon icon in ImportSystem.ScopePreviews)
            {
                if (icon.IconName.Equals(name, StringComparison.Ordinal))
                {
                    return icon;
                }
            }
        }
        return new FoxIcon(string.Empty);
    }

    [JsonIgnore]
    public DisplayStatDiscriptor? DisplayStat1 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor? DisplayStat2 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor? DisplayStat3 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor? DisplayStat4 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor? DisplayStat5 { get; set; }
    [JsonIgnore]
    public DisplayStatDiscriptor? DisplayStat6 { get; set; }

    [JsonIgnore]
    public double None
    {
        get
        {
            return UID;
        }
    }

    [JsonIgnore]
    public double Accuracy
    {
        get
        {
            return Category switch
            {
                ImportSystem.SECONDARY_CATEGORY or ImportSystem.PRIMARY_CATEGORY => WeaponStats?.Accuracy ?? 0,
                _ => WeaponModifiers?.Accuracy ?? 0,
            };
        }
    }
    [JsonIgnore]
    public double Aim
    {
        get
        {
            return Category switch
            {
                ImportSystem.SECONDARY_CATEGORY or ImportSystem.PRIMARY_CATEGORY => BLREditWeapon.CalculateSpread(this, 0, 0, this, this).ZoomSpread,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double Ammo
    {
        get
        {
            return Category switch
            {
                ImportSystem.SECONDARY_CATEGORY or ImportSystem.PRIMARY_CATEGORY => WikiStats?.AmmoMag ?? 0,
                ImportSystem.MAGAZINES_CATEGORY => WeaponModifiers?.Ammo ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double Damage
    {
        get
        {
            return Category switch
            {
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLREditWeapon.CalculateDamage(this, 0).DamageIdeal,
                _ => WeaponModifiers?.Damage ?? 0,
            };
        }
    }

    [JsonIgnore]
    public double ElectroProtection
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.ATTACHMENTS_CATEGORY => PawnModifiers?.ElectroProtection ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double ExplosiveProtection
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.ATTACHMENTS_CATEGORY => PawnModifiers?.ExplosiveProtection ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double GearSlots
    {
        get
        {
            return Category switch
            {
                ImportSystem.UPPER_BODIES_CATEGORY or ImportSystem.LOWER_BODIES_CATEGORY => PawnModifiers?.GearSlots ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double Health
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.UPPER_BODIES_CATEGORY or ImportSystem.LOWER_BODIES_CATEGORY => PawnModifiers?.Health ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double HeadProtection
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY => PawnModifiers?.HelmetDamageReduction ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double Hip
    {
        get
        {
            return Category switch
            {
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLREditWeapon.CalculateSpread(this, 0, 0, this, this).HipSpread,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double HRVDuration
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY => PawnModifiers?.HRVDuration ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double HRVRecharge
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY => PawnModifiers?.HRVRechargeRate ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double IncendiaryProtection
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.ATTACHMENTS_CATEGORY => PawnModifiers?.IncendiaryProtection ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double InfraredProtection
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.ATTACHMENTS_CATEGORY => PawnModifiers?.InfraredProtection ?? 0,
                _ => 0,
            };
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
                        return WikiStats?.Zoom ?? 0;
                    }
                    else
                    {
                        return (WikiStats?.Zoom ?? 0) + 10;
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
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.ATTACHMENTS_CATEGORY => PawnModifiers?.MeleeProtection ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double Move
    {
        get
        {
            return Category switch
            {
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLREditWeapon.CalculateSpread(this, 0, 0, this, this).MovmentSpread,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double Range
    {
        get
        {
            return Category switch
            {
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => WeaponStats?.Range ?? 0,
                _ => WeaponModifiers?.Range ?? 0,
            };
        }
    }
    [JsonIgnore]
    public double Reload
    {
        get
        {
            return Category switch
            {
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLREditWeapon.CalculateReloadRate(this, 0, 0),
                _ => WeaponModifiers?.ReloadSpeed ?? 0,
            };
        }
    }

    [JsonIgnore]
    public double Recoil
    {
        get
        {
            return Category switch
            {
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLREditWeapon.CalculateRecoil(this, 0).RecoilHip,
                _ => WeaponModifiers?.Recoil ?? 0,
            };
        }
    }
    [JsonIgnore]
    public double Run
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.UPPER_BODIES_CATEGORY or ImportSystem.LOWER_BODIES_CATEGORY => PawnModifiers?.MovementSpeed ?? 0,
                ImportSystem.BARRELS_CATEGORY or ImportSystem.STOCKS_CATEGORY or ImportSystem.MUZZELS_CATEGORY => WeaponModifiers?.MovementSpeed ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double ScopeInTime
    {
        get
        {
            return Category switch
            {
                ImportSystem.SCOPES_CATEGORY => WikiStats?.ScopeInTime ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double ToxicProtection
    {
        get
        {
            return Category switch
            {
                ImportSystem.HELMETS_CATEGORY or ImportSystem.ATTACHMENTS_CATEGORY => PawnModifiers?.ToxicProtection ?? 0,
                _ => 0,
            };
        }
    }
    [JsonIgnore]
    public double Zoom
    {
        get
        {
            return Category switch
            {
                ImportSystem.SCOPES_CATEGORY => WikiStats?.Zoom ?? 0,
                _ => 0,
            };
        }
    }

}

[JsonConverter(typeof(JsonBLRPawnModifiersConverter))]
public sealed class BLRPawnModifiers
{
    public double BodyDamageReduction { get; set; }
    public double ElectroProtection { get; set; }
    public double ExplosiveProtection { get; set; }
    public double GearSlots { get; set; }
    public double HRVDuration { get; set; }
    public double HRVRechargeRate { get; set; }
    public double Health { get; set; }
    public double HealthRecharge { get; set; }
    public double HelmetDamageReduction { get; set; }
    public double IncendiaryProtection { get; set; }
    public double InfraredProtection { get; set; }
    public double LegsDamageReduction { get; set; }
    public double MeleeProtection { get; set; }
    public double MeleeRange { get; set; }
    public double MovementSpeed { get; set; }
    public double PermanentHealthProtection { get; set; }
    public double SprintMultiplier { get; set; } = 1;
    public double Stamina { get; set; }
    public double SwitchWeaponSpeed { get; set; }
    public double ToxicProtection { get; set; }
}

[JsonConverter(typeof(JsonBLRWeaponModifiersConverter))]
public sealed class BLRWeaponModifiers
{
    public double Accuracy { get; set; }
    public double Ammo { get; set; }
    public double Damage { get; set; }
    public double MovementSpeed { get; set; }
    public double Range { get; set; }
    public double RateOfFire { get; set; }
    public double Rating { get; set; }
    public double Recoil { get; set; }
    public double ReloadSpeed { get; set; }
    public double SwitchWeaponSpeed { get; set; }
    public double WeaponWeight { get; set; }
}

[JsonConverter(typeof(JsonBLRWeaponStatsConverter))]
public sealed class BLRWeaponStats
{
    public double Accuracy { get; set; }
    public double Damage { get; set; }
    public double MovementSpeed { get; set; }
    public double Range { get; set; }
    public double RateOfFire { get; set; }
    public double Recoil { get; set; }
    public double ReloadSpeed { get; set; }
    public double WeaponWeight { get; set; }

    public double ApplyTime { get; set; }
    public double RecoveryTime { get; set; }
    public double BaseSpread { get; set; } = 0.04f;
    public double Burst { get; set; }
    public double FragmentsPerShell { get; set; } = 1;
    public double ZoomRateOfFire { get; set; }
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
    public double MovementSpreadConstant { get; set; }
    public double MovementSpreadMultiplier { get; set; } = 2.5f;
    public double RecoilAccumulation { get; set; }
    public double RecoilAccumulationMultiplier { get; set; } = 0.95f;
    public double RecoilSize { get; set; }
    public Vector3 RecoilVector { get; set; } = Vector3.Zero;
    public Vector3 RecoilVectorMultiplier { get; set; } = Vector3.Zero;
    public double RecoilZoomMultiplier { get; set; } = 0.5f;
    public double ReloadShortMultiplier { get; set; } = 1.0f; // not actually a thing, but this is currently the easiest way with how we do the reload numbers
    public double ROF { get; set; }
    public StatDecriptor[] StatDecriptors { get; set; } = [new()];
    public double TABaseSpread { get; set; }
    public double TightAimTime { get; set; }
    public bool UseTABaseSpread { get; set; }
    public double Weight { get; set; } = 150.0f;
    public double ZoomSpreadMultiplier { get; set; } = 0.4f;
}

public sealed class StatDecriptor
{
    public string Name { get; set; } = "Classic";
    public int Points { get; set; }
}

[JsonConverter(typeof(JsonBLRWikiStatsConverter))]
public sealed class BLRWikiStats
{
    public double AimSpread { get; set; }
    public double AmmoMag { get; set; }
    public double AmmoReserve { get; set; }
    public double Damage { get; set; }
    public double Firerate { get; set; }
    public double HipSpread { get; set; }
    public double MoveSpread { get; set; }
    public double RangeClose { get; set; }
    public double RangeFar { get; set; }
    public double Recoil { get; set; }
    public double Reload { get; set; }
    public double Run { get; set; }
    public double ScopeInTime { get; set; }
    public double Swaprate { get; set; }
    public double Zoom { get; set; }
}
