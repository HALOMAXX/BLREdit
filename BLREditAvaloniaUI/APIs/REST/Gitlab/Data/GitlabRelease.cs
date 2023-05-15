using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("released_at")]
    public DateTime ReleasedAt { get; set; }
    [JsonPropertyName("author")]
    public GitlabAuthor Author { get; set; }
    [JsonPropertyName("commit")]
    public GitlabCommit Commit { get; set; }
    [JsonPropertyName("milestones")]
    public GitlabMilestone[] Milestones { get; set; }
    [JsonPropertyName("commit_path")]
    public string CommitPath { get; set; }
    [JsonPropertyName("tag_path")]
    public string TagPath { get; set; }
    [JsonPropertyName("assets")]
    public GitlabAssets Assets { get; set; }
    [JsonPropertyName("evidences")]
    public GitlabEvidence[] Evidences { get; set; }

    [JsonConstructor]
    public GitlabRelease(string tagName, string description, string name, DateTime createdAt, DateTime releasedAt, GitlabAuthor author, GitlabCommit commit, GitlabMilestone[] milestones, string commitPath, string tagPath, GitlabAssets assets, GitlabEvidence[] evidences)
    {
        TagName = tagName;
        Description = description;
        Name = name;
        CreatedAt = createdAt;
        ReleasedAt = releasedAt;
        Author = author;
        Commit = commit;
        Milestones = milestones;
        CommitPath = commitPath;
        TagPath = tagPath;
        Assets = assets;
        Evidences = evidences;
    }
}
