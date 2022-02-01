namespace BLREdit
{
    public class SEGear
    {
        public bool Female { get; set; } = false;
        public bool Bot { get; set; } = false;
        public int BodyCamo { get; set; } = 0;
        public int UpperBody { get; set; } = 0;
        public int LowerBody { get; set; } = 0;
        public int Helmet { get; set; } = 0;
        public int Badge { get; set; } = 0;
        public int Gear_R1 { get; set; } = 0;
        public int Gear_R2 { get; set; } = 0;
        public int Gear_L1 { get; set; } = 0;
        public int Gear_L2 { get; set; } = 0;
        public int Tactical { get; set; } = 0;
        public int ButtPack { get; set; } = 0;
        public int Avatar { get; set; } = 0;
        public int PatchIcon { get; set; } = 0;
        public int PatchIconColor { get; set; } = 0;
        public int PatchShape { get; set; } = 0;
        public int PatchShapeColor { get; set; } = 0;
        public int Hanger { get; set; } = 0;

        public static SEGear CreateFromMagiCowsLoadout(MagiCowsLoadout loadout)
        {
            return new SEGear {
                Female = loadout.IsFemale,
                BodyCamo = loadout.Camo,
                UpperBody = loadout.UpperBody,
                LowerBody = loadout.LowerBody,
                Helmet = loadout.Helmet,
                Gear_R1 = loadout.Gear1,
                Gear_R2 = loadout.Gear2,
                Gear_L1 = loadout.Gear3,
                Gear_L2 = loadout.Gear4,
                Tactical = loadout.Tactical
            };
        }
    }
}
