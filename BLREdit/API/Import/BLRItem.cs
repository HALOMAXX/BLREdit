using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BLREdit.UI.Views;
using BLREdit.UI;
using BLREdit.Export;
using System.Text.Json;
using System;
using Gameloop.Vdf.Linq;
using System.Buffers.Text;
using BLREdit.Properties;
using System.Globalization;

namespace BLREdit.Import;

[JsonConverter(typeof(JsonBLRItemConverter))]
public sealed class BLRItem : INotifyPropertyChanged
{
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
    public int NameID { get; set; } = -1;
    [JsonIgnore] public string? Category { get; set; }
    public string? DescriptorName { get; set; }
    public string Icon { get; set; } = "";
    public string? Name { get; set; }
    public double CP { get; set; } = 0;

    [JsonIgnore] public string DisplayName { get { return ItemNames.ResourceManager.GetString(NameID.ToString("000000")); } }
    [JsonIgnore] public UIBool IsValid { get; set; } = new(true);

    public BLRPawnModifiers? PawnModifiers { get; set; }
    public List<string>? SupportedMods { get; set; }
    [JsonIgnore] public string DisplayTooltip { get { return ItemTooltips.ResourceManager.GetString(NameID.ToString("000000")); } }
    public int UID { get; set; }
    public List<int>? ValidFor { get; set; }
    public BLRWeaponModifiers? WeaponModifiers { get; set; }
    public BLRWeaponStats? WeaponStats { get; set; }
    public BLRWikiStats? WikiStats { get; set; }

    private bool female = false;

    [JsonIgnore] public FoxIcon? Image { get { if (this.female) { return FemaleIcon; } else { return MaleIcon; } } }

    [JsonIgnore] public FoxIcon? MaleIcon { get; private set; }
    [JsonIgnore] public FoxIcon? FemaleIcon { get; private set; }

    [JsonIgnore] public FoxIcon? ScopePreview { get; private set; }

    /// <summary>
    /// Gets the Loadout-Manager ID for the item.
    /// </summary>
    /// <param name="item">The item to get the Loadout-Manager ID from</param>
    /// <returns>ID for Loadout-Manager</returns>
    public static int GetLMID(BLRItem? item, int defaultLMID = -1)
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
        if (Name is null) return false;
        foreach (var name in names)
        {
            if (Name.Contains(name)) return true;
        }
        return false;
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
                    currentbest = st.Name;
                }
            }
        }
        return currentbest;
    }


    public bool IsValidForItemIDS(params int[] uids)
    {
        if (ValidFor is null) return true;
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

    public bool IsValidFor(BLRItem? item)
    {
        if (item is null || (this.Category == ImportSystem.PRIMARY_CATEGORY || this.Category == ImportSystem.SECONDARY_CATEGORY)) return true;

        if (DataStorage.Settings.AdvancedModding.Is)
        {
            return AdvancedFilter(this, item);
        }

        return ValidForTest(item);
    }

    public bool ValidForTest(BLRItem? filter)
    {
        if (filter is null || ValidFor == null || ValidFor.Count <= 0) { return true; }

        foreach (int id in ValidFor)
        {
            if (id == filter.UID)
            { return true; }
        }
        return false;
    }

    private static bool AdvancedFilter(BLRItem item, BLRItem filter)
    {
        if(item is null) return false;
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
    }

    public void LoadCrosshair(BLRWeapon weapon)
    {
        ScopePreview = GetBitmapCrosshair(GetSecondaryScope(weapon));
    }

    public string GetSecondaryScope(BLRWeapon weapon)
    {
        var recieverName = weapon?.Reciever?.Name ?? "";
        switch (Name)
        {
            case "No Optic Mod":

                if (recieverName.Contains("Prestige"))
                {
                    return Name + " Light Pistol";
                }
                else if (recieverName.Contains("Rocket"))
                {
                    return "AV Rocket Launcher Scope"; //Not in use anymore as Rocketlaunchers are not selectable anymore
                }
                else
                {
                    return Name + " " + recieverName;
                }

            //Pistols Only
            case "OPRL Holo Sight":
            case "Lightsky Reflex Sight":
            case "Krane Tactical Scope":
            case "EON Electric Scope":
            case "EMI Electric Scope":
            case "ArmCom CQC Scope":
            case "Aim Point Ammo Counter":
                return Name + GetSecondayScopePistol(recieverName);

            //Pistols and shotguns
            case "Titan Rail Sight":
            case "MMRS Flip-Up Rail Sight":
            case "Lightsky Red Dot Sight":
            case "Krane Holo Sight":
                return Name + GetSecondayScopePistol(recieverName) + GetSecondayScopeShotgun(recieverName);

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

    public static int GetMagicCowsID(BLRItem? item, int defaultID = 0)
    {
        if(item is null) return defaultID;
        return ImportSystem.GetIDOfItem(item);
    }

    public static FoxIcon GetBitmapCrosshair(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            foreach (FoxIcon icon in ImportSystem.ScopePreviews)
            {
                if (icon.IconName.Equals(name))
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
                ImportSystem.SECONDARY_CATEGORY or ImportSystem.PRIMARY_CATEGORY => BLRWeapon.CalculateSpread(this, 0, 0, this, this).ZoomSpread,
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
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLRWeapon.CalculateDamage(this, 0).DamageIdeal,
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
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLRWeapon.CalculateSpread(this, 0, 0, this, this).HipSpread,
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
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLRWeapon.CalculateSpread(this, 0, 0, this, this).MovmentSpread,
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
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLRWeapon.CalculateReloadRate(this, 0, 0),
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
                ImportSystem.PRIMARY_CATEGORY or ImportSystem.SECONDARY_CATEGORY => BLRWeapon.CalculateRecoil(this, 0).RecoilHip,
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

[JsonConverter(typeof(JsonBLRWeaponModifiersConverter))]
public sealed class BLRWeaponModifiers
{
    public double Accuracy { get; set; } = 0;
    public double Ammo { get; set; } = 0;
    public double Damage { get; set; } = 0;
    public double MovementSpeed { get; set; } = 0;
    public double Range { get; set; } = 0;
    public double RateOfFire { get; set; } = 0;
    public double Rating { get; set; } = 0;
    public double Recoil { get; set; } = 0;
    public double ReloadSpeed { get; set; } = 0;
    public double SwitchWeaponSpeed { get; set; } = 0;
    public double WeaponWeight { get; set; } = 0;
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

public sealed class StatDecriptor
{
    public string Name { get; set; } = "Classic";
    public int Points { get; set; } = 0;
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
