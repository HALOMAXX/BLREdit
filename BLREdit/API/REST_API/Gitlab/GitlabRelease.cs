using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabRelease
{
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("released_at")]
    public DateTime ReleasedAt { get; set; }
    [JsonPropertyName("author")]
    public GitlabAuthor? Author { get; set; }
    [JsonPropertyName("commit")]
    public GitlabCommit? Commit { get; set; }
    [JsonPropertyName("milestones")]
    public GitlabMilestone[]? Milestones { get; set; }
    [JsonPropertyName("commit_path")]
    public string? CommitPath { get; set; }
    [JsonPropertyName("tag_path")]
    public string? TagPath { get; set; }
    [JsonPropertyName("assets")]
    public GitlabAssets? Assets { get; set; }
    [JsonPropertyName("evidences")]
    public GitlabEvidence[]? Evidences { get; set; }
}
