namespace BLREdit
{
    public class SELoadout
    {
        public SEWeapon Primary { get; set; }
        public SEWeapon Secondary { get; set; }
        public SEGear Gear { get; set; }

        public static SELoadout[] CreateFromMagiCowsProfile(MagiCowsProfile profile)
        { 
            return new SELoadout[] {
                SELoadout.CreateFromMagiCowsLoadout(profile.Loadout1),
                SELoadout.CreateFromMagiCowsLoadout(profile.Loadout2),
                SELoadout.CreateFromMagiCowsLoadout(profile.Loadout3),
            };

        }

        public static SELoadout CreateFromMagiCowsLoadout(MagiCowsLoadout loadout)
        {
            return new SELoadout { 
                Primary = SEWeapon.CreateFromMagiCowsWeapon(loadout.Primary),
                Secondary = SEWeapon.CreateFromMagiCowsWeapon(loadout.Secondary),
                Gear = SEGear.CreateFromMagiCowsLoadout(loadout)
            };
        }
    }
}
