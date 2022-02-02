﻿using System;
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

        public MagiCowsProfile ConvertToNew()
        {
            MagiCowsProfile profile = new MagiCowsProfile 
            {
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
            profile.Loadout1.Helmet = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(Helmet, ImportSystem.Gear.helmets));
            profile.Loadout2.Helmet = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(Helmet, ImportSystem.Gear.helmets));
            profile.Loadout3.Helmet = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(Helmet, ImportSystem.Gear.helmets));
            //Upper Body
            profile.Loadout1.UpperBody = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(UpperBody, ImportSystem.Gear.upperBodies));
            profile.Loadout2.UpperBody = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(UpperBody, ImportSystem.Gear.upperBodies));
            profile.Loadout3.UpperBody = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(UpperBody, ImportSystem.Gear.upperBodies));
            //Lower Body
            profile.Loadout1.LowerBody = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(LowerBody, ImportSystem.Gear.lowerBodies));
            profile.Loadout2.LowerBody = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(LowerBody, ImportSystem.Gear.lowerBodies));
            profile.Loadout3.LowerBody = ImportSystem.GetHelmetID(ImportSystem.GetItemByName(LowerBody, ImportSystem.Gear.lowerBodies));

            return profile;
        }
    }
}