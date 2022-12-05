using BLREdit.Import;
using BLREdit.UI.Windows;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
namespace BLREdit.API.REST_API.Server;

public sealed class ServerUtilsInfo
{
    public bool IsOnline { get; set; } = false;
    public int BotCount { get; set; } = 0;
    public string GameMode { get; set; } = "DM";
    [JsonIgnore] public string ModeName { get { return GameMode.ToUpper(); } set { } }
    public int GoalScore { get; set; } = 100;
    public string Map { get; set; } = "Lobby";
    public int MaxPlayers { get; set; } = 16;
    public int PlayerCount { get; set; } = 0;
    public int RemainingTime { get; set; } = 180;
    public string ServerName { get; set; } = "";
    public ServerUtilsTeam[] TeamList { get; set; } = new ServerUtilsTeam[] { new ServerUtilsTeam(), new ServerUtilsTeam() };
    public int TimeLimit { get; set; } = 180;

    private BLRMap map;
    [JsonIgnore]
    public BLRMap BLRMap
    {
        get
        {
            if (map is null) { foreach (var m in MapModeSelect.Maps) { if (m.MapName.ToLower() == Map.ToLower()) { map = m; break; } } }
            LoggingSystem.Log(Map);
            return map;
        }
    }

    private ObservableCollection<string> list;
    [JsonIgnore]
    public ObservableCollection<string> List
    {
        get
        {
            if (list is null) { list = new() { $"{PlayerCount}/16" }; foreach (var team in TeamList) { foreach (var player in team.PlayerList) { list.Add($"[{player.Name}]: ({player.Score}) {player.Kills}/{player.Deaths}"); } } }
            return list;
        }
    }

}

public sealed class ServerUtilsTeam
{ 
    public int BotCount { get; set; } = 0;
    public ServerUtilsAgent[] BotList { get; set; } = Array.Empty<ServerUtilsAgent>();
    public int PlayerCount { get; set; } = 0;
    public ServerUtilsAgent[] PlayerList { get; set; } = Array.Empty<ServerUtilsAgent>();
    public int TeamScore { get; set; } = 0;
}

public sealed class ServerUtilsAgent
{
    public int Deaths { get; set; } = 0;
    public int Kills { get; set; } = 0;
    public string Name { get; set; } = "Agent";
    public int Score { get; set; } = 0;
}
