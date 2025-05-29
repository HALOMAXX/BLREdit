using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace BLREdit.Game.Proxy;

public sealed class RepositoryProxyModule
{
    public RepositoryProvider RepositoryProvider { get; set; } = RepositoryProvider.Gitlab;
    [JsonIgnore] public string InstallName { get { return $"{PackageFileName}"; } }
    [JsonIgnore] public string TrimedOwner { get { return Owner.Split(Path.GetInvalidFileNameChars())[0]; } }
    [JsonIgnore] public string CacheName { get { return $"{InstallName}-{TrimedOwner}"; } }
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
                LoggingSystem.Log($"failed to get release info for {CacheName}\n{error}");
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
            var result = GitlabClient.DownloadPackage(packages.Result[0], $"{CacheName}.dll", PackageFileName);
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

        if (RepositoryProvider == RepositoryProvider.GitHub && GitHubRelease is not null)
        {
            (bool, string) dl = GitHubClient.DownloadFileFromRelease(GitHubRelease, $"{CacheName}.dll", ModuleName);

            ProxyModule? module = null;
            if (dl.Item1)
            {
                module = new ProxyModule(GitHubRelease, Owner, Repository, ModuleName, FileName, Client, Server);
                UpdateModuleCache(module);
            }
            lockDownload = false;
            return module;
        }
        lockDownload = false;
        return null;
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

public sealed class ProxyModuleSetting : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    [JsonPropertyName("Name")]
    public string SettingName { get; set; } = "SettingName";
    [JsonPropertyName("Type")]
    public ModuleSettingType SettingType { get; set; } = ModuleSettingType.String;
    public object? DefaultValue { get; set; } = "value";
    [JsonIgnore]
    public object? CurrentValue { get; set; }

    [JsonIgnore]
    public bool CurrentValueAsBool { get{ if (CurrentValue is bool b) { return b; } else { return false; } } set { CurrentValue = value; OnPropertyChanged(); } }
    [JsonIgnore]
    public string CurrentValueAsString { get { if (CurrentValue is string s) { return s; } else { return string.Empty; } } 
        set { CurrentValue = value; OnPropertyChanged(); } }
    [JsonIgnore]
    public double CurrentValueAsDouble { get { if (CurrentValue is double d) { return d; } else { return 0; } } set { CurrentValue = value; OnPropertyChanged(); } }

    [JsonIgnore]
    public int MinNumberValue { get; set; } = int.MinValue;
    [JsonIgnore]
    public int MaxNumberValue { get; set; } = int.MaxValue;

    [JsonIgnore]
    public Type ValueType
    {
        get
        {
            return SettingType switch
            {
                ModuleSettingType.Number => typeof(double),
                ModuleSettingType.String => typeof(string),
                ModuleSettingType.Bool => typeof(bool),
                _ => typeof(object),
            };
        }
    }

    [JsonIgnore]
    public Visibility NumberVisibility { get { return SettingType switch { ModuleSettingType.Number => Visibility.Visible, _ => Visibility.Collapsed, }; } }
    [JsonIgnore]
    public Visibility ToggleVisibility { get { return SettingType switch { ModuleSettingType.Bool => Visibility.Visible, _ => Visibility.Collapsed, }; } }
    [JsonIgnore]
    public Visibility TextVisibility { get { return SettingType switch { ModuleSettingType.String => Visibility.Visible, _ => Visibility.Collapsed, }; } }

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
                if (DefaultValue is JsonElement n3 && n3.ValueKind == JsonValueKind.True) { return new(SettingName, JsonValue.Create(true)); }
                return new(SettingName, JsonValue.Create(false)); ;
            case ModuleSettingType.Array:
            case ModuleSettingType.Object:
            case ModuleSettingType.Undefined:
            case ModuleSettingType.Null:
            default:
                return null;
        }
    }

    internal JsonNode? CreateCurrentValue()
    {
        switch (SettingType)
        {
            case ModuleSettingType.Number:
                if (CurrentValue is double d) { return JsonValue.Create(d); }
                return 0;
            case ModuleSettingType.String:
                if (CurrentValue is string s) { return JsonValue.Create(s); }
                return string.Empty;
            case ModuleSettingType.Bool:
                if (CurrentValue is bool b) { return JsonValue.Create(b); }
                return JsonValue.Create(false); ;
            case ModuleSettingType.Array:
            case ModuleSettingType.Object:
            case ModuleSettingType.Undefined:
            case ModuleSettingType.Null:
            default:
                return null;
        }
    }

    public ProxyModuleSetting() {}

    public ProxyModuleSetting(ProxyModuleSetting origin, JsonNode? jsonData)
    {
        if (origin is null) { LoggingSystem.FatalLog("Original ProxyModuleSetting was null"); return; }
        if (jsonData is null) { LoggingSystem.FatalLog("JsonData was null"); return; }
        SettingName = origin.SettingName;
        DefaultValue = origin.DefaultValue;
        SettingType = origin.SettingType;

        switch (SettingType)
        {
            case ModuleSettingType.Number:
                if (jsonData.GetValueKind() == JsonValueKind.Number) { CurrentValue = jsonData.GetValue<double>(); }
                break;
            case ModuleSettingType.String:
                if (jsonData.GetValueKind() == JsonValueKind.String) { CurrentValue = jsonData.GetValue<string>(); }
                break;
            case ModuleSettingType.Bool:
                if (jsonData.GetValueKind() == JsonValueKind.True || jsonData.GetValueKind() == JsonValueKind.False) { CurrentValue = jsonData.GetValue<bool>(); }
                break;
            case ModuleSettingType.Array:
            case ModuleSettingType.Object:
            case ModuleSettingType.Undefined:
            case ModuleSettingType.Null:
            default:
                break;
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