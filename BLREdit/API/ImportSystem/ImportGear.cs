using System.Collections.Generic;

namespace BLREdit;

public sealed class ImportGear
{
    public List<ImportItem> attachments { get; set; }
    public List<ImportItem> avatars { get; set; }
    public List<ImportItem> badges { get; set; }
    public object[] crosshairs { get; set; }
    public List<ImportItem> emotes { get; set; }
    public List<ImportItem> hangers { get; set; }
    public List<ImportItem> helmets { get; set; }
    public List<ImportItem> lowerBodies { get; set; }
    public List<ImportItem> tactical { get; set; }
    public List<ImportItem> upperBodies { get; set; }

    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }
}
