using System;

namespace BLREdit
{
    public class Profile
    {
        public string PlayerName { get; set; } = "Player";
        public Loadout Loadout1 { get; set; } = Loadout.DefaultLoadout1;
        public Loadout Loadout2 { get; set; } = Loadout.DefaultLoadout2;
        public Loadout Loadout3 { get; set; } = Loadout.DefaultLoadout3;
        public bool IsFemale { get; set; } = false;
        public int Helmet { get; set; } = 0;
        public int UpperBody { get; set; } = 0;
        public int LowerBody { get; set; } = 0;
        public int Camo { get; set; } = 0;

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

        public void SaveProfile()
        {
            IOResources.Serialize(AppDomain.CurrentDomain.BaseDirectory + IOResources.PROFILE_DIR + "\\" + PlayerName + ".json", this);
        }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}