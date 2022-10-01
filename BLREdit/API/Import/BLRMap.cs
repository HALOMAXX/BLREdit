using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Import;

public sealed class BLRMap
{
    private static readonly bool AddFolderLine = AppDomain.CurrentDomain.BaseDirectory.EndsWith("\\");

    public string MapName { get; set; }
    public string MagiCowName { get; set; }
    public string MapDescription { get; set; }

    public string LongImageName { get; set; }
    [JsonIgnore] public string LongImage 
    {
        get
        {
            if (string.IsNullOrEmpty(LongImageName)) { return ""; }
            if (!AddFolderLine)
            {
                return AppDomain.CurrentDomain.BaseDirectory + "\\Assets\\textures\\" + LongImageName;
            }
            else
            {
                return AppDomain.CurrentDomain.BaseDirectory + "Assets\\textures\\" + LongImageName;
            }
        }
    }
    public string SquareImageName { get; set; }
    [JsonIgnore] public string SquareImage 
    { 
        get 
        {
            if (string.IsNullOrEmpty(SquareImageName)) { return ""; }
            if (!AddFolderLine)
            {
                return AppDomain.CurrentDomain.BaseDirectory + "\\Assets\\textures\\" + SquareImageName;
            }
            else
            {
                return AppDomain.CurrentDomain.BaseDirectory + "Assets\\textures\\" + SquareImageName;
            }
        }
    }
    public List<string> Available { get; set; } = new();
    public List<string> SupportedPlaylists { get; set; } = new() { "DM", "KC", "TDM", "LMS", "LTS", "CTF", "KOTH","DOM", "SND", "OS_Medium", "OS_Easy", "OS_Hard" };
    [JsonIgnore] public List<BLRMode> SupportedGameModes { get; } = new();
}
