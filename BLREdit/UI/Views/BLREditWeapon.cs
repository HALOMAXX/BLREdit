using BLREdit.API.Utils;
using BLREdit.Export;
using BLREdit.Import;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using static BLREdit.API.Utils.HelperFunctions;

namespace BLREdit.UI.Views;

public sealed class BLREditWeapon : INotifyPropertyChanged
{
    private IBLRWeapon? _weapon;
    public IBLRWeapon? InternalWeapon { get { return _weapon; } }

    static readonly Type thisClassType = typeof(BLREditWeapon);
    public static PropertyInfo[] WeaponPartInfo { get; } = ([.. (from property in thisClassType.GetProperties() where Attribute.IsDefined(property, typeof(BLRItemAttribute)) orderby ((BLRItemAttribute)property.GetCustomAttributes(typeof(BLRItemAttribute), false).Single()).PropertyOrder select property)]);
    private static readonly Dictionary<string?, Tuple<PropertyInfo, BLRItemAttribute>> WeaponPartInfoDictonary = GetWeaponPartPropertyInfo();
    private static Dictionary<string?, Tuple<PropertyInfo, BLRItemAttribute>> GetWeaponPartPropertyInfo()
    {
        var dict = new Dictionary<string?, Tuple<PropertyInfo, BLRItemAttribute>>();
        foreach (var sett in WeaponPartInfo)
        {
            var attri = sett.GetCustomAttribute<BLRItemAttribute>();
            dict.Add(sett.Name, new(sett, attri));
        }
        return dict;
    }

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    private void ItemChanged([CallerMemberName] string? propertyName = null)
    {
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteWeapon)) Write();
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Calculate)) CalculateStats();
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) OnPropertyChanged(propertyName);
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Update)) IsChanged = true;
    }
    #endregion Event

    public bool IsPrimary { get; set; }

    [JsonIgnore] public bool HasSkinEquipped { get { return Skin is not null && Skin.Name != "No Weapon Skin"; } }

    public BLREditLoadout? Loadout { get; private set; }

    private bool isChanged;
    [JsonIgnore] public bool IsChanged { get { return isChanged; } set { isChanged = value; OnPropertyChanged(); } }

    private readonly Dictionary<int, BLREditItem?> WeaponParts = InitDict();

    private static Dictionary<int, BLREditItem?> InitDict()
    {
        Dictionary<int, BLREditItem?> dict = [];
        foreach (var info in WeaponPartInfoDictonary)
        {
            dict.Add(info.Value.Item2.PropertyOrder, null);
        }
        return dict;
    }

    #region Weapon Parts
    [BLRItem($"{ImportSystem.PRIMARY_CATEGORY}|{ImportSystem.SECONDARY_CATEGORY}")] public BLREditItem? Receiver { get { return GetValueOf(); } set { if (AllowReceiver(value)) { SetValueOf(value); RemoveIncompatibleMods(); AddMissingDefaultParts(); UpdateScopeIcons(); } } }
    [BLRItem(ImportSystem.BARRELS_CATEGORY)] public BLREditItem? Barrel { get { return GetValueOf(); } set { SetValueOf(value); RemoveIncompatibleMods(); } }
    public UIBool IsValidBarrel { get; } = new(true);
    [BLRItem(ImportSystem.MAGAZINES_CATEGORY)] public BLREditItem? Magazine { get { return GetValueOf(); } set { SetValueOf(value); var ammo = Ammo; if (IsAmmoOk(ref ammo)) { SetValueOf(ammo, nameof(Ammo)); } else { ammo = null; if (IsAmmoOk(ref ammo)) { SetValueOf(ammo, nameof(Ammo)); } } } }
    public UIBool IsValidMagazine { get; } = new(true);
    [BLRItem(ImportSystem.MUZZELS_CATEGORY)] public BLREditItem? Muzzle { get { return GetValueOf(); } set { SetValueOf(value); } }
    public UIBool IsValidMuzzle { get; } = new(true);
    [BLRItem(ImportSystem.STOCKS_CATEGORY)] public BLREditItem? Stock { get { return GetValueOf(); } set { if (AllowStock(Receiver, Barrel, value, true)) { SetValueOf(value); } } }
    public UIBool IsValidStock { get; } = new(true);
    [BLRItem(ImportSystem.SCOPES_CATEGORY)] public BLREditItem? Scope { get { return GetValueOf(); } set { SetValueOf(value); UpdateScopeIcons(); } }
    public UIBool IsValidScope { get; } = new(true);
    [BLRItem(ImportSystem.GRIPS_CATEGORY)] public BLREditItem? Grip { get { return GetValueOf(); } set { SetValueOf(value); } }
    public UIBool IsValidGrip { get; } = new(true);
    [BLRItem(ImportSystem.HANGERS_CATEGORY)] public BLREditItem? Tag { get { return GetValueOf(); } set { SetValueOf(value); } }
    public UIBool IsValidTag { get; } = new(true);
    [BLRItem(ImportSystem.CAMOS_WEAPONS_CATEGORY)] public BLREditItem? Camo { get { return GetValueOf(); } set { SetValueOf(value); } }
    public UIBool IsValidCamo { get; } = new(true);
    [BLRItem(ImportSystem.AMMO_CATEGORY)] public BLREditItem? Ammo { get { return GetValueOf(); } set { if (IsAmmoOk(ref value)) { SetValueOf(value); } } }
    public UIBool IsValidAmmo { get; } = new(true);
    [BLRItem(ImportSystem.PRIMARY_SKIN_CATEGORY)] public BLREditItem? Skin { get { return GetValueOf(); } set { SetValueOf(value); OnPropertyChanged(nameof(HasSkinEquipped)); } }
    public UIBool IsValidSkin { get; } = new(true);
    #endregion Weapon Parts

    private BLREditItem? GetValueOf([CallerMemberName] string? name = null)
    {
        if (string.IsNullOrEmpty(name)) return null;

        var propAndAttri = WeaponPartInfoDictonary[name];
        if (WeaponParts.TryGetValue(propAndAttri.Item2.PropertyOrder, out var value))
        { return value; }
        else
        { return null; }
    }

    private void SetValueOf(BLREditItem? value, [CallerMemberName] string? name = null)
    {
        if (string.IsNullOrEmpty(name)) return;

        var propAndAttri = WeaponPartInfoDictonary[name];
        if (!UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.SetValueTest))
        {
            var receiver = GetValueOf(nameof(Receiver));
            if (value is null && (Loadout?.IsAdvanced.IsNot ?? false)) value = MagiCowsWeapon.GetDefaultSetupOfReceiver(receiver)?.GetItemByType(propAndAttri.Item2.ItemType) ?? null;
            if (value is not null && (!value.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false) || !propAndAttri.Item2.ItemType.Contains(value.Category)))
            { return; }
            bool isValid = BLREditItem.IsValidFor(value, receiver, false);
            switch ($"IsValid{name}")
            {
                case nameof(IsValidBarrel):
                    IsValidBarrel.Set(isValid);
                    break;
                case nameof(IsValidMagazine):
                    IsValidMagazine.Set(isValid);
                    break;
                case nameof(IsValidMuzzle):
                    IsValidMuzzle.Set(isValid);
                    break;
                case nameof(IsValidStock):
                    IsValidStock.Set(isValid);
                    break;
                case nameof(IsValidScope):
                    IsValidScope.Set(isValid);
                    break;
                case nameof(IsValidGrip):
                    IsValidGrip.Set(isValid);
                    break;
                case nameof(IsValidTag):
                    IsValidTag.Set(isValid);
                    break;
                case nameof(IsValidCamo):
                    IsValidCamo.Set(isValid);
                    break;
                case nameof(IsValidAmmo):
                    IsValidAmmo.Set(isValid);
                    break;
                case nameof(IsValidSkin):
                    IsValidSkin.Set(isValid);
                    break;
            }
        }
        WeaponParts[propAndAttri.Item2.PropertyOrder] = value;
        ItemChanged(name);
    }

    public BLREditWeapon Copy()
    {
        BLREditWeapon wpn = new(IsPrimary);
        foreach (var property in WeaponPartInfo)
        {
            if (property.GetValue(this) is BLREditItem item)
            {
                property.SetValue(wpn, item);
            }
        }
        return wpn;
    }

    public void ApplyCopy(BLREditWeapon? weapon)
    {
        LoggingSystem.Log("Applying Weapon Copy!");
        if (weapon is not null && this.IsPrimary == weapon.IsPrimary)
        {
            foreach (var property in WeaponPartInfo)
            {
                if (property.GetValue(weapon) is BLREditItem item)
                {
                    UndoRedoSystem.DoValueChange(item, property, this, BlockEvents.AllExceptUpdate);
                }
            }
            UndoRedoSystem.DoValueChange(Ammo, WeaponPartInfoDictonary[nameof(Ammo)].Item1, this); //to Trigger all Events at the end of the Sequence
            LoggingSystem.Log($"Current:{UndoRedoSystem.CurrentActionCount} After:{UndoRedoSystem.AfterActionCount}");

            UndoRedoSystem.EndUndoRecord(true);
            var wpnCat = weapon.IsPrimary ? "Primary" : "Secondary";
            MainWindow.ShowAlert($"Pasted { wpnCat } Weapon!");
        }
        LoggingSystem.Log("Finished Applying Weapon Copy!");
    }

    //TODO: Add Premade Weapon Setup's

    [JsonIgnore] public FoxIcon ScopePreview { get { return GetBitmapCrosshair(Scope?.GetSecondaryScope(this) ?? ""); } }

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

    private bool IsAmmoOk(ref BLREditItem? ammo)
    {
        if (Magazine is null) return true;
        if (ammo is null)
        {
            var uid = Magazine?.AmmoType ?? -1;
            if (uid == -1) { uid = 90000; }
            ammo = ImportSystem.GetItemByUIDAndType(ImportSystem.AMMO_CATEGORY, uid);
            return true;
        }
        if (Magazine.AmmoType == -1) return true;
        if (Magazine.AmmoType == ammo.UID) return true;
        if (Loadout?.IsAdvanced.Is ?? false) return true;
        return false;
    }

    private void AddMissingDefaultParts()
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.AddMissing)) { return; }
        if (Receiver is null) { LoggingSystem.Log($"can't check for default setup of weapon as receiver is missing!"); return; }
        var wpn = MagiCowsWeapon.GetDefaultSetupOfReceiver(Receiver);
        if (wpn is null) { LoggingSystem.Log($"missing default setup for {Receiver?.Name}"); return; }

        if (Barrel is null || Barrel.Name == MagiCowsWeapon.NoBarrel)
        {
            Barrel = wpn.GetBarrelItem();
        }
        if (Scope is null || Scope.Name == MagiCowsWeapon.NoScope)
        {
            Scope = wpn.GetScopeItem();
        }
        if (Stock is null || Stock.Name == MagiCowsWeapon.NoStock)
        {
            Stock = wpn.GetStockItem();
        }
        if (Grip is null || Grip.Name == MagiCowsWeapon.NoGrip)
        {
            Grip = wpn.GetGripItem();
        }

        if (Muzzle is null || BLREditItem.GetMagicCowsID(Muzzle) == MagiCowsWeapon.NoMuzzle)
        {
            Muzzle = wpn.GetMuzzleItem();
        }
        if (Magazine is null || BLREditItem.GetMagicCowsID(Magazine) == MagiCowsWeapon.NoMagazine)
        {
            Magazine = wpn.GetMagazineItem();
            var uid = Magazine?.UID ?? -1;
            if (uid == -1) { uid = 90000; }
            Ammo = ImportSystem.GetItemByUIDAndType(ImportSystem.AMMO_CATEGORY, uid);
        }

        Skin ??= ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, 0);
    }



    private static bool IsPistol(BLREditItem? receiver)
    {
        if (receiver == null) return false;
        return receiver.Name == "Light Pistol" || receiver.Name == "Heavy Pistol" || receiver.Name == "Prestige Light Pistol";
    }
    private bool AllowReceiver(BLREditItem? item)
    {
        if (item is null) return true;
        if (IsPrimary)
        {
            if (item.Category != ImportSystem.PRIMARY_CATEGORY)
            {
                return false;
            }
        }
        else
        {
            if (item.Category != ImportSystem.SECONDARY_CATEGORY)
            {
                return false;
            }
        }
        return true;
    }

    private bool AllowStock(BLREditItem? receiver, BLREditItem? barrel, BLREditItem? stock, bool displayMessage = false)
    {
        if (stock is null || stock.Name == "No Stock") return true;
        if (!IsPrimary)
        {
            if (IsPistol(receiver) && (barrel?.Name ?? MagiCowsWeapon.NoBarrel) == MagiCowsWeapon.NoBarrel)
            {
                if(displayMessage) MainWindow.ShowAlert("Can't set Stock\nwhen no Barrel is attached!"); //TODO: Add Localization Support for Alert
                return false;
            }
        }
        return true;
    }


    private void UpdateScopeIcons()
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ScopeUpdate)) { return; }
        OnPropertyChanged(nameof(ScopePreview));
    }

    public BLREditWeapon(bool isPrimary, BLREditLoadout? loadout = null, IBLRWeapon? weapon = null, bool readBackEvent = false)
    {
        IsPrimary = isPrimary;
        Loadout = loadout;

        SetWeapon(weapon, readBackEvent);
        Read();
    }

    #region Properties
    public double AccuracyPercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.Accuracy ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.Accuracy ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.Accuracy ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.Accuracy ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.Accuracy ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.Accuracy ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.Accuracy ?? 0;
            return total;
        }
    }
    public double AdditionalAmmo
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.Ammo ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.Ammo ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.Ammo ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.Ammo ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.Ammo ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.Ammo ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.Ammo ?? 0;
            return total;
        }
    }
    public double DamagePercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.Damage ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.Damage ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.Damage ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.Damage ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.Damage ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.Damage ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.Damage ?? 0;

            // arrows don't directly affect damage and were set by the projectile, my modifiers were causing misleading damage changes on other guns, so here's a hacky fix
            if (Receiver?.UID == 40024)
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
            total += Receiver?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.MovementSpeed ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.MovementSpeed ?? 0;
            return total;
        }
    }
    public double RangePercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.Range ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.Range ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.Range ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.Range ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.Range ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.Range ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.Range ?? 0;
            return total;
        }
    }
    public double RateOfFirePercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.RateOfFire ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.RateOfFire ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.RateOfFire ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.RateOfFire ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.RateOfFire ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.RateOfFire ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.RateOfFire ?? 0;
            return total;
        }
    }
    public double TotalRatingPoints
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.Rating ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.Rating ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.Rating ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.Rating ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.Rating ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.Rating ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.Rating ?? 0;
            return total;
        }
    }
    public double RecoilPercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.Recoil ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.Recoil ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.Recoil ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.Recoil ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.Recoil ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.Recoil ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.Recoil ?? 0;
            return total;
        }
    }
    public double ReloadSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.ReloadSpeed ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.ReloadSpeed ?? 0;
            return total;
        }
    }
    public double SwitchWeaponSpeedPercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.SwitchWeaponSpeed ?? 0;
            return total;
        }
    }
    public double WeaponWeightPercentage
    {
        get
        {
            double total = 0;
            total += Receiver?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WeaponModifiers?.WeaponWeight ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WeaponModifiers?.WeaponWeight ?? 0;
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
            if (Receiver?.UID == 40019)
            {
                return 1; // cheat because for some reason it isn't reading AMR's currently, might be due to lack of mag but am not sure
            }
            else
            { return Receiver?.WeaponStats?.MagSize ?? 0; }
        }
    }
    public double ModifiedAmmoMagazine
    { get { return RawAmmoMagazine + AdditionalAmmo; } }
    public double FinalAmmoMagazine // for eventual cases of advanced modding that i cant explain
    {
        get
        {
            if (Receiver?.UID == 40019 || Receiver?.UID == 40015)
            {
                return 1; // Forcing AMR and BLP mag to 1 while trying not to change how its reserve ammo is modified, because oddly enough typical gun mags don't increase its base ammo but still treat reserve as if base was modified, which makes no sense
            }
            return ModifiedAmmoMagazine;
        }
    }
    public double RawAmmoReserve
    { get { return RawAmmoMagazine * (Receiver?.WeaponStats?.InitialMagazines ?? 0); } }
    public double ModifiedAmmoReserve
    { get { return ModifiedAmmoMagazine * (Receiver?.WeaponStats?.InitialMagazines ?? 0); } }
    public double RawRateOfFire
    { get { return Receiver?.WeaponStats?.RateOfFire ?? 0; } }
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
            total += Receiver?.WikiStats?.Reload ?? 0;
            if (Barrel?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Barrel?.WikiStats?.Reload ?? 0;
            if (Magazine?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Magazine?.WikiStats?.Reload ?? 0;
            if (Muzzle?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Muzzle?.WikiStats?.Reload ?? 0;
            if (Stock?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Stock?.WikiStats?.Reload ?? 0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WikiStats?.Reload ?? 0;
            if (Grip?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Grip?.WikiStats?.Reload ?? 0;

            // Corrected the reload rate multiplier calc and changed the ranges from zeros in the BPFA and BFR so they can now correctly use the reloadspeed percentage instead of flat wiki value add/sub
            // Will eventually fix all guns but I am still uncertain about a few cases that don't go along with it for whatever reason
            if (Receiver?.UID == 40020 || Receiver?.UID == 40009)
            {
                return Receiver?.WikiStats?.Reload ?? 0;
            }

            return total;
        }
    }

    public double VerticalRecoilRatio
    {
        get
        {
            if (Receiver != null && Receiver.WeaponStats != null)
            {
                double vertical = Receiver.WeaponStats.RecoilVector.Y * Receiver.WeaponStats.RecoilVectorMultiplier.Y * 0.3535;
                double horizontal = Receiver.WeaponStats.RecoilVector.X * Receiver.WeaponStats.RecoilVectorMultiplier.X * 0.5;
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
            if (Receiver != null && Receiver.WeaponStats != null)
            {
                if (Receiver.WeaponStats.RecoveryTime > 0)
                {
                    return Receiver.WeaponStats.RecoveryTime;
                }
                return 60 / Receiver.WeaponStats.ROF;
            }
            return 0;
        }
    }

    public double RecoilAccumulation
    {
        get
        {
            if (Receiver != null && Receiver.WeaponStats != null)
            {
                double accumExponent = Receiver?.WeaponStats?.RecoilAccumulation ?? 0;
                if (accumExponent > 1)
                {
                    accumExponent = (accumExponent - 1.0) * (Receiver?.WeaponStats?.RecoilAccumulationMultiplier ?? 0) + 1.0;
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
            if (Receiver != null && Receiver.WeaponStats != null)
            {
                if (Receiver.WeaponStats.ZoomRateOfFire > 0)
                {
                    return Receiver.WeaponStats.ZoomRateOfFire * CockRateMultiplier;
                }
                return Receiver.WeaponStats.ROF * CockRateMultiplier;
            }
            return 0;
        }
    }

    public double SpreadCenterWeight
    {
        get
        {
            return Receiver?.WeaponStats?.SpreadCenterWeight ?? 0;
        }
    }

    public double SpreadCenter
    {
        get
        {
            return Receiver?.WeaponStats?.SpreadCenter ?? 0;
        }
    }
    public double FragmentsPerShell
    {
        get
        {
            return Receiver?.WeaponStats?.FragmentsPerShell ?? 0;
        }
    }
    public double FragmentSpread
    {
        get
        {
            return Receiver?.WeaponStats?.FragmentSpread ?? 0;
        }
    }
    public double SpreadCrouchMultiplier
    {
        get
        {
            return Receiver?.WeaponStats?.CrouchSpreadMultiplier ?? 0;
        }
    }
    public double SpreadJumpMultiplier
    {
        get
        {
            return Receiver?.WeaponStats?.JumpSpreadMultiplier ?? 0;
        }
    }
    public double SpreadJumpConstant
    {
        get
        {
            return Receiver?.WeaponStats?.SpreadJumpConstant ?? 0;
        }
    }
    public double RawScopeInTime
    {
        get
        {
            double total = 0.0;
            if (Scope?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? false) total += Scope?.WikiStats?.ScopeInTime ?? 0.105;
            return total;
        }
    }
    public bool ScopeEnabled
    {
        get
        {
            return Scope?.WikiStats?.EnableScope ?? false;
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
    { get { return Receiver?.WikiStats?.Swaprate ?? 0; } }
    public double ShortReload
    {
        get
        {
            if (Magazine?.UID == 44014 || Magazine?.UID == 44015)
            {
                return 1;
            }
            return Receiver?.WeaponStats?.ReloadShortMultiplier ?? 0;
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

    private string? weaponDescriptor;
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
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.Remove)) return;
        if (Receiver is null) return;

        MagiCowsWeapon ? wpn = MagiCowsWeapon.GetDefaultSetupOfReceiver(Receiver);

        if (Muzzle is null || !Muzzle.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetMuzzleItem(), GetType().GetProperty(nameof(Muzzle)), this); }

        if (Barrel is null || !Barrel.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetBarrelItem(), GetType().GetProperty(nameof(Barrel)), this); }

        if (Stock is null || !Stock.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) || !AllowStock(Receiver, Barrel, Stock))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetStockItem(), GetType().GetProperty(nameof(Stock)), this); }

        if (Scope is null || !Scope.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetScopeItem(), GetType().GetProperty(nameof(Scope)), this); }

        if (Magazine is null || !Magazine.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetMagazineItem(), GetType().GetProperty(nameof(Magazine)), this); }

        if (Ammo is null || !Ammo.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetAmmoItem(), GetType().GetProperty(nameof(Ammo)), this); }

        if (Grip is null || !Grip.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetGripItem(), GetType().GetProperty(nameof(Grip)), this); }

        if (Camo is null || !Camo.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetCamoItem(), GetType().GetProperty(nameof(Camo)), this); }

        if (Tag is null || !Tag.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetTagItem(), GetType().GetProperty(nameof(Tag)), this); }

        if (!Skin?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? true)
        { UndoRedoSystem.DoValueChangeAfter(ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, 0), GetType().GetProperty(nameof(Skin)), this); }
    }

    public void RemoveIncompatibleAttachments()
    {
        if (Receiver is null) return;

        UndoRedoSystem.CurrentlyBlockedEvents.Value |= BlockEvents.Remove;
        UndoRedoSystem.CurrentlyBlockedEvents.Value |= BlockEvents.AddMissing;

        MagiCowsWeapon? wpn;

        if (!BLREditItem.IsValidFor(Receiver, null))
        {
            wpn = IsPrimary ? MagiCowsWeapon.DefaultWeapons.AssaultRifle : MagiCowsWeapon.DefaultWeapons.LightPistol;
            UndoRedoSystem.DoValueChange(wpn?.GetReceiverItem(), GetType().GetProperty(nameof(Receiver)), this, BlockEvents.Remove | BlockEvents.AddMissing);
        }
        else
        { 
            wpn = MagiCowsWeapon.GetDefaultSetupOfReceiver(Receiver);
        }

        if (Muzzle is null || !Muzzle.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetMuzzleItem(), GetType().GetProperty(nameof(Muzzle)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Barrel is null || !Barrel.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetBarrelItem(), GetType().GetProperty(nameof(Barrel)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Stock is null || !Stock.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) || !AllowStock(Receiver, Barrel, Stock))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetStockItem(), GetType().GetProperty(nameof(Stock)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Scope is null || !Scope.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetScopeItem(), GetType().GetProperty(nameof(Scope)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Magazine is null || !Magazine.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetMagazineItem(), GetType().GetProperty(nameof(Magazine)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Ammo is null || !Ammo.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetAmmoItem(), GetType().GetProperty(nameof(Ammo)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Grip is null || !Grip.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetGripItem(), GetType().GetProperty(nameof(Grip)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Camo is null || !Camo.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetCamoItem(), GetType().GetProperty(nameof(Camo)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (Tag is null || !Tag.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false))
        { UndoRedoSystem.DoValueChangeAfter(wpn?.GetTagItem(), GetType().GetProperty(nameof(Tag)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        if (!Skin?.IsValidFor(Receiver, Loadout?.IsAdvanced.Is ?? false) ?? true)
        { UndoRedoSystem.DoValueChangeAfter(ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, 0), GetType().GetProperty(nameof(Skin)), this, BlockEvents.Remove | BlockEvents.AddMissing); }

        UndoRedoSystem.CurrentlyBlockedEvents.Value ^= BlockEvents.Remove;
        UndoRedoSystem.CurrentlyBlockedEvents.Value ^= BlockEvents.AddMissing;
    }

    public void AddMissingDefaultAttachments()
    {
        if (Receiver is null) { LoggingSystem.Log($"can't check for default setup of weapon as receiver is missing!"); return; }
        var wpn = MagiCowsWeapon.GetDefaultSetupOfReceiver(Receiver);

        UndoRedoSystem.CurrentlyBlockedEvents.Value |= BlockEvents.Remove;
        UndoRedoSystem.CurrentlyBlockedEvents.Value |= BlockEvents.AddMissing;

        if (wpn is null) { LoggingSystem.Log($"missing default setup for {Receiver?.Name}"); return; }

        if (Barrel is null || Barrel.Name == MagiCowsWeapon.NoBarrel)
        {
            Barrel = wpn.GetBarrelItem();
        }
        if (Scope is null || Scope.Name == MagiCowsWeapon.NoScope)
        {
            Scope = wpn.GetScopeItem();
        }
        if (Stock is null || Stock.Name == MagiCowsWeapon.NoStock)
        {
            Stock = wpn.GetStockItem();
        }
        if (Grip is null || Grip.Name == MagiCowsWeapon.NoGrip)
        {
            Grip = wpn.GetGripItem();
        }

        if (Muzzle is null || BLREditItem.GetMagicCowsID(Muzzle) == MagiCowsWeapon.NoMuzzle)
        {
            Muzzle = wpn.GetMuzzleItem();
        }
        if (Magazine is null || BLREditItem.GetMagicCowsID(Magazine) == MagiCowsWeapon.NoMagazine)
        {
            Magazine = wpn.GetMagazineItem();
            var uid = Magazine?.UID ?? -1;
            if (uid == -1) { uid = 90000; }
            Ammo = ImportSystem.GetItemByUIDAndType(ImportSystem.AMMO_CATEGORY, uid);
        }

        Skin ??= ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, 0);

        UndoRedoSystem.CurrentlyBlockedEvents.Value ^= BlockEvents.Remove;
        UndoRedoSystem.CurrentlyBlockedEvents.Value ^= BlockEvents.AddMissing;
    }

    #region Calulations
    public void CalculateStats()
    {
        if (Receiver is null) return;
        CockRateMultiplier = CalculateCockRate(Receiver, RecoilPercentage);
        (DamageClose, DamageFar) = CalculateDamage(Receiver, DamagePercentage);
        ModifiedRunSpeed = CalculateMovementSpeed(Receiver, MovementSpeedPercentage);
        ModifiedPawnRunSpeed = CalculatePawnMovementSpeed(Receiver, Loadout, MovementSpeedPercentage);
        (RangeClose, RangeFar, RangeTracer) = CalculateRange(Receiver, RangePercentage);
        RecoilHip = CalculateRecoil(Receiver, RecoilPercentage, false);
        RecoilZoom = CalculateRecoil(Receiver, RecoilPercentage, true);
        ReloadMultiplier = CalculateReloadRate(Receiver, ReloadSpeedPercentage, RecoilPercentage);
        double BarrelStockMovementSpeed = Barrel?.WeaponModifiers?.MovementSpeed ?? 0;
        BarrelStockMovementSpeed += Stock?.WeaponModifiers?.MovementSpeed ?? 0;
        ModifiedScopeInTime = CalculateScopeInTime(Receiver, Scope, BarrelStockMovementSpeed, RawScopeInTime, ScopeEnabled);
        (SpreadWhileADS, SpreadWhileStanding, SpreadWhileMoving) = CalculateSpread(Receiver, AccuracyPercentage, BarrelStockMovementSpeed, Magazine, Ammo, DamagePercentage);
        WeaponDescriptorPart1 = CompareItemDescriptor1(Barrel, Magazine);
        WeaponDescriptorPart2 = CompareItemDescriptor2(Stock, Muzzle, Scope);
        WeaponDescriptorPart3 = Receiver.SelectDescriptorName(TotalRatingPoints);
        WeaponDescriptor = WeaponDescriptorPart1 + ' ' + WeaponDescriptorPart2 + ' ' + WeaponDescriptorPart3;

        CreateDisplayProperties();
    }

    private void CreateDisplayProperties()
    {
        DamageDisplay = Math.Round(DamageClose).ToString("0") + " / " + Math.Round(DamageFar).ToString("0");
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
        RunDisplay = ModifiedRunSpeed.ToString("0");
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
        RecoilAccumulationDisplay = RecoilAccumulation.ToString("0.000");

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
    /// <param name="receiver">Receiver</param>
    /// <param name="RecoilPercentage">all raw Recoil modifiers</param>
    /// <returns>CockRateMultiplier</returns>
    public static double CalculateCockRate(BLREditItem receiver, double RecoilPercentage)
    {
        if (receiver is null || receiver.WeaponStats is null) return 0;
        double allRecoil = Percentage(RecoilPercentage);
        double alpha = Math.Abs(allRecoil);
        double cockrate;
        if (receiver.WeaponStats.ModificationRangeCockRate.Z != 0)
        {
            if (allRecoil > 0)
            {
                cockrate = Lerp(receiver.WeaponStats.ModificationRangeCockRate.Z, receiver.WeaponStats.ModificationRangeCockRate.Y, alpha);
            }
            else
            {
                cockrate = Lerp(receiver.WeaponStats.ModificationRangeCockRate.Z, receiver.WeaponStats.ModificationRangeCockRate.X, alpha);
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
    /// <param name="receiver">Receiver</param>
    /// <param name="ReloadSpeedPercentage">all raw ReloadSpeed modifiers</param>
    /// <param name="RecoilPercentage"> all raw Recoil modifiers</param>
    /// <returns>calculated ReloadMultiplier</returns>
    public static double CalculateReloadRate(BLREditItem receiver, double ReloadSpeedPercentage, double RecoilPercentage)
    {
        if (receiver is null || receiver.WeaponStats is null) return 0;
        double allReloadSpeed = ReloadSpeedPercentage / 100; // Reload speed is actually seemingly unclamped
        double allRecoil = Percentage(RecoilPercentage);
        double WeaponReloadRate = 1.0;
        double rate_alpha;
        if (receiver.WeaponStats.ModificationRangeReloadRate.Z > 0)
        {
            rate_alpha = Math.Abs(allReloadSpeed);
            if (allReloadSpeed > 0)
            {
                WeaponReloadRate = Lerp(receiver.WeaponStats.ModificationRangeReloadRate.Z, receiver.WeaponStats.ModificationRangeReloadRate.Y, rate_alpha);
            }
            else
            {
                WeaponReloadRate = Lerp(receiver.WeaponStats.ModificationRangeReloadRate.Z, receiver.WeaponStats.ModificationRangeReloadRate.X, rate_alpha);
            }
        }

        if (receiver.WeaponStats.ModificationRangeRecoilReloadRate.Z == 1)
        {
            rate_alpha = Math.Abs(allRecoil);
            if (allRecoil > 0)
            {
                WeaponReloadRate += (Lerp(receiver.WeaponStats.ModificationRangeRecoilReloadRate.Z, receiver.WeaponStats.ModificationRangeRecoilReloadRate.Y, rate_alpha) - 1.0);
            }
            else
            {
                WeaponReloadRate += (Lerp(receiver.WeaponStats.ModificationRangeRecoilReloadRate.Z, receiver.WeaponStats.ModificationRangeRecoilReloadRate.X, rate_alpha) - 1.0);
            }
        }

        WeaponReloadRate = 1.0 / WeaponReloadRate;

        return WeaponReloadRate;
    }

    /// <summary>
    /// Not yet Implemented
    /// </summary>
    /// <param name="receiver"></param>
    public static void CalculateReloadSpeed(BLREditItem receiver)
    {
        // Placeholder so I don't forget
        // It didn't help I ended up still forgetting
    }

    /// <summary>
    /// Calculates the Movementspeed
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <param name="MovementSpeedPercentage">all raw MovementSpeed modifiers</param>
    /// <returns>calculated Movementspeed</returns>
    public static double CalculateMovementSpeed(BLREditItem receiver, double MovementSpeedPercentage)
    {
        if (receiver is null || receiver.WeaponStats is null) return 0;
        double allMovementSpeed = MovementSpeedPercentage / 100.0; // Weapon modifier is uncapped, but coalesced mods to the pawn are capped
        double move_alpha = Math.Abs(allMovementSpeed);
        double move_modifier;
        // Is casted to an int in scripts, so I'm flooring it
        if (allMovementSpeed > 0)
        {
            move_modifier = Math.Floor(Lerp(receiver.WeaponStats.ModificationRangeMoveSpeed.Z, receiver.WeaponStats.ModificationRangeMoveSpeed.Y, move_alpha));
        }
        else
        {
            move_modifier = Math.Floor(Lerp(receiver.WeaponStats.ModificationRangeMoveSpeed.Z, receiver.WeaponStats.ModificationRangeMoveSpeed.X, move_alpha));
        }
        return move_modifier;
        //return (765 + move_modifier * 0.9) / 100.0f; // Apparently percent of movement from gear is applied to weapons, and not percent of movement from weapons
    }

    /// <summary>
    /// Calculates the combined armor and weapon Movementspeed
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <param name="Loadout">Loadout</param>
    /// <param name="WeapSpeedPercentage">all raw weapon MovementSpeed modifiers</param>
    /// <returns>calculated Movementspeed</returns>
    public static double CalculatePawnMovementSpeed(BLREditItem receiver, BLREditLoadout? Loadout, double WeapSpeedPercentage)
    {
        if (Loadout is null || receiver is null || receiver.WeaponStats is null) return 0;
        double weap_alpha = Math.Abs(WeapSpeedPercentage) / 100.0;
        double weap_modifier;
        if (WeapSpeedPercentage > 0)
        {
            weap_modifier = Lerp(receiver.WeaponStats.ModificationRangeMoveSpeed.Z, receiver.WeaponStats.ModificationRangeMoveSpeed.Y, weap_alpha);
        }
        else
        {
            weap_modifier = Lerp(receiver.WeaponStats.ModificationRangeMoveSpeed.Z, receiver.WeaponStats.ModificationRangeMoveSpeed.X, weap_alpha);
        }

        double combined = Math.Floor((Loadout.RawMoveSpeed * 0.9) + (weap_modifier * 0.667));
        double pawnalpha = Math.Min(Math.Abs(combined), 100.0) / 100.0;
        double pawnspeed;
        if (combined > 0)
        {
            pawnspeed = Lerp(765.0, 900.0, pawnalpha);
        }
        else
        {
            pawnspeed = Lerp(765.0, 630.0, pawnalpha);
        }

        return pawnspeed / 100.0;
    }

    /// <summary>
    /// Calculates the Spread values for AimDownSight, Hip and Move firing.
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <param name="AccuracyPercentage">all raw Accuracy modifiers</param>
    /// <param name="BarrelStockMovementSpeed">Barrel and Stock raw MovmentSpeed modifiers</param>
    /// <returns></returns>
    public static (double ZoomSpread, double HipSpread, double MovmentSpread) CalculateSpread(BLREditItem receiver, double AccuracyPercentage, double BarrelStockMovementSpeed, BLREditItem? Magazine, BLREditItem? Ammo, double DamagePercentage)
    {
        if (receiver is null || receiver.WeaponStats is null) return (0, 0, 0);
        double allMoveSpeed = Percentage(BarrelStockMovementSpeed);
        double allAccuracy = Percentage(AccuracyPercentage);
        double accuracyBaseModifier;
        double accuracyTABaseModifier;
        double alpha = Math.Abs(allAccuracy);
        double Rad2Deg = 180.0 / Math.PI; // The amount of these was annoying me, also converted only at the very end now
        if (allAccuracy > 0)
        {
            accuracyBaseModifier = Lerp(receiver.WeaponStats.ModificationRangeBaseSpread.Z, receiver.WeaponStats.ModificationRangeBaseSpread.Y, alpha);
            accuracyTABaseModifier = Lerp(receiver.WeaponStats.ModificationRangeTABaseSpread.Z, receiver.WeaponStats.ModificationRangeTABaseSpread.Y, alpha);
        }
        else
        {
            accuracyBaseModifier = Lerp(receiver.WeaponStats.ModificationRangeBaseSpread.Z, receiver.WeaponStats.ModificationRangeBaseSpread.X, alpha);
            accuracyTABaseModifier = Lerp(receiver.WeaponStats.ModificationRangeTABaseSpread.Z, receiver.WeaponStats.ModificationRangeTABaseSpread.X, alpha);
        }

        double hip = accuracyBaseModifier;
        double aim = accuracyBaseModifier * receiver.WeaponStats.ZoomSpreadMultiplier;
        if (receiver.WeaponStats.UseTABaseSpread)
        {
            aim = accuracyTABaseModifier;
        }

        double damagealpha = Math.Abs(Percentage(DamagePercentage));
        if (receiver.UID == 40007)
        {
            if (DamagePercentage > 0)
            {
                // Only the BAR can actually use this
                // Seemingly replaces TABaseSpread value in SingleActionBase
                // Currently unused in 3.02 (but still exists)
                // I do not currently know how this plays with MovementSpreadConstant or MovementSpreadMultiplier, though luckily we don't need to know yet
                aim = Lerp(receiver.WeaponStats.ModificationRangeDamageAccuracyRange.Z, receiver.WeaponStats.ModificationRangeDamageAccuracyRange.X, damagealpha);
            }
            else
            {
                aim = Lerp(receiver.WeaponStats.ModificationRangeDamageAccuracyRange.Z, receiver.WeaponStats.ModificationRangeDamageAccuracyRange.Y, damagealpha);
            }
        }

        // The gun's actual move spread, confirmed by ingame testing
        // Customization menu's stat was wrong and improperly using the now known to be unused Weight value
        // All of my prior hacks were to mimic the wrong values, creating a giant mess of wrong
        double speed_alpha = Math.Min(Math.Abs(BarrelStockMovementSpeed) / 80, 1.0);
        double funny_number = 0.425; // Magic number that Zombie pulled out of their ass
        double speed_multiplier;
        if (BarrelStockMovementSpeed > 0)
        {
            speed_multiplier = Lerp(1.0, 0.5, speed_alpha);
        }
        else
        {
            speed_multiplier = Lerp(1.0, 2.0, speed_alpha);
        }
        double move_mult = accuracyBaseModifier * (1.0 + (receiver.WeaponStats.MovementSpreadMultiplier - 1.0) * funny_number * speed_multiplier);
        double move_const = Math.Max(receiver.WeaponStats.MovementSpreadConstant, 0.0) * funny_number * speed_multiplier;
        double move = (move_mult + move_const);

        // Add FragmentSpread to result for guns that use it
        // The normal spread values only affect the fragment scatter placement, the whole scatter is treated as a single shot that the spread moves
        if (receiver.UID == 40015) // BLP, only the scattershot uses FragmentSpread
        {
            if ((Magazine?.UID == 44177) || (Ammo?.UID == 90010)) {
                aim += 0.2;
                hip += 0.2;
                move += 0.2;
            }
        } else if (receiver.UID == 40005) // Shotgun
        {
            aim += 0.1;
            hip += 0.1;
            move += 0.1;
        } else if (receiver.UID == 40016) // Sark
        {
            aim += 0.05;
            hip += 0.05;
            move += 0.05;
        }

        aim *= Rad2Deg;
        hip *= Rad2Deg;
        move *= Rad2Deg;

        // Average spread over multiple shots to account for random center weight multiplier
        // (Currently unused)
        // Can probably be heavily simplified
        double[] averageSpread = [0, 0, 0];
        double magsize = Math.Min(receiver.WeaponStats.MagSize, 15.0);
        if (magsize <= 1)
        {
            magsize = receiver.WeaponStats.InitialMagazines + 1.0;
        }
        if (magsize > 0)
        {
            double averageShotCount = Math.Max(magsize, 5.0);
            for (int shot = 1; shot <= averageShotCount; shot++)
            {
                if (shot > averageShotCount - averageShotCount * receiver.WeaponStats.SpreadCenterWeight)
                {
                    averageSpread[0] += aim * receiver.WeaponStats.SpreadCenter;
                    averageSpread[1] += hip * receiver.WeaponStats.SpreadCenter;
                    averageSpread[2] += move * receiver.WeaponStats.SpreadCenter;
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
    /// Calculates the ScopeInTime for Receiver and Scope
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <param name="Scope">Scope</param>
    /// <param name="BarrelStockMovementSpeed">Barrel and Stock raw MovementSpeed modifier</param>
    /// <param name="RawScopeInTime">all raw ScopeInTime modifiers</param>
    /// <returns>The FOV scope transition time</returns>
    public static double CalculateScopeInTime(BLREditItem receiver, BLREditItem? Scope, double BarrelStockMovementSpeed, double RawScopeInTime, bool isScopeEnabled)
    {
        double allMovementSpeed = Clamp(BarrelStockMovementSpeed / 80.0, -1.0, 1.0);
        double TTTA_alpha = Math.Abs(allMovementSpeed);

        // hardcoded wall of lerps removed, all is right in the world now
        double TightAimTime;
        double scopeInTime;
        double speedMod;
        if (allMovementSpeed > 0)
        {
            speedMod = Lerp(1.0, 0.66, TTTA_alpha);
            TightAimTime = Lerp(0.225, 0.15, TTTA_alpha);
        }
        else
        {
            speedMod = Lerp(1.0, 1.34, TTTA_alpha);
            TightAimTime = Lerp(0.225, 0.30, TTTA_alpha);
        }

        if (isScopeEnabled)
        {
            scopeInTime = RawScopeInTime - (0.11 * speedMod);
        }
        else
        {
            scopeInTime = 0.0;
        }

        if ((receiver?.WeaponStats?.TightAimTime ?? 0) > 0)
        {
            return receiver?.WeaponStats?.TightAimTime ?? 0;
        }
        else
        {
            return TightAimTime + scopeInTime;
        }
    }

    /// <summary>
    /// Calculates the three ranges
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <param name="RangePercentage">all Range modifiers</param>
    /// <returns>1:Ideal Range, 2:Max Range, 3:Trace Range</returns>
    public static (double IdealRange, double MaxRange, double TracerRange) CalculateRange(BLREditItem receiver, double RangePercentage)
    {
        double allRange = Percentage(RangePercentage);
        double idealRange;
        double maxRange;
        double alpha = Math.Abs(allRange);
        if (allRange > 0)
        {
            idealRange = (int)Lerp(receiver?.WeaponStats?.ModificationRangeIdealDistance.Z ?? 0, receiver?.WeaponStats?.ModificationRangeIdealDistance.Y ?? 0, alpha);
            maxRange = Lerp(receiver?.WeaponStats?.ModificationRangeMaxDistance.Z ?? 0, receiver?.WeaponStats?.ModificationRangeMaxDistance.Y ?? 0, alpha);
        }
        else
        {
            idealRange = (int)Lerp(receiver?.WeaponStats?.ModificationRangeIdealDistance.Z ?? 0, receiver?.WeaponStats?.ModificationRangeIdealDistance.X ?? 0, alpha);
            maxRange = Lerp(receiver?.WeaponStats?.ModificationRangeMaxDistance.Z ?? 0, receiver?.WeaponStats?.ModificationRangeMaxDistance.X ?? 0, alpha);
        }
        double traceRange = Math.Max(maxRange,(receiver?.WeaponStats?.MaxTraceDistance ?? 0)); // NOTE: trace distance is apparently the max of tracerange and maxrange, so maxrange gets priority

        return (IdealRange: idealRange / 100.0,
                MaxRange: maxRange / 100.0,
                TracerRange: traceRange / 100.0);
    }

    /// <summary>
    /// Calculates the range for weapon sorting
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <returns>Sorted range</returns>
    public static double CalculateSortedRange(BLREditItem receiver)
    {
        double idealRange = receiver?.WeaponStats?.ModificationRangeIdealDistance.Z ?? 0;
        double maxRange = receiver?.WeaponStats?.ModificationRangeMaxDistance.Z ?? 0;

        double baseDamage = receiver?.WeaponStats?.ModificationRangeDamage.Z ?? 0;
        double minDamage = baseDamage * (receiver?.WeaponStats?.MaxRangeDamageMultiplier ?? 0.1);

        // Might eventually try something more complicated, but this will do for now. Varying damage multipliers makes any serious range comparison a bit tricky
        // Maybe we can additionally highlight the max range damage (separately from the main damage number on left) when sorting range to show that it's related to range
        // Adding a small amount of the min range to account for cases of identical max ranges between receivers
        // Additionally adding a small amount of min damage at range for cases of identical min and max ranges
        double sortedRange = (idealRange / 1000.0) + Math.Max(idealRange, maxRange);
        sortedRange += minDamage / 1000.0;

        if (receiver?.UID == 40024) // Bow; projectile not affected by range and can travel for a long time
        {
            sortedRange = 999999.0;
        } 
        else if (receiver?.UID == 40015) // BLP
        {
            sortedRange = 99999.0;
        }

            return sortedRange;
    }

    /// <summary>
    /// Calculates a clamped recoil offset based on accumulated recoil, expects newrecoil and vectors to be in URotation units
    /// </summary>
    /// <param name="currentrecoil">Current accumulated recoil before offset</param>
    /// <param name="newrecoil">Projected new recoil offset</param>
    /// <param name="maxvector">Vector for maximum recoil (starting point)</param>
    /// <param name="minvector">Vector for minimum recoil (ending point)</param>
    /// <param name="minmult">Minimum recoil multiplier at minrecoil vector</param>
    /// <param name="accuminfluence">Influence of accumulated recoil</param>
    /// <returns>A new modified offset, scaled between min/max recoil towards minrecoilmultiplier</returns>
    public static double ClampRecoilURot(double currentrecoil, double newrecoil, double maxvector, double minvector, double minmult, double accuminfluence)
    {
        if (minmult == 1.0)
        {
            return newrecoil;
        }

        double maxRatio = 1.0;
        double baseDiff = 0.0;

        double modifiedCurRecoil = currentrecoil * accuminfluence;
        double newRecoilOffset = newrecoil;
        double projectedOffset = modifiedCurRecoil + newRecoilOffset;
        double offsetDiff = Math.Abs(projectedOffset) - maxvector;

        if (offsetDiff > 0 && newRecoilOffset > 0)
        {
            maxRatio = offsetDiff / (minvector - maxvector);
            maxRatio = Lerp2(1.0, minmult, Math.Min(1.0, maxRatio));
            if (Math.Abs(modifiedCurRecoil) < maxvector)
            {
                baseDiff = maxvector - Math.Abs(modifiedCurRecoil);
                baseDiff *= Clamp(projectedOffset, -1.0, 1.0);
            }
            else
            {
                offsetDiff += (maxvector - Math.Abs(modifiedCurRecoil));
                baseDiff = 0.0;
            }
            offsetDiff *= Clamp(newRecoilOffset, -1.0, 1.0);
            newRecoilOffset = Math.Floor(baseDiff + (offsetDiff * maxRatio));
        }

        return Math.Floor(newRecoilOffset);
    }

    /// <summary>
    /// Calculates the Hip and AimDownSight Recoil
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <param name="RecoilPercentage">all Recoil modifiers</param>
    /// <param name="isTightAim">Whether scoped in or not</param>
    /// <returns>Recoil</returns>
    public static double CalculateRecoil(BLREditItem receiver, double RecoilPercentage, bool isTightAim)
    {
        double Rad2Deg = 180.0 / Math.PI;
        double Rad2URot = 10430.3783505;

        double allRecoil = Percentage(RecoilPercentage);
        double alpha = Math.Abs(allRecoil);
        double recoilModifier;
        if (allRecoil > 0)
        {
            recoilModifier = Lerp(receiver?.WeaponStats?.ModificationRangeRecoil.Z ?? 0, receiver?.WeaponStats?.ModificationRangeRecoil.Y ?? 0, alpha);
        }
        else
        {
            recoilModifier = Lerp(receiver?.WeaponStats?.ModificationRangeRecoil.Z ?? 0, receiver?.WeaponStats?.ModificationRangeRecoil.X ?? 0, alpha);
        }

        if ((receiver?.WeaponStats?.MagSize ?? 0) > 0)
        {
            double randX = 0.25;                                            // 0.25 in place of X's Rand(0,1)-0.5
            double randY = Math.Abs(Math.Sqrt(0.5 - (randX*randX)) * 0.25); // 0.25 in place of Y's Rand(0,1)-0.5

            double multiplier = 1.0;
            if (isTightAim)
            {
                multiplier = (receiver?.WeaponStats?.RecoilZoomMultiplier ?? 0.5) * 0.8; // NOTE: the 0.8 zoom multiply did not exist in preparity
            }

            double accumExponent = receiver?.WeaponStats?.RecoilAccumulation ?? 0;
            if (accumExponent > 1.0)
            {
                accumExponent = ((accumExponent - 1.0) * (receiver?.WeaponStats?.RecoilAccumulationMultiplier ?? 0)) + 1.0;
            }

            double averageShotCount = Math.Min(receiver?.WeaponStats?.MagSize ?? 0, 15.0);
            Vector3 averageRecoil = new(0, 0, 0);

            for (int shot = 1; shot <= averageShotCount; shot++)
            {
                Vector3 newRecoil = new(0, 0, 0)
                {
                    // in the recoil, recoil vector is actually a multiplier on a random X and Y value in the -0.5/0.5 and roughly 0.0/0.3535 range respectively
                    // NOTE: in preparity, the order of operations for the raw offset go (RandXY + RecoilVectorOffset[]) * RecoilSizeVector
                    // NOTE: in parity, the order of operations for the raw offset go (RandXY * RecoilSizeVector) + RecoilVectorOffset[]
                    X = (receiver?.WeaponStats?.RecoilVector.X ?? 0) * (receiver?.WeaponStats?.RecoilVectorMultiplier.X ?? 0) * (float)randX,
                    Y = (receiver?.WeaponStats?.RecoilVector.Y ?? 0) * (receiver?.WeaponStats?.RecoilVectorMultiplier.Y ?? 0) * (float)randY
                };

                // TODO: RecoilVectorOffset[]

                double adjustedShot = shot;
                if (receiver?.WeaponStats?.Burst > 0)
                {
                    adjustedShot = 1.0 + (Math.Floor(adjustedShot) / Math.Max(receiver?.WeaponStats?.Burst ?? 0,1.0));
                }
                double previousMultiplier = (receiver?.WeaponStats?.RecoilSize ?? 0) * Math.Pow(adjustedShot - 1.0, accumExponent);
                double currentMultiplier = (receiver?.WeaponStats?.RecoilSize ?? 0) * Math.Pow(adjustedShot + 0.0, accumExponent);
                double deltaRecoil = currentMultiplier - previousMultiplier;
                double totalRecoil = multiplier * deltaRecoil;
                newRecoil *= (float)totalRecoil * (float)recoilModifier;

                double minRecoilMult = 0.9 + (receiver?.WeaponStats?.MinRecoilMultiplier ?? 0.1); // adding 0.9 to disable it for now
                averageRecoil.Y += (float)ClampRecoilURot(averageRecoil.Y, newRecoil.Y * Rad2URot, (receiver?.WeaponStats?.MaxRecoilVector.Y ?? 0.04f) * Rad2URot, (receiver?.WeaponStats?.MinRecoilVector.Y ?? 0.16f) * Rad2URot, minRecoilMult, 1.0);
                averageRecoil.X += (float)ClampRecoilURot(averageRecoil.X, newRecoil.X * Rad2URot, (receiver?.WeaponStats?.MaxRecoilVector.X ?? 0.025f) * Rad2URot, (receiver?.WeaponStats?.MinRecoilVector.X ?? 0.1f) * Rad2URot, minRecoilMult, 1.0);
            }
            averageRecoil /= (float)Rad2URot; // convert back to radians from URotation

            if (averageShotCount > 0)
            {
                averageRecoil /= (float)averageShotCount;
            }
            if ((receiver?.WeaponStats?.ROF ?? 0) > 0 && (receiver?.WeaponStats?.ApplyTime ?? 0) > 60 / (receiver?.WeaponStats?.ROF ?? 600))
            {
                averageRecoil *= (float)(60 / ((receiver?.WeaponStats?.ROF ?? 600) * (receiver?.WeaponStats?.ApplyTime ?? 0)));
            }
            double recoil = averageRecoil.Length();
            recoil *= Rad2Deg;

            return recoil;
        }
        else
        {
            double recoil = (receiver?.WeaponStats?.RecoilSize ?? 0);
            recoil *= Rad2Deg;
            return recoil;
        }
    }

    /// <summary>
    /// Calculates the IdealRange and MaxRange Damage
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <param name="DamagePercentage">all raw Damage modifiers</param>
    /// <returns>1:Damage 2:Damage at max range</returns>
    public static (double DamageIdeal, double DamageMax) CalculateDamage(BLREditItem receiver, double DamagePercentage)
    {
        double allDamage = Percentage(DamagePercentage);
        double damageModifier;
        double alpha = Math.Abs(allDamage);
        if (allDamage > 0)
        {
            damageModifier = Lerp(receiver?.WeaponStats?.ModificationRangeDamage.Z ?? 0, receiver?.WeaponStats?.ModificationRangeDamage.Y ?? 0, alpha);
        }
        else
        {
            damageModifier = Lerp(receiver?.WeaponStats?.ModificationRangeDamage.Z ?? 0, receiver?.WeaponStats?.ModificationRangeDamage.X ?? 0, alpha);
        }

        return (DamageIdeal: damageModifier,
                DamageMax: damageModifier * (receiver?.WeaponStats?.MaxRangeDamageMultiplier ?? 0.1d));
    }

    /// <summary>
    /// Calculates the damage for weapon sorting
    /// </summary>
    /// <param name="receiver">Receiver</param>
    /// <returns>Sorted damage</returns>
    public static double CalculateSortedDamage(BLREditItem receiver)
    {
        double baseDamage = receiver?.WeaponStats?.ModificationRangeDamage.Z ?? 0;
        double minDamage = baseDamage * (receiver?.WeaponStats?.MaxRangeDamageMultiplier ?? 0.1d);

        // Adding a small amount of the min damage at range to account for possible future cases of identical damages between receivers but differing range damage multipliers
        double sortedDamage = (Math.Round(minDamage) / 100.0) + baseDamage;

        return sortedDamage;
    }

    public static BLREditItem? CompareItemRating(BLREditItem? item1, BLREditItem? item2)
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

    public static string CompareItemDescriptor1(BLREditItem? itembarrel, BLREditItem? itemmag)
    {
        return CompareItemRating(itemmag, itembarrel)?.DescriptorName ?? "Standard";
    }
    public static string CompareItemDescriptor2(BLREditItem? itemstock, BLREditItem? itemmuzzle, BLREditItem? itemscope)
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
        return Clamp(input / 100.0, -1.0, 1.0);
    }
    public static double Lerp(double start, double target, double time)
    {
        return start * (1.0 - time) + target * time;
    }
    public static double Lerp2(double start, double target, double alpha)
    {
        return start + (target - start) * Clamp(alpha,0.0,1.0);
    }
    public static double Clamp(double input, double min, double max)
    {
        return Math.Min(Math.Max(input, min), max);
    }

    public static int Clamp(int input, int min, int max)
    {
        return Math.Min(Math.Max(input, min), max);
    }
    #endregion Calulations

    public void SetWeapon(IBLRWeapon? weapon, bool registerReadBackEvent = false)
    {
        if (_weapon is not null) { _weapon.WasWrittenTo -= ReadCallback; }
        _weapon = weapon;
        if (registerReadBackEvent && _weapon is not null) { _weapon.WasWrittenTo += ReadCallback; }
    }

    private void ReadCallback(object sender, EventArgs e)
    {
        if (sender != this)
        { Read(); }
    }

    public void Read()
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadWeapon)) return;
        _weapon?.Read(this);
    }

    public void Write() 
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteWeapon)) return;
        _weapon?.Write(this);
    }

    static readonly Random rng = new();
    public void Randomize()
    {
        var FilteredReceivers = ImportSystem.GetItemArrayOfType(IsPrimary ? ImportSystem.PRIMARY_CATEGORY : ImportSystem.SECONDARY_CATEGORY).Where(ItemFilters.PartialFilter).ToArray();
        
        BLREditItem receiver = FilteredReceivers[rng.Next(0, FilteredReceivers.Length)];

        var FilteredBarrels = ImportSystem.GetItemArrayOfType(ImportSystem.BARRELS_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredScopes = ImportSystem.GetItemArrayOfType(ImportSystem.SCOPES_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredMagazines = ImportSystem.GetItemArrayOfType(ImportSystem.MAGAZINES_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredMuzzles = ImportSystem.GetItemArrayOfType(ImportSystem.MUZZELS_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredStocks = ImportSystem.GetItemArrayOfType(ImportSystem.STOCKS_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredCamos = ImportSystem.GetItemArrayOfType(ImportSystem.CAMOS_WEAPONS_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredHangers = ImportSystem.GetItemArrayOfType(ImportSystem.HANGERS_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredAmmo = ImportSystem.GetItemArrayOfType(ImportSystem.AMMO_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredGrips = ImportSystem.GetItemArrayOfType(ImportSystem.GRIPS_CATEGORY).Where(o=> o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();
        var FilteredSkins = ImportSystem.GetItemArrayOfType(ImportSystem.PRIMARY_SKIN_CATEGORY).Where(o => o.IsValidFor(receiver, Loadout?.IsAdvanced.Is ?? false)).ToArray();

        BLREditItem? barrel = null;
        if (FilteredBarrels.Length > 0)
        {
            barrel = FilteredBarrels[rng.Next(0, FilteredBarrels.Length)];
        }
        BLREditItem? scope = null;
        if (FilteredScopes.Length > 0)
        {
            scope = FilteredScopes[rng.Next(0, FilteredScopes.Length)];
        }
        BLREditItem? magazine = null;
        if (FilteredMagazines.Length > 0)
        {
            magazine = FilteredMagazines[rng.Next(0, FilteredMagazines.Length)];
        }
        BLREditItem? stock = null;
        if (FilteredStocks.Length > 0)
        {
            stock = FilteredStocks[rng.Next(0, FilteredStocks.Length)];
        }
        BLREditItem? muzzle = null;
        if (FilteredMuzzles.Length > 0)
        {
            muzzle = FilteredMuzzles[rng.Next(0, FilteredMuzzles.Length)];
        }
        BLREditItem? hanger = null;
        if (FilteredHangers.Length > 0 && IsPrimary)
        {
            hanger = FilteredHangers[rng.Next(0, FilteredHangers.Length)];
        }
        BLREditItem? camo = null;
        if (FilteredCamos.Length > 0)
        {
            camo = FilteredCamos[rng.Next(0, FilteredCamos.Length)];
        }
        BLREditItem? ammo = null;
        if (FilteredAmmo.Length > 0)
        {
            if ((magazine is not null && !magazine.ItemNameContains("Toxic", "Incendiary", "Electro", "Explosive", "Magnum")) || magazine is null)
            {
                ammo = FilteredAmmo[rng.Next(0, FilteredAmmo.Length)];
            }
        }
        BLREditItem? grip = null;
        if (FilteredGrips.Length > 0)
        { 
            grip = FilteredGrips[rng.Next(0, FilteredGrips.Length)];
        }
        BLREditItem? skin = null;
        if (FilteredSkins.Length > 0 && IsPrimary)
        {
            // Temporarily disabling weapon skin randomization, skins are currently broken and inconvenient to manually remove on each randomization (especially with people not knowing how to begin with)
            // Skins kinda defeat the fun of randomization anyway and have limited choices on most guns so I'm open to it staying like this for now

            //skin = FilteredSkins[rng.Next(0, FilteredSkins.Length)];
        }
        //TODO: Use the text filter to theme Randomization where possible(when the list is not empty after filtering).

        if (!AllowStock(receiver, barrel, stock)) 
        { stock = null; }

        UndoRedoSystem.DoValueChange(receiver, this.GetType().GetProperty(nameof(Receiver)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(barrel, this.GetType().GetProperty(nameof(Barrel)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(stock, this.GetType().GetProperty(nameof(Stock)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(muzzle, this.GetType().GetProperty(nameof(Muzzle)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(magazine, this.GetType().GetProperty(nameof(Magazine)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(camo, this.GetType().GetProperty(nameof(Camo)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(grip, this.GetType().GetProperty(nameof(Grip)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(skin, this.GetType().GetProperty(nameof(Skin)), this, BlockEvents.AllExceptUpdate);
        if (ammo is not null) UndoRedoSystem.DoValueChange(ammo, this.GetType().GetProperty(nameof(Ammo)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(hanger, this.GetType().GetProperty(nameof(Tag)), this, BlockEvents.AllExceptUpdate);
        UndoRedoSystem.DoValueChange(scope, this.GetType().GetProperty(nameof(Scope)), this);
        UndoRedoSystem.EndUndoRecord();
    }

    /// <summary>
    /// Check if the weapon is vanilla conform
    /// </summary>
    /// <returns>true if its valid and flase if its invalid</returns>
    public bool ValidateWeapon(ref string message)
    {
        bool valid = true;

        string attachment = "";

        if (Receiver is null) { message += $"\n\t\tNo Receiver Equipped"; return false; }
        if (!(Receiver?.IsValidFor(null) ?? false)) { valid = false; attachment += $"\n\t\tReceiver is invalid"; }
        if (!BLREditItem.IsValidFor(Barrel, Receiver)) { valid = false; attachment += $"\n\t\tBarrel is invalid"; }
        if (!BLREditItem.IsValidFor(Muzzle, Receiver)) { valid = false; attachment += $"\n\t\tMuzzle is invalid"; }
        if (!BLREditItem.IsValidFor(Grip, Receiver)) { valid = false; attachment += $"\n\t\tGrip is invalid"; }
        if (!BLREditItem.IsValidFor(Camo, Receiver)) { valid = false; attachment += $"\n\t\tCamo is invalid"; }
        if (!BLREditItem.IsValidFor(Magazine, Receiver)) { valid = false; attachment += $"\n\t\tMagazine is invalid"; }
        if (!BLREditItem.IsValidFor(Ammo, Receiver)) { valid = false; attachment += $"\n\t\tAmmo is invalid"; }
        if (!BLREditItem.IsValidFor(Tag, Receiver)) { valid = false; attachment += $"\n\t\tTag is invalid"; }
        if (!BLREditItem.IsValidFor(Stock, Receiver)) { valid = false; attachment += $"\n\t\tStock is invalid"; }
        if (!BLREditItem.IsValidFor(Scope, Receiver)) { valid = false; attachment += $"\n\t\tScope is invalid"; }
        if (!BLREditItem.IsValidFor(Skin, Receiver)) { valid = false; attachment += $"\n\t\tSkin is invalid"; }

        if (valid)
            message += " ✔️" + attachment;
        else
            message += " ❌" + attachment;

        return valid;
    }

    public WeaponErrorReport GenerateReport()
    {
        return new(
            ItemCheck(Receiver, null),
            ItemCheck(Barrel, Receiver),
            ItemCheck(Muzzle, Receiver),
            ItemCheck(Grip, Receiver),
            ItemCheck(Camo, Receiver),
            ItemCheck(Magazine, Receiver),
            ItemCheck(Ammo, Receiver),
            ItemCheck(Tag, Receiver),
            ItemCheck(Stock, Receiver),
            ItemCheck(Scope, Receiver),
            ItemCheck(Skin, Receiver)
            );
    }
}
public readonly struct WeaponErrorReport(ItemReport receiver, ItemReport barrel, ItemReport muzzle, ItemReport grip, ItemReport camo, ItemReport magazine, ItemReport ammo, ItemReport tag, ItemReport stock, ItemReport scope, ItemReport skin) : IEquatable<WeaponErrorReport>
{
    public readonly bool IsValid { get { return AllTruth(ItemReport.Valid, ReceiverReport, BarrelReport, MuzzleReport, GripReport, CamoReport, MagazineReport, AmmoReport, TagReport, StockReport, ScopeReport, SkinReport); } }
    public ItemReport ReceiverReport { get; } = receiver;
    public ItemReport BarrelReport { get; } = barrel;
    public ItemReport MuzzleReport { get; } = muzzle;
    public ItemReport GripReport { get; } = grip;
    public ItemReport CamoReport { get; } = camo;
    public ItemReport MagazineReport { get; } = magazine;
    public ItemReport AmmoReport { get; } = ammo;
    public ItemReport TagReport { get; } = tag;
    public ItemReport StockReport { get; } = stock;
    public ItemReport ScopeReport { get; } = scope;
    public ItemReport SkinReport { get; } = skin;

    public override bool Equals(object obj)
    {
        if (obj is WeaponErrorReport r)
        {
            return Equals(r);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (ReceiverReport, BarrelReport, MuzzleReport, GripReport, CamoReport, MagazineReport, AmmoReport, TagReport, StockReport, ScopeReport, SkinReport).GetHashCode();
    }

    public static bool operator ==(WeaponErrorReport left, WeaponErrorReport right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(WeaponErrorReport left, WeaponErrorReport right)
    {
        return !(left == right);
    }

    public bool Equals(WeaponErrorReport other)
    {
        return
            ReceiverReport.Equals(other.ReceiverReport) &&
            BarrelReport.Equals(other.BarrelReport) &&
            MuzzleReport.Equals(other.MuzzleReport) &&
            GripReport.Equals(other.GripReport) &&
            CamoReport.Equals(other.CamoReport) &&
            MagazineReport.Equals(other.MagazineReport) &&
            AmmoReport.Equals(other.AmmoReport) &&
            TagReport.Equals(other.TagReport) &&
            StockReport.Equals(other.StockReport) &&
            ScopeReport.Equals(other.ScopeReport) &&
            SkinReport.Equals(other.SkinReport);
    }
}