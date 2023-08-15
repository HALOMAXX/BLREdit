using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.GitHub;

public sealed class GitHubLink
{
    [JsonPropertyName("git")]
    public string? Git { get; set; }
    [JsonPropertyName("self")]
    public string? Self { get; set; }
    [JsonPropertyName("html")]
    public string? Html { get; set; }
}
