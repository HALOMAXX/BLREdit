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

    public RangeObservableCollection<string> MetaData { get; } = new() { "v302" };

    [DoNotNotify] public BLRWeapon? Primary { get { return _primary; } set { if (IsValidPrimary(value)) { _primary = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRWeapon? Secondary { get { return _secondary; } set { if (IsValidSecondary(value)) { _secondary = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRGear? Gear { get { return _gear; } set { if (IsValidGear(value)) { _gear = value; OnPropertyChanged(); } } }
    public RangeObservableCollection<BLRItem?> Depot { get; }
    public RangeObservableCollection<BLRItem?> Taunt { get; }

    public BLRLoadout()
    {
        Depot = new();
        Taunt = new();
        AddChangedEventsAndDefaultItems();
    }

    void AddChangedEventsAndDefaultItems()
    {
        Depot.CollectionChanged += DepotChanged;
        Taunt.CollectionChanged += TauntChanged;
        Depot.AddRange(BLRItemList.ItemLists[MetaData[0]].Categories[21]);
        Taunt.AddRange(BLRItemList.ItemLists[MetaData[0]].Categories[3]);
    }

    void DepotChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListChanged(Depot, e, 5);
    }

    void TauntChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListChanged(Taunt, e, 8);
    }

    static void ListChanged(RangeObservableCollection<BLRItem?> list, NotifyCollectionChangedEventArgs e, int max)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                if (list.Count > max)
                {
                    list.RemoveRange(max, list.Count - max);
                }
                break;
        }
    }

    #region ItemValidation
    public static bool IsValidPrimary(BLRWeapon? weapon)
    {
        if (weapon is null || weapon.Reciever is null || weapon.Reciever.CategoryName is "Primaries") return true;
        return false;
    }
    public static bool IsValidSecondary(BLRWeapon? weapon)
    {
        if (weapon is null || weapon.Reciever is null || weapon.Reciever.CategoryName is "Secondaries") return true;
        return false;
    }

    public static bool IsValidGear(BLRGear? gear)
    {
        return true;
    }

    public static bool IsValidDepot(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Shop") return true;
        return false;
    }
    public static bool IsValidTaunt(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Emotes") return true;
        return false;
    }
    #endregion ItemValidation
}