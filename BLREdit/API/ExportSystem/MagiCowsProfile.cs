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
    }
}