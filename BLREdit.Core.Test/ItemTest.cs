using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Utils;

namespace BLREdit.Core.Test;

[TestClass]
public class ItemTest
{
    const string FOLDER_NAME = "Items";
    static readonly string OutputFolder = $"Data\\{FOLDER_NAME}\\out";
    static readonly DirectoryInfo OutputListFolder = new($"{OutputFolder}\\List");

    [TestMethod]
    public void SerializeItemList()
    {
        Assert.IsTrue(BLRItemList.ItemLists.Any(), "No ItemLists got Deserialized");
        Assert.IsTrue(BLRItemList.ItemLists.ContainsKey("v302"), "ItemList is not for v302!");

        BLRItemList.SaveItemLists($"{OutputListFolder.FullName}\\List.json", false);
        BLRItemList.SaveItemLists($"{OutputListFolder.FullName}\\ListCompact.json");

        var check = IOResources.DeserializeDirectory<BLRItemList>(OutputListFolder);

        Assert.IsTrue(BLRItemList.ItemLists.Count * 2 == check.Count, $"Source:{BLRItemList.ItemLists.Count}*2 = {BLRItemList.ItemLists.Count * 2} and Output:{check.Count} are not the same");

        for (int i = 0; i < BLRItemList.ItemLists.Count; i++)
        {
            var key = BLRItemList.ItemLists.Keys.ElementAt(i);
            Assert.IsTrue(BLRItemList.ItemLists[key].Equals(check[i]), $"Source[{i}] != Serialized[{i}]");
            Assert.IsTrue(BLRItemList.ItemLists[key].Equals(check[i + BLRItemList.ItemLists.Count]), $"Source[{i}] != Serialized[{i + BLRItemList.ItemLists.Count}](Compact)");
        }
    }
}