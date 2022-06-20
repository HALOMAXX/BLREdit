using System.Collections.Generic;

namespace BLREdit
{
    public class ImportWeapons
    {
        public List<BLRItem> depot { get; set; }
        public List<BLRItem> primary { get; set; }
        public List<BLRItem> secondary { get; set; }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
