using System;
using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.Gitlab;

public sealed class GitlabMilestone
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("iid")]
    public int IID { get; set; }
    [JsonPropertyName("project_id")]
    public int ProjectID { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("due_date")]
    public string DueDate { get; set; }
    [JsonPropertyName("start_date")]
    public string StartDate { get; set; }
    [JsonPropertyName("web_url")]
    public string WebURL { get; set; }
    [JsonPropertyName("issue_stats")]
    public GitlabIssueStats IssueStats { get; set; }

    [JsonConstructor]
    public GitlabMilestone(int iD, int iID, int projectID, string title, string description, string state, DateTime createdAt, DateTime updatedAt, string dueDate, string startDate, string webURL, GitlabIssueStats issueStats)
    {
        ID = iD;
        IID = iID;
        ProjectID = projectID;
        Title = title;
        Description = description;
        State = state;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DueDate = dueDate;
        StartDate = startDate;
        WebURL = webURL;
        IssueStats = issueStats;
    }
}
