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
        public string ProfileName { get; set; } = "0";
        public string Name { get { return '(' + ProfileName + ')' + PlayerName; } }
        public int Index { get { return int.Parse(ProfileName); } }
        [JsonIgnore]
        public string OriginFileName { get; set; }
    }
}
