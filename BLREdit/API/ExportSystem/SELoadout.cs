namespace BLREdit
{
    public class SELoadout
    {
        SEWeapon Primary { get; set; }
        SEWeapon Secondary { get; set; }
        SEGear Gear { get; set; }

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
