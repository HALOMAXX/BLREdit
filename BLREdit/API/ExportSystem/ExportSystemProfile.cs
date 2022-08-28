using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit
{
    public class ExportSystemProfile : MagiCowsProfile
    {
        [JsonIgnore] public string Name { get { return '(' + Index.ToString() + ')' + PlayerName; } }
        public int Index { get; set; }

        public new ExportSystemProfile Clone()
        {
            ExportSystemProfile duplicate = base.Clone() as ExportSystemProfile;
            duplicate.Index = ExportSystem.Profiles.Count;
            return duplicate;
        }

        public MagiCowsLoadout GetLoadout(int id)
        {
            switch (id)
            {
                case 1: return Loadout1;
                case 2: return Loadout2;
                case 3: return Loadout3;
                default: return Loadout1;
            }
        }
    }
}
