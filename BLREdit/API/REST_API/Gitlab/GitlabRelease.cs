using System;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabRelease
{
    public string tag_name { get; set; }
    public string description { get; set; }
    public string name { get; set; }
    public DateTime created_at { get; set; }
    public DateTime released_at { get; set; }
    public GitlabAuthor author { get; set; }
    public GitlabCommit commit { get; set; }
    public GitlabMilestone[] milestones { get; set; }
    public string commit_path { get; set; }
    public string tag_path { get; set; }
    public GitlabAssets assets { get; set; }
    public GitlabEvidence[] evidences { get; set; }
}
