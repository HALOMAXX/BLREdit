﻿using System;

namespace BLREdit
{
    public class MagiCowsProfile
    {
        public string PlayerName { get; set; } = "Player";
        public MagiCowsLoadout Loadout1 { get; set; } = MagiCowsLoadout.DefaultLoadout1;
        public MagiCowsLoadout Loadout2 { get; set; } = MagiCowsLoadout.DefaultLoadout2;
        public MagiCowsLoadout Loadout3 { get; set; } = MagiCowsLoadout.DefaultLoadout3;

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}