using BLREdit.Core.Utils;

using System.ComponentModel.Design;
using System.Resources;

namespace BLREditResourceCreator;

public static class Program
{
    static void Main()
    {
        Dictionary<string, List<BLRItem>>? gear = IOResources.DeserializeFile<Dictionary<string, List<BLRItem>>>("Data\\Old\\Items\\gear.json");
        Dictionary<string, List<BLRItem>>? mods = IOResources.DeserializeFile<Dictionary<string, List<BLRItem>>>("Data\\Old\\Items\\mods.json");
        Dictionary<string, List<BLRItem>>? weapons = IOResources.DeserializeFile<Dictionary<string, List<BLRItem>>>("Data\\Old\\Items\\weapons.json");

        Dictionary<string, List<BLREdit.Core.Models.BLR.Item.BLRItem>>? allnew2 = IOResources.DeserializeFile<Dictionary<string, List<BLREdit.Core.Models.BLR.Item.BLRItem>>>("Data\\Old\\Items\\itemList.json");

        Dictionary<string, List<BLRItem>> AllOldItems = new();

        AddItems(gear ?? new(), AllOldItems);
        AddItems(mods ?? new(), AllOldItems);
        AddItems(weapons ?? new(), AllOldItems);


        var current = BLREdit.Core.Models.BLR.Item.BLRItemList.ItemLists["v302"];

        using var nameWriter = new ResXResourceWriter("Data\\out\\resx\\ItemNames.resx");
        using var tooltipWriter = new ResXResourceWriter("Data\\out\\resx\\ItemTooltips.resx");

        Dictionary<string, ResXDataNode> nameNodes = new();
        Dictionary<string, ResXDataNode> tooltipNodes = new();

        foreach (var category in current.Categories)
        {
            int ID = 0;
            foreach (var newItem in category)
            {
                foreach (var itemType in AllOldItems)
                {
                    foreach (var item in itemType.Value)
                    {
                        if (newItem.UnlockID == item.uid)
                        {
                            newItem.Name = item.name ?? "";
                            newItem.CategoryName = itemType.Key;
                            newItem.Type = item.type;
                            newItem.Tooltip = item.tooltip ?? "";
                            newItem.SupportedModCategorieNames.Clear();
                            if(item.supportedMods is not null) newItem.SupportedModCategorieNames.AddRange(item.supportedMods);
                        }
                    }
                }

                foreach (var itemType in allnew2)
                {
                    foreach (var item in itemType.Value)
                    {
                        if (newItem.UnlockID == item.UnlockID)
                        {
                            newItem.NameID = item.NameID;
                            if (string.IsNullOrEmpty(newItem.CategoryName))
                            {
                                newItem.CategoryName = itemType.Key;
                            }
                        }
                    }
                }

                ResXDataNode nameNode = new(newItem.UnlockID.ToString(), newItem.DisplayName)
                {
                    Comment = $"{newItem.CategoryName}-{ID:0000}"
                };

                ResXDataNode tooltipNode = new(newItem.UnlockID.ToString(), newItem.DisplayTooltip)
                {
                    Comment = $"{newItem.CategoryName}-{ID:0000}"
                };

                if (nameNodes.TryGetValue(nameNode.Name, out var nNode))
                {
                    if (nNode.GetValue(null as ITypeResolutionService) is string s && !string.IsNullOrEmpty(s))
                    {

                    }
                    else
                    {
                        nameNodes.Remove(nameNode.Name);
                        nameNodes.Add(nameNode.Name, nameNode);
                    }
                }
                else
                {
                    nameNodes.Add(nameNode.Name, nameNode);
                }

                if (tooltipNodes.TryGetValue(tooltipNode.Name, out var tNode))
                {
                    if (tNode.GetValue(null as ITypeResolutionService) is string s && !string.IsNullOrEmpty(s))
                    {

                    }
                    else
                    { 
                        tooltipNodes.Remove(tooltipNode.Name);
                        tooltipNodes.Add(tooltipNode.Name, tooltipNode);
                    }
                }
                else
                {
                    tooltipNodes.Add(tooltipNode.Name, nameNode);
                }

                ID++;
            }
        }

        foreach (var pair in nameNodes)
        {
            nameWriter.AddResource(pair.Value);
        }
        nameWriter.Close();

        foreach (var pair in tooltipNodes)
        {
            tooltipWriter.AddResource(pair.Value);
        }
        tooltipWriter.Close();
        

        
        



        IOResources.SerializeFile("Data\\out\\json\\FullItems.json", current);
        IOResources.SerializeFile("Data\\out\\json\\FullItemsCompact.json", current, true);

    }

    public static void AddItems(Dictionary<string, List<BLRItem>> itemList, Dictionary<string, List<BLRItem>> allItems)
    {
        foreach (var itemType in itemList)
        {
            List<BLRItem> list = new();
            if (allItems.TryGetValue(itemType.Key, out var outlist))
            {
                list = outlist;
            }
            foreach (var item in itemType.Value)
            {
                list.Add(item);
            }
            allItems[itemType.Key] = list;
        }
    }
}