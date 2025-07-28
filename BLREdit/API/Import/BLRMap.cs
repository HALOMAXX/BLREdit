using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Import;

public sealed class BLRMap
{
    [JsonIgnore] public string DisplayName { get { if (string.IsNullOrEmpty(MapDisplayName)) { return MapName; } else { return MapDisplayName; } } }
    public string MapDisplayName { get; set; } = "";
    public string MapName { get; set; } = "";
    public string PlaylistProviderName { get; set; } = "";
    public string MagiCowName { get; set; } = "";
    public string MapDescription { get; set; } = "";

    public string LongImageName { get; set; } = "";
    [JsonIgnore] public string LongImage 
    {
        get
        {
            if (string.IsNullOrEmpty(LongImageName)) { return ""; }
            return IOResources.BaseDirectory + "Assets\\textures\\" + LongImageName;
        }
    }
    public string SquareImageName { get; set; } = "";
    [JsonIgnore] public string SquareImage 
    { 
        get 
        {
            if (string.IsNullOrEmpty(SquareImageName)) { return ""; }
            return IOResources.BaseDirectory + "Assets\\textures\\" + SquareImageName;
        }
    }
#pragma warning disable CA2227 // Collection properties should be read only
    public Collection<string> Available { get; set; } = [];
    public Collection<string> SupportedPlaylists { get; set; } = ["DM", "KC", "TDM", "LMS", "LTS", "CTF", "KOTH","DOM", "SND", "OS_Medium", "OS_Easy", "OS_Hard"];
    [JsonIgnore] public Collection<BLRMode> SupportedGameModes { get; } = [];
#pragma warning restore CA2227 // Collection properties should be read only
}
