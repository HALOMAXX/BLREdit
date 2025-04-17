using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BLREdit.Game.Proxy;

public sealed class ProxyModule
{
    public string InstallName { get { return $"{PackageFileName}"; } }
    public string Owner { get; set; } = "blrevive";
    public string Repository { get; set; } = "modules/loadout-manager";
    public string ModuleName { get; set; } = "LoadoutManager";
    public string? FileName { get; set; }
    [JsonIgnore] public string PackageFileName { get { if (FileName is null) { return ModuleName; } else { return FileName; } } }
    public string AuthorName { get; set; } = "SuperEwald";
    public DateTime Published { get; set; } = DateTime.MinValue;
    public string ReleaseID { get; set; } = string.Empty;
    public bool Client { get; set; } = true;
    public bool Server { get; set; } = true;
    [JsonIgnore] public int FileAppearances { get; set; } = 0;

    public ProxyModule() { }
    public ProxyModule(string owner, string repo) { Client = true; Server = true; FileAppearances = 1; Owner = owner; Repository = repo; }
    public ProxyModule(GitHubRelease? release, string owner, string repo, string moduleName, string? fileName, bool client, bool server)
    {
        Owner = owner;
        Repository = repo;
        FileName = fileName;
        AuthorName = release?.Author?.URL ?? string.Empty;
        ModuleName = moduleName;
        Published = release?.PublishedAt ?? DateTime.MinValue;
        Client = client;
        Server = server;
    }

    public ProxyModule(GitlabRelease? release, string owner, string repo, string moduleName, string? fileName,bool client, bool server)
    {
        Owner = owner;
        Repository = repo;
        FileName = fileName;
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