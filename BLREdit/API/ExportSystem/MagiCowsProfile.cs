using System;

namespace BLREdit
{
    public class MagiCowsProfile
    {
        public string PlayerName { get; set; } = "Player";
        public MagiCowsLoadout Loadout1 { get; set; } = (MagiCowsLoadout)MagiCowsLoadout.DefaultLoadout1.Clone();
        public MagiCowsLoadout Loadout2 { get; set; } = (MagiCowsLoadout)MagiCowsLoadout.DefaultLoadout2.Clone();
        public MagiCowsLoadout Loadout3 { get; set; } = (MagiCowsLoadout)MagiCowsLoadout.DefaultLoadout3.Clone();

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }

        public bool IsHealthOkAndRepair()
        {
            bool isHealthy = true;
            if (!Loadout1.IsHealthOkAndRepair())
            {
                isHealthy = false;
            }
            if (!Loadout2.IsHealthOkAndRepair())
            {
                isHealthy = false;
            }
            if (!Loadout3.IsHealthOkAndRepair())
            {
                isHealthy = false;
            }
            return isHealthy;
        }
    }
}