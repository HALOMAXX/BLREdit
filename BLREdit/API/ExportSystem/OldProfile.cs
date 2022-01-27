using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit
{
    public class OldProfile
    {
        public string PlayerName { get; set; } = "Player";
        public Loadout Loadout1 { get; set; } = Loadout.DefaultLoadout1;
        public Loadout Loadout2 { get; set; } = Loadout.DefaultLoadout2;
        public Loadout Loadout3 { get; set; } = Loadout.DefaultLoadout3;
        public bool IsFemale { get; set; } = false;
        public string Helmet { get; set; }
        public string UpperBody { get; set; }
        public string LowerBody { get; set; }
        public int Camo { get; set; } = 0;

        public Profile ConvertToNew()
        {
            Profile profile = new Profile
            {
                PlayerName = PlayerName,
                Loadout1 = Loadout1,
                Loadout2 = Loadout2,
                Loadout3 = Loadout3,
                Camo = Camo,
                LowerBody = ImportSystem.GetLowerBodyID(ImportSystem.GetItemByName(LowerBody, ImportSystem.Gear.lowerBodies)),
                UpperBody = ImportSystem.GetUpperBodyID(ImportSystem.GetItemByName(UpperBody, ImportSystem.Gear.upperBodies)),
                Helmet = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(Helmet, ImportSystem.Gear.helmets))
            };
            return profile;
        }
    }
}
