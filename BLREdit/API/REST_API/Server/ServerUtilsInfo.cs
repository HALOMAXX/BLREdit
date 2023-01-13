using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
namespace BLREdit.API.REST_API.Server;

public sealed class ServerUtilsInfo
{
    public bool IsOnline { get; set; } = false;
    public int BotCount { get; set; } = 0;
    public string GameMode { get; set; } = "";
    public string GameModeFullName { get; set; } = "";
    public int GoalScore { get; set; } = 0;
    public string Map { get; set; } = "FoxEntry";
    public int MaxPlayers { get; set; } = 0;
    public int PlayerCount { get; set; } = 0;
    public string Playlist { get; set; } = "";
    public int RemainingTime { get; set; } = 0;
    public string ServerName { get; set; } = "";
    public ObservableCollection<ServerUtilsTeam> TeamList { get; set; } = new ObservableCollection<ServerUtilsTeam>();
    public int TimeLimit { get; set; } = 0;

    public string MatchStats { get { return $"Time: {GetTimeDisplay()}\nLeader: {GetScoreDisplay()}"; } }

    //TODO Add Team Id

    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }

    public string GetTimeDisplay()
    {
        var limit = new TimeSpan(0, 0, TimeLimit);
        var remaining = new TimeSpan(0, 0, RemainingTime);
        return $"{remaining} / {limit}";
    }

    public string GetScoreDisplay()
    {
        var player = new ServerUtilsAgent() { Name = "", Score = int.MinValue, Deaths = int.MinValue, Kills = int.MinValue };
        var team = new ServerUtilsTeam() { TeamScore = int.MinValue };
        switch (GameModeFullName)
        {
            case "Team Deathmatch":
                
                foreach (var serverTeam in TeamList)
                {
                    if (serverTeam.TeamScore > team.TeamScore)
                    {
                        team = serverTeam;
                    }
                }
                foreach (var agent in team.BotList)
                {
                    if (agent.Score > player.Score)
                    { player = agent; }
                }
                foreach (var agent in team.PlayerList)
                {
                    if (agent.Score > player.Score)
                    { player = agent; }
                }
                if (player.Name == "" && player.Score == int.MinValue && player.Deaths == int.MinValue && player.Kills == int.MinValue)
                { return ""; }
                return $"[{player.Name}]: ({player.Score}) {player.Kills}/{player.Deaths}";
            case "Deathmatch":
                
                foreach (var serverTeam in TeamList)
                {
                    foreach (var agent in serverTeam.BotList)
                    {
                        if (agent.Score > player.Score)
                            player = agent;
                    }
                    foreach (var agent in serverTeam.PlayerList)
                    {
                        if (agent.Score > player.Score)
                            player = agent;
                    }
                }
                if (player.Name == "" && player.Score == int.MinValue && player.Deaths == int.MinValue && player.Kills == int.MinValue)
                { return ""; }
                return $"[{player.Name}]: ({player.Score}) {player.Kills}/{player.Deaths}";

            default:
                return "";
        }
    }

    private BLRMap map;
    [JsonIgnore]
    public BLRMap BLRMap
    {
        get
        {
            if (map is null) { foreach (var m in MapModeSelect.Maps) { if (m.MapName.ToLower() == Map.ToLower()) { map = m; break; } } }
            return map;
        }
    }

    private BLRMode mode;
    [JsonIgnore]
    public BLRMode BLRMode
    {
        get 
        {
            if (mode is null) { foreach (var m in MapModeSelect.Modes) { if (m.ModeName.ToLower() == GameMode.ToLower()) { mode = m; break; } } }
            return mode;
        }
    }

    private ObservableCollection<string> list;
    [JsonIgnore]
    public ObservableCollection<string> List
    {
        get
        {
            if (list is null) { list = new() { $"{PlayerCount}/{MaxPlayers} Players" }; foreach (var team in TeamList) { foreach (var player in team.PlayerList) { list.Add($"[{player.Name}]: ({player.Score}) {player.Kills}/{player.Deaths}"); } } }
            return list;
        }
    }

    private ObservableCollection<string> team1list;
    [JsonIgnore]
    public ObservableCollection<string> Team1List
    {
        get
        {
            if (team1list is null && TeamList.Count > 0) { team1list = new() { $"{TeamList[0].PlayerCount}/{MaxPlayers/2} Team 1" };  foreach (var player in TeamList[0].PlayerList) { team1list.Add($"[{player.Name}]: ({player.Score}) {player.Kills}/{player.Deaths}"); } }
            return team1list;
        }
    }

    private ObservableCollection<string> team2list;
    [JsonIgnore]
    public ObservableCollection<string> Team2List
    {
        get
        {
            if (team2list is null && TeamList.Count > 1) { team2list = new() { $"{TeamList[1].PlayerCount}/{MaxPlayers/2} Team 2" }; foreach (var player in TeamList[1].PlayerList) { team2list.Add($"[{player.Name}]: ({player.Score}) {player.Kills}/{player.Deaths}"); } }
            return team2list;
        }
    }

}

public sealed class ServerUtilsTeam
{ 
    public int BotCount { get; set; } = 0;
    public ObservableCollection<ServerUtilsAgent> BotList { get; set; } = new();
    public int PlayerCount { get; set; } = 0;
    public ObservableCollection<ServerUtilsAgent> PlayerList { get; set; } = new();
    public int TeamScore { get; set; } = 0;
}

public sealed class ServerUtilsAgent
{
    public int Deaths { get; set; } = 0;
    public int Kills { get; set; } = 0;
    public string Name { get; set; } = "Agent";
    public int Score { get; set; } = 0;
}
