namespace BLREdit
{
    public class Loadout
    {
        public Weapon Primary { get; set; } = Weapon.DefaultAssaultRifle;
        public Weapon Secondary { get; set; } = Weapon.DefaultLightPistol;
        public int Gear1 { get; set; } = 1;
        public int Gear2 { get; set; } = 2;
        public int Gear3 { get; set; } = 0;
        public int Gear4 { get; set; } = 0;
        public int Tactical { get; set; } = 0;

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }

        public static Loadout DefaultLoadout1 { get; } = new Loadout() { Primary = Weapon.DefaultAssaultRifle, Secondary = Weapon.DefaultLightPistol };
        public static Loadout DefaultLoadout2 { get; } = new Loadout() { Primary = Weapon.DefaultSubmachineGun, Secondary = Weapon.DefaultLightPistol };
        public static Loadout DefaultLoadout3 { get; } = new Loadout() { Primary = Weapon.DefaultBAR, Secondary = Weapon.DefaultLightPistol };

        internal static ImportItem GetGear(int GearID)
        {
            return ImportSystem.Gear.attachments[GearID];
        }

        internal ImportItem GetTactical()
        {
            return ImportSystem.Gear.tactical[Tactical];
        }
    }
}