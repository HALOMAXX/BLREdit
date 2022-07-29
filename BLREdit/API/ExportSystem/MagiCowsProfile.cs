using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit
{
    public class MagiCowsProfile
    {
        [JsonIgnore] private string playerName = "Player";
        public string PlayerName { get { return playerName; } set { if (playerName != value) { playerName = value; isDirty = true; } } }
        public MagiCowsLoadout Loadout1 { get; set; } = MagiCowsLoadout.DefaultLoadout1.Clone();
        public MagiCowsLoadout Loadout2 { get; set; } = MagiCowsLoadout.DefaultLoadout2.Clone();
        public MagiCowsLoadout Loadout3 { get; set; } = MagiCowsLoadout.DefaultLoadout3.Clone();
        [JsonIgnore] bool isDirty = true;
        [JsonIgnore] public bool IsDirty { get { return (isDirty || Loadout1.IsDirty || Loadout2.IsDirty || Loadout3.IsDirty); } set { isDirty = value; Loadout1.IsDirty = value; Loadout2.IsDirty = value; Loadout3.IsDirty = value; } }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }

        public MagiCowsProfile Clone()
        {
            MagiCowsProfile clone = this.MemberwiseClone() as MagiCowsProfile;
            clone.PlayerName = string.Copy(this.PlayerName);
            clone.Loadout1 = this.Loadout1.Clone();
            clone.Loadout2 = this.Loadout2.Clone();
            clone.Loadout3 = this.Loadout3.Clone();
            clone.isDirty = true;
            return clone;
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