using System;
using System.Collections.ObjectModel;

namespace BLREdit.Models;

public sealed class BLRItem : ModelBase
{
    public int LoadoutManagerID { get; set; } = -69;
    public int NameID { get; set; } = -1;
    public int UnlockID { get; set; } = -1;
    public int CategoryID { get; set; } = -1;
    public string IconName { get; set; } = "Missing";
    public RangeObservableCollection<string> SupportedMods { get; set; } = new();
    public RangeObservableCollection<int> ValidForUnlockID { get; set; } = new();
    public BLRPawnModifiers PawnModifiers { get; set; } = new();
    public BLRWeaponModifiers WeaponModifiers { get; set; } = new();
    public BLRWeaponStats WeaponStats { get; set; } = new();
    public BLRWikiStats WikiStats { get; set; } = new();
}