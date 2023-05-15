using BLREdit.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BLREdit;

public class ServerInfo
{
    public string GameMode { get; set; }
    public int GoalScore { get; set; }
    public string Map { get; set; }
    public int MaxPlayers { get; set; }
    public string Playlist { get; set; }
    public int RemainingTime { get; set; }
    public string ServerName { get; set; }
    public List<ServerInfoTeamList> TeamList { get; set; }
    public int TimeLimit { get; set; }
    private BLRMap? _mapInfo;
    private BLRGameMode? _gameModeInfo;
    [JsonIgnore] public BLRMap MapInfo { get { _mapInfo ??= BLRMap.GetMap(Map); return _mapInfo; } }
    [JsonIgnore] public BLRGameMode GameModeInfo { get { _gameModeInfo ??= BLRGameMode.GetGameMode(GameMode); return _gameModeInfo; } }

    [JsonConstructor]
    public ServerInfo(string gameMode, int goalScore, string map, int maxPlayers, string playlist, int remainingTime, string serverName, List<ServerInfoTeamList> teamList, int timeLimit)
    {
        GameMode = gameMode;
        GoalScore = goalScore;
        Map = map;
        MaxPlayers = maxPlayers;
        Playlist = playlist;
        RemainingTime = remainingTime;
        ServerName = serverName;
        TeamList = teamList;
        TimeLimit = timeLimit;
    }
}
