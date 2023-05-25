using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.Gitlab;

public sealed class GitlabAssets
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("sources")]
    public ObservableCollection<GitlabSource> Sources { get; set; }
    [JsonPropertyName("links")]
    public ObservableCollection<GitlabLink> Links { get; set; }
    [JsonPropertyName("evidence_file_path")]
    public string EvidenceFilePath { get; set; }

    [JsonConstructor]
    public GitlabAssets(int count, ObservableCollection<GitlabSource> sources, ObservableCollection<GitlabLink> links, string evidenceFilePath)
    {
        Count = count;
        Sources = sources;
        Links = links;
        EvidenceFilePath = evidenceFilePath;
    }
}
