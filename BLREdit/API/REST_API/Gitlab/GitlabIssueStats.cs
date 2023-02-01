using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabIssueStats
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    [JsonPropertyName("closed")]
    public int Closed { get; set; }
}
