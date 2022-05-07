using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit
{
    public class MagiCowsOldProfile
    {
        public string PlayerName { get; set; } = "Player";
        public MagiCowsLoadout Loadout1 { get; set; } = MagiCowsLoadout.DefaultLoadout1;
        public MagiCowsLoadout Loadout2 { get; set; } = MagiCowsLoadout.DefaultLoadout2;
        public MagiCowsLoadout Loadout3 { get; set; } = MagiCowsLoadout.DefaultLoadout3;
        public string Helmet { get; set; }
        public string UpperBody { get; set; }
        public string LowerBody { get; set; }
        public int Camo { get; set; }

        public ExportSystemProfile ConvertToNew()
        {
            ExportSystemProfile profile = new ExportSystemProfile
            {
                ProfileName = PlayerName,
                PlayerName = PlayerName,
                Loadout1 = Loadout1,
                Loadout2 = Loadout2,
                Loadout3 = Loadout3,
            };

            //Camo
            profile.Loadout1.Camo = Camo;
            profile.Loadout2.Camo = Camo;
            profile.Loadout3.Camo = Camo;
            //Helmet
            profile.Loadout1.Helmet = ImportSystem.GetIDByNameAndType("helmets", Helmet);
            profile.Loadout2.Helmet = ImportSystem.GetIDByNameAndType("helmets", Helmet);
            profile.Loadout3.Helmet = ImportSystem.GetIDByNameAndType("helmets", Helmet);
            //Upper Body
            profile.Loadout1.UpperBody = ImportSystem.GetIDByNameAndType("upperBodies", UpperBody);
            profile.Loadout2.UpperBody = ImportSystem.GetIDByNameAndType("upperBodies", UpperBody);
            profile.Loadout3.UpperBody = ImportSystem.GetIDByNameAndType("upperBodies", UpperBody);
            //Lower Body
            profile.Loadout1.LowerBody = ImportSystem.GetIDByNameAndType("lowerBodies", LowerBody);
            profile.Loadout2.LowerBody = ImportSystem.GetIDByNameAndType("lowerBodies", LowerBody);
            profile.Loadout3.LowerBody = ImportSystem.GetIDByNameAndType("lowerBodies", LowerBody);

            return profile;
        }
    }
}
