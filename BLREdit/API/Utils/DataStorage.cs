using BLREdit.API.Export;
using BLREdit.Export;
using BLREdit.Game;
using BLREdit.Game.Proxy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BLREdit;

public static class DataStorage
{
    public static event EventHandler? DataSaving;

    #region Fields
    private static ObservableCollection<BLRClient>? _gameClients;
    private static ObservableCollection<BLRServer>? _servers;
    private static Dictionary<string, BLRProfileSettingsWrapper>? _profileSettings;
    private static ObservableCollection<ShareableProfile>? _shareableProfiles;
    private static ObservableCollection<ShareableLoadout>? _shareableLoadouts;
    private static ObservableCollection<BLRLoadoutStorage>? _blrProfile;
    private static BLREditSettings? _settings;
    private static ObservableCollection<ProxyModule>? _cachedModules;
    #endregion Fields

    #region Locks
    private static readonly object cachedModulesLock = new();
    private static readonly object gameClientsLock = new();
    private static readonly object serverListLock = new();
    private static readonly object profileSettingLock = new();
    private static readonly object shareableLock = new();
    private static readonly object shareableLoadoutLock = new();
    private static readonly object loadoutLock = new();
    private static readonly object settingsLock = new();
    #endregion Locks

    #region Properties
    public static ObservableCollection<ProxyModule> CachedModules { get { lock (cachedModulesLock) { _cachedModules ??= IOResources.DeserializeFile<ObservableCollection<ProxyModule>>($"ModuleCache.json") ?? []; } return _cachedModules; } }
    public static ObservableCollection<BLRClient> GameClients { get { lock (gameClientsLock) { _gameClients ??= IOResources.DeserializeFile<ObservableCollection<BLRClient>>($"GameClients.json") ?? []; } return _gameClients; } }
    public static ObservableCollection<BLRServer> ServerList { get { lock (serverListLock) { _servers ??= IOResources.DeserializeFile<ObservableCollection<BLRServer>>($"ServerList.json") ?? []; } return _servers; } }
    public static Dictionary<string, BLRProfileSettingsWrapper> ProfileSettings { get { lock (profileSettingLock) { _profileSettings ??= IOResources.DeserializeFile<Dictionary<string, BLRProfileSettingsWrapper>>($"PlayerSettings.json") ?? []; } return _profileSettings; } }
    public static ObservableCollection<ShareableProfile> ShareableProfiles { get { lock (shareableLock) { _shareableProfiles ??= ExportSystem.LoadShareableProfiles(); } return _shareableProfiles; } }
    public static ObservableCollection<ShareableLoadout> ShareableLoadouts { get { lock (shareableLoadoutLock) { _shareableLoadouts ??= ExportSystem.LoadShareableLoadouts(); } return _shareableLoadouts; } }
    public static ObservableCollection<BLRLoadoutStorage> Loadouts { get { lock (loadoutLock) { _blrProfile ??= ExportSystem.LoadStorage(); } return _blrProfile; } }
    public static BLREditSettings Settings { get { lock (settingsLock) { _settings ??= LoadBLREditSettings(); } return _settings; } set { _settings = value; } }
    #endregion Properties

    public static int FindIn<T>(IList<T> list, T? item)
    {
        if (item is null) { return -1; }
        for (int i = 0; i < list.Count; i++)
        {
            if ((object)item == list[i] as object) return i;
        }
        return -1;
    }

    public static void Save()
    {
        DataSaving?.Invoke(null, EventArgs.Empty);
        IOResources.SerializeFile($"PlayerSettings.json", _profileSettings);
        //IOResources.SerializeFile($"{IOResources.PROFILE_DIR}profileList.json", _shareableProfiles);
        IOResources.SerializeFile($"{IOResources.PROFILE_DIR}loadoutList.json", _shareableLoadouts);
        IOResources.SerializeFile($"GameClients.json", _gameClients);
        IOResources.SerializeFile($"ServerList.json", _servers);
        IOResources.SerializeFile($"ModuleCache.json", _cachedModules);
        BLREditSettings.Save();
    }

    private static BLREditSettings LoadBLREditSettings()
    { 
        var settings = IOResources.DeserializeFile<BLREditSettings>($"{IOResources.SETTINGS_FILE}") ?? new();

        if (!BLREditSettings.AvailableBLReviveVersions.Contains(settings.SelectedBLReviveVersion))
        {
            settings.SelectedBLReviveVersion = BLREditSettings.AvailableBLReviveVersions.First();
        }

        if (!BLREditSettings.AvailableBLREditVersions.Contains(settings.SelectedBLREditVersion))
        {
            settings.SelectedBLREditVersion = BLREditSettings.AvailableBLREditVersions.First();
        }

        if (!BLREditSettings.AvailableSDKTypes.Contains(settings.SelectedSDKType))
        {
            settings.SelectedSDKType = BLREditSettings.AvailableSDKTypes.First();
        }

        return settings;
    }
}
