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

    public override bool Equals(object? obj)
    {
        if (obj is ServerInfoAgent agent)
        {
            return
                Name.Equals(agent.Name, StringComparison.Ordinal) &&
                Score.Equals(agent.Score) &&
                Deaths.Equals(agent.Deaths) &&
                Kills.Equals(agent.Kills);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Name);
        hash.Add(Score);
        hash.Add(Deaths);
        hash.Add(Kills);

        return hash.ToHashCode();
    }
}