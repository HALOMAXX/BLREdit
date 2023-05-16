using Avalonia.Collections;

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Models.BLR;

public sealed class BLRItemList : ModelBase
{
    public static DirectoryInfo ItemDataLocation { get; }
    public static AvaloniaDictionary<string, BLRItemList> ItemLists { get; }

    static BLRItemList()
    {
        ItemDataLocation = new("Data\\ItemData");
        ItemLists = LoadItemLists();
    }

    static AvaloniaDictionary<string, BLRItemList> LoadItemLists()
    {
        var all = IOResources.DeserializeDirectory<BLRItemList>(ItemDataLocation);
        AvaloniaDictionary<string, BLRItemList> dict = new();
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
