using BLREdit.Core.Utils;

using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

[JsonConverter(typeof(JsonBLRWeaponModifiersConverter))]
public sealed class BLRWeaponModifiers : ModelBase
{
    public double Accuracy { get; set; }
    public double Ammo { get; set; }
    public double Damage { get; set; }
    public double MovementSpeed { get; set; }
    public double Range { get; set; }
    public double RateOfFire { get; set; }
    public double Rating { get; set; }
    public double Recoil { get; set; }
    public double ReloadSpeed { get; set; }
    public double SwitchWeaponSpeed { get; set; }
    public double WeaponWeight { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is BLRWeaponModifiers mod)
        {
            return
                Accuracy.Equals(mod.Accuracy) &&
                Ammo.Equals(mod.Ammo) &&
                Damage.Equals(mod.Damage) &&
                MovementSpeed.Equals(mod.MovementSpeed) &&
                Range.Equals(mod.Range) &&
                RateOfFire.Equals(mod.RateOfFire) &&
                Rating.Equals(mod.Rating) &&
                Recoil.Equals(mod.Recoil) &&
                ReloadSpeed.Equals(mod.ReloadSpeed) &&
                SwitchWeaponSpeed.Equals(mod.SwitchWeaponSpeed) &&
                WeaponWeight.Equals(mod.WeaponWeight);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Accuracy);
        hash.Add(Ammo);
        hash.Add(Damage);
        hash.Add(MovementSpeed);
        hash.Add(Range);
        hash.Add(RateOfFire);
        hash.Add(Rating);
        hash.Add(Recoil);
        hash.Add(ReloadSpeed);
        hash.Add(SwitchWeaponSpeed);
        hash.Add(WeaponWeight);

        return hash.ToHashCode();
    }
}

public class JsonBLRWeaponModifiersConverter : JsonGenericConverter<BLRWeaponModifiers> { }