namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabAssets
{
    public int count { get; set; }
    public GitlabSource[] sources { get; set; }
    public GitlabLink[] links { get; set; }
    public string evidence_file_path { get; set; }
}
