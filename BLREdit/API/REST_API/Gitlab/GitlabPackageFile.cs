using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public class GitlabPackageFile
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("package_id")]
    public int PackageID { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; }
    [JsonPropertyName("file_md5")]
    public string? FileMD5 { get; set; }
    [JsonPropertyName("file_sha1")]
    public string? FileSHA1 { get; set; }
    [JsonPropertyName("file_sha256")]
    public string? FileSHA256 { get; set; }
    [JsonPropertyName("pipelines")]
    public GitlabPipeline[]? Pipelines { get; set; }
}