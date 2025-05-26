using BLREdit.Import;
using BLREdit.UI.Views;

using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace BLREdit.UI;

public sealed class ItemFilters : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    public static ItemFilters Instance { get; private set; } = new ItemFilters();

    private BLREditWeapon? weaponFilter;
    public BLREditWeapon? WeaponFilter { get { return weaponFilter; } set { weaponFilter = value; OnPropertyChanged(); } }
    private string searchFilter = "";
    public string SearchFilter { get { return searchFilter; } set { searchFilter = value; MainWindow.Instance?.ApplySearchAndFilter(); OnPropertyChanged(); } }

    public static bool FilterBySearch(BLREditItem item)
    {
        if(item == null) return false;
        string searchText = Instance.SearchFilter.Trim().ToUpperInvariant();
        string itemName = item.Name?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(searchText)) { return true; }
        return itemName.Contains(searchText);
    }

    public static bool FullFilter(object o)
    {
        if (MainWindow.Instance?.WasLastImageScopePreview ?? true) { return true; }
        if (o is BLREditItem item)
        {
            if (FilterBySearch(item))
            {
                return FilterByValidity(item);
            }
        }
        return false;
    }

    public static bool PartialFilter(object o)
    {
        if (o is BLREditItem item)
        {
            return FilterByValidity(item);
        }
        return false;
    }

    public static bool FilterByValidity(BLREditItem item)
    {
        if (item is null) { LoggingSystem.LogNull(); return false; }
        item.IsValid.Set(item.ValidForTest(Instance?.WeaponFilter?.Receiver ?? null));
        switch ( item.Category )
        {
            case ImportSystem.PRIMARY_CATEGORY: 
                return true;
            case ImportSystem.SECONDARY_CATEGORY:
                return !(item.Icon?.Contains("Depot") ?? false);
            case ImportSystem.SHOP_CATEGORY:
                if (Instance?.WeaponFilter?.Loadout?.IsAdvanced.Is ?? false) return true;
                return item.Name != "HRV Jammer";
            default:
                return item.IsValidFor(Instance?.WeaponFilter?.Receiver ?? null, Instance?.WeaponFilter?.Loadout?.IsAdvanced.Is ?? false);
        };
    }

}
