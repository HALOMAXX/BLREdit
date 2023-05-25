using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit;

public sealed class ServerInfoTeamList
{ 
    public int TeamScore { get; set; }
    public RangeObservableCollection<ServerInfoAgent> BotList { get; set; }
    public RangeObservableCollection<ServerInfoAgent> PlayerList { get; set; }

    [JsonConstructor]
    public ServerInfoTeamList(int teamScore, RangeObservableCollection<ServerInfoAgent> botList, RangeObservableCollection<ServerInfoAgent> playerList)
    {
        TeamScore = teamScore;
        BotList = botList;
        PlayerList = playerList;
    }

    public override bool Equals(object? obj)
    {
        if (obj is ServerInfoTeamList team)
        {
            if (BotList.Count == team.BotList.Count)
            {
                for (int i = 0; i < BotList.Count; i++)
                {
                    if (!BotList[i].Equals(team.BotList[i])) return false;
                }
            }
            else
            {
                return false;
            }

            if (PlayerList.Count == team.PlayerList.Count)
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    if (!PlayerList[i].Equals(team.PlayerList[i])) return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(TeamScore);

        foreach (var bot in BotList)
            hash.Add(bot);

        foreach (var player in PlayerList)
            hash.Add(player);

        return hash.ToHashCode();
    }
}
