using System.Text;

namespace BLREdit
{
    public class WeaponModifiers
    {
        public int accuracy { get; set; }
        public int ammo { get; set; }
        public int damage { get; set; }
        public int movementSpeed { get; set; }
        public int range { get; set; }
        public int rateOfFire { get; set; }
        public int rating { get; set; }
        public int recoil { get; set; }
        public int reloadSpeed { get; set; }
        public int switchWeaponSpeed { get; set; }
        public int weaponWeight { get; set; }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
