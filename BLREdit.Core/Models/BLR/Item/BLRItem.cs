using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Properties;
using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

public sealed class BLRItem : ModelBase
{
    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is BLRItem item)
        {
            return
                LoadoutManagerID.Equals(item.LoadoutManagerID) &&
                UnlockID.Equals(item.UnlockID) &&
                CategoryID.Equals(item.CategoryID) &&
                CategoryName.Equals(item.CategoryName, StringComparison.Ordinal) &&
                Type.Equals(item.Type) &&
                CP.Equals(item.CP) &&
                Name.Equals(item.Name, StringComparison.Ordinal) &&
                Tooltip.Equals(item.Tooltip, StringComparison.Ordinal) &&
                NameID.Equals(item.NameID) &&
                IconName.Equals(item.IconName, StringComparison.Ordinal) &&
                SupportedModCategories.Equals(item.SupportedModCategories) &&
                SupportedModCategorieNames.Equals(item.SupportedModCategorieNames) &&
                ValidForUnlockID.Equals(item.ValidForUnlockID) &&
                PawnModifiers.Equals(item.PawnModifiers) &&
                WeaponModifiers.Equals(item.WeaponModifiers) &&
                WeaponStats.Equals(item.WeaponStats) &&
                WikiStats.Equals(item.WikiStats);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(LoadoutManagerID);
        hash.Add(UnlockID);
        hash.Add(CategoryID);
        hash.Add(CategoryName);
        hash.Add(Type);
        hash.Add(CP);
        hash.Add(Name);
        hash.Add(Tooltip);
        hash.Add(NameID);
        hash.Add(IconName);
        hash.Add(SupportedModCategories);
        hash.Add(SupportedModCategorieNames);
        hash.Add(ValidForUnlockID);
        hash.Add(PawnModifiers);
        hash.Add(WeaponModifiers);
        hash.Add(WeaponStats);
        hash.Add(WikiStats);

        return hash.ToHashCode();
    }
    #endregion Overrides

    [JsonIgnore] public string DisplayName => ItemNames.ResourceManager.GetString(UnlockID.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture) ?? Name;
    [JsonIgnore] public string DisplayTooltip => ItemTooltips.ResourceManager.GetString(UnlockID.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture) ?? Tooltip;
    public int LoadoutManagerID { get; set; } = -69;
    public int UnlockID { get; set; } = -1;
    public int CategoryID { get; set; } = -1;
    public string CategoryName { get; set; } = "";
    public int Type { get; set; } = -1;
    public int CP { get; set; } = -1;
    public string Name { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public int NameID { get; set; } = -1;
    public string IconName { get; set; } = "Missing";
    public RangeObservableCollection<int> SupportedModCategories { get; set; } = new();
    public RangeObservableCollection<string> SupportedModCategorieNames { get; set; } = new();
    public RangeObservableCollection<int> ValidForUnlockID { get; set; } = new();
    public BLRPawnModifiers PawnModifiers { get; set; } = new();
    public BLRWeaponModifiers WeaponModifiers { get; set; } = new();
    public BLRWeaponStats WeaponStats { get; set; } = new();
    public BLRWikiStats WikiStats { get; set; } = new();
}

public sealed class JsonBLRItemConverter : JsonGenericConverter<BLRItem> 
{
    static JsonBLRItemConverter() 
    { 
        Default = new();
    }
}