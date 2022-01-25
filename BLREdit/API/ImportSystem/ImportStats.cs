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
            return LoggingSystem.ObjectToTextWall(this);
        }

    }
}
