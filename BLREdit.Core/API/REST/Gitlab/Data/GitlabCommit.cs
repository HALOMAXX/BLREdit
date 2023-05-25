using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Core.API.REST.Gitlab;

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
    public ObservableCollection<string> ParentIDs { get; set; }
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

    [JsonConstructor]
    public GitlabCommit(string iD, string shortID, string title, DateTime createAt, ObservableCollection<string> parentIDs, string message, string authorName, string authorEmail, DateTime authoredDate, string committerName, string committerEmail, DateTime committedDate)
    {
        ID = iD;
        ShortID = shortID;
        Title = title;
        CreateAt = createAt;
        ParentIDs = parentIDs;
        Message = message;
        AuthorName = authorName;
        AuthorEmail = authorEmail;
        AuthoredDate = authoredDate;
        CommitterName = committerName;
        CommitterEmail = committerEmail;
        CommittedDate = committedDate;
    }
}
