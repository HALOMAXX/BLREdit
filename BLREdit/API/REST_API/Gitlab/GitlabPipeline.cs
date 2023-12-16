using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public class GitlabPipeline
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("iid")]
    public int IID { get; set; }
    [JsonPropertyName("project_id")]
    public int ProjectId { get; set; }
    [JsonPropertyName("sha")]
    public string? SHA { get; set; }
    [JsonPropertyName("_ref")]
    public string? Ref { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    [JsonPropertyName("web_url")]
    public string? WebURL { get; set; }
    [JsonPropertyName("user")]
    public GitlabAuthor? User { get; set; }
}