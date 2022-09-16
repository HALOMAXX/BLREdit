using BLREdit.Game.Proxy;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.GitHub;

public static class GitHubClient
{
    public static readonly RESTAPIClient Client = new(RepositoryProvider.GitHub, "https://api.github.com/");

    public static async Task<GitHubRelease> GetLatestRelease(string owner, string repo)
    {
        return await Client.GetLatestRelease<GitHubRelease>(owner, repo);
    }

    public static async Task<GitHubRelease[]> GetReleases(string owner, string repo)
    {
        return await Client.GetReleases<GitHubRelease>(owner, repo);
    }

    public static async Task<GitHubFile> GetFile(string owner, string repo, string branch, string file)
    {
        return await Client.GetFile<GitHubFile>(owner, repo, branch, file);
    }
}