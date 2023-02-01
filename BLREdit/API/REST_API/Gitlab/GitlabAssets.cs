using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabAssets
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("sources")]
    public GitlabSource[] Sources { get; set; }
    [JsonPropertyName("links")]
    public GitlabLink[] Links { get; set; }
    [JsonPropertyName("evidence_file_path")]
    public string EvidenceFilePath { get; set; }
}
