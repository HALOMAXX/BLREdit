using System.Collections.Generic;

namespace BLREdit.Import;

public sealed class ImportMods
{
    public List<ImportItem> ammo { get; set; }
    public List<ImportItem> ammos { get; set; }
    public List<ImportItem> barrels { get; set; }
    public List<ImportItem> camosBody { get; set; }
    public List<ImportItem> camosWeapon { get; set; }
    public List<ImportItem> grips { get; set; }
    public List<ImportItem> magazines { get; set; }
    public List<ImportItem> muzzles { get; set; }
    public List<ImportItem> primarySkins { get; set; }
    public List<ImportItem> scopes { get; set; }
    public object[] secondarySkins { get; set; }
    public List<ImportItem> stocks { get; set; }

    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }
}
