using BLREdit.Core.Models.BLR.Server;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.ServerUtils;

public class ServerInfo
{
    public string GameMode { get; set; } = "DM";
    public int GoalScore { get; set; }
    public string Map { get; set; } = "FoxEntry";
    public int MaxPlayers { get; set; }
    public string Playlist { get; set; } = "DM";
    public int RemainingTime { get; set; } 
    public string ServerName { get; set; } = string.Empty;
    public RangeObservableCollection<ServerInfoTeamList> TeamList { get; set; } = new();
    public int TimeLimit { get; set; }
    private BLRMap? _mapInfo;
    private BLRGameMode? _gameModeInfo;
    [JsonIgnore] public BLRMap MapInfo { get { _mapInfo ??= BLRMap.GetMap(Map); return _mapInfo; } }
    [JsonIgnore] public BLRGameMode GameModeInfo { get { _gameModeInfo ??= BLRGameMode.GetGameMode(GameMode); return _gameModeInfo; } }

    public override bool Equals(object? obj)
    {
        if (obj is ServerInfo info)
        { 
            return 
                GoalScore.Equals(info.GoalScore) &&
                MaxPlayers.Equals(info.MaxPlayers) &&
                RemainingTime.Equals(info.RemainingTime) &&
                ServerName.Equals(info.ServerName, StringComparison.Ordinal) &&
                TeamEquals(TeamList, info.TeamList) &&
                TimeLimit.Equals(info.TimeLimit) &&
                MapInfo.Equals(info.MapInfo) &&
                GameModeInfo.Equals(info.GameModeInfo);
        }
        return false;
    }

    private static bool TeamEquals(RangeObservableCollection<ServerInfoTeamList> teamListA, RangeObservableCollection<ServerInfoTeamList> teamListB)
    {
        if (teamListA.Count == teamListB.Count)
        {
            for (int i = 0; i < teamListA.Count; i++)
            {
                if (!teamListA[i].Equals(teamListB[i])) return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(GoalScore);
        hash.Add(MaxPlayers);
        hash.Add(RemainingTime);
        hash.Add(ServerName);
        hash.Add(TimeLimit);

        foreach (var team in TeamList)
        {
            hash.Add(team);
        }

        hash.Add(MapInfo);
        hash.Add(GameModeInfo);

        return hash.ToHashCode();
    }
}
