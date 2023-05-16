namespace BLREdit.Models.BLR;

public sealed class BLRWeaponModifiers : ModelBase
{
    public double Accuracy { get; set; } = 0;
    public double Ammo { get; set; } = 0;
    public double Damage { get; set; } = 0;
    public double MovementSpeed { get; set; } = 0;
    public double Range { get; set; } = 0;
    public double RateOfFire { get; set; } = 0;
    public double Rating { get; set; } = 0;
    public double Recoil { get; set; } = 0;
    public double ReloadSpeed { get; set; } = 0;
    public double SwitchWeaponSpeed { get; set; } = 0;
    public double WeaponWeight { get; set; } = 0;
}
