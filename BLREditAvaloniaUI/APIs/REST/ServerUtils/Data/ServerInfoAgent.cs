using System.Text.Json.Serialization;

namespace BLREdit;

public sealed class ServerInfoAgent
{
    public string Name { get; set; }
    public int Score { get; set; }
    public int Deaths { get; set; }
    public int Kills { get; set; }

    [JsonConstructor]
    public ServerInfoAgent(string name, int score, int deaths, int kills)
    {
        Name = name;
        Score = score;
        Deaths = deaths;
        Kills = kills;
    }
}