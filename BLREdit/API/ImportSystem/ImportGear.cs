using System.Collections.Generic;

namespace BLREdit
{
    public class ImportGear
    {
        public List<BLRItem> attachments { get; set; }
        public List<BLRItem> avatars { get; set; }
        public List<BLRItem> badges { get; set; }
        public object[] crosshairs { get; set; }
        public List<BLRItem> emotes { get; set; }
        public List<BLRItem> hangers { get; set; }
        public List<BLRItem> helmets { get; set; }
        public List<BLRItem> lowerBodies { get; set; }
        public List<BLRItem> tactical { get; set; }
        public List<BLRItem> upperBodies { get; set; }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
