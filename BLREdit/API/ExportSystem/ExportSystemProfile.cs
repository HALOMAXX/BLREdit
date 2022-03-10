using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit
{
    public class ExportSystemProfile : MagiCowsProfile
    {
        public string ProfileName { get; set; } = "0";
        public string Name { get { return PlayerName + '(' + ProfileName + ')'; } }
    }
}
