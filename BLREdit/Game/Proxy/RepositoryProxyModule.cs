using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime;
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
    public ObservableCollection<ProxyModuleSetting> ModuleSettings { get; set; } = [];

    public JsonObject GetDefaultSettings()
    {
        JsonObject moduleSettings = [];

        foreach (var setting in ModuleSettings)
        {
            var settingLocation = moduleSettings;
            for (int i = 0; i < setting.SettingPathParts.Count - 1; i++)
            {
                bool found = false;
                foreach (var path in settingLocation)
                {
                    if (path.Key == setting.SettingPathParts[i] && path.Value is not null && path.Value.AsObject() is JsonObject baseObj) { found = true; settingLocation = baseObj; break; }
                }
                if (!found)
                {
                    var newSettingLocation = new JsonObject();
                    settingLocation.Add(setting.SettingPathParts[i], newSettingLocation);
                    settingLocation = newSettingLocation;
                }
            }
            settingLocation.Add(setting.SettingName, setting.CreateDefaultValue());
        }

        return moduleSettings;
    }

    public JsonObject GetCurrentSettings()
    {
        JsonObject moduleSettings = [];

        foreach (var setting in ModuleSettings)
        {
            var settingLocation = moduleSettings;
            for (int i = 0; i < setting.SettingPathParts.Count - 1; i++)
            {
                bool found = false;
                foreach (var path in settingLocation)
                {
                    if (path.Key == setting.SettingPathParts[i] && path.Value is not null && path.Value.AsObject() is JsonObject baseObj) { found = true; settingLocation = baseObj; break; }
                }
                if (!found)
                {
                    var newSettingLocation = new JsonObject();
                    settingLocation.Add(setting.SettingPathParts[i], newSettingLocation);
                    settingLocation = newSettingLocation;
                }
            }
            settingLocation.Add(setting.SettingName, setting.CreateCurrentValue());
        }

        return moduleSettings;
    }

    private static bool NeedToGoDeeperCheck(JsonValueKind baseValueKind, JsonValueKind newValueKind, ref bool skip)
    {
        bool deeper = false;
        switch (newValueKind)
        {
            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
                // value kind check if base is also value kind if not we can overwrite it or have to skip
                switch (baseValueKind)
                {
                    case JsonValueKind.Number:
                    case JsonValueKind.String:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                    case JsonValueKind.Object:
                        //skip
                        skip = true;
                        break;
                    default:
                        //overwrite
                        break;
                }
                break;
            case JsonValueKind.Object:
                // we have to check if we need to go deeper or skip
                switch (baseValueKind)
                {
                    case JsonValueKind.Number:
                    case JsonValueKind.String:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        //skip
                        skip = true;
                        break;
                    case JsonValueKind.Object:
                        //deeper and skip
                        skip = true;
                        deeper = true;
                        break;
                    default:
                        //overwrite
                        break;
                }
                break;
            default:
                //overwrite
                break;
        }
        return deeper;
    }

    public void CombineSettings(JsonObject baseSettings, JsonObject newSettings) 
    {
        if (baseSettings is null || newSettings is null) { LoggingSystem.LogNull(); return; }

        foreach (var newSetting in newSettings)
        {
            if (newSetting.Value is not null)
            {
                bool skip = false;
                foreach (var baseSetting in baseSettings)
                { 
                    if (baseSetting.Key == newSetting.Key) 
                    {
                        if (baseSetting.Value is not null)
                        {
                            if(NeedToGoDeeperCheck(baseSetting.Value.GetValueKind(), newSetting.Value.GetValueKind(), ref skip))
                            {
                                CombineSettings(baseSetting.Value.AsObject(), newSetting.Value.AsObject());
                            }

                        }
                        break;
                    }                    
                }
                if (!skip)
                {
                    baseSettings.Remove(newSetting.Key);
                    baseSettings.Add(newSetting.Key, newSetting.Value.DeepClone());
                }
            }

            //var baseSettingLocation = baseSettings;
            //var newSettingLocation = newSettings;
            //for (int i = 0; i < setting.SettingPathParts.Count - 1; i++)
            //{
            //    bool baseFound = false;
            //    bool newFound = false;
            //    foreach (var basePath in baseSettingLocation)
            //    {
            //        if (basePath.Key == setting.SettingPathParts[i] && basePath.Value is not null && basePath.Value.AsObject() is JsonObject baseObj) { baseFound = true; baseSettingLocation = baseObj; break; }
            //    }
            //    foreach (var newPath in newSettingLocation)
            //    {
            //        if (newPath.Key == setting.SettingPathParts[i] && newPath.Value is not null && newPath.Value.AsObject() is JsonObject newObj) { newFound = true; newSettingLocation = newObj; break; }
            //    }
            //    if (!baseFound)
            //    {
            //        var settingLocation = new JsonObject();
            //        baseSettingLocation.Add(setting.SettingPathParts[i], settingLocation);
            //        baseSettingLocation = settingLocation;
            //    }
            //    if (!newFound)
            //    {
            //        var settingLocation = new JsonObject();
            //        newSettingLocation.Add(setting.SettingPathParts[i], settingLocation);
            //        newSettingLocation = settingLocation;
            //    }
            //}

            //if (!newSettingLocation.ContainsKey(setting.SettingName)) { newSettingLocation.Add(setting.SettingName, setting.CreateDefaultValue()); } //TODO: will add default value to original JsonObject might be unwanted beghaviour!!!!
            //if (!baseSettingLocation.ContainsKey(setting.SettingName) && newSettingLocation.TryGetPropertyValue(setting.SettingName, out var value)) { baseSettingLocation.Add(setting.SettingName, value); }
        }
    }

    public void ReadSettings(JsonObject settings)
    {
        if(settings is null) { LoggingSystem.LogNull(); return; }
        foreach (var setting in ModuleSettings)
        {
            var settingLocation = settings;
            bool found = true;
            for (int i = 0; i < setting.SettingPathParts.Count - 1; i++)
            {
                bool found2 = false;
                foreach (var path in settingLocation)
                {
                    if (path.Key == setting.SettingPathParts[i] && path.Value is not null && path.Value.AsObject() is JsonObject baseObj) { found2 = true; settingLocation = baseObj; break; }
                }
                if (!found2) { found = false; break; }
            }
            if (found && settingLocation.TryGetPropertyValue(setting.SettingName, out var value) && value is not null)
            {
                setting.SetNodeData(value);
            }
        }
    }

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
                    hubRelease ??= await GitHubClient.GetLatestRelease(Owner, Repository).ConfigureAwait(false);
                }
                else
                {
                    labRelease ??= await GitlabClient.GetLatestRelease(Owner, Repository).ConfigureAwait(false);
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

    [JsonPropertyName("Path")]
    public string SettingPath { get; set; } = "SettingPath";
    [JsonIgnore] ReadOnlyCollection<string>? settingPathParts;
    [JsonIgnore] public ReadOnlyCollection<string> SettingPathParts { get { settingPathParts ??= new(SettingPath.Split('.')); return settingPathParts; } }
    [JsonIgnore] public string SettingName { get { return SettingPathParts.Last(); } }
    [JsonPropertyName("Type")]
    public ModuleSettingType SettingType { get; set; } = ModuleSettingType.String;
    public object? DefaultValue { get; set; } = "value";
    [JsonIgnore]
    public object? CurrentValue { get; set; }

    [JsonIgnore]
    public bool CurrentValueAsBool { 
        get{ if (CurrentValue is bool b) { return b; } else { return false; } } 
        set { CurrentValue = value; OnPropertyChanged(); } }
    [JsonIgnore]
    public string CurrentValueAsString { 
        get { if (CurrentValue is string s) { return s; } else { return string.Empty; } } 
        set { CurrentValue = value; OnPropertyChanged(); } }
    [JsonIgnore]
    public double CurrentValueAsDouble { 
        get { if (CurrentValue is double d) { return d; } else { return 0; } } 
        set { CurrentValue = value; OnPropertyChanged(); } }

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

    public JsonNode? CreateDefaultValue()
    {
        switch (SettingType)
        {
            case ModuleSettingType.Number:
                if (DefaultValue is double d) { return JsonValue.Create(d); }
                return 0;
            case ModuleSettingType.String:
                if (DefaultValue is string s) { return JsonValue.Create(s); }
                return string.Empty;
            case ModuleSettingType.Bool:
                if (DefaultValue is bool b) { return JsonValue.Create(b); }
                return JsonValue.Create(false); ;
            case ModuleSettingType.Array:
            case ModuleSettingType.Object:
            case ModuleSettingType.Undefined:
            case ModuleSettingType.Null:
            default:
                return null;
        }
    }

    public JsonNode? CreateCurrentValue()
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

    public void SetNodeData(JsonNode jsonData)
    {
        if (jsonData is null) { LoggingSystem.FatalLog("JsonData was null"); return; }
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

    public ProxyModuleSetting() {}

    public ProxyModuleSetting(ProxyModuleSetting origin, JsonNode? jsonData)
    {
        if (origin is null) { LoggingSystem.FatalLog("Original ProxyModuleSetting was null"); return; }
        if (jsonData is null) { LoggingSystem.FatalLog("JsonData was null"); return; }
        SettingPath = origin.SettingPath;
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