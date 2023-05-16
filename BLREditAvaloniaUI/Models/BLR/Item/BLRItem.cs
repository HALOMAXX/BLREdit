using BLREdit.Properties;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Models.BLR;

public sealed class BLRItem : ModelBase
{
    [JsonIgnore] public string Name { get { return BLRItemNames.ResourceManager.GetString(NameID.ToString()) ?? NameID.ToString(); } }
    public int LoadoutManagerID { get; set; } = -69;
    public int NameID { get; set; } = -1;
    public int UnlockID { get; set; } = -1;
    public int CategoryID { get; set; } = -1;
    public int CP { get; set; } = -1;
    public string IconName { get; set; } = "Missing";
    public RangeObservableCollection<int> SupportedModCategories { get; set; } = new();
    public RangeObservableCollection<int> ValidForUnlockID { get; set; } = new();
    public BLRPawnModifiers PawnModifiers { get; set; } = new();
    public BLRWeaponModifiers WeaponModifiers { get; set; } = new();
    public BLRWeaponStats WeaponStats { get; set; } = new();
    public BLRWikiStats WikiStats { get; set; } = new();
}