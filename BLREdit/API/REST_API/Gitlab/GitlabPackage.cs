using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public class GitlabPackage
{
    public string Owner { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("version")]
    public string? Version { get; set; }
    [JsonPropertyName("package_type")]
    public string? PackageType { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("_links")]
    public GitlabPackageLink? Links { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("last_downloaded_at")]
    public DateTime? LastDownloadedAt { get; set; }
    [JsonPropertyName("tags")]
    public object[]? Tags { get; set; }
    [JsonPropertyName("pipeline")]
    public GitlabPipeline? Pipeline { get; set; }
    [JsonPropertyName("pipelines")]
    public GitlabPipeline[]? Pipelines { get; set; }
}