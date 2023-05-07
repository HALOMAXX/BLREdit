using HtmlAgilityPack;
using System;

namespace BLREdit.API.REST_API.GitHub;

public sealed class GitHubModuleRepository : ProxyModuleRepository
{
    private GitHubRelease? _latestReleaseInfo;
    public GitHubRelease? LatestReleaseInfo { get { return _latestReleaseInfo; } set { _latestReleaseInfo = value; OnPropertyChanged(); } }

    public override void DownloadModuleToCache()
    {
        if (IsDownloadingModuleToCache.Is) return;
        IsDownloadingModuleToCache.Set(true);
        foreach (var asset in LatestReleaseInfo.Assets)
        {
            if (asset.Name.StartsWith(Name) && asset.Name.EndsWith(".dll"))
            {
                IOResources.DownloadFile(asset.BrowserDownloadURL, $"downloads\\moduleCache\\{FullName}.dll");
                this.ReleaseTime = LatestReleaseInfo.PublishedAt;
                break;
            }
        }
        IsDownloadingModuleToCache.Set(false);
    }

    public override DateTime GetLatestReleaseDateTime()
    {
        if (LatestReleaseInfo is null)
        {
            var task = GitHubClient.GetLatestRelease(Owner, Repository);
            task.Wait();
            LatestReleaseInfo = task.Result;
        }
        return LatestReleaseInfo?.PublishedAt ?? DateTime.MinValue;
    }

    public override async void GetMetaData()
    {
        if (IsGettingMetaData.Is) return;
        IsGettingMetaData.Set(true);

        var rawHtml = await IOResources.HttpClientWeb.GetStringAsync($"https://github.com/{Owner}/{Repository}");
        HtmlDocument doc = new();
        doc.LoadHtml(rawHtml);

        foreach (var docNode in doc.DocumentNode.ChildNodes)
        {
            if (docNode.Name is "html")
            {
                foreach (var htmlNode in docNode.ChildNodes)
                {
                    if (htmlNode.Name is "head")
                    {
                        foreach (var headNode in htmlNode.ChildNodes)
                        {
                            if (headNode.Name is "meta")
                            {
                                if (headNode.Attributes.Count >= 2)
                                {
                                    if (!MetaData.ContainsKey(headNode.Attributes[0].Value)) { MetaData.Add(headNode.Attributes[0].Value, headNode.Attributes[1].Value); }
                                }
                            }
                        }
                    }
                }
            }
        }

        IsGettingMetaData.Set(false);
        NotifyMetaDataPropertiesChanged();
    }
}
