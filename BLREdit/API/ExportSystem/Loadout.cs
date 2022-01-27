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
        public bool IsFemale { get; set; } = false;
        public int Helmet { get; set; } = 0;
        public int UpperBody { get; set; } = 0;
        public int LowerBody { get; set; } = 0;
        public int Camo { get; set; } = 0;

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }

        public static Loadout DefaultLoadout1 { get; } = new Loadout() { Primary = Weapon.DefaultAssaultRifle, Secondary = Weapon.DefaultLightPistol };
        public static Loadout DefaultLoadout2 { get; } = new Loadout() { Primary = Weapon.DefaultSubmachineGun, Secondary = Weapon.DefaultLightPistol };
        public static Loadout DefaultLoadout3 { get; } = new Loadout() { Primary = Weapon.DefaultBAR, Secondary = Weapon.DefaultLightPistol };

        public static ImportItem GetGear(int GearID)
        {
            return ImportSystem.Gear.attachments[GearID];
        }

        public ImportItem GetTactical()
        {
            return ImportSystem.Gear.tactical[Tactical];
        }
        public ImportItem GetHelmet()
        {
            return ImportSystem.GetItemByID(this.Helmet, ImportSystem.Gear.helmets);
        }

        public ImportItem GetUpperBody()
        {
            return ImportSystem.GetItemByID(this.UpperBody, ImportSystem.Gear.upperBodies);
        }

        public ImportItem GetLowerBody()
        {
            return ImportSystem.GetItemByID(this.LowerBody, ImportSystem.Gear.lowerBodies);
        }

        public ImportItem GetCamo()
        {
            return ImportSystem.GetItemByID(this.Camo, ImportSystem.Mods.camosBody);
        }
    }
}