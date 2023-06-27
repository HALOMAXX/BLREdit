using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

public sealed class BLRItemList : ModelBase
{
    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is BLRItemList list)
        {
            return
                ClientVersion.Equals(list.ClientVersion, StringComparison.Ordinal) &&
                CategoryNames.Equals(list.CategoryNames) &&
                Categories.Equals(list.Categories);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(ClientVersion);
        hash.Add(CategoryNames);
        hash.Add(Categories);

        return hash.ToHashCode();
    }
    #endregion Overrides
    public static DirectoryInfo ItemDataLocation { get; }
    public static Dictionary<string, BLRItemList> ItemLists { get; }

    static BLRItemList()
    {
        ItemDataLocation = new("Data\\Items\\List");
        ItemLists = LoadItemLists(ItemDataLocation);
    }

    public static void SaveItemLists(string file, bool compact = true)
    {
        if (file is null) return;
        IOResources.SerializeFile(file, ItemLists.Values, compact);
    }

    public static Dictionary<string, BLRItemList> LoadItemLists(DirectoryInfo location)
    {
        var all = IOResources.DeserializeDirectory<BLRItemList>(location);
        Dictionary<string, BLRItemList> dict = new();
        foreach (var list in all)
        {
            if (!dict.TryGetValue(list.ClientVersion, out _))
            { 
                dict.Add(list.ClientVersion, list);
            }
        }
        return dict;
    }

    public string ClientVersion { get; }
    public string Name { get; }
    public RangeObservableCollection<string> CategoryNames { get; }
    public RangeObservableCollection<RangeObservableCollection<BLRItem>> Categories { get; }
    
    [JsonConstructor]
    public BLRItemList(string clientVersion, string name ,RangeObservableCollection<string> categoryNames, RangeObservableCollection<RangeObservableCollection<BLRItem>> categories)
    {
        ClientVersion = clientVersion;
        Name = name;
        CategoryNames = categoryNames;
        Categories = categories;
        if(!string.IsNullOrEmpty(clientVersion) && categoryNames is not null && categories is not null) Parallel.For(0, Categories.Count, (index) => { foreach (var item in Categories[index]) { item.CategoryID = index; } });
    }

    public BLRItem? GetItemByLoadoutManagerIDAndCategory(string category, int lmid)
    { 
        return GetItemByLoadoutManagerIDAndCategoryID(CategoryNames.IndexOf(category), lmid);
    }

    public BLRItem? GetItemByLoadoutManagerIDAndCategoryID(int categoryID, int lmid)
    {
        foreach (var item in Categories[categoryID])
        {
            if (item.LoadoutManagerID == lmid) return item;
        }
        return null;
    }

    public BLRItem? GetItemByUnlockID(int uid)
    {
        foreach (var category in Categories)
        {
            foreach (var item in category)
            { 
                if(item.UnlockID == uid) return item;
            }
        }
        return null;
    }
}
