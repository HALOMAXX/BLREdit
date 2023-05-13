using System;

namespace BLREdit.Models;

public sealed class BLRMap
{
    public int MapID { get; set; } = -1;
    public string MapFilename { get; set; } = "";
    public string MagiCowInfoName { get; set; } = "";
    public string UtilsInfoName { get; set; } = "";
    public string[] SupportedGameVersions { get; set; } = Array.Empty<string>();
    public string[] SupportedPlaylists { get; set; } = Array.Empty<string>();
    public BLRGameMode[] SupportedGameModes { get; set; } = Array.Empty<BLRGameMode>();
}
