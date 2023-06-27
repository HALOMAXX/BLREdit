using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Models.BLR.Loadout;

using System.ComponentModel;

namespace BLREdit.Core.Test;

[TestClass]
public class NotifyTest
{
    static readonly List<string> NotifiedProperties = new();

    [TestMethod]
    public void TestPropertyChangedEvent()
    {
        var ar = BLRItemList.ItemLists["v302"].Categories[19][9];
        ar.PropertyChanged += NotifyCallback;
        ar.CategoryName = "Primaries";
        BLRWeapon weapon = new();
        weapon.PropertyChanged += NotifyCallback;
        weapon.Reciever = null;
        Assert.IsTrue(NotifiedProperties.Contains(nameof(weapon.Reciever)), "Weapon.Reciever PropertyChanged did not Fire!");
        Assert.IsTrue(NotifiedProperties.Contains(nameof(ar.CategoryName)), "BLRItem.CategoryName PropertyChanged did not Fire!");
    }

    public void NotifyCallback(object? sender, PropertyChangedEventArgs e)
    {
        if(e is not null && e.PropertyName is not null) NotifiedProperties.Add(e.PropertyName);
    }
}