using System.Text;

namespace BLREdit
{
    public class PawnModifiers
    {
        public int BodyDamageReduction { get; set; }
        public int ElectroProtection { get; set; }
        public int ExplosiveProtection { get; set; }
        public int GearSlots { get; set; }
        public int HRVDuration { get; set; }
        public int HRVRechargeRate { get; set; }
        public int Health { get; set; }
        public int HealthRecharge { get; set; }
        public int HelmetDamageReduction { get; set; }
        public int IncendiaryProtection { get; set; }
        public int InfraredProtection { get; set; }
        public int LegsDamageReduction { get; set; }
        public int MeleeProtection { get; set; }
        public int MeleeRange { get; set; }
        public int MovementSpeed { get; set; }
        public int PermanentHealthProtection { get; set; }
        public float SprintMultiplier { get; set; }
        public int Stamina { get; set; }
        public int SwitchWeaponSpeed { get; set; }
        public int ToxicProtection { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("BodyDamageReduction:{0}, ElectroProtection:{1}, ExplosiveProtection:{2}, GearSlots:{3}, HRVDuration:{4}, HRVRechargeRate:{5}, Health:{6}, HealthRecharge:{7}, HelmetDamageReduction:{8}, IncendiaryProtection:{9}, InfraredProtection:{10}, LegsDamageReduction:{11}, MeleeProtection:{12}, MeleeRange:{13}, MovementSpeed:{14}, PermanentHealthProtection:{15}, SprintMultiplier:{16}, Stamina:{17}, SwitchWeaponSpeed:{18}, ToxicProtection{19}", BodyDamageReduction, ElectroProtection, ExplosiveProtection, GearSlots, HRVDuration, HRVRechargeRate, Health, HealthRecharge, HelmetDamageReduction, IncendiaryProtection, InfraredProtection, LegsDamageReduction, MeleeProtection, MeleeRange, MovementSpeed, PermanentHealthProtection, SprintMultiplier, Stamina, SwitchWeaponSpeed, ToxicProtection);
            return sb.ToString();
        }
    }
}
