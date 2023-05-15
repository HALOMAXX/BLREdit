using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabSource
{
    [JsonPropertyName("format")]
    public string Format { get; set; }
    [JsonPropertyName("url")]
    public string URL { get; set; }
    
    [JsonConstructor]
    public GitlabSource(string format, string uRL)
    {
        Format = format;
        URL = uRL;
    }
}
