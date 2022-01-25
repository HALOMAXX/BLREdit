namespace BLREdit
{
    public class WeaponModifiers
    {
        public int accuracy { get; set; } = 0;
        public int ammo { get; set; } = 0;
        public int damage { get; set; } = 0;
        public int movementSpeed { get; set; } = 0;
        public int range { get; set; } = 0;
        public int rateOfFire { get; set; } = 0;
        public int rating { get; set; } = 0;
        public int recoil { get; set; } = 0;
        public int reloadSpeed { get; set; } = 0;
        public int switchWeaponSpeed { get; set; } = 0;
        public int weaponWeight { get; set; } = 0;

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
