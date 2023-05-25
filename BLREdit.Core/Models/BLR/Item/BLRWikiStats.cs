using BLREdit.Core.Utils;

using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

[JsonConverter(typeof(JsonBLRWikiStatsConverter))]
public sealed class BLRWikiStats : ModelBase
{
    public double AimSpread { get; set; }
    public double AmmoMag { get; set; }
    public double AmmoReserve { get; set; }
    public double Damage { get; set; }
    public double Firerate { get; set; }
    public double HipSpread { get; set; }
    public double MoveSpread { get; set; }
    public double RangeClose { get; set; }
    public double RangeFar { get; set; }
    public double Recoil { get; set; }
    public double Reload { get; set; }
    public double Run { get; set; }
    public double ScopeInTime { get; set; }
    public double Swaprate { get; set; }
    public double Zoom { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is BLRWikiStats wiki)
        {
            return
                AimSpread.Equals(wiki.AimSpread) &&
                AmmoMag.Equals(wiki.AmmoMag) &&
                AmmoReserve.Equals(wiki.AmmoReserve) &&
                Damage.Equals(wiki.Damage) &&
                Firerate.Equals(wiki.Firerate) &&
                HipSpread.Equals(wiki.HipSpread) &&
                MoveSpread.Equals(wiki.MoveSpread) &&
                RangeClose.Equals(wiki.RangeClose) &&
                RangeFar.Equals(wiki.RangeFar) &&
                Recoil.Equals(wiki.Recoil) &&
                Reload.Equals(wiki.Reload) &&
                Run.Equals(wiki.Run) &&
                ScopeInTime.Equals(wiki.ScopeInTime) &&
                Swaprate.Equals(wiki.Swaprate) &&
                Zoom.Equals(wiki.Zoom);
        }
        return false;
    }
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(AimSpread);
        hash.Add(AmmoMag);
        hash.Add(AmmoReserve);
        hash.Add(Damage);
        hash.Add(Firerate);
        hash.Add(HipSpread);
        hash.Add(MoveSpread);
        hash.Add(RangeClose);
        hash.Add(RangeFar);
        hash.Add(Recoil);
        hash.Add(Reload);
        hash.Add(Run);
        hash.Add(ScopeInTime);
        hash.Add(Swaprate);
        hash.Add(Zoom);

        return hash.ToHashCode();
    }
}

public class JsonBLRWikiStatsConverter : JsonGenericConverter<BLRWikiStats> { }