using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace BLREdit.Game.Proxy;

public sealed class RepositoryProxyModule
{
    public RepositoryProvider RepositoryProvider { get; set; } = RepositoryProvider.Gitlab;
    [JsonIgnore] public string InstallName { get { return $"{ModuleName}-{Owner}-{Repository}".Replace('/', '-'); } }
    public string Owner { get; set; } = "blrevive";
    public string Repository { get; set; } = "modules/loadout-manager";
    public string ModuleName { get; set; } = "LoadoutManager";
    public bool Client { get; set; } = true;
    public bool Server { get; set; } = true;
    public bool Required { get; set; } = false;

    private GitHubRelease hubRelease = null;
    [JsonIgnore]
    public GitHubRelease GitHubRelease
    {
        get
        {
            if (RepositoryProvider == RepositoryProvider.GitHub)
            {
                if (hubRelease is null) { Task.Run(GetLatestReleaseInfo); }
                return hubRelease;
            }
            else
            {
                return null;
            }
        }
    }
    private GitlabRelease labRelease = null;
    [JsonIgnore]
    public GitlabRelease GitlabRelease
    {
        get
        {
            if (RepositoryProvider == RepositoryProvider.Gitlab)
            {
                if (labRelease is null) { Task.Run(GetLatestReleaseInfo); }
                return labRelease;
            }
            else
            {
                return null;
            }
        }
    }

    private bool lockLatestReleaseInfo = false;
    public async void GetLatestReleaseInfo()
    {
        if (!lockLatestReleaseInfo)
        {
            lockLatestReleaseInfo = true;
            try
            {
                if (RepositoryProvider == RepositoryProvider.GitHub)
                {
                    hubRelease ??= await GitHubClient.GetLatestRelease(Owner, Repository);
                }
                else
                {
                    labRelease ??= await GitlabClient.GetLatestRelease(Owner, Repository);
                }
            }
            catch(Exception error)
            {
                LoggingSystem.Log($"failed to get release info for {InstallName}\n{error}");
            }

            lockLatestReleaseInfo = false;
        }
    }

    private bool lockDownload = false;
    public async Task<ProxyModule> DownloadLatest()
    {
        if (lockDownload) return null;
        lockDownload = true;

        string dl = "";
        if (RepositoryProvider == RepositoryProvider.GitHub)
        {
            if (hubRelease is null) { lockLatestReleaseInfo = true; hubRelease = await GitHubClient.GetLatestRelease(Owner, Repository); lockLatestReleaseInfo = false; }
            foreach (var asset in hubRelease.assets)
            {
                if (asset.name.StartsWith(ModuleName) && asset.name.EndsWith(".dll"))
                {
                    dl = asset.browser_download_url;
                    break;
                }
            }
        }
        else
        {
            if (labRelease is null) { lockLatestReleaseInfo = true; labRelease = await GitlabClient.GetLatestRelease(Owner, Repository); lockLatestReleaseInfo = false; }
            foreach (var asset in labRelease.assets.links)
            {
                if (asset.name.StartsWith(ModuleName) && asset.name.EndsWith(".dll"))
                {
                    dl = asset.url;
                    break;
                }
            }

            if (string.IsNullOrEmpty(dl))
            {
                LoggingSystem.Log($"No file found in Asset links gonna go down the deep end!");
                Regex regex = new($@"(\/uploads\/\w+\/{ModuleName}.dll)");
                if (regex.Match(labRelease.description) is Match match)
                {
                    LoggingSystem.Log($"Found {match.Captures.Count} matches");
                    if (match.Captures.Count > 0)
                    {
                        foreach (var capture in match.Captures)
                        {
                            LoggingSystem.Log($"\t{capture}");
                            if (string.IsNullOrEmpty(dl))
                            {
                                dl = $"https://gitlab.com/{Owner}/{Repository}{capture}";
                            }
                        }
                    }
                }
            }
        }
        string dlTarget = $"downloads\\{InstallName}.dll";
        if (File.Exists(dlTarget)) { LoggingSystem.Log($"Deleting {dlTarget}"); File.Delete(dlTarget); }
        LoggingSystem.Log($"Downloading ({dl}) to ({dlTarget})");
        if (!Directory.Exists($"downloads\\")) { Directory.CreateDirectory($"downloads\\"); }
        IOResources.DownloadFile(dl, dlTarget);
        LoggingSystem.Log($"Finished Downloading {dl}");

        ProxyModule module;

        if (RepositoryProvider == RepositoryProvider.GitHub)
        { module = new ProxyModule(hubRelease, ModuleName, Owner, Repository, Client, Server); }
        else
        { module = new ProxyModule(labRelease, ModuleName, Owner, Repository, Client, Server); }

        ProxyModule toRemoveModule = null;
        foreach (var mod in ProxyModule.CachedModules)
        {
            if (mod.InstallName == module.InstallName)
            {
                toRemoveModule = mod;
                break;
            }
        }

        if (toRemoveModule is not null) ProxyModule.CachedModules.Remove(toRemoveModule);

        ProxyModule.CachedModules.Add(module);

        lockDownload = false;
        return module;
    }
}
