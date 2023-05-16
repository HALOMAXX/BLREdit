using BLREdit.Models.BLR;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BLREdit;

public class ServerInfo
{
    public string GameMode { get; set; } = "DM";
    public int GoalScore { get; set; } = 0;
    public string Map { get; set; } = "FoxEntry";
    public int MaxPlayers { get; set; } = 0;
    public string Playlist { get; set; } = "DM";
    public int RemainingTime { get; set; } = 0;
    public string ServerName { get; set; } = string.Empty;
    public List<ServerInfoTeamList> TeamList { get; set; } = new();
    public int TimeLimit { get; set; } = 0;
    private BLRMap? _mapInfo;
    private BLRGameMode? _gameModeInfo;
    [JsonIgnore] public BLRMap MapInfo { get { _mapInfo ??= BLRMap.GetMap(Map); return _mapInfo; } }
    [JsonIgnore] public BLRGameMode GameModeInfo { get { _gameModeInfo ??= BLRGameMode.GetGameMode(GameMode); return _gameModeInfo; } }
}
