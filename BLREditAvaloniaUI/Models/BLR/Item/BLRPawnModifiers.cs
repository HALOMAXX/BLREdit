using System.Text.Json.Serialization;

namespace BLREdit.Models.BLR;

public sealed class BLRPawnModifiers : ModelBase
{
    public double BodyDamageReduction { get; set; } = 0;
    public double ElectroProtection { get; set; } = 0;
    public double ExplosiveProtection { get; set; } = 0;
    public double GearSlots { get; set; } = 0;
    public double HRVDuration { get; set; } = 0;
    public double HRVRechargeRate { get; set; } = 0;
    public double Health { get; set; } = 0;
    public double HealthRecharge { get; set; } = 0;
    public double HelmetDamageReduction { get; set; } = 0;
    public double IncendiaryProtection { get; set; } = 0;
    public double InfraredProtection { get; set; } = 0;
    public double LegsDamageReduction { get; set; } = 0;
    public double MeleeProtection { get; set; } = 0;
    public double MeleeRange { get; set; } = 0;
    public double MovementSpeed { get; set; } = 0;
    public double PermanentHealthProtection { get; set; } = 0;
    public double SprintMultiplier { get; set; } = 1;
    public double Stamina { get; set; } = 0;
    public double SwitchWeaponSpeed { get; set; } = 0;
    public double ToxicProtection { get; set; } = 0;
}
