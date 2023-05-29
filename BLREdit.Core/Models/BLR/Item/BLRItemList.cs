using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

public sealed class BLRItemList : ModelBase
{
    //TODO Implement overrides(Hash and Equals)
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
            if (!dict.TryGetValue(list.ClientHash, out _))
            { 
                dict.Add(list.ClientHash, list);
            }
        }
        return dict;
    }

    public string ClientHash { get; set; } = string.Empty;
    public RangeObservableCollection<string> CategoryNames { get; set; } = new();
    public RangeObservableCollection<RangeObservableCollection<BLRItem>> Categories { get; set; } = new();
    
    [JsonConstructor]
    public BLRItemList(string clientHash, RangeObservableCollection<string> categoryNames, RangeObservableCollection<RangeObservableCollection<BLRItem>> categories)
    {
        ClientHash = clientHash;
        CategoryNames = categoryNames;
        Categories = categories;
        if(!string.IsNullOrEmpty(clientHash) && categoryNames is not null && categories is not null) Parallel.For(0, Categories.Count, (index) => { foreach (var item in Categories[index]) { item.CategoryID = index; } });
    }
}
