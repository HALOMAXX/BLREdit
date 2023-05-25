using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.Gitlab;

public sealed class GitlabIssueStats
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    [JsonPropertyName("closed")]
    public int Closed { get; set; }

    [JsonConstructor]
    public GitlabIssueStats(int total, int closed)
    {
        Total = total;
        Closed = closed;
    }
}
