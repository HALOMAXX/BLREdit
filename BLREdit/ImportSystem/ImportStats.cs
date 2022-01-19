using System.Text;

namespace BLREdit
{
    public class ImportStats
    {
        public float accuracy { get; set; }
        public float damage { get; set; }
        public float movementSpeed { get; set; }
        public float range { get; set; }
        public int rateOfFire { get; set; }
        public float recoil { get; set; }
        public float reloadSpeed { get; set; }
        public float weaponWeight { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Accuracy:{0}, Damage:{1}, MovementSpeed:{2}, Range:{3}, RateOfFire:{4}, Recoil:{5}, ReloadSpeed:{6}, WeaponWeight:{7}", accuracy, damage, movementSpeed, range, rateOfFire, recoil, reloadSpeed, weaponWeight);
            return sb.ToString();
        }

    }
}
