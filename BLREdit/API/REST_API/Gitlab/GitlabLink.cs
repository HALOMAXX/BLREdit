namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabLink
{
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public bool external { get; set; }
    public string link_type { get; set; }
}
