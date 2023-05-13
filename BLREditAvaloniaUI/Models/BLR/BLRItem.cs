using System;

namespace BLREdit.Models;

public sealed class BLRItem
{
    public int LoadoutManagerID { get; set; } = -69;
    public int NameID { get; set; } = -1;
    public int UnlockID { get; set; } = -1;
    public int CategoryID { get; set; } = -1;
    public string IconName { get; set; } = "Missing";
    public string[] SupportedMods { get; set; } = Array.Empty<string>();
    public int[] ValidForUnlockID { get; set; } = Array.Empty<int>();
    public BLRPawnModifiers PawnModifiers { get; set; } = new();
    public BLRWeaponModifiers WeaponModifiers { get; set; } = new();
    public BLRWeaponStats WeaponStats { get; set; } = new();
    public BLRWikiStats WikiStats { get; set; } = new();
}