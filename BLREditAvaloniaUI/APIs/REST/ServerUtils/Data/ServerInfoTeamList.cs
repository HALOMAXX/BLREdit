using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BLREdit;

public sealed class ServerInfoTeamList
{ 
    public int TeamScore { get; set; }
    public List<ServerInfoAgent> BotList { get; set; }
    public List<ServerInfoAgent> PlayerList { get; set; }

    [JsonConstructor]
    public ServerInfoTeamList(int teamScore, List<ServerInfoAgent> botList, List<ServerInfoAgent> playerList)
    {
        TeamScore = teamScore;
        BotList = botList;
        PlayerList = playerList;
    }
}
