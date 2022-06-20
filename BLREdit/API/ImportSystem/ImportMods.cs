using System.Collections.Generic;

namespace BLREdit
{
    public class ImportMods
    {
        public List<BLRItem> ammo { get; set; }
        public List<BLRItem> ammos { get; set; }
        public List<BLRItem> barrels { get; set; }
        public List<BLRItem> camosBody { get; set; }
        public List<BLRItem> camosWeapon { get; set; }
        public List<BLRItem> grips { get; set; }
        public List<BLRItem> magazines { get; set; }
        public List<BLRItem> muzzles { get; set; }
        public List<BLRItem> primarySkins { get; set; }
        public List<BLRItem> scopes { get; set; }
        public object[] secondarySkins { get; set; }
        public List<BLRItem> stocks { get; set; }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
