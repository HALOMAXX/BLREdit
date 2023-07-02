using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Utils;

using PropertyChanged;

using System.Collections.ObjectModel;

namespace BLREdit.Core.Models.BLR.Loadout;

public sealed class BLRWeapon : ModelBase
{
    private BLRItem? _reciever;
    private BLRItem? _barrel;
    private BLRItem? _muzzle;
    private BLRItem? _stock;
    private BLRItem? _scope;
    private BLRItem? _magazine;
    private BLRItem? _ammo;
    private BLRItem? _grip;
    private BLRItem? _hanger;
    private BLRItem? _camo;
    private BLRItem? _skin;

    public RangeObservableCollection<string> MetaData { get; } = new(){ "v302" };

    [DoNotNotify] public BLRItem? Reciever { get { return _reciever; } set { if (IsValidReciever(value)) { _reciever = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Barrel { get { return _barrel; } set { if (IsValidBarrel(value)) { _barrel = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Muzzle { get { return _muzzle; } set { if (IsValidMuzzle(value)) { _muzzle = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Stock { get { return _stock; } set { if (IsValidStock(value)) { _stock = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Scope { get { return _scope; } set { if (IsValidScope(value)) { _scope = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Magazine { get { return _magazine; } set { if (IsValidMagazine(value)) { _magazine = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Ammo { get { return _ammo; } set { if (IsValidAmmo(value)) { _ammo = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Grip { get { return _grip; } set { if (IsValidGrip(value)) { _grip = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Hanger { get { return _hanger; } set { if (IsValidHanger(value)) { _hanger = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? WeaponCamo { get { return _camo; } set { if (IsValidCamo(value)) { _camo = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Skin { get { return _skin; } set { if (IsValidSkin(value)) { _skin = value; OnPropertyChanged(); } } }
    

    #region ItemValidation
    public static bool IsValidReciever(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Primaries" || item.CategoryName is "Secondaries") return true;
        return false;
    }

    public static bool IsValidBarrel(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Barrels") return true;
        return false;
    }

    public static bool IsValidMuzzle(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Muzzles") return true;
        return false;
    }

    public static bool IsValidStock(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Stocks") return true;
        return false;
    }

    public static bool IsValidGrip(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Grips") return true;
        return false;
    }

    public static bool IsValidScope(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Scopes") return true;
        return false;
    }

    public static bool IsValidMagazine(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Magazines") return true;
        return false;
    }

    public static bool IsValidAmmo(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Ammos") return true;
        return false;
    }

    public static bool IsValidHanger(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Hangers") return true;
        return false;
    }

    public static bool IsValidCamo(BLRItem? item)
    {
        if (item is null || item.CategoryName is "WeaponCamos") return true;
        return false;
    }

    public static bool IsValidSkin(BLRItem? item)
    {
        if (item is null || item.CategoryName is "PrimarySkin" || item.CategoryName is "SecondarySkin") return true;
        return false;
    }
    #endregion ItemValidation
}

public sealed class LMIDWeapon
{
    public string MetaData { get; set; } = "v302";
    public int Reciever { get; set; }
    public int Barrel { get; set; }
    public int Muzzle { get; set; }
    public int Stock { get; set; }
    public int Scope { get; set; }
    public int Magazine { get; set; }
    public int Ammo { get; set; }
    public int Grip { get; set; }
    public int Hanger { get; set; }
    public int CamoIndex { get; set; }
    public int Skin { get; set; }

    public static void WriteToBLRWeapon(LMIDWeapon lm, BLRWeapon blr)
    {
        if (blr is null || lm is null) throw new ArgumentNullException($"LMIDWeapon or BLRWeapon were null LM:{lm is null} BLR:{blr is null}"); //TODO Replace Exception
        blr.MetaData.Clear();
        blr.MetaData.AddRange(lm.MetaData.Split("|"));
        if (blr.MetaData.Count < 2) throw new ArgumentOutOfRangeException($"LMIDWeapon MetaData was too small {blr.MetaData.Count}/2"); //TODO Replace Exception
        if (BLRItemList.ItemLists.TryGetValue(blr.MetaData[0], out var list))
        {
            blr.Reciever = blr.MetaData[1] switch
            {
                "Primary" => list.GetItemByLoadoutManagerIDAndCategoryID(19, lm.Reciever),
                _ => list.GetItemByLoadoutManagerIDAndCategoryID(20, lm.Reciever),
            };
            blr.Barrel = list.GetItemByLoadoutManagerIDAndCategoryID(10, lm.Barrel);
            blr.Scope = list.GetItemByLoadoutManagerIDAndCategoryID(14, lm.Scope);
            blr.Grip = list.GetItemByLoadoutManagerIDAndCategoryID(11, lm.Grip);
            blr.Stock = list.GetItemByLoadoutManagerIDAndCategoryID(15, lm.Stock);
            blr.Magazine = list.GetItemByLoadoutManagerIDAndCategoryID(12, lm.Magazine);
            blr.Ammo = list.GetItemByLoadoutManagerIDAndCategoryID(9, lm.Ammo);
            blr.Muzzle = list.GetItemByLoadoutManagerIDAndCategoryID(13, lm.Muzzle);
            blr.Skin = list.GetItemByLoadoutManagerIDAndCategoryID(18, lm.Skin);
            blr.WeaponCamo = list.GetItemByLoadoutManagerIDAndCategoryID(17, lm.CamoIndex);
            blr.Hanger = list.GetItemByLoadoutManagerIDAndCategoryID(4, lm.Hanger);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Failed to get ItemList for ({lm.MetaData})[{blr.MetaData[0]}]"); //TODO Replace Exception
        }
    }

    public static void ReadFromBLRWeapon(LMIDWeapon lm, BLRWeapon blr)
    {
        if (blr is null || lm is null) throw new ArgumentNullException($"LMIDWeapon or BLRWeapon were null LM:{lm is null} BLR:{blr is null}"); //TODO Replace Exception
        lm.MetaData = string.Join("|",blr.MetaData);
        lm.Reciever = blr.Reciever?.LoadoutManagerID ?? -1;
        lm.Barrel = blr.Barrel?.LoadoutManagerID ?? -1;
        lm.Scope = blr.Scope?.LoadoutManagerID ?? -1;
        lm.Grip = blr.Grip?.LoadoutManagerID ?? -1;
        lm.Stock = blr.Stock?.LoadoutManagerID ?? -1;
        lm.Ammo = blr.Ammo?.LoadoutManagerID ?? -1;
        lm.Muzzle = blr.Muzzle?.LoadoutManagerID ?? -1;
        lm.Magazine = blr.Magazine?.LoadoutManagerID ?? -1;
        lm.Skin = blr.Skin?.LoadoutManagerID ?? -1;
        lm.CamoIndex = blr.WeaponCamo?.LoadoutManagerID ?? -1;
        lm.Hanger = blr.Hanger?.LoadoutManagerID ?? -1;
    }
}

public sealed class UIDWeapon
{
    public string MetaData { get; set; } = "v302";
    public int Reciever { get; set; }
    public int Barrel { get; set; }
    public int Muzzle { get; set; }
    public int Stock { get; set; }
    public int Scope { get; set; }
    public int Magazine { get; set; }
    public int Ammo { get; set; }
    public int Grip { get; set; }
    public int Hanger { get; set; }
    public int CamoIndex { get; set; }
    public int Skin { get; set; }

    public static void WriteToBLRWeapon(UIDWeapon uid, BLRWeapon blr)
    {
        if (blr is null || uid is null) throw new ArgumentNullException($"UIDWeapon or BLRWeapon were null LM:{uid is null} BLR:{blr is null}"); //TODO Replace Exception
        blr.MetaData.Clear();
        blr.MetaData.AddRange(uid.MetaData.Split("|"));
        if (blr.MetaData.Count < 2) throw new ArgumentOutOfRangeException($"UIDWeapon MetaData was too small {blr.MetaData.Count}/2"); //TODO Replace Exception
        if (BLRItemList.ItemLists.TryGetValue(blr.MetaData[0], out var list))
        {
            blr.Reciever = blr.MetaData[1] switch
            {
                "Primary" => list.GetItemByUnlockID(uid.Reciever),
                _ => list.GetItemByUnlockID(uid.Reciever),
            };
            blr.Barrel = list.GetItemByUnlockID(uid.Barrel);
            blr.Scope = list.GetItemByUnlockID(uid.Scope);
            blr.Grip = list.GetItemByUnlockID(uid.Grip);
            blr.Stock = list.GetItemByUnlockID(uid.Stock);
            blr.Magazine = list.GetItemByUnlockID(uid.Magazine);
            blr.Ammo = list.GetItemByUnlockID(uid.Ammo);
            blr.Muzzle = list.GetItemByUnlockID(uid.Muzzle);
            blr.Skin = list.GetItemByUnlockID(uid.Skin);
            blr.WeaponCamo = list.GetItemByUnlockID(uid.CamoIndex);
            blr.Hanger = list.GetItemByUnlockID(uid.Hanger);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Failed to get ItemList for ({uid.MetaData})[{blr.MetaData[0]}]"); //TODO Replace Exception
        }
    }

    public static void ReadFromBLRWeapon(UIDWeapon uid, BLRWeapon blr)
    {
        if (blr is null || uid is null) throw new ArgumentNullException($"UIDWeapon or BLRWeapon were null LM:{uid is null} BLR:{blr is null}"); //TODO Replace Exception
        uid.MetaData = string.Join("|", blr.MetaData);
        uid.Reciever = blr.Reciever?.UnlockID ?? -1;
        uid.Barrel = blr.Barrel?.UnlockID ?? -1;
        uid.Scope = blr.Scope?.UnlockID ?? -1;
        uid.Grip = blr.Grip?.UnlockID ?? -1;
        uid.Stock = blr.Stock?.UnlockID ?? -1;
        uid.Ammo = blr.Ammo?.UnlockID ?? -1;
        uid.Muzzle = blr.Muzzle?.UnlockID ?? -1;
        uid.Magazine = blr.Magazine?.UnlockID ?? -1;
        uid.Skin = blr.Skin?.UnlockID ?? -1;
        uid.CamoIndex = blr.WeaponCamo?.UnlockID ?? -1;
        uid.Hanger = blr.Hanger?.UnlockID ?? -1;
    }
}

public sealed class JsonBLRWeaponConverter : JsonGenericConverter<BLRWeapon>
{
    static JsonBLRWeaponConverter()
    {
        Default = new();
    }
}