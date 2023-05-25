using BLREdit.Core;
using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Core.Models.BLR.Item;

public sealed class BLRItemList : ModelBase
{
    public static DirectoryInfo ItemDataLocation { get; }
    public static Dictionary<string, BLRItemList> ItemLists { get; }

    static BLRItemList()
    {
        ItemDataLocation = new("Data\\Items");
        ItemLists = LoadItemLists();
    }

    static Dictionary<string, BLRItemList> LoadItemLists()
    {
        var all = IOResources.DeserializeDirectory<BLRItemList>(ItemDataLocation);
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
        Parallel.For(0, Categories.Count, (index) => { foreach (var item in Categories[index]) { item.CategoryID = index; } });
    }
}
