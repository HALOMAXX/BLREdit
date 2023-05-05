using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.Game.Proxy;
using BLREdit.Model.BLR;
using BLREdit.UI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API;

[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(ProxyModuleRepository), typeDiscriminator: "Base")]
[JsonDerivedType(typeof(CustomModuleRepository), typeDiscriminator: "Custom")]
[JsonDerivedType(typeof(GitHubModuleRepository), typeDiscriminator: "GitHub")]
[JsonDerivedType(typeof(GitlabModuleRepository), typeDiscriminator: "Gitlab")]
public abstract class ProxyModuleRepository : INotifyPropertyChanged
{
    public static RangeObservableCollection<ProxyModuleRepository> CachedModules { get; } = IOResources.DeserializeFile<RangeObservableCollection<ProxyModuleRepository>>($"ModuleCache.json") ?? new();

    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    private readonly Dictionary<string, string> _metaData = new();

    private string? _owner;
    private string? _repo;
    private string? _name;
    private string? _fullName;

    private string? _ogTitle;
    private string? _ogImage;
    private string? _ogUrl;
    private string? _ogDescription;
    private DateTime? _releaseTime = DateTime.MinValue;

    public string? Owner { get { return _owner; } set { _owner = value; OnPropertyChanged(); } }
    public string? Repository { get { return _repo; } set { _repo = value; OnPropertyChanged(); } }
    public string? Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }

    public string FullName { get { _fullName ??= string.Join("-", $"{Name}-{Owner}-{Repository}".Split(Path.GetInvalidFileNameChars())); return _fullName; } }

    public Dictionary<string, string> MetaData { get { if (_metaData.Count <= 0) { Task.Run(GetMetaData); } return _metaData; } }
    public string? OGTitle { get { if (_ogTitle is null && MetaData.TryGetValue("og:title", out string title)) { _ogTitle = title; } return _ogTitle; } }
    public string? OGImage { get { if (_ogImage is null && MetaData.TryGetValue("og:image", out string image)) { _ogImage = image; } return _ogImage; } }
    public string? OGUrl { get { if (_ogUrl is null && MetaData.TryGetValue("og:url", out string url)) { _ogUrl = url; } return _ogUrl; } }
    public string? OGDescription { get { if (_ogDescription is null && MetaData.TryGetValue("og:description", out string description)) { _ogDescription = description; } return _ogDescription; } }
    public DateTime? ReleaseTime { get { return _releaseTime; } set { _releaseTime = value; OnPropertyChanged(); } }


    public virtual void NotifyMetaDataPropertiesChanged()
    {
        OnPropertyChanged(nameof(MetaData));

        OnPropertyChanged(nameof(OGTitle));
        OnPropertyChanged(nameof(OGImage));
        OnPropertyChanged(nameof(OGUrl));
        OnPropertyChanged(nameof(OGDescription));
    }

    public override bool Equals(object obj)
    {
        if (obj is ProxyModuleRepository repo)
        { 
            return repo.GetHashCode() == this.GetHashCode();
        }
        else
        { return false; }
    }

    [JsonIgnore] public UIBool IsGettingMetaData { get; set; } = new(false);
    public abstract void GetMetaData();
    public abstract DateTime GetLatestReleaseDateTime();
    [JsonIgnore] public UIBool IsDownloadingModuleToCache { get; set; } = new(false);
    public abstract void DownloadModuleToCache();

    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }
}

public sealed class CustomModuleRepository : ProxyModuleRepository
{
    public CustomModuleRepository(string owner = "custom", string repository = "custom", string name = "custom") : base()
    { 
        Owner = owner;
        Repository = repository;
        Name = name;
    }

    public override void DownloadModuleToCache()
    { }

    public override DateTime GetLatestReleaseDateTime()
    { return DateTime.Now; }

    public override void GetMetaData()
    {}
}
