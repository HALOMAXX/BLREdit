using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Utils;

using PropertyChanged;

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
    private BLRItem? _hanger;
    private BLRItem? _camo;
    private BLRItem? _skin;
    private BLRItem? _grip;

    public string ClientVersion { get; set; } = "v302";

    [DoNotNotify] public BLRItem? Reciever { get { return _reciever; } set { if (IsValidReciever(value)) { _reciever = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Barrel { get { return _barrel; } set { if (IsValidBarrel(value)) { _barrel = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Muzzle { get { return _muzzle; } set { if (IsValidMuzzle(value)) { _muzzle = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Stock { get { return _stock; } set { if (IsValidStock(value)) { _stock = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Scope { get { return _scope; } set { if (IsValidScope(value)) { _scope = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Magazine { get { return _magazine; } set { if (IsValidMagazine(value)) { _magazine = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Ammo { get { return _ammo; } set { if (IsValidAmmo(value)) { _ammo = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Hanger { get { return _hanger; } set { if (IsValidHanger(value)) { _hanger = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? WeaponCamo { get { return _camo; } set { if (IsValidCamo(value)) { _camo = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Skin { get { return _skin; } set { if (IsValidSkin(value)) { _skin = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Grip { get { return _grip; } set { if (IsValidGrip(value)) { _grip = value; OnPropertyChanged(); } } }

    #region ItemValidation
    public bool IsValidReciever(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Primaries") return true;
        return false;
    }

    public bool IsValidBarrel(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Barrels") return true;
        return false;
    }

    public bool IsValidMuzzle(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Muzzles") return true;
        return false;
    }

    public bool IsValidStock(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Stocks") return true;
        return false;
    }

    public bool IsValidGrip(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Grips") return true;
        return false;
    }

    public bool IsValidScope(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Scopes") return true;
        return false;
    }

    public bool IsValidMagazine(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Magazines") return true;
        return false;
    }

    public bool IsValidAmmo(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Ammos") return true;
        return false;
    }

    public bool IsValidHanger(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Hangers") return true;
        return false;
    }

    public bool IsValidCamo(BLRItem? item)
    {
        if (item is null || item.CategoryName is "WeaponCamos") return true;
        return false;
    }

    public bool IsValidSkin(BLRItem? item)
    {
        if (item is null || item.CategoryName is "PrimarySkin" || item.CategoryName is "SecondarySkin") return true;
        return false;
    }
    #endregion ItemValidation
}

public sealed class JsonBLRWeaponConverter : JsonGenericConverter<BLRWeapon>
{
    static JsonBLRWeaponConverter()
    {
        Default = new();
    }
}