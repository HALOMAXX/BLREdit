namespace BLREdit.Import;

public sealed class WeaponModifiers
{
    public double accuracy { get; set; } = 0;
    public double ammo { get; set; } = 0;
    public double damage { get; set; } = 0;
    public double movementSpeed { get; set; } = 0;
    public double range { get; set; } = 0;
    public double rateOfFire { get; set; } = 0;
    public double rating { get; set; } = 0;
    public double recoil { get; set; } = 0;
    public double reloadSpeed { get; set; } = 0;
    public double switchWeaponSpeed { get; set; } = 0;
    public double weaponWeight { get; set; } = 0;


    public bool IsNotZero { get 
        { 
            return (accuracy != 0) || (damage != 0) || (movementSpeed != 0) || (recoil != 0);
        } }
    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }
}
