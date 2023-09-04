using BLREdit.API.Export;
using BLREdit.Export;
using BLREdit.Import;

using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace BLREdit.UI.Views;

public sealed class BLRWeapon : INotifyPropertyChanged
{
    private IBLRWeapon? _weapon;

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    private void ItemChanged([CallerMemberName] string? propertyName = null)
    {
        var value = GetType().GetProperty(propertyName).GetValue(this);
        LoggingSystem.Log($"{propertyName} has been set to {(value as BLRItem)?.Name ?? value}");
        if (!UndoRedoSystem.BlockUpdate) Write();
        CalculateStats();
        OnPropertyChanged(propertyName);
        IsChanged = true;
    }
    #endregion Event

    public bool IsPrimary { get; set; } = false;

    private BLRLoadout? Loadout { get; set; }

    private bool isChanged = false;
    [JsonIgnore] public bool IsChanged { get { return isChanged; } set { isChanged = value; OnPropertyChanged(); } }

    #region Weapon Parts
    private BLRItem? reciever = null;
    [WeaponPart] public BLRItem? Reciever {
        get { return reciever; }
        set {
            if (value?.Category == ImportSystem.PRIMARY_SKIN_CATEGORY)
            { Skin = value; }
            if (BLREditSettings.Settings.AdvancedModding.Is)
            { reciever = value; AddMissingDefaultParts(); ItemChanged(); UpdateScopeIcons(); return; }
            if (value is null || reciever != value && AllowReciever(value))
            {
                if (value is null)
                {
                    if (IsPrimary) { reciever = MagiCowsWeapon.DefaultWeapons.AssaultRifle.GetReciever(); }
                    else { reciever = MagiCowsWeapon.DefaultWeapons.LightPistol.GetReciever(); }
                }
                else
                { reciever = value; } RemoveIncompatibleMods(); ItemChanged(); UpdateScopeIcons(); } } }

    private BLRItem? barrel = null;
    [WeaponPart] public BLRItem? Barrel
    {
        get { return barrel; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { barrel = value; ItemChanged(); return; }
            if (value is null || reciever is null || barrel != value && value.IsValidFor(reciever) && value.Category == ImportSystem.BARRELS_CATEGORY) { if (value is null) { barrel = ImportSystem.GetItemByNameAndType(ImportSystem.BARRELS_CATEGORY, MagiCowsWeapon.NoBarrel); } else { barrel = value; } AllowStock(); ItemChanged(); } }
    }

    private BLRItem? magazine = null;
    [WeaponPart] public BLRItem? Magazine
    {
        get { return magazine; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { magazine = value; ItemChanged(); return; }
            if (value is null || reciever is null || magazine != value && value.IsValidFor(reciever) && value.Category == ImportSystem.MAGAZINES_CATEGORY) { if (value is null && Reciever is not null) { magazine = MagiCowsWeapon.GetDefaultSetupOfReciever(Reciever)?.GetMagazine(); } else { magazine = value; } ApplyCorrectAmmo(); ItemChanged(); } }
    }

    private BLRItem? muzzle = null;
    [WeaponPart] public BLRItem? Muzzle
    {
        get { return muzzle; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { muzzle = value; ItemChanged(); return; }
            if (value is null || reciever is null || muzzle != value && value.IsValidFor(reciever) && value.Category == ImportSystem.MUZZELS_CATEGORY) { if (value is null) { muzzle = ImportSystem.GetItemByIDAndType(ImportSystem.MUZZELS_CATEGORY, MagiCowsWeapon.NoMuzzle); } else { muzzle = value; } ItemChanged(); } }
    }

    private BLRItem? stock = null;
    [WeaponPart] public BLRItem? Stock
    {
        get { return stock; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { stock = value; ItemChanged(); return; }
            if (value is null || reciever is null || stock != value && AllowStock() && value.IsValidFor(reciever) && value.Category == ImportSystem.STOCKS_CATEGORY) { if (value is null) { stock = ImportSystem.GetItemByNameAndType(ImportSystem.STOCKS_CATEGORY, MagiCowsWeapon.NoStock); } else { stock = value; } ItemChanged(); } }
    }

    private BLRItem? scope = null;
    [WeaponPart] public BLRItem? Scope
    {
        get { return scope; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { if (value is null) { scope = ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, MagiCowsWeapon.NoScope); UpdateScopeIcons(); } else { scope = value; } ItemChanged(); UpdateScopeIcons(); } if (value is null || reciever is null || scope != value && value.IsValidFor(reciever) && value.Category == ImportSystem.SCOPES_CATEGORY) { if (value is null) { scope = ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, MagiCowsWeapon.NoScope); UpdateScopeIcons(); } else { scope = value; } ItemChanged(); UpdateScopeIcons(); } }
    }

    private BLRItem? grip = null;
    [WeaponPart] public BLRItem? Grip
    {
        get { return grip; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { grip = value; ItemChanged(); return; }
            if (value is null || reciever is null || grip != value && value.IsValidFor(reciever) && value.Category == ImportSystem.GRIPS_CATEGORY) { grip = value; ItemChanged(); } }
    }

    private BLRItem? tag = null;
    [WeaponPart] public BLRItem? Tag
    {
        get { return tag; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { tag = value; ItemChanged(); return; }
            if (value is null || reciever is null || tag != value && value.IsValidFor(reciever) && value.Category == ImportSystem.HANGERS_CATEGORY) { if (value is null) { tag = ImportSystem.GetItemByIDAndType(ImportSystem.HANGERS_CATEGORY, MagiCowsWeapon.NoTag); } else { tag = value; } ItemChanged(); } }
    }

    private BLRItem? camo = null;
    [WeaponPart] public BLRItem? Camo
    {
        get { return camo; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { camo = value; ItemChanged(); return; }
            if (value is null || reciever is null || camo != value && value.IsValidFor(reciever) && value.Category == ImportSystem.CAMOS_WEAPONS_CATEGORY) { if (value is null) { camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, MagiCowsWeapon.NoCamo); } else { camo = value; } ItemChanged(); } }
    }

    private BLRItem? ammo = null;
    [WeaponPart] public BLRItem? Ammo
    {
        get { return ammo; }
        set {
            if (BLREditSettings.Settings.AdvancedModding.Is) { ammo = value; ItemChanged(); return; }
            if (value is null || reciever is null || ammo != value && value.IsValidFor(reciever) && value.Category == ImportSystem.AMMO_CATEGORY) { if (value is null) { ApplyCorrectAmmo(); } else { ammo = value; } ItemChanged(); } }
    }

    private BLRItem? skin = null;
    [WeaponPart] public BLRItem? Skin
    {
        get { return skin; }
        set { if (BLREditSettings.Settings.AdvancedModding.Is) { skin = value; ItemChanged(); return; } if (value is null || reciever is null || skin != value && value.IsValidFor(reciever) && value.Category == ImportSystem.PRIMARY_SKIN_CATEGORY) { skin = value; ItemChanged(); } }
    }
    #endregion Weapon Parts

    public static PropertyInfo[] WeaponParts { get; } = ((from property in typeof(BLRWeapon).GetProperties() where Attribute.IsDefined(property, typeof(WeaponPartAttribute)) orderby ((WeaponPartAttribute)property.GetCustomAttributes(typeof(WeaponPartAttribute), false).Single()).WeaponPartOrder select property).ToArray());

    public BLRWeapon Copy()
    {
        BLRWeapon wpn = new(IsPrimary, null);
        string WeaponCategoryName;
        if (IsPrimary) { WeaponCategoryName = "Primary"; } else { WeaponCategoryName = "Secondary"; }
        foreach (var property in WeaponParts)
        {
            if (property.GetValue(this) is BLRItem item)
            {
                property.SetValue(wpn, item);
            }
        }
        MainWindow.ShowAlert($"Copied {WeaponCategoryName} Weapon!");
        return wpn;
    }

    public void ApplyCopy(BLRWeapon? weapon)
    {
        if (weapon is not null && this.IsPrimary == weapon.IsPrimary)
        {
            string WeaponCategoryName;
            if (weapon.IsPrimary) { WeaponCategoryName = "Primary"; } else { WeaponCategoryName = "Secondary"; }
            foreach (var property in WeaponParts)
            {
                if (property.GetValue(weapon) is BLRItem item)
                {
                    UndoRedoSystem.DoAction(item, property, this);
                }
            }
            UndoRedoSystem.EndAction();
            MainWindow.ShowAlert($"Pasted {WeaponCategoryName} Weapon!");
        }
    }

    //TODO: Add Premade Weapon Setup's

    [JsonIgnore] public FoxIcon ScopePreview { get { return GetBitmapCrosshair(Scope?.GetSecondaryScope(this) ?? ""); } }

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

    private void AddMissingDefaultParts()
    {
        if (Reciever is null) { LoggingSystem.Log($"can't check for default setup of Weapons as Reciever is missing!"); return; }
        var wpn = MagiCowsWeapon.GetDefaultSetupOfReciever(Reciever);
        if (wpn is null) { LoggingSystem.Log($"missing default setup for {Reciever?.Name}"); return; }

        if (Barrel is null || Barrel.Name == MagiCowsWeapon.NoBarrel)
        {
            Barrel = wpn.GetBarrel();
        }
        if (Scope is null || Scope.Name == MagiCowsWeapon.NoScope)
        {
            Scope = wpn.GetScope();
        }
        if (Stock is null || Stock.Name == MagiCowsWeapon.NoStock)
        {
            Stock = wpn.GetStock();
        }
        if (Grip is null || Grip.Name == MagiCowsWeapon.NoGrip)
        {
            Grip = wpn.GetGrip();
        }

        if (Muzzle is null || BLRItem.GetMagicCowsID(Muzzle) == MagiCowsWeapon.NoMuzzle)
        {
            Muzzle = wpn.GetMuzzle();
        }
        if (Magazine is null || BLRItem.GetMagicCowsID(Magazine) == MagiCowsWeapon.NoMagazine)
        {
            Magazine = wpn.GetMagazine();
            ApplyCorrectAmmo();
        }

        Skin ??= ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, 0);
    }

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
            if (item.Category != ImportSystem.SECONDARY_CATEGORY)
            {
                allow = false;
            }
        }
        return allow;
    }

    private void ApplyCorrectAmmo()
    {
        if (Reciever is null) return;
        switch (Reciever.UID)
        {
            case 40024:
                Ammo = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, Magazine?.Name ?? string.Empty);
                break;
            case 40015:
                switch (Magazine?.Name ?? string.Empty)
                {
                    case "Flechette Chamber Boring":
                        Ammo = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Canister");
                        break;
                    case "High Explosive Round Bore":
                        Ammo = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Explosive Flare");
                        break;
                    case "Incendiary Round Bore":
                        Ammo = ImportSystem.GetItemByNameAndType(ImportSystem.AMMO_CATEGORY, "Incendiary Flare");
                        break;
                }
                break;
            default:
                if ((Magazine?.Name?.Contains("Magnum") ?? false))
                {
                    Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, 0);
                }
                else if ((Magazine?.Name?.Contains("Electro") ?? false))
                {
                    Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, 3);
                }
                else if ((Magazine?.Name?.Contains("Explosive") ?? false))
                {
                    Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, 4);
                }
                else if ((Magazine?.Name?.Contains("Incendiary") ?? false))
                {
                    Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, 6);
                }
                else if ((Magazine?.Name?.Contains("Toxic") ?? false))
                {
                    Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, 7);
                }
                else
                {
                    Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, 2);
                }
                break;
        }
    }

    private bool AllowStock()
    {
        bool allow = true;
        if (!IsPrimary)
        {
            if (IsPistol() && (Barrel?.Name ?? MagiCowsWeapon.NoBarrel) == MagiCowsWeapon.NoBarrel)
            {
                allow = false;
                stock = ImportSystem.GetItemByNameAndType(ImportSystem.STOCKS_CATEGORY, MagiCowsWeapon.NoStock);
                OnPropertyChanged(nameof(Stock));
            }
        }
        return allow;
    }


    private void UpdateScopeIcons()
    {
        OnPropertyChanged(nameof(ScopePreview));
    }

    public BLRWeapon(bool isPrimary, BLRLoadout? loadout, ShareableWeapon? weapon = null)
    {
        IsPrimary = isPrimary;
        Loadout = loadout;
        if (weapon is null) return;
        if (isPrimary)
        {
            reciever = ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_CATEGORY, weapon.Reciever);
        }
        else
        {
            reciever = ImportSystem.GetItemByIDAndType(ImportSystem.SECONDARY_CATEGORY, weapon.Reciever);
        }

        barrel = ImportSystem.GetItemByIDAndType(ImportSystem.BARRELS_CATEGORY, weapon.Barrel);
        muzzle = ImportSystem.GetItemByIDAndType(ImportSystem.MUZZELS_CATEGORY, weapon.Muzzle);
        magazine = ImportSystem.GetItemByIDAndType(ImportSystem.MAGAZINES_CATEGORY, weapon.Magazine);
        ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, weapon.Ammo);
        camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, weapon.Camo);
        skin = ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, weapon.Skin);
        stock = ImportSystem.GetItemByIDAndType(ImportSystem.STOCKS_CATEGORY, weapon.Stock);
        scope = ImportSystem.GetItemByIDAndType(ImportSystem.SCOPES_CATEGORY, weapon.Scope);
        tag = ImportSystem.GetItemByIDAndType(ImportSystem.HANGERS_CATEGORY, weapon.Tag);
        grip = ImportSystem.GetItemByIDAndType(ImportSystem.GRIPS_CATEGORY, weapon.Grip);
    }

    #region Properties
    public double AccuracyPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.Accuracy ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.Accuracy ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.Accuracy ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.Accuracy ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.Accuracy ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.Accuracy ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.Accuracy ?? 0;
            return total;
        }
    }
    public double AdditionalAmmo
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.Ammo ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.Ammo ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.Ammo ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.Ammo ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.Ammo ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.Ammo ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.Ammo ?? 0;
            return total;
        }
    }
    public double DamagePercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.Damage ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.Damage ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.Damage ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.Damage ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.Damage ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.Damage ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.Damage ?? 0;

            // arrows don't directly affect damage and were set by the projectile, my modifiers were causing misleading damage changes on other guns, so here's a hacky fix
            if (Reciever?.UID == 40024)
            {
                switch (Magazine?.UID)
                {
                    case 44211:
                        total += -50;
                        break;
                    case 44212:
                        total += -100;
                        break;
                    case 44213:
                        total += 28.57D;
                        break;
                    case 44214:
                        total += -87.5D;
                        break;
                    case 44215:
                        total += 100;
                        break;
                }
            }

            return total;
        }
    }
    public double MovementSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.MovementSpeed ?? 0;
            return total;
        }
    }
    public double RangePercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.Range ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.Range ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.Range ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.Range ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.Range ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.Range ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.Range ?? 0;
            return total;
        }
    }
    public double RateOfFirePercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.RateOfFire ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.RateOfFire ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.RateOfFire ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.RateOfFire ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.RateOfFire ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.RateOfFire ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.RateOfFire ?? 0;
            return total;
        }
    }
    public double TotalRatingPoints
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.Rating ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.Rating ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.Rating ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.Rating ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.Rating ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.Rating ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.Rating ?? 0;
            return total;
        }
    }
    public double RecoilPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.Recoil ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.Recoil ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.Recoil ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.Recoil ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.Recoil ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.Recoil ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.Recoil ?? 0;
            return total;
        }
    }
    public double ReloadSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.ReloadSpeed ?? 0;
            return total;
        }
    }
    public double SwitchWeaponSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            return total;
        }
    }
    public double WeaponWeightPercentage
    {
        get
        {
            double total = 0;
            total += Reciever?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WeaponModifiers?.WeaponWeight ?? 0;
            return total;
        }
    }
    public double RawZoomMagnification
    {
        get
        {
            return Scope?.WikiStats?.Zoom ?? 0;
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
            { return Reciever?.WeaponStats?.MagSize ?? 0; }
        }
    }
    public double ModifiedAmmoMagazine
    { get { return RawAmmoMagazine + AdditionalAmmo; } }
    public double FinalAmmoMagazine // for eventual cases of advanced modding that i cant explain
    {
        get
        {
            if (Reciever?.UID == 40019 || Reciever?.UID == 40015)
            {
                return 1; // Forcing AMR and BLP mag to 1 while trying not to change how its reserve ammo is modified, because oddly enough typical gun mags don't increase its base ammo but still treat reserve as if base was modified, which makes no sense
            }
            return ModifiedAmmoMagazine;
        }
    }
    public double RawAmmoReserve
    { get { return RawAmmoMagazine * (Reciever?.WeaponStats?.InitialMagazines ?? 0); } }
    public double ModifiedAmmoReserve
    { get { return ModifiedAmmoMagazine * (Reciever?.WeaponStats?.InitialMagazines ?? 0); } }
    public double RawRateOfFire
    { get { return Reciever?.WeaponStats?.RateOfFire ?? 0; } }
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
            total += Reciever?.WikiStats?.Reload ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WikiStats?.Reload ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WikiStats?.Reload ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WikiStats?.Reload ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WikiStats?.Reload ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WikiStats?.Reload ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WikiStats?.Reload ?? 0;

            // Corrected the reload rate multiplier calc and changed the ranges from zeros in the BPFA and BFR so they can now correctly use the reloadspeed percentage instead of flat wiki value add/sub
            // Will eventually fix all guns but I am still uncertain about a few cases that don't go along with it for whatever reason
            if (Reciever?.UID == 40020 || Reciever?.UID == 40009)
            {
                return Reciever?.WikiStats?.Reload ?? 0;
            }

            return total;
        }
    }

    public double VerticalRecoilRatio
    {
        get
        {
            if (Reciever != null && Reciever.WeaponStats != null)
            {
                double vertical = Reciever.WeaponStats.RecoilVector.Y * Reciever.WeaponStats.RecoilVectorMultiplier.Y * 0.3535;
                double horizontal = Reciever.WeaponStats.RecoilVector.X * Reciever.WeaponStats.RecoilVectorMultiplier.X * 0.5;
                if (vertical + horizontal != 0)
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

    public double RecoilAccumulation
    {
        get
        {
            if (Reciever != null && Reciever.WeaponStats != null)
            {
                double accumExponent = Reciever?.WeaponStats?.RecoilAccumulation ?? 0;
                if (accumExponent > 1)
                {
                    accumExponent = (accumExponent - 1.0) * (Reciever?.WeaponStats?.RecoilAccumulationMultiplier ?? 0) + 1.0;
                }
                return accumExponent;
            }
            return 1;
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
            total += Reciever?.WikiStats?.ScopeInTime ?? 0;
            if (Barrel?.IsValidFor(Reciever) ?? false) total += Barrel?.WikiStats?.ScopeInTime ?? 0;
            if (Magazine?.IsValidFor(Reciever) ?? false) total += Magazine?.WikiStats?.ScopeInTime ?? 0;
            if (Muzzle?.IsValidFor(Reciever) ?? false) total += Muzzle?.WikiStats?.ScopeInTime ?? 0;
            if (Stock?.IsValidFor(Reciever) ?? false) total += Stock?.WikiStats?.ScopeInTime ?? 0;
            if (Scope?.IsValidFor(Reciever) ?? false) total += Scope?.WikiStats?.ScopeInTime ?? 0;
            if (Grip?.IsValidFor(Reciever) ?? false) total += Grip?.WikiStats?.ScopeInTime ?? 0;
            return total;
        }
    }
    #endregion Properties

    #region CalculatedProperties
    public double ModifiedReloadSpeed { get { return RawReloadSpeed * ReloadMultiplier; } }
    private double? reloadMultiplier;
    public double ReloadMultiplier { get { return reloadMultiplier ?? 1; } private set { reloadMultiplier = value; OnPropertyChanged(); } }
    private double? cockRateMultiplier;
    public double CockRateMultiplier { get { return cockRateMultiplier ?? 1; } private set { cockRateMultiplier = value; OnPropertyChanged(); } }
    public double? RawSwapRate
    { get { return Reciever?.WikiStats?.Swaprate ?? 0; } }
    public double ShortReload
    {
        get
        {
            if (Magazine?.UID == 44014 || Magazine?.UID == 44015)
            {
                return 1;
            }
            return Reciever?.WeaponStats?.ReloadShortMultiplier ?? 0;
        }
    }

    #region Damage
    private double? damageClose;
    public double DamageClose { get { return damageClose ?? 0; } private set { damageClose = value; OnPropertyChanged(); } }
    private double? damageFar;
    public double DamageFar { get { return damageFar ?? 0; } private set { damageFar = value; OnPropertyChanged(); } }
    #endregion Damage

    #region Range
    private double? rangeClose;
    public double RangeClose { get { return rangeClose ?? 0; } private set { rangeClose = value; OnPropertyChanged(); } }
    private double? rangeFar;
    public double RangeFar { get { return rangeFar ?? 0; } private set { rangeFar = value; OnPropertyChanged(); } }
    private double? rangeTracer;
    public double RangeTracer { get { return rangeTracer ?? 0; } private set { rangeTracer = value; OnPropertyChanged(); } }
    #endregion Range

    #region Recoil
    private double? recoilHip;
    public double RecoilHip { get { return recoilHip ?? 0; } private set { recoilHip = value; OnPropertyChanged(); } }
    private double? recoilZoom;
    public double RecoilZoom { get { return recoilZoom ?? 0; } private set { recoilZoom = value; OnPropertyChanged(); } }
    #endregion Recoil

    #region Spread
    private double? spreadWhileMoving;
    public double SpreadWhileMoving { get { return spreadWhileMoving ?? 0; } private set { spreadWhileMoving = value; OnPropertyChanged(); } }
    private double? spreadWhileStanding;
    public double SpreadWhileStanding { get { return spreadWhileStanding ?? 0; } private set { spreadWhileStanding = value; OnPropertyChanged(); } }
    private double? spreadWhileADS;
    public double SpreadWhileADS { get { return spreadWhileADS ?? 0; } private set { spreadWhileADS = value; } }
    #endregion Spread

    private double? modifiedScopeInTime;
    public double ModifiedScopeInTime { get { return modifiedScopeInTime ?? 0; } private set { modifiedScopeInTime = value; OnPropertyChanged(); } }
    private double? modifiedRunSpeed;
    public double ModifiedRunSpeed { get { return modifiedRunSpeed ?? 0; } private set { modifiedRunSpeed = value; OnPropertyChanged(); } }
    private double? modifiedPawnRunSpeed;
    public double ModifiedPawnRunSpeed { get { return modifiedPawnRunSpeed ?? 0; } private set { modifiedPawnRunSpeed = value; OnPropertyChanged(); } }

    #region Weapon Descriptor
    private string? weaponDesc1;
    public string WeaponDescriptorPart1 { get { return weaponDesc1 ?? string.Empty; } private set { weaponDesc1 = value; OnPropertyChanged(); } }
    private string? weaponDesc2;
    public string WeaponDescriptorPart2 { get { return weaponDesc2 ?? string.Empty; } private set { weaponDesc2 = value; OnPropertyChanged(); } }
    private string? weaponDesc3;

    public string WeaponDescriptorPart3 { get { return weaponDesc3 ?? string.Empty; } private set { weaponDesc3 = value; OnPropertyChanged(); } }

    public string? weaponDescriptor;
    public string WeaponDescriptor { get { return weaponDescriptor ?? string.Empty; } private set { weaponDescriptor = value; OnPropertyChanged(); } }
    #endregion Weapon Descriptor

    #endregion CalculatedProperties

    #region DisplayStats
    private string? damageDisplay;
    public string DamageDisplay { get { return damageDisplay ?? string.Empty; } private set { damageDisplay = value; OnPropertyChanged(); } }
    private string? rateOfFireDsiplay;
    public string RateOfFireDisplay { get { return rateOfFireDsiplay ?? string.Empty; } private set { rateOfFireDsiplay = value; OnPropertyChanged(); } }
    private string? ammoDisplay;
    public string AmmoDisplay { get { return ammoDisplay ?? string.Empty; } private set { ammoDisplay = value; OnPropertyChanged(); } }
    private string? reloadTimeDisplay;
    public string ReloadTimeDisplay { get { return reloadTimeDisplay ?? string.Empty; } private set { reloadTimeDisplay = value; OnPropertyChanged(); } }
    private string? swapDsiplay;
    public string SwapDisplay { get { return swapDsiplay ?? string.Empty; } private set { swapDsiplay = value; OnPropertyChanged(); } }
    private string? aimSpreadDisplay;
    public string AimSpreadDisplay { get { return aimSpreadDisplay ?? string.Empty; } private set { aimSpreadDisplay = value; OnPropertyChanged(); } }
    private string? hipSpreadDisplay;
    public string HipSpreadDisplay { get { return hipSpreadDisplay ?? string.Empty; } private set { hipSpreadDisplay = value; OnPropertyChanged(); } }
    private string? moveSpreadDisplay;
    public string MoveSpreadDisplay { get { return moveSpreadDisplay ?? string.Empty; } private set { moveSpreadDisplay = value; OnPropertyChanged(); } }
    private string? hipRecoilDisplay;
    public string HipRecoilDisplay { get { return hipRecoilDisplay ?? string.Empty; } private set { hipRecoilDisplay = value; OnPropertyChanged(); } }
    private string? aimRecoilDisplay;
    public string AimRecoilDisplay { get { return aimRecoilDisplay ?? string.Empty; } private set { aimRecoilDisplay = value; OnPropertyChanged(); } }
    private string? scopeInTimeDisplay;
    public string ScopeInTimeDisplay { get { return scopeInTimeDisplay ?? string.Empty; } private set { scopeInTimeDisplay = value; OnPropertyChanged(); } }
    private string? rangeDisaply;
    public string RangeDisplay { get { return rangeDisaply ?? string.Empty; } private set { rangeDisaply = value; OnPropertyChanged(); } }
    private string? runDisplay;
    public string RunDisplay { get { return runDisplay ?? string.Empty; } private set { runDisplay = value; OnPropertyChanged(); } }
    private string? pawnRunDisplay;
    public string PawnRunDisplay { get { return pawnRunDisplay ?? string.Empty; } private set { pawnRunDisplay = value; OnPropertyChanged(); } }

    private string? zoomDisplay;
    public string ZoomDisplay { get { return zoomDisplay ?? string.Empty; } private set { zoomDisplay = value; OnPropertyChanged(); } }


    private string? fragmentsPerShellDisplay;
    public string FragmentsPerShellDisplay { get { return fragmentsPerShellDisplay ?? string.Empty; } private set { fragmentsPerShellDisplay = value; OnPropertyChanged(); } }

    private string? zoomFirerateDisplay;
    public string ZoomFirerateDisplay { get { return zoomFirerateDisplay ?? string.Empty; } private set { zoomFirerateDisplay = value; OnPropertyChanged(); } }

    private string? spreadCrouchMultiplierDisplay;
    public string SpreadCrouchMultiplierDisplay { get { return spreadCrouchMultiplierDisplay ?? string.Empty; } private set { spreadCrouchMultiplierDisplay = value; OnPropertyChanged(); } }

    private string? spreadJumpMultiplierDisplay;
    public string SpreadJumpMultiplierDisplay { get { return spreadJumpMultiplierDisplay ?? string.Empty; } private set { spreadJumpMultiplierDisplay = value; OnPropertyChanged(); } }

    private string? spreadCenterWeightDisplay;
    public string SpreadCenterWeightDisplay { get { return spreadCenterWeightDisplay ?? string.Empty; } private set { spreadCenterWeightDisplay = value; OnPropertyChanged(); } }

    private string? spreadCenterDisplay;
    public string SpreadCenterDisplay { get { return spreadCenterDisplay ?? string.Empty; } private set { spreadCenterDisplay = value; OnPropertyChanged(); } }

    private string? recoilVerticalRatioDisplay;
    public string RecoilVerticalRatioDisplay { get { return recoilVerticalRatioDisplay ?? string.Empty; } private set { recoilVerticalRatioDisplay = value; OnPropertyChanged(); } }

    private string? recoilRecoveryTimeDisplay;
    public string RecoilRecoveryTimeDisplay { get { return recoilRecoveryTimeDisplay ?? string.Empty; } private set { recoilRecoveryTimeDisplay = value; OnPropertyChanged(); } }

    private string? recoilAccumulationDisplay;
    public string RecoilAccumulationDisplay { get { return recoilAccumulationDisplay ?? string.Empty; } private set { recoilAccumulationDisplay = value; OnPropertyChanged(); } }


    private string? damagePercentDisplay;
    public string DamagePercentageDisplay { get { return damagePercentDisplay ?? string.Empty; } private set { damagePercentDisplay = value; OnPropertyChanged(); } }
    private string? accuracyPercentageDisplay;
    public string AccuracyPercentageDisplay { get { return accuracyPercentageDisplay ?? string.Empty; } private set { accuracyPercentageDisplay = value; OnPropertyChanged(); } }
    private string? rangePercentageDisplay;
    public string RangePercentageDisplay { get { return rangePercentageDisplay ?? string.Empty; } private set { rangePercentageDisplay = value; OnPropertyChanged(); } }
    private string? reloadPercentageDisplay;
    public string ReloadPercentageDisplay { get { return reloadPercentageDisplay ?? string.Empty; } private set { reloadPercentageDisplay = value; OnPropertyChanged(); } }
    private string? recoilPercentageDisplay;
    public string RecoilPercentageDisplay { get { return recoilPercentageDisplay ?? string.Empty; } private set { recoilPercentageDisplay = value; OnPropertyChanged(); } }
    private string? runPercentageDisplay;
    public string RunPercentageDisplay { get { return runPercentageDisplay ?? string.Empty; } private set { runPercentageDisplay = value; OnPropertyChanged(); } }



    #endregion DisplayStats
    private void RemoveIncompatibleMods()
    {
        if (UndoRedoSystem.BlockEvent) return;
        if (Reciever is null) return;
        MagiCowsWeapon? wpn = MagiCowsWeapon.GetDefaultSetupOfReciever(Reciever);
        if (Reciever.IsValidModType(ImportSystem.MUZZELS_CATEGORY))
        {
            if (Muzzle is null || !Muzzle.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetMuzzle(), GetType().GetProperty(nameof(Muzzle)), this); }
        }
        else
        {
            UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Muzzle)), this);
        }

        if (Reciever.IsValidModType(ImportSystem.BARRELS_CATEGORY))
        {
            if (Barrel is null || !Barrel.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetBarrel(), GetType().GetProperty(nameof(Barrel)), this); }
        }
        else
        {
            UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Barrel)), this);
        }

        if (Reciever.IsValidModType(ImportSystem.STOCKS_CATEGORY))
        {
            if (Stock is null || !Stock.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetStock(), GetType().GetProperty(nameof(Stock)), this); }
        }
        else
        {
            UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Stock)), this);
        }

        if (Reciever.IsValidModType(ImportSystem.SCOPES_CATEGORY))
        {
            if (Scope is null || !Scope.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetScope(), GetType().GetProperty(nameof(Scope)), this); }
        }
        else
        {
            UndoRedoSystem.DoActionAfter(ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, MagiCowsWeapon.NoScope), GetType().GetProperty(nameof(Scope)), this);
        }

        if (Reciever.IsValidModType(ImportSystem.MAGAZINES_CATEGORY))
        {
            if (Magazine is null || !Magazine.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetMagazine(), GetType().GetProperty(nameof(Magazine)), this); }
        }
        else
        { UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Magazine)), this); }

        if (Reciever.IsValidModType(ImportSystem.AMMO_CATEGORY))
        {
            if (Ammo is null || !Ammo.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetAmmo(), GetType().GetProperty(nameof(Ammo)), this); }
        }
        else
        { UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Ammo)), this); }

        if (Reciever.IsValidModType(ImportSystem.GRIPS_CATEGORY))
        {
            if (Grip is null || !Grip.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetGrip(), GetType().GetProperty(nameof(Grip)), this); }
        }
        else
        { UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Grip)), this); }

        if (Reciever.IsValidModType(ImportSystem.CAMOS_WEAPONS_CATEGORY))
        {
            if (Camo is null || !Camo.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetCamo(), GetType().GetProperty(nameof(Camo)), this); }
        }
        else
        { UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Camo)), this); }

        if (Tag is null || Reciever.IsValidModType(ImportSystem.HANGERS_CATEGORY))
        {
            if (Tag is null || !Tag.IsValidFor(Reciever))
            { UndoRedoSystem.DoActionAfter(wpn?.GetTag(), GetType().GetProperty(nameof(Tag)), this); }
        }
        else
        { UndoRedoSystem.DoActionAfter(null, GetType().GetProperty(nameof(Tag)), this); }

        if (!Skin?.IsValidFor(Reciever) ?? true)
        {
            UndoRedoSystem.DoActionAfter(ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, 0), GetType().GetProperty(nameof(Skin)), this);
        }
    }

    public void CalculateStats()
    {
        if (Reciever is null) return;
        CockRateMultiplier = CalculateCockRate(Reciever, RecoilPercentage);
        (DamageClose, DamageFar) = CalculateDamage(Reciever, DamagePercentage);
        ModifiedRunSpeed = CalculateMovementSpeed(Reciever, MovementSpeedPercentage);
        ModifiedPawnRunSpeed = CalculatePawnMovementSpeed(Reciever, Loadout, MovementSpeedPercentage);
        (RangeClose, RangeFar, RangeTracer) = CalculateRange(Reciever, RangePercentage);
        (RecoilHip, RecoilZoom) = CalculateRecoil(Reciever, RecoilPercentage);
        ReloadMultiplier = CalculateReloadRate(Reciever, ReloadSpeedPercentage, RecoilPercentage);
        double BarrelStockMovementSpeed = Barrel?.WeaponModifiers?.MovementSpeed ?? 0;
        BarrelStockMovementSpeed += Stock?.WeaponModifiers?.MovementSpeed ?? 0;
        ModifiedScopeInTime = CalculateScopeInTime(Reciever, Scope, BarrelStockMovementSpeed, RawScopeInTime);
        (SpreadWhileADS, SpreadWhileStanding, SpreadWhileMoving) = CalculateSpread(Reciever, AccuracyPercentage, BarrelStockMovementSpeed, Magazine, Ammo);
        WeaponDescriptorPart1 = CompareItemDescriptor1(Barrel, Magazine);
        WeaponDescriptorPart2 = CompareItemDescriptor2(Stock, Muzzle, Scope);
        WeaponDescriptorPart3 = Reciever.GetDescriptorName(TotalRatingPoints);
        WeaponDescriptor = WeaponDescriptorPart1 + ' ' + WeaponDescriptorPart2 + ' ' + WeaponDescriptorPart3;

        CreateDisplayProperties();
    }

    private void CreateDisplayProperties()
    {
        DamageDisplay = DamageClose.ToString("0.0") + " / " + DamageFar.ToString("0.0");
        RateOfFireDisplay = ModifiedRateOfFire.ToString("0");
        AmmoDisplay = FinalAmmoMagazine.ToString("0") + " / " + ModifiedAmmoReserve.ToString("0");
        ReloadTimeDisplay = (ModifiedReloadSpeed * ShortReload).ToString("0.00") + 's'; // changed reload to short reload
        //SwapDisplay = RawSwapRate.ToString("0.00");
        SwapDisplay = ModifiedReloadSpeed.ToString("0.00") + 's'; // moved normal reload time to here, since the normal stat is the empty reload
        AimSpreadDisplay = SpreadWhileADS.ToString("0.00") + '°';
        HipSpreadDisplay = SpreadWhileStanding.ToString("0.00") + '°';
        MoveSpreadDisplay = SpreadWhileMoving.ToString("0.00") + '°';
        HipRecoilDisplay = RecoilHip.ToString("0.00") + '°';
        AimRecoilDisplay = RecoilZoom.ToString("0.00") + '°';
        ScopeInTimeDisplay = ModifiedScopeInTime.ToString("0.000") + 's';
        RangeDisplay = RangeClose.ToString("0.0") + " / " + RangeFar.ToString("0.0") + " / " + RangeTracer.ToString("0");
        RunDisplay = ModifiedRunSpeed.ToString("0.00");
        PawnRunDisplay = ModifiedPawnRunSpeed.ToString("0.00");
        ZoomDisplay = ZoomMagnification.ToString("0.00") + 'x';

        FragmentsPerShellDisplay = FragmentsPerShell.ToString("0");
        ZoomFirerateDisplay = ZoomRateOfFire.ToString("0");
        SpreadCrouchMultiplierDisplay = SpreadCrouchMultiplier.ToString("0.00");
        SpreadJumpMultiplierDisplay = SpreadJumpMultiplier.ToString("0.00");
        SpreadCenterWeightDisplay = SpreadCenterWeight.ToString("0.00");
        SpreadCenterDisplay = SpreadCenter.ToString("0.00");
        RecoilVerticalRatioDisplay = VerticalRecoilRatio.ToString("0.00");
        RecoilRecoveryTimeDisplay = RecoilRecoveryTime.ToString("0.00");
        RecoilAccumulationDisplay = RecoilAccumulation.ToString("0.00");

        DamagePercentageDisplay = DamagePercentage.ToString("0") + '%';
        AccuracyPercentageDisplay = AccuracyPercentage.ToString("0") + '%';
        RangePercentageDisplay = RangePercentage.ToString("0") + '%';
        ReloadPercentageDisplay = ReloadSpeedPercentage.ToString("0") + '%';
        RecoilPercentageDisplay = RecoilPercentage.ToString("0") + '%';
        RunPercentageDisplay = MovementSpeedPercentage.ToString("0") + '%';
    }

    /// <summary>
    /// Calculates the Cockrate Multiplier
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="RecoilPercentage">all raw Recoil modifiers</param>
    /// <returns>CockRateMultiplier</returns>
    public static double CalculateCockRate(BLRItem Reciever, double RecoilPercentage)
    {
        if (Reciever is null || Reciever.WeaponStats is null) return 0;
        double allRecoil = Percentage(RecoilPercentage);
        double alpha = Math.Abs(allRecoil);
        double cockrate;
        if (Reciever.WeaponStats.ModificationRangeCockRate.Z != 0)
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
            return cockrate;
        }
        else
        {
            return 1.0;
        }
    }

    /// <summary>
    /// Calculates the Reload Multiplier
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="ReloadSpeedPercentage">all raw ReloadSpeed modifiers</param>
    /// <param name="RecoilPercentage"> all raw Recoil modifiers</param>
    /// <returns>calculated ReloadMultiplier</returns>
    public static double CalculateReloadRate(BLRItem Reciever, double ReloadSpeedPercentage, double RecoilPercentage)
    {
        if (Reciever is null || Reciever.WeaponStats is null) return 0;
        double allReloadSpeed = ReloadSpeedPercentage / 100; // Reload speed is actually seemingly unclamped
        double allRecoil = Percentage(RecoilPercentage);
        double WeaponReloadRate = 1.0;
        double rate_alpha;
        if (Reciever.WeaponStats.ModificationRangeReloadRate.Z > 0)
        {
            rate_alpha = Math.Abs(allReloadSpeed);
            if (allReloadSpeed > 0)
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeReloadRate.Z, Reciever.WeaponStats.ModificationRangeReloadRate.Y, rate_alpha);
            }
            else
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeReloadRate.Z, Reciever.WeaponStats.ModificationRangeReloadRate.X, rate_alpha);
            }
        }

        if (Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Z == 1)
        {
            rate_alpha = Math.Abs(allRecoil);
            if (allRecoil > 0)
            {
                WeaponReloadRate += (Lerp(Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Z, Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Y, rate_alpha) - 1.0);
            }
            else
            {
                WeaponReloadRate += (Lerp(Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Z, Reciever.WeaponStats.ModificationRangeRecoilReloadRate.X, rate_alpha) - 1.0);
            }
        }

        WeaponReloadRate = 1 / WeaponReloadRate;

        return WeaponReloadRate;
    }

    /// <summary>
    /// Not yet Implemented
    /// </summary>
    /// <param name="Reciever"></param>
    public static void CalculateReloadSpeed(BLRItem Reciever)
    {
        // Placeholder so I don't forget
    }

    /// <summary>
    /// Calculates the Movementspeed
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="MovementSpeedPercentage">all raw MovementSpeed modifiers</param>
    /// <returns>calculated Movementspeed</returns>
    public static double CalculateMovementSpeed(BLRItem Reciever, double MovementSpeedPercentage)
    {
        if (Reciever is null || Reciever.WeaponStats is null) return 0;
        double allMovementSpeed = MovementSpeedPercentage / 100; // Movement speed is apparently also uncapped
        double move_alpha = Math.Abs(allMovementSpeed);
        double move_modifier;
        if (allMovementSpeed > 0)
        {
            move_modifier = Lerp(Reciever.WeaponStats.ModificationRangeMoveSpeed.Z, Reciever.WeaponStats.ModificationRangeMoveSpeed.Y, move_alpha);
        }
        else
        {
            move_modifier = Lerp(Reciever.WeaponStats.ModificationRangeMoveSpeed.Z, Reciever.WeaponStats.ModificationRangeMoveSpeed.X, move_alpha);
        }
        return move_modifier;
        //return (765 + move_modifier * 0.9) / 100.0f; // Apparently percent of movement from gear is applied to weapons, and not percent of movement from weapons
    }

    /// <summary>
    /// Calculates the combined armor and weapon Movementspeed
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="Loadout">Reciever</param>
    /// <param name="WeapSpeedPercentage">all raw weapon MovementSpeed modifiers</param>
    /// <returns>calculated Movementspeed</returns>
    public static double CalculatePawnMovementSpeed(BLRItem Reciever, BLRLoadout? Loadout, double WeapSpeedPercentage)
    {
        if (Loadout is null || Reciever is null || Reciever.WeaponStats is null) return 0;
        double weap_alpha = Math.Abs(WeapSpeedPercentage) / 100;
        double weap_modifier;
        if (WeapSpeedPercentage > 0)
        {
            weap_modifier = Lerp(Reciever.WeaponStats.ModificationRangeMoveSpeed.Z, Reciever.WeaponStats.ModificationRangeMoveSpeed.Y, weap_alpha);
        }
        else
        {
            weap_modifier = Lerp(Reciever.WeaponStats.ModificationRangeMoveSpeed.Z, Reciever.WeaponStats.ModificationRangeMoveSpeed.X, weap_alpha);
        }

        double combined = (Loadout.RawMoveSpeed * 0.9) + (weap_modifier * 0.667);
        double pawnalpha = Math.Min(Math.Abs(combined), 100) / 100;
        double pawnspeed;
        if (combined > 0)
        {
            pawnspeed = Lerp(765, 900, pawnalpha);
        }
        else
        {
            pawnspeed = Lerp(765, 630, pawnalpha);
        }

        return pawnspeed / 100;
    }

    /// <summary>
    /// Calculates the Spread values for AimDownSight, Hip and Move firing.
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="AccuracyPercentage">all raw Accuracy modifiers</param>
    /// <param name="BarrelStockMovementSpeed">Barrel and Stock raw MovmentSpeed modifiers</param>
    /// <returns></returns>
    public static (double ZoomSpread, double HipSpread, double MovmentSpread) CalculateSpread(BLRItem Reciever, double AccuracyPercentage, double BarrelStockMovementSpeed, BLRItem? Magazine, BLRItem? Ammo)
    {
        if (Reciever is null || Reciever.WeaponStats is null) return (0, 0, 0);
        double allMoveSpeed = Percentage(BarrelStockMovementSpeed);
        double allAccuracy = Percentage(AccuracyPercentage);
        double accuracyBaseModifier;
        double accuracyTABaseModifier;
        double alpha = Math.Abs(allAccuracy);
        if (allAccuracy > 0)
        {
            accuracyBaseModifier = Lerp(Reciever.WeaponStats.ModificationRangeBaseSpread.Z, Reciever.WeaponStats.ModificationRangeBaseSpread.Y, alpha);
            accuracyTABaseModifier = Lerp(Reciever.WeaponStats.ModificationRangeTABaseSpread.Z, Reciever.WeaponStats.ModificationRangeTABaseSpread.Y, alpha);
        }
        else
        {
            accuracyBaseModifier = Lerp(Reciever.WeaponStats.ModificationRangeBaseSpread.Z, Reciever.WeaponStats.ModificationRangeBaseSpread.X, alpha);
            accuracyTABaseModifier = Lerp(Reciever.WeaponStats.ModificationRangeTABaseSpread.Z, Reciever.WeaponStats.ModificationRangeTABaseSpread.X, alpha);
        }

        double hip = accuracyBaseModifier * (180 / Math.PI);
        double aim = accuracyBaseModifier * Reciever.WeaponStats.ZoomSpreadMultiplier * (180 / Math.PI);
        if (Reciever.WeaponStats.UseTABaseSpread)
        {
            aim = accuracyTABaseModifier * (float)(180 / Math.PI);
        }

        double weight_alpha = Math.Abs(Reciever.WeaponStats.Weight / 80.0);
        double weight_clampalpha = Math.Min(Math.Max(weight_alpha, -1.0), 1.0); // Don't ask me why they clamp the absolute value with a negative, I have no idea.
        double weight_multiplier;
        if (Reciever.WeaponStats.Weight > 0)   // It was originally supposed to compare the total weight of equipped mods, but from what I can currently gather from the scripts, nothing modifies weapon weight so I'm just comparing base weight for now.
        {
            weight_multiplier = Lerp(Reciever.WeaponStats.ModificationRangeWeightMultiplier.Z, Reciever.WeaponStats.ModificationRangeWeightMultiplier.Y, weight_clampalpha);  // Originally supposed to be a weapon specific range, but they all set the same values so it's not worth setting elsewhere.
        }
        else
        {
            weight_multiplier = Lerp(Reciever.WeaponStats.ModificationRangeWeightMultiplier.Z, Reciever.WeaponStats.ModificationRangeWeightMultiplier.X, weight_clampalpha);
        }

        double move_alpha = Math.Abs(allMoveSpeed); // Combined movement speed modifiers from only barrel and stock, divided by 100
        double move_multiplier; // Applying movement to it like this isn't how it's done to my current knowledge, but seems to be consistently closer to how it should be in most cases so far.
        if (allMoveSpeed > 0)
        {
            move_multiplier = Lerp(Reciever.WeaponStats.ModificationRangeWeightMultiplier.Z, Reciever.WeaponStats.ModificationRangeWeightMultiplier.Y, move_alpha);
        }
        else
        {
            move_multiplier = Lerp(Reciever.WeaponStats.ModificationRangeWeightMultiplier.Z, Reciever.WeaponStats.ModificationRangeWeightMultiplier.X, move_alpha);
        }

        double movemultiplier_current = 1.0 + (Reciever.WeaponStats.MovementSpreadMultiplier - 1.0) * (weight_multiplier * move_multiplier);
        double moveconstant_current = Reciever.WeaponStats.MovementSpreadConstant * (weight_multiplier * move_multiplier);

        double move = (accuracyBaseModifier + moveconstant_current) * (180 / Math.PI) * movemultiplier_current;

        if (Reciever.UID == 40015) // BLP
        {
            if ((Magazine?.UID == 44177) || (Ammo?.UID == 90010)) {
                aim += (0.2 * (180 / Math.PI));
                hip += (0.2 * (180 / Math.PI));
                move += (0.2 * (180 / Math.PI));
            }
        } else if (Reciever.UID == 40005) // Shotgun
        {
            aim += (0.1 * (180 / Math.PI));
            hip += (0.1 * (180 / Math.PI));
            move += (0.1 * (180 / Math.PI));
        } else if (Reciever.UID == 40016) // Sark
        {
            aim += (0.05 * (180 / Math.PI));
            hip += (0.05 * (180 / Math.PI));
            move += (0.05 * (180 / Math.PI));
        }

        // Average spread over multiple shots to account for random center weight multiplier
        double[] averageSpread = { 0, 0, 0 };
        double magsize = Math.Min(Reciever.WeaponStats.MagSize, 15.0f);
        if (magsize <= 1)
        {
            magsize = Reciever.WeaponStats.InitialMagazines + 1.0;
        }
        if (magsize > 0)
        {
            double averageShotCount = Math.Max(magsize, 5.0f);
            for (int shot = 1; shot <= averageShotCount; shot++)
            {
                if (shot > averageShotCount - averageShotCount * Reciever.WeaponStats.SpreadCenterWeight)
                {
                    averageSpread[0] += aim * Reciever.WeaponStats.SpreadCenter;
                    averageSpread[1] += hip * Reciever.WeaponStats.SpreadCenter;
                    averageSpread[2] += move * Reciever.WeaponStats.SpreadCenter;
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

        // Going back to using original spread values for now, until I change my mind again
        // (realized averaging it would have made the advanced info spread center stuff hard to understand, being baked into the results, for those not knowing the original values)
        return (ZoomSpread: aim,
                HipSpread: hip,
                MovmentSpread: move);
    }

    /// <summary>
    /// Calculates the ScopeInTime for Reciever and Scope
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="Scope">Scope</param>
    /// <param name="BarrelStockMovementSpeed">Barrel and Stock raw MovementSpeed modifier</param>
    /// <param name="RawScopeInTime">all raw ScopeInTime modifiers</param>
    /// <returns></returns>
    public static double CalculateScopeInTime(BLRItem Reciever, BLRItem? Scope, double BarrelStockMovementSpeed, double RawScopeInTime)
    {
        double allMovementSpeed = Clamp(BarrelStockMovementSpeed / 80.0D, -1.0D, 1.0D);
        double TTTA_alpha = Math.Abs(allMovementSpeed);
        double TightAimTime, ComboScopeMod, FourXAmmoCounterMod, ArmComInfraredMod, EMIACOGMod, EMITechScopeMod, EMIInfraredMod, EMIInfraredMK2Mod, ArmComSniperMod, KraneSniperScopeMod, SilverwoodHeavyMod, FrontierSniperMod;

        // giant cheat incoming, please lord forgive me for what i am about to do
        if (allMovementSpeed > 0)
        {
            TightAimTime = Lerp(0.225, 0.15, TTTA_alpha);
            ComboScopeMod = Lerp(0.0, 0.032, TTTA_alpha);
            FourXAmmoCounterMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            ArmComInfraredMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            EMIACOGMod = Lerp(RawScopeInTime, 0.19, TTTA_alpha);
            EMITechScopeMod = Lerp(RawScopeInTime, 0.2185, TTTA_alpha);
            EMIInfraredMod = Lerp(RawScopeInTime, 0.2185, TTTA_alpha);
            EMIInfraredMK2Mod = Lerp(RawScopeInTime, 0.36, TTTA_alpha);
            ArmComSniperMod = Lerp(RawScopeInTime, 0.275, TTTA_alpha);
            KraneSniperScopeMod = Lerp(RawScopeInTime, 0.275, TTTA_alpha);
            SilverwoodHeavyMod = Lerp(RawScopeInTime, 0.275, TTTA_alpha);
            FrontierSniperMod = Lerp(RawScopeInTime, 0.315, TTTA_alpha);
        }
        else
        {
            TightAimTime = Lerp(0.225, 0.30, TTTA_alpha);
            ComboScopeMod = Lerp(0.0, -0.042, TTTA_alpha);
            FourXAmmoCounterMod = Lerp(RawScopeInTime, 0.105, TTTA_alpha);
            ArmComInfraredMod = Lerp(RawScopeInTime, 0.105, TTTA_alpha);
            EMIACOGMod = Lerp(RawScopeInTime, 0.14, TTTA_alpha);
            EMITechScopeMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            EMIInfraredMod = Lerp(RawScopeInTime, 0.16, TTTA_alpha);
            EMIInfraredMK2Mod = Lerp(RawScopeInTime, 0.305, TTTA_alpha);
            ArmComSniperMod = Lerp(RawScopeInTime, 0.205, TTTA_alpha);
            KraneSniperScopeMod = Lerp(RawScopeInTime, 0.205, TTTA_alpha);
            SilverwoodHeavyMod = Lerp(RawScopeInTime, 0.205, TTTA_alpha);
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
                    45002 => TightAimTime + EMIACOGMod,
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

    /// <summary>
    /// Calculates the three ranges
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="RangePercentage">all Range modifiers</param>
    /// <returns>1:Ideal Range, 2:Max Range, 3:Tracer Range</returns>
    public static (double IdealRange, double MaxRange, double TracerRange) CalculateRange(BLRItem Reciever, double RangePercentage)
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

        return (IdealRange: idealRange / 100.0D,
                MaxRange: maxRange / 100.0D,
                TracerRange: (Reciever?.WeaponStats?.MaxTraceDistance ?? 0) / 100.0D);
    }

    /// <summary>
    /// Calculates the Hip and AimDownSight Recoil
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="RecoilPercentage">all Recoil modifiers</param>
    /// <returns>1:HipRecoil 2:ZoomRecoil</returns>
    public static (double RecoilHip, double RecoilZoom) CalculateRecoil(BLRItem Reciever, double RecoilPercentage)
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
                    X = (Reciever?.WeaponStats?.RecoilVector.X ?? 0) * (Reciever?.WeaponStats?.RecoilVectorMultiplier.X ?? 0) * 0.5f / 2.0f, // in the recoil, recoil vector is actually a multiplier on a random X and Y value in the -0.5/0.5 and 0.0/0.3535 range respectively
                    Y = (Reciever?.WeaponStats?.RecoilVector.Y ?? 0) * (Reciever?.WeaponStats?.RecoilVectorMultiplier.Y ?? 0) * 0.3535f
                };

                double accumExponent = Reciever?.WeaponStats?.RecoilAccumulation ?? 0;
                if (accumExponent > 1)
                {
                    accumExponent = (accumExponent - 1.0) * (Reciever?.WeaponStats?.RecoilAccumulationMultiplier ?? 0) + 1.0; // Apparently this is how they apply the accumulation multiplier in the actual recoil
                }

                double previousMultiplier = (Reciever?.WeaponStats?.RecoilSize ?? 0) * Math.Pow(shot / (Reciever?.WeaponStats?.Burst ?? 0), accumExponent);
                double currentMultiplier = (Reciever?.WeaponStats?.RecoilSize ?? 0) * Math.Pow(shot / (Reciever?.WeaponStats?.Burst ?? 0) + 1.0f, accumExponent);
                double multiplier = currentMultiplier - previousMultiplier;
                newRecoil *= (float)multiplier;
                averageRecoil += newRecoil;
            }

            // Magic numbers because I can't yet figure out the weird and overcomplicated clamping system, these gun's set Min and MaxWeaponRecoil values cause more overall recoil than their other values suggest
            // So it might be a bit messy here until I figure it out (luckily many guns use the default values so I can ignore them)
            if (Reciever?.UID == 40011) // LMG
            {
                averageRecoil.Y *= 1.1f;
            }
            else if (Reciever?.UID == 40014 || Reciever?.UID == 40007 || Reciever?.UID == 40008) // LMGR - BAR - CR
            {
                averageRecoil.Y *= 1.3f;
            }
            else if (Reciever?.UID == 40021 || Reciever?.UID == 40019 || Reciever?.UID == 40015 || Reciever?.UID == 40005 || Reciever?.UID == 40002) // Snub - AMR - BLP - Shotgun - Revolver
            {
                averageRecoil.Y *= 1.5f;
            }
            else
            {
                averageRecoil.Y *= 1.005f;
            }

            if (averageShotCount > 0)
            {
                averageRecoil /= (float)averageShotCount;
            }
            if ((Reciever?.WeaponStats?.ROF ?? 0) > 0 && (Reciever?.WeaponStats?.ApplyTime ?? 0) > 60 / (Reciever?.WeaponStats?.ROF ?? 999))
            {
                averageRecoil *= (float)(60 / ((Reciever?.WeaponStats?.ROF ?? 0) * (Reciever?.WeaponStats?.ApplyTime ?? 0)));
            }
            double recoil = averageRecoil.Length() * recoilModifier;
            recoil *= 180 / Math.PI;

            return (RecoilHip: recoil,
                    RecoilZoom: recoil * (Reciever?.WeaponStats?.RecoilZoomMultiplier ?? 0) * 0.8D);
        }
        else
        {
            return (RecoilHip: 0,
                    RecoilZoom: 0);
        }
    }

    /// <summary>
    /// Calculates the IdealRange and MaxRange Damage
    /// </summary>
    /// <param name="Reciever">Reciever</param>
    /// <param name="DamagePercentage">all raw Damage modifiers</param>
    /// <returns></returns>
    public static (double DamageIdeal, double DamageMax) CalculateDamage(BLRItem Reciever, double DamagePercentage)
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

        return (DamageIdeal: damageModifier,
                DamageMax: damageModifier * (Reciever?.WeaponStats?.MaxRangeDamageMultiplier ?? 0.1d));
    }

    public static BLRItem? CompareItemRating(BLRItem? item1, BLRItem? item2)
    {
        if ((item1?.WeaponModifiers?.Rating ?? -1) >= (item2?.WeaponModifiers?.Rating ?? -1))
        {
            return item1;
        }
        else
        {
            return item2;
        }
    }

    public static string CompareItemDescriptor1(BLRItem? itembarrel, BLRItem? itemmag)
    {
        return CompareItemRating(itemmag, itembarrel)?.DescriptorName ?? "Standard";
    }
    public static string CompareItemDescriptor2(BLRItem? itemstock, BLRItem? itemmuzzle, BLRItem? itemscope)
    {
        var item = CompareItemRating(itemscope, itemstock);
        item = CompareItemRating(item, itemmuzzle);
        if (item?.WeaponModifiers?.Rating <= 0)
        {
            return "Basic";
        }
        else
        {
            return item?.DescriptorName ?? "Basic";
        }
    }

    public static double Percentage(double input)
    {
        return Clamp(input / 100.0D, -1.0D, 1.0D);
    }
    public static double Lerp(double start, double target, double time)
    {
        return start * (1.0d - time) + target * time;
    }
    public static double Clamp(double input, double min, double max)
    {
        return Math.Min(Math.Max(input, min), max);
    }

    public static int Clamp(int input, int min, int max)
    {
        return Math.Min(Math.Max(input, min), max);
    }

    public void SetWeapon(IBLRWeapon weapon)
    {
        _weapon = weapon;
    }

    public void Read()
    {
        _weapon?.Read(this);
    }

    public void Write() 
    {
        _weapon?.Write(this);
    }

    static readonly Random rng = new();
    public void Randomize()
    {
        BLRItem reciever;
        if (IsPrimary)
        {
            var filtered = ImportSystem.GetItemArrayOfType(ImportSystem.PRIMARY_CATEGORY).Where(ItemFilters.PartialFilter).ToArray();
            reciever = filtered[rng.Next(0, filtered.Length)];
        }
        else
        {
            var filtered = ImportSystem.GetItemArrayOfType(ImportSystem.SECONDARY_CATEGORY).Where(ItemFilters.PartialFilter).ToArray();
            reciever = filtered[rng.Next(0, filtered.Length)];
        }

        var FilteredBarrels = ImportSystem.GetItemArrayOfType(ImportSystem.BARRELS_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();
        var FilteredScopes = ImportSystem.GetItemArrayOfType(ImportSystem.SCOPES_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();
        var FilteredMagazines = ImportSystem.GetItemArrayOfType(ImportSystem.MAGAZINES_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();
        //Dependant of Barrel on secondarioes
        var FilteredMuzzles = ImportSystem.GetItemArrayOfType(ImportSystem.MUZZELS_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();
        var FilteredStocks = ImportSystem.GetItemArrayOfType(ImportSystem.STOCKS_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();
        var FilteredCamos = ImportSystem.GetItemArrayOfType(ImportSystem.CAMOS_WEAPONS_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();
        var FilteredHangers = ImportSystem.GetItemArrayOfType(ImportSystem.HANGERS_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();
        var FilteredAmmo = ImportSystem.GetItemArrayOfType(ImportSystem.AMMO_CATEGORY).Where(o => o.IsValidFor(reciever)).ToArray();


        BLRItem? barrel = null;
        if (FilteredBarrels.Length > 0)
        {
            barrel = FilteredBarrels[rng.Next(0, FilteredBarrels.Length)];
        }
        BLRItem? scope = null;
        if (FilteredScopes.Length > 0)
        {
            scope = FilteredScopes[rng.Next(0, FilteredScopes.Length)];
        }
        BLRItem? magazine = null;
        if (FilteredMagazines.Length > 0)
        {
            magazine = FilteredMagazines[rng.Next(0, FilteredMagazines.Length)];
        }

        BLRItem? stock = null;
        if (FilteredStocks.Length > 0)
        {
            stock = FilteredStocks[rng.Next(0, FilteredStocks.Length)];
        }
        BLRItem? muzzle = null;
        if (FilteredMuzzles.Length > 0)
        {
            muzzle = FilteredMuzzles[rng.Next(0, FilteredMuzzles.Length)];
        }
        BLRItem? hanger = null;
        if (FilteredHangers.Length > 0)
        {
            hanger = FilteredHangers[rng.Next(0, FilteredHangers.Length)];
        }
        BLRItem? camo = null;
        if (FilteredCamos.Length > 0)
        {
            camo = FilteredCamos[rng.Next(0, FilteredCamos.Length)];
        }

        BLRItem? ammo = null;
        if (FilteredAmmo.Length > 0)
        {
            if ((magazine is not null && !magazine.ItemNameContains("Toxic", "Incendiary", "Electro", "Explosive", "Magnum")) || magazine is null)
            {
                ammo = FilteredAmmo[rng.Next(0, FilteredAmmo.Length)];
            }
        }

        //TODO: Randomize Ammo(we first need to generate the weapon then we can properly randomize the Ammo according to the reciever and magazine we also need to be carefull of Element mags as they have the elemental effect priority eg it doesn't mater what ammo type is equipped)
        //TODO: Randomize Grip(we need to generate the grip for weapons that have a Grip slot)
        //TODO: Use the text filter to theme Randomization where possible(when the list is not empty after filtering).

        var grip = ImportSystem.GetItemByIDAndType(ImportSystem.GRIPS_CATEGORY, rng.Next(0, ImportSystem.GetItemArrayOfType(ImportSystem.GRIPS_CATEGORY)?.Length ?? 0));

        UndoRedoSystem.DoAction(reciever, this.GetType().GetProperty(nameof(Reciever)), this);
        UndoRedoSystem.DoAction(barrel, this.GetType().GetProperty(nameof(Barrel)), this);
        UndoRedoSystem.DoAction(scope, this.GetType().GetProperty(nameof(Scope)), this);
        UndoRedoSystem.DoAction(stock, this.GetType().GetProperty(nameof(Stock)), this);
        UndoRedoSystem.DoAction(muzzle, this.GetType().GetProperty(nameof(Muzzle)), this);
        UndoRedoSystem.DoAction(magazine, this.GetType().GetProperty(nameof(Magazine)), this);
        UndoRedoSystem.DoAction(camo, this.GetType().GetProperty(nameof(Camo)), this);
        UndoRedoSystem.DoAction(hanger, this.GetType().GetProperty(nameof(Tag)), this);
        if(ammo is not null) UndoRedoSystem.DoAction(ammo, this.GetType().GetProperty(nameof(Ammo)), this);
    }
}