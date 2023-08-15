using BLREdit.API.REST_API.Server;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit;

public class ServerInfo
{
    public string? GameModeFullName { get; set; }
    public ObservableCollection<ServerTeam> TeamsList { get; set; } = new();
    public int TimeLimit { get; set; }
    public int RemainingTime { get; set; }

    public string GetTimeDisplay()
    {
        var limit = new TimeSpan(0, 0, TimeLimit);
        var remaining = new TimeSpan(0, 0, RemainingTime);
        return $"{remaining} / {limit}";
    }

    public string GetScoreDisplay()
    {
        var player = new ServerAgent() { Name = "", Score = int.MinValue, Deaths = int.MinValue, Kills = int.MinValue };
        var team = new ServerTeam() { TeamScore = int.MinValue };
        switch (GameModeFullName)
        {
            case "Team Deathmatch":

                foreach (var serverTeam in TeamsList)
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

                foreach (var serverTeam in TeamsList)
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
}
