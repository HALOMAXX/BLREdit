using System;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabMilestone
{
    public int id { get; set; }
    public int iid { get; set; }
    public int project_id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string state { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string due_date { get; set; }
    public string start_date { get; set; }
    public string web_url { get; set; }
    public GitlabIssueStats issue_stats { get; set; }
}
