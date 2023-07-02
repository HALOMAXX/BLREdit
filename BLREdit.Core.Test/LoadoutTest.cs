using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Models.BLR.Loadout;

namespace BLREdit.Core.Test;

[TestClass]
public class LoadoutTest
{
    static BLRProfile Profile { get; } = new();


    [TestMethod]
    public void DepotListTest()
    {
        Assert.IsTrue(Profile.Loadouts[0].Depot.Count == 5, $"Depot has more items then it can hold({Profile.Loadouts[0].Depot.Count}/5)"); //TODO add configurable max count for Depot
    }

    [TestMethod]
    public void TauntListTest()
    {
        Assert.IsTrue(Profile.Loadouts[0].Taunt.Count == 8, $"Taunt has more items then it can hold({Profile.Loadouts[0].Taunt.Count}/8)"); //TODO add configurable max count for Taunt
    }
}