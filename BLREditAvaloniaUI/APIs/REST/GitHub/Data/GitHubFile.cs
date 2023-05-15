using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.GitHub;

public sealed class GitHubFile
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("encoding")]
    public string Encoding { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("sha")] 
    public string SHA { get; set; }
    [JsonPropertyName("url")] 
    public string URL { get; set; }
    [JsonPropertyName("git_url")] 
    public string GitURL { get; set; }
    [JsonPropertyName("html_url")] 
    public string HtmlURL { get; set; }
    [JsonPropertyName("download_url")] 
    public string DownloadURL { get; set; }
    [JsonPropertyName("_links")] 
    public GitHubLink Links { get; set; }

    private string? decodedContent;
    [JsonIgnore] public string DecodedContent 
    { 
        get
        {
            decodedContent ??= System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Content));
            return decodedContent;
        } 
    }

    [JsonConstructor]
    public GitHubFile(string type, string encoding, int size, string name, string path, string content, string sHA, string uRL, string gitURL, string htmlURL, string downloadURL, GitHubLink links)
    {
        Type = type;
        Encoding = encoding;
        Size = size;
        Name = name;
        Path = path;
        Content = content;
        SHA = sHA;
        URL = uRL;
        GitURL = gitURL;
        HtmlURL = htmlURL;
        DownloadURL = downloadURL;
        Links = links;
    }
}
