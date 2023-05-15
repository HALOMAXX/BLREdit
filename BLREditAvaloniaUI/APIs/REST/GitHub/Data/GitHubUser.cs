using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.GitHub;

public sealed class GitHubUser
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("node_id")]
    public string NodeID { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarURL { get; set; }
    [JsonPropertyName("gravatar_id")]
    public string GravatarID { get; set; }
    [JsonPropertyName("url")]
    public string URL { get; set; }
    [JsonPropertyName("html_url")]
    public string HtmlURL { get; set; }
    [JsonPropertyName("followers_url")]
    public string FollowersURL { get; set; }
    [JsonPropertyName("following_url")]
    public string FollowingURL { get; set; }
    [JsonPropertyName("gists_url")]
    public string GistsURL { get; set; }
    [JsonPropertyName("starred_url")]
    public string StarredURL { get; set; }
    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsURL { get; set; }
    [JsonPropertyName("organizations_url")]
    public string OrganizationsURL { get; set; }
    [JsonPropertyName("repos_url")]
    public string ReposURL { get; set; }
    [JsonPropertyName("events_url")]
    public string EventsURL { get; set; }
    [JsonPropertyName("received_events_url")]
    public string RecievedEventsURL { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("site_admin")]
    public bool SiteAdmin { get; set; }

    [JsonConstructor]
    public GitHubUser(string login, int iD, string nodeID, string avatarURL, string gravatarID, string uRL, string htmlURL, string followersURL, string followingURL, string gistsURL, string starredURL, string subscriptionsURL, string organizationsURL, string reposURL, string eventsURL, string recievedEventsURL, string type, bool siteAdmin)
    {
        Login = login;
        ID = iD;
        NodeID = nodeID;
        AvatarURL = avatarURL;
        GravatarID = gravatarID;
        URL = uRL;
        HtmlURL = htmlURL;
        FollowersURL = followersURL;
        FollowingURL = followingURL;
        GistsURL = gistsURL;
        StarredURL = starredURL;
        SubscriptionsURL = subscriptionsURL;
        OrganizationsURL = organizationsURL;
        ReposURL = reposURL;
        EventsURL = eventsURL;
        RecievedEventsURL = recievedEventsURL;
        Type = type;
        SiteAdmin = siteAdmin;
    }
}

