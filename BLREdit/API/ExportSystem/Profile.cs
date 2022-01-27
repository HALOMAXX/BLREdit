using System;

namespace BLREdit
{
    public class Profile
    {
        public string PlayerName { get; set; } = "Player";
        public Loadout Loadout1 { get; set; } = Loadout.DefaultLoadout1;
        public Loadout Loadout2 { get; set; } = Loadout.DefaultLoadout2;
        public Loadout Loadout3 { get; set; } = Loadout.DefaultLoadout3;

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