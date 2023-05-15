using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabEvidence
{
    [JsonPropertyName("sha")]
    public string SHA { get; set; }
    [JsonPropertyName("filepath")]
    public string Filepath { get; set; }
    [JsonPropertyName("collected_at")]
    public DateTime CollectedAt { get; set; }

    [JsonConstructor]
    public GitlabEvidence(string sHA, string filepath, DateTime collectedAt)
    {
        SHA = sHA;
        Filepath = filepath;
        CollectedAt = collectedAt;
    }
}
