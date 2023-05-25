using BLREdit.Core.Utils;

using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

[JsonConverter(typeof(JsonBLRWeaponStatDescriptorConverter))]
public sealed class BLRWeaponStatDecriptor : ModelBase
{
    public string Name { get; set; } = "Classic";
    public int Points { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is BLRWeaponStatDecriptor stat)
        {
            return
                Name.Equals(stat.Name, StringComparison.Ordinal) && 
                Points.Equals(stat.Points);
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Name);
        hash.Add(Points);

        return hash.ToHashCode();
    }
}

public class JsonBLRWeaponStatDescriptorConverter : JsonGenericConverter<BLRWeaponStatDecriptor> { }