using BLREdit.Import;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.UI;

public sealed class ItemFilters : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    public static ItemFilters Instance { get; private set; } = new ItemFilters();

    private BLRItem weaponFilter;
    public BLRItem WeaponFilter { get { return weaponFilter; } set { weaponFilter = value; OnPropertyChanged(); } }
    private string searchFilter = "";
    public string SearchFilter { get { return searchFilter; } set { searchFilter = value; MainWindow.Self.ApplySearchAndFilter(); OnPropertyChanged(); } }

    public static bool FilterBySearch(BLRItem item)
    {
        string searchText = Instance.SearchFilter.Trim().ToLower();
        string itemName = item.Name.ToLower();
        if (string.IsNullOrEmpty(searchText)) { return true; }
        return itemName.Contains(searchText);
    }

    public static bool FullFilter(object o)
    {
        if (MainWindow.Self?.wasLastImageScopePreview ?? true) { return true; }
        if (o is BLRItem item)
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
        if (o is BLRItem item)
        {
            return FilterByValidity(item);
        }
        return false;
    }

    public static bool FilterByValidity(BLRItem item)
    {
        if (item is null) { return false; }
        item.IsValid.Set(item.ValidForTest(Instance.WeaponFilter));
        switch ( item.Category )
        {
            case ImportSystem.EMOTES_CATEGORY:
                return !string.IsNullOrEmpty(item.Name);
            case ImportSystem.PRIMARY_CATEGORY: 
                return true;
            case ImportSystem.SECONDARY_CATEGORY:
                return !(item.Icon?.Contains("Depot") ?? false);
            case ImportSystem.SHOP_CATEGORY:
                if (BLREditSettings.Settings.AdvancedModding.Is) return true;
                return item.Name != "HRV Jammer";
            default:
                return item.IsValidFor(Instance.WeaponFilter);
        };
    }

}
