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
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
