using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BLREdit.Core.Models.BLR.Server;

public sealed class BLRGameMode : ModelBase
{
    public static DirectoryInfo GameModeInfoLocation { get; }
    public static RangeObservableCollection<BLRGameMode> GameModes { get; } = new();

    public string GameModeName { get; set; } = "TDM";
    public string Description { get; set; } = "Team Deathmatch";
    public string PlaylistLaunchArg { get; set; } = "Playlist=TDM";
    public string GameModeLaunchArg { get; set; } = "Game=FoxGame.FoxGameMP_TDM";
    public bool IsTeammode { get; set; } = true;

    static BLRGameMode()
    {
        GameModeInfoLocation = new("Data\\Modes");
        IOResources.DeserializeDirectoryInto(GameModes, GameModeInfoLocation);
    }

    public static BLRGameMode GetGameMode(string modeName)
    {
        if (modeName is not null)
        {
            foreach (var mode in GameModes)
            {
                if (modeName.Equals(mode.GameModeName, StringComparison.Ordinal))
                {
                    return mode;
                }
            }
        }
        return new();
    }
}

public class JsonBLRGameModeConverter : JsonGenericConverter<BLRGameMode>
{
    static JsonBLRGameModeConverter()
    {
        Default = new BLRGameMode() { GameModeName = "", Description = "", PlaylistLaunchArg = "", GameModeLaunchArg = "" };
        IOResources.JSOSerialization.Converters.Add(new JsonBLRGameModeConverter());
        IOResources.JSOSerializationCompact.Converters.Add(new JsonBLRGameModeConverter());
    }
}