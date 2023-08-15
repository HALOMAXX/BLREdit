using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabLink
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("url")]
    public string? URL { get; set; }
    [JsonPropertyName("external")]
    public bool External { get; set; }
    [JsonPropertyName("link_type")]
    public string? LinkType { get; set; }
}
