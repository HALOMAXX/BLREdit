using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.GitHub;

public sealed class GitHubLink
{
    [JsonPropertyName("git")]
    public string Git { get; set; }
    [JsonPropertyName("self")]
    public string Self { get; set; }
    [JsonPropertyName("html")]
    public string Html { get; set; }

    [JsonConstructor]
    public GitHubLink(string git, string self, string html)
    {
        Git = git;
        Self = self;
        Html = html;
    }
}
