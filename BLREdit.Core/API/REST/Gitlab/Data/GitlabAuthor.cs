using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.Gitlab;

public sealed class GitlabAuthor
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarURL { get; set; }
    [JsonPropertyName("web_url")]
    public string WebURL { get; set; }

    [JsonConstructor]
    public GitlabAuthor(int iD, string name, string username, string state, string avatarURL, string webURL)
    {
        ID = iD;
        Name = name;
        Username = username;
        State = state;
        AvatarURL = avatarURL;
        WebURL = webURL;
    }
}
