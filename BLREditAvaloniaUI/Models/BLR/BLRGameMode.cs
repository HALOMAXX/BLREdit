namespace BLREdit.Models;

public sealed class BLRGameMode
{
    public int GameModeID { get; set; } = -1;
    public string PlaylistLaunchArg { get; set; } = "";
    public string GameModeLaunchArg { get; set; } = "";
    public bool IsTeammode { get; set; } = true;
}