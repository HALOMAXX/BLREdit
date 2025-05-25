using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Game.Proxy;

public sealed class RepositoryProxyModule
{
    public RepositoryProvider RepositoryProvider { get; set; } = RepositoryProvider.Gitlab;
    [JsonIgnore] public string InstallName { get { return $"{PackageFileName}"; } }
    public string Owner { get; set; } = "blrevive";
    public string Repository { get; set; } = "modules/loadout-manager";
    public string ModuleName { get; set; } = "LoadoutManager";
    public string? FileName { get; set; }
    [JsonIgnore] public string PackageFileName { get { if (FileName is null) { return ModuleName; } else { return FileName; } } }
    public bool Client { get; set; } = true;
    public bool Server { get; set; } = true;
    public bool Required { get; set; }
    public string ProxyVersion { get; set; } = "v1.0.0-beta.2";
    public List<ProxyModuleSetting> ModuleSettings { get; set; } = [];

    private GitHubRelease? hubRelease;
    [JsonIgnore]
    public GitHubRelease? GitHubRelease
    {
        get
        {
            if (RepositoryProvider == RepositoryProvider.GitHub)
            {
                if (hubRelease is null) { Task.Run(GetLatestReleaseInfo).Wait(); }
                return hubRelease;
            }
            else
            {
                return null;
            }
        }
    }
    private GitlabRelease? labRelease;
    [JsonIgnore]
    public GitlabRelease? GitlabRelease
    {
        get
        {
            if (RepositoryProvider == RepositoryProvider.Gitlab)
            {
                if (labRelease is null) { Task.Run(GetLatestReleaseInfo).Wait(); }
                return labRelease;
            }
            else
            {
                return null;
            }
        }
    }

    private bool lockLatestReleaseInfo;
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

    private bool lockDownload;
    public ProxyModule? DownloadLatest()
    {
        if (lockDownload) return null;
        lockDownload = true;

        if (RepositoryProvider == RepositoryProvider.Gitlab)
        {
            var packages = GitlabClient.GetGenericPackages(Owner, Repository, ModuleName);
            packages.Wait();
            if (packages.Result is null || packages.Result.Length <= 0) { LoggingSystem.Log($"Failed to to get generic package list for ({Owner}/{Repository}/{ModuleName})"); return null; }
            var result = GitlabClient.DownloadPackage(packages.Result[0], $"{InstallName}.dll", PackageFileName);
            if (result.Item1)
            {
                var rel = new GitlabRelease() { Owner = Owner, Repository = Repository, ReleasedAt = result.Item3 };
                ProxyModule mod = new(rel, Owner, Repository, ModuleName, FileName, Client, Server);
                UpdateModuleCache(mod);
                lockDownload = false;
                return mod;
            }
            else
            {
                lockDownload = false;
                return null;
            }
        }

        (bool, string) dl = GitHubClient.DownloadFileFromRelease(GitHubRelease, $"{InstallName}.dll", ModuleName);

        ProxyModule? module = null;
        if (dl.Item1)
        {
            if (RepositoryProvider == RepositoryProvider.GitHub && GitHubRelease is not null)
            { module = new ProxyModule(GitHubRelease, Owner, Repository, ModuleName, FileName, Client, Server); }

            if (module is null) return module;

            UpdateModuleCache(module);
        }
        lockDownload = false;
        return module;
    }

    private static void UpdateModuleCache(ProxyModule module)
    {
        var index = DataStorage.CachedModules.IndexOf(module);
        if (index >= 0)
        {
            if (module is not null) DataStorage.CachedModules[index] = module;
        }
        else
        {
            if (module is not null) DataStorage.CachedModules.Add(module);
        }
    }
}

public sealed class ProxyModuleSetting
{
    [JsonPropertyName("Name")]
    public string SettingName { get; set; } = "SettingName";
    [JsonPropertyName("Type")]
    public ModuleSettingType SettingType { get; set; } = ModuleSettingType.String;
    public object? DefaultValue { get; set; } = "value";
    [JsonIgnore]
    public Type ValueType
    {
        get
        {
            switch (SettingType)
            {
                case ModuleSettingType.Number:
                    return typeof(double);
                case ModuleSettingType.String:
                    return typeof(string);
                case ModuleSettingType.Bool:
                    return typeof(bool);
                case ModuleSettingType.Array:
                case ModuleSettingType.Object:
                case ModuleSettingType.Undefined:
                case ModuleSettingType.Null:
                default:
                    return typeof(object);

            }
        }
    }

    public KeyValuePair<string, JsonNode>? CreateDefaultSetting()
    {
        //LoggingSystem.Log($"{DefaultValue.GetType().Name}");
        switch (SettingType)
        {
            case ModuleSettingType.Number:
                if (DefaultValue is JsonElement n1 && n1.ValueKind == JsonValueKind.Number && n1.GetDouble() is double d) { return new(SettingName, JsonValue.Create(d)); }
                return null;
            case ModuleSettingType.String:
                if (DefaultValue is JsonElement n2 && n2.ValueKind == JsonValueKind.String && n2.GetString() is string s) { return new(SettingName, JsonValue.Create(s)); }
                return null;
            case ModuleSettingType.Bool:
                if (DefaultValue is JsonElement n3 && n3.ValueKind == JsonValueKind.True && n3.ValueKind == JsonValueKind.True) { return new(SettingName, JsonValue.Create(true)); }
                return new(SettingName, JsonValue.Create(false)); ;
            case ModuleSettingType.Array:
            case ModuleSettingType.Object:
            case ModuleSettingType.Undefined:
            case ModuleSettingType.Null:
            default:
                return null;
        }
    }
}

public enum ModuleSettingType
{
    Undefined,
    Bool,
    String,
    Number,
    Array,
    Object,
    Null,
}