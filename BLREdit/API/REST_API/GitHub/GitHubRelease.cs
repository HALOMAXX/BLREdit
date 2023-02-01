using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.GitHub;

public sealed class GitHubRelease
{
    [JsonPropertyName("url")]
    public string URL { get; set; }
    [JsonPropertyName("assets_url")]
    public string AssetsURL { get; set; }
    [JsonPropertyName("upload_url")]
    public string UploadURL { get; set; }
    [JsonPropertyName("html_url")]
    public string HtmlURL { get; set; }
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("author")]
    public GitHubUser Author { get; set; }
    [JsonPropertyName("node_id")]
    public string NodeID { get; set; }
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }
    [JsonPropertyName("target_commitish")]
    public string TargetCommitish { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("draft")]
    public bool Draft { get; set; }
    [JsonPropertyName("prerelease")]
    public bool PreRelease { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }
    [JsonPropertyName("assets")]
    public GitHubAsset[] Assets { get; set; }
    [JsonPropertyName("tarball_url")]
    public string TarballURL { get; set; }
    [JsonPropertyName("zipball_url")]
    public string ZipballURL { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("mentions_count")]
    public int MentionsCount { get; set; }
}