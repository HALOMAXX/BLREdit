using BLREdit.Game.Proxy;

using System.IO;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.GitHub;

public static class GitHubClient
{
    public static readonly RESTAPIClient Client = new(RepositoryProvider.GitHub, "https://api.github.com/");

    public static async Task<GitHubRelease?> GetLatestRelease(string owner, string repo)
    {
        return await Client.GetLatestRelease<GitHubRelease>(owner, repo);
    }

    public static async Task<GitHubRelease[]?> GetReleases(string owner, string repo, int amount = 10, int page = 1)
    {
        return await Client.GetReleases<GitHubRelease>(owner, repo, amount, page);
    }

    public static async Task<GitHubFile?> GetFile(string owner, string repo, string branch, string file)
    {
        return await Client.GetFile<GitHubFile>(owner, repo, branch, file);
    }

    public static (bool, string) DownloadFileFromRelease(GitHubRelease? release, string destFile, string file, string fileExt = ".dll")
    {
        if (release is null || release.Assets is null) return (false,string.Empty);
        string downloadLink = string.Empty;
        foreach (var asset in release.Assets)
        {
            if (asset.Name is not null && asset.Name.StartsWith(file) && asset.Name.EndsWith(fileExt))
            {
                downloadLink = asset.BrowserDownloadURL ?? string.Empty;
                break;
            }
        }
        Directory.CreateDirectory(RESTAPIClient.DOWNLOAD);
        string downloadTarget = $"{RESTAPIClient.DOWNLOAD}{destFile}";
        if (File.Exists(downloadTarget)) File.Delete(downloadTarget);

        return (WebResources.DownloadFile(downloadLink, downloadTarget), downloadTarget);
    }
}