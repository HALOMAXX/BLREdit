using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public class GitlabPackageLink
{
    [JsonPropertyName("web_path")]
    public string? WebPath { get; set; }
}