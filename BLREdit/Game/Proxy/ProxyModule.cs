using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Game.Proxy;

public sealed class ProxyModule
{
    public static void Save() { IOResources.SerializeFile($"ModuleCache.json", CachedModules); }
    public static ObservableCollection<ProxyModule> CachedModules { get; set; } = IOResources.DeserializeFile<ObservableCollection<ProxyModule>>($"ModuleCache.json") ?? new();

    private string? installName = null;
    public string InstallName { get { installName ??= $"{ModuleName}-{RepoName}"; return installName; } set { installName = value; } }
    public string RepoName { get; set; } = "";
    public string AuthorName { get; set; } = "SuperEwald";
    public string ModuleName { get; set; } = "LoadoutManager";
    public DateTime Published { get; set; } = DateTime.MinValue;
    public string ReleaseID { get; set; } = string.Empty;
    public bool Client { get; set; } = true;
    public bool Server { get; set; } = true;
    [JsonIgnore] public int FileAppearances { get; set; } = 0;

    public ProxyModule() { }
    public ProxyModule(string customInstallName) { Client = true; Server = true; FileAppearances = 1; installName = customInstallName; }
    public ProxyModule(GitHubRelease? release, string moduleName, string owner, string repo, bool client, bool server)
    {
        RepoName = $"{owner}/{repo}".Replace('/', '-');
        AuthorName = release?.Author?.URL ?? string.Empty;
        ModuleName = moduleName;
        Published = release?.PublishedAt ?? DateTime.MinValue;
        Client = client;
        Server = server;
    }

    public ProxyModule(GitlabRelease? release, string moduleName, string owner, string repo, bool client, bool server)
    {
        RepoName = $"{owner}/{repo}".Replace('/', '-');
        AuthorName = release?.Author?.WebURL ?? string.Empty;
        ModuleName = moduleName;
        Published = release?.ReleasedAt ?? DateTime.MinValue;
        Client = client;
        Server = server;
    }
}

class ProxyModuleComparer : IEqualityComparer<ProxyModule>
{
    bool IEqualityComparer<ProxyModule>.Equals(ProxyModule x, ProxyModule y)
    {
        if(x is null || y is null) return false;
        return x.InstallName == y.InstallName;
    }

    int IEqualityComparer<ProxyModule>.GetHashCode(ProxyModule obj)
    {
        //Check whether the object is null
        if (obj is null) return 0;

        //Get hash code for the Name field if it is not null.
        int hashProductName = obj.InstallName == null ? 0 : obj.InstallName.GetHashCode();

        //Calculate the hash code for the product.
        return hashProductName;
    }
}