using BLREdit.Core.Models.BLR.Item;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BLREdit.Core.Models.BLR.Loadout;

public sealed class BLRLoadout : ModelBase
{
    private BLRWeapon? _primary;
    private BLRWeapon? _secondary;

    private BLRGear? _gear;

    public string ClientVersion { get; set; } = "v302";

    [DoNotNotify] public BLRWeapon? Primary { get { return _primary; } set { if (IsValidPrimary(value)) { _primary = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRWeapon? Secondary { get { return _secondary; } set { if (IsValidSecondary(value)) { _secondary = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRGear? Gear { get { return _gear; } set { if (IsValidGear(value)) { _gear = value; OnPropertyChanged(); } } }
    public RangeObservableCollection<BLRItem?> Depot { get; }
    public RangeObservableCollection<BLRItem?> Taunt { get; }

    public BLRLoadout()
    {
        Depot = new();
        Taunt = new();
        Depot.CollectionChanged += DepotChanged;
        Taunt.CollectionChanged += TauntChanged;
    }

    public BLRLoadout(IList<BLRItem> depotItems, IList<BLRItem> taunts)
    {
        Depot = new(depotItems);
        Taunt = new(taunts);
        Depot.CollectionChanged += DepotChanged;
        Taunt.CollectionChanged += TauntChanged;
    }

    void DepotChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                e.NewStartingIndex
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
        }
    }

    void TauntChanged(object? sender, NotifyCollectionChangedEventArgs e)
    { 
        
    }




    #region ItemValidation
    public bool IsValidPrimary(BLRWeapon? weapon)
    {
        if (weapon is null || weapon.Reciever is null || weapon.Reciever.CategoryName is "Primaries") return true;
        return false;
    }
    public bool IsValidSecondary(BLRWeapon? weapon)
    {
        if (weapon is null || weapon.Reciever is null || weapon.Reciever.CategoryName is "Secondaries") return true;
        return false;
    }

    public bool IsValidGear(BLRGear? gear)
    {
        return true;
    }

    public bool IsValidDepot(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Shop") return true;
        return false;
    }
    public bool IsValidTaunt(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Emotes") return true;
        return false;
    }
    #endregion ItemValidation
}