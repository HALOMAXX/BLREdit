using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.GitHub;

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
    public ObservableCollection<GitHubAsset> Assets { get; set; }
    [JsonPropertyName("tarball_url")]
    public string TarballURL { get; set; }
    [JsonPropertyName("zipball_url")]
    public string ZipballURL { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("mentions_count")]
    public int MentionsCount { get; set; }

    [JsonConstructor]
    public GitHubRelease(string uRL, string assetsURL, string uploadURL, string htmlURL, int iD, GitHubUser author, string nodeID, string tagName, string targetCommitish, string name, bool draft, bool preRelease, DateTime createdAt, DateTime publishedAt, ObservableCollection<GitHubAsset> assets, string tarballURL, string zipballURL, string body, int mentionsCount)
    {
        URL = uRL;
        AssetsURL = assetsURL;
        UploadURL = uploadURL;
        HtmlURL = htmlURL;
        ID = iD;
        Author = author;
        NodeID = nodeID;
        TagName = tagName;
        TargetCommitish = targetCommitish;
        Name = name;
        Draft = draft;
        PreRelease = preRelease;
        CreatedAt = createdAt;
        PublishedAt = publishedAt;
        Assets = assets;
        TarballURL = tarballURL;
        ZipballURL = zipballURL;
        Body = body;
        MentionsCount = mentionsCount;
    }
}