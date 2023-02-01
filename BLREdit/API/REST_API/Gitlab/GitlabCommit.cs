using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabCommit
{
    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("short_id")]
    public string ShortID { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreateAt { get; set; }
    [JsonPropertyName("parent_ids")]
    public string[] ParentIDs { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; }
    [JsonPropertyName("author_email")]
    public string AuthorEmail { get; set; }
    [JsonPropertyName("authored_date")]
    public DateTime AuthoredDate { get; set; }
    [JsonPropertyName("committer_name")]
    public string CommitterName { get; set; }
    [JsonPropertyName("committer_email")]
    public string CommitterEmail { get; set; }
    [JsonPropertyName("committed_date")]
    public DateTime CommittedDate { get; set; }
}
