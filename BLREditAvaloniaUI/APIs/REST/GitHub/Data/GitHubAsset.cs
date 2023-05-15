using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.GitHub;

public sealed class GitHubAsset
{
    [JsonPropertyName("url")]
    public string URL { get; set; }
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("label")]
    public object Label { get; set; }
    [JsonPropertyName("uploader")]
    public GitHubUser Uploader { get; set; }
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; }
    [JsonPropertyName("download_count")]
    public int DownloadCount { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadURL { get; set; }

    [JsonConstructor]
    public GitHubAsset(string uRL, int iD, string nodeId, string name, object label, GitHubUser uploader, string contentType, string state, int size, int downloadCount, DateTime createdAt, DateTime updatedAt, string browserDownloadURL)
    {
        URL = uRL;
        ID = iD;
        NodeId = nodeId;
        Name = name;
        Label = label;
        Uploader = uploader;
        ContentType = contentType;
        State = state;
        Size = size;
        DownloadCount = downloadCount;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        BrowserDownloadURL = browserDownloadURL;
    }
}

