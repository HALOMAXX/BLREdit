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
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Accuracy:{0}, Ammo:{1}, Damage:{2}, MovementSpeed:{3}, Range:{4}, RateOfFire:{5}, Rating:{6}, Recoil:{7}, ReloadSpeed:{8}, WeaponSwitchSpeed:{9}, WeaponWeight:{10}", accuracy, ammo, damage, movementSpeed, range, rateOfFire, rating, recoil, reloadSpeed, switchWeaponSpeed, weaponWeight);
            return sb.ToString();
        }
    }
}
