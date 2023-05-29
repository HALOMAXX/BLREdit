using BLREdit.Core.Properties;
using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

public sealed class BLRItem : ModelBase
{
    [JsonIgnore] public string DisplayName => ItemNames.ResourceManager.GetString(NameID.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture) ?? Name;
    [JsonIgnore] public string DisplayTooltip => ItemTooltips.ResourceManager.GetString(NameID.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture) ?? Tooltip;
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

public class JsonBLRItemConverter : JsonGenericConverter<BLRItem> {
    static JsonBLRItemConverter()
    {
        Default = new();
        IOResources.JSOSerialization.Converters.Add(new JsonBLRItemConverter());
        IOResources.JSOSerializationCompact.Converters.Add(new JsonBLRItemConverter());
    }
}