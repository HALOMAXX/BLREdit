using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace BLREdit.Models;

public sealed class BLRGameMode : ModelBase
{
    public static DirectoryInfo GameModeInfoLocation { get; }
    public static RangeObservableCollection<BLRGameMode> GameModes { get; }

    public string GameModeName { get; set; } = "DM";
    public string Description { get; set; } = "Deathmatch";
    public string PlaylistLaunchArg { get; set; } = "Playlist=DM";
    public string GameModeLaunchArg { get; set; } = "Game=FoxGame.FoxGameMP_DM";
    public bool IsTeammode { get; set; } = true;

    static BLRGameMode()
    {
        GameModeInfoLocation = new("Data\\GameModeInfos");
        GameModes = new(IOResources.DeserializeDirectory<BLRGameMode>(GameModeInfoLocation));
    }

    public static BLRGameMode GetGameMode(string modeName)
    {
        foreach (var mode in GameModes)
        {
            if (modeName.Equals(mode.GameModeName))
            {
                return mode;
            }
        }
        return new();
    }
}