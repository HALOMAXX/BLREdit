namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabAuthor
{
    public int id { get; set; }
    public string name { get; set; }
    public string username { get; set; }
    public string state { get; set; }
    public string avatar_url { get; set; }
    public string web_url { get; set; }
}
