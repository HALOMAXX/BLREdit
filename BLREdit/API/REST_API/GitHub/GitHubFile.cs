using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.GitHub;

public class GitHubFile
{
    public string type { get; set; }
    public string encoding { get; set; }
    public int size { get; set; }
    public string name { get; set; }
    public string path { get; set; }
    public string content { get; set; }
    public string sha { get; set; }
    public string url { get; set; }
    public string git_url { get; set; }
    public string html_url { get; set; }
    public string download_url { get; set; }
    public GitHubLink _links { get; set; }
    private string _decoded_content;
    [JsonIgnore] public string decoded_content { 
        get {
            if (_decoded_content is null)
            { 
                _decoded_content = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content));
            }
            return _decoded_content;
        } }
}
