using BLREdit.Core.Utils;

using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Item;

[JsonConverter(typeof(JsonBLRPawnModifiersConverter))]
public sealed class BLRPawnModifiers : ModelBase
{
    public double BodyDamageReduction { get; set; }
    public double ElectroProtection { get; set; }
    public double ExplosiveProtection { get; set; }
    public double GearSlots { get; set; }
    public double HRVDuration { get; set; }
    public double HRVRechargeRate { get; set; }
    public double Health { get; set; }
    public double HealthRecharge { get; set; }
    public double HelmetDamageReduction { get; set; }
    public double IncendiaryProtection { get; set; }
    public double InfraredProtection { get; set; }
    public double LegsDamageReduction { get; set; }
    public double MeleeProtection { get; set; }
    public double MeleeRange { get; set; }
    public double MovementSpeed { get; set; }
    public double PermanentHealthProtection { get; set; }
    public double SprintMultiplier { get; set; } = 1;
    public double Stamina { get; set; }
    public double SwitchWeaponSpeed { get; set; }
    public double ToxicProtection { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is BLRPawnModifiers mod)
        {
            return
                BodyDamageReduction.Equals(mod.BodyDamageReduction) &&
                ElectroProtection.Equals(mod.ElectroProtection) &&
                ExplosiveProtection.Equals(mod.ExplosiveProtection) &&
                GearSlots.Equals(mod.GearSlots) &&
                HRVDuration.Equals(mod.HRVDuration) &&
                HRVRechargeRate.Equals(mod.HRVRechargeRate) &&
                Health.Equals(mod.Health) &&
                HealthRecharge.Equals(mod.HealthRecharge) &&
                HelmetDamageReduction.Equals(mod.HelmetDamageReduction) &&
                IncendiaryProtection.Equals(mod.IncendiaryProtection) &&
                InfraredProtection.Equals(mod.InfraredProtection) &&
                LegsDamageReduction.Equals(mod.LegsDamageReduction) &&
                MeleeProtection.Equals(mod.MeleeProtection) &&
                MeleeRange.Equals(mod.MeleeRange) &&
                MovementSpeed.Equals(mod.MovementSpeed) &&
                PermanentHealthProtection.Equals(mod.PermanentHealthProtection) &&
                SprintMultiplier.Equals(mod.SprintMultiplier) &&
                Stamina.Equals(mod.Stamina) &&
                SwitchWeaponSpeed.Equals(mod.SwitchWeaponSpeed) &&
                ToxicProtection.Equals(mod.ToxicProtection);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(BodyDamageReduction);
        hash.Add(ElectroProtection);
        hash.Add(ExplosiveProtection);
        hash.Add(GearSlots);
        hash.Add(HRVDuration);
        hash.Add(HRVRechargeRate);
        hash.Add(Health);
        hash.Add(HealthRecharge);
        hash.Add(HelmetDamageReduction);
        hash.Add(IncendiaryProtection);
        hash.Add(InfraredProtection);
        hash.Add(LegsDamageReduction);
        hash.Add(MeleeProtection);
        hash.Add(MeleeRange);
        hash.Add(MovementSpeed);
        hash.Add(PermanentHealthProtection);
        hash.Add(SprintMultiplier);
        hash.Add(Stamina);
        hash.Add(SwitchWeaponSpeed);
        hash.Add(ToxicProtection);
        
        return hash.ToHashCode();
    }
}

public class JsonBLRPawnModifiersConverter : JsonGenericConverter<BLRPawnModifiers> { }