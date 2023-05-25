using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.Gitlab;

public sealed class GitlabLink
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("url")]
    public string URL { get; set; }
    [JsonPropertyName("external")]
    public bool External { get; set; }
    [JsonPropertyName("link_type")]
    public string LinkType { get; set; }

    [JsonConstructor]
    public GitlabLink(int iD, string name, string uRL, bool external, string linkType)
    {
        ID = iD;
        Name = name;
        URL = uRL;
        External = external;
        LinkType = linkType;
    }
}
