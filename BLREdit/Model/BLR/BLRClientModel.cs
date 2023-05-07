using BLREdit.Export;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.Model.Proxy;
using BLREdit.UI;
using BLREdit.UI.Windows;

using PeNet;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BLREdit.Model.BLR;

public sealed class BLRClientModel : INotifyPropertyChanged
{
    public static RangeObservableCollection<BLRClientModel> Clients { get; } = IOResources.DeserializeFile<RangeObservableCollection<BLRClientModel>>($"Clients.json") ?? new();

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Event

    #region UIBools
    [JsonIgnore] public UIBool IsPatched { get; } = new(false);
    [JsonIgnore] public UIBool IsBeingPatched { get; } = new(false);
    [JsonIgnore] public UIBool HasModulesInstalled { get; } = new(false);
    [JsonIgnore] public UIBool AreModulesBeingInstalled { get; } = new(false);
    #endregion UIBools

    #region Data
    public static Dictionary<string, string> VersionHashes => new()
    {
        {"0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7", "v302"},
        {"de08147e419ed89d6db050b4c23fa772338132587f6b533b6233733f9bce46c3", "v301"},
        {"1742df917761f9dc01b079ae2aad78ef2ff17562af1dad6ad6ea7cf3622fe7f6", "v300"},
        {"4032ed1c45e717757a280e4cfe2408bb0c4e366676b785f0ffd177c3054c13a5", "v140"},
        {"01890318303354f588d9b89bb1a34c5c49ff881d2515388fcc292b54eb036b58", "v130"},
        {"d4f9cec736a83f7930f04438344d35ff9f0e57212755974bd51f48ff89d303c4", "v120"},
        {"d0bc0ae14ab4dd9f407de400da4f333ee0b6dadf6d68b7504db3fc46c4baa59f", "v1100"},
        {"9200705daddbbc10fee56db0586a20df1abf4c57a9384a630c578f772f1bd116", "v0993"}
    };

    private string _originalPath = "";
    private string? _originalHash;
    private string? _patchedPath;
    private string? _patchedHash;

    private string? _basePath;
    private string? _proxyConfigFolder;
    private string? _gameConfigFolder;
    private string? _proxyModuleFolder;
    private string? _proxyLogFolder;

    public string ClientVersion { get { if (VersionHashes.TryGetValue(OriginalHash ?? "", out string version)) { return version; } else { return "Unknown"; } } }

    public string OriginalPath {
        get { return _originalPath; }
        set { if (value != _originalPath) { _originalPath = value; OnPropertyChanged(); } }
    }
    public string? OriginalHash
    {
        get { return _originalHash; }
        set { if (value != _originalHash) { _originalHash = value; OnPropertyChanged(); } }
    }
    public string? PatchedPath
    {
        get { return _patchedPath; }
        set { if (value != _patchedPath) { _patchedPath = value; OnPropertyChanged(); } }
    }
    public string? PatchedHash
    {
        get { return _patchedHash; }
        set { if (value != _patchedHash) { _patchedHash = value; OnPropertyChanged(); } }
    }

    public string BasePath { get { _basePath ??= CreateBasePath(this); return _basePath; } set { _basePath = value; } }
    public string ProxyConfigFolder { get { _proxyConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\BLRevive\\").FullName; return _proxyConfigFolder; } }
    public string GameConfigFolder { get { _gameConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\PCConsole\\Cooked\\").FullName; return _gameConfigFolder; } }
    public string ProxyModuleFolder { get { _proxyModuleFolder ??= Directory.CreateDirectory($"{BasePath}Binaries\\Win32\\Modules\\").FullName; return _proxyModuleFolder; } }
    public string ProxyLogFolder { get { _proxyLogFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Logs\\").FullName; return _proxyLogFolder; } }

    [JsonIgnore] public Dictionary<string, BLRProfileSettingsWrapper> ProfileSettings { get; } = new();

    public RangeObservableCollection<BLRClientPatch> InstalledPatches { get; set; } = new();
    public RangeObservableCollection<ProxyModuleModel> InstalledModules { get; set; } = new();
    #endregion Data

    public static void LoadOrUpdateProfileSettings(BLRClientModel client)
    {
        var dirs = Directory.EnumerateDirectories($"{client.ProxyConfigFolder}");
        foreach (var dir in dirs)
        {
            if (dir.Contains("settings_manager_"))
            {
                var data = dir.Split('\\');
                var name = data[data.Length - 1].Substring(17);

                var onlineProfile = IOResources.DeserializeFile<BLRProfileSettings[]>($"{dir}\\UE3_online_profile.json");
                var keyBinds = IOResources.DeserializeFile<BLRKeyBindings>($"{dir}\\keybinding.json");

                var profile = new BLRProfileSettingsWrapper(name, onlineProfile, keyBinds);

                if (client.ProfileSettings.TryGetValue(name, out _))
                {
                    client.ProfileSettings[name] = profile;
                }
                else
                {
                    client.ProfileSettings.Add(name, profile);
                }
            }
        }
    }

    public static void ApplyProfileSettings(BLRClientModel client, BLRProfileSettingsWrapper profileSettings)
    {
        if (client.ProfileSettings.TryGetValue(profileSettings.ProfileName, out var _))
        {
            client.ProfileSettings.Remove(profileSettings.ProfileName);
            client.ProfileSettings.Add(profileSettings.ProfileName, profileSettings);
        }
        else
        {
            Directory.CreateDirectory($"{client.ProxyConfigFolder}settings_manager_{profileSettings.ProfileName}");
            client.ProfileSettings.Add(profileSettings.ProfileName, profileSettings);
        }
        IOResources.SerializeFile($"{client.ProxyConfigFolder}settings_manager_{profileSettings.ProfileName}\\UE3_online_profile.json", profileSettings.Settings.Values);
        IOResources.SerializeFile($"{client.ProxyConfigFolder}settings_manager_{profileSettings.ProfileName}\\keybinding.json", profileSettings.KeyBindings);
    }

    public static bool ValidateClientHash(string currentHash, string fileLocation, out string newHash)
    {
        if (string.IsNullOrEmpty(currentHash) || string.IsNullOrEmpty(fileLocation)) { newHash = null; return false; }
        newHash = IOResources.CreateFileHash(fileLocation);
        return currentHash.Equals(newHash);
    }

    public static void UpdateOrInstallModules(BLRClientModel client, List<ProxyModuleModel> modules)
    {
        if (client.AreModulesBeingInstalled.Is) return;
        client.AreModulesBeingInstalled.Set(true);
        List<Task> DownloadTasks = new();
        foreach (var module in modules)
        {
            DownloadTasks.Add(Task.Run(module.DownloadModuleToCache));
        }
        Task.WaitAll(DownloadTasks.ToArray());
        foreach (var module in modules)
        {
            int index = client.InstalledModules.IndexOf(module);
            if (index == -1 || client.InstalledModules[index].Repository.ReleaseTime < module.Repository.ReleaseTime)
            {
                var fileInfo = new FileInfo($"downloads\\moduleCache\\{module.Repository.FullName}.dll");
                if (fileInfo.Exists)
                {
                    fileInfo.CopyTo($"{client.ProxyModuleFolder}{module.Repository.FullName}.dll", true);
                    client.InstalledModules.Add(module);
                }
            }
        }
        client.HasModulesInstalled.Set(true);
        client.AreModulesBeingInstalled.Set(false);
    }

    public static bool PatchClient(BLRClientModel client)
    {
        bool state = false;
        if (client is null || client.IsBeingPatched.Is) return false;
        client.IsBeingPatched.Set(true);
        try
        {
            if (client is not null)
            {
                if (IsPatchable(client) && (HasPatchedClientChanged(client) || HasOrignalClientChanged(client)))
                {
                    state = BinaryPatchClient(client);
                }
                var proxySrcInfo = new FileInfo($"{IOResources.ASSET_DIR}{IOResources.DLL_DIR}{IOResources.PROXY_FILE}");
                var proxyDestInfo = new FileInfo($"{client.BasePath}Binaries\\Win32\\{IOResources.PROXY_FILE}");
                if (!proxyDestInfo.Exists && proxySrcInfo.Exists)
                {
                    proxySrcInfo.CopyTo(proxyDestInfo.FullName, true);
                }
            }
        }
        catch (Exception error)
        {
            LoggingSystem.MessageLog($"[{client}]Client Patch Failed: {error.Message}\n{error.StackTrace}");
        }
        client.IsBeingPatched.Set(false);
        client.IsPatched.Set(state);
        return state;
    }

    public static bool BinaryPatchClient(BLRClientModel client)
    {
        client.OriginalHash = IOResources.CreateFileHash(client.OriginalPath);
        LoggingSystem.Log($"Patching{client.BasePath}");
        File.Copy(client.OriginalPath, client.PatchedPath, true);
        if (BLRClientPatch.AvailablePatches.TryGetValue(client.OriginalHash, out List<BLRClientPatch> patches))
        {
            using var stream = File.Open(client.PatchedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var reader = new BinaryReader(stream);
            using var writer = new BinaryWriter(stream);
            List<byte> rawFile = new(reader.ReadBytes((int)stream.Length));
            stream.Position = 0;

            foreach (var patch in patches)
            {
                LoggingSystem.Log($"Applying Patch:{patch.PatchName} to Client:{client}");
                foreach (var part in patch.PatchParts)
                {
                    OverwriteBytes(rawFile, part.Key, part.Value.ToArray());
                }
            }

            PeFile peFile = new(rawFile.ToArray());
            peFile.AddImport(IOResources.PROXY_FILE, "InitializeThread");
            stream.SetLength(peFile.RawFile.Length);
            writer.Write(peFile.RawFile.ToArray());
            stream.Flush();
            stream.Close();
            client.InstalledPatches.Clear();
            client.InstalledPatches.AddRange(patches);
            client.PatchedHash = IOResources.CreateFileHash(client.PatchedPath);
        }
        else
        {
            LoggingSystem.Log($"No patches found for [{client.OriginalHash}]{client}");
        }
        return true;
    }

    private static void OverwriteBytes(List<byte> bytes, int offsetFromBegining, byte[] bytesToWrite)
    {
        int i2 = 0;
        for (int i = offsetFromBegining; i < bytes.Count && i2 < bytesToWrite.Length; i++)
        {
            bytes[i] = bytesToWrite[i2];
            i2++;
        }
    }

    public static bool IsPatchable(BLRClientModel client)
    {
        if (!ValidateOriginalPath(client))
        {
            LoggingSystem.MessageLog($"Client is not valid, original file is missing!\nMaybe client got moved or deleted\nClient can't be patched!");
            return false;
        }

        if (!ValidatePatchedPath(client))
        {
            LoggingSystem.Log($"Patched Client doesn't exist.");
            client.BasePath = CreateBasePath(client);
        }
        return true;
    }

    public static bool HasOrignalClientChanged(BLRClientModel client)
    {
        if (!ValidateClientHash(client.OriginalHash, client.OriginalPath, out string hash))
        {
            LoggingSystem.Log($"[{client}]: Original Client Changed!\n{client.OriginalHash} / {hash}");
            return true;
        }
        return false;
    }

    public static bool HasPatchedClientChanged(BLRClientModel client)
    {
        if (!ValidateClientHash(client.PatchedHash, client.PatchedPath, out string hash))
        {
            LoggingSystem.Log($"[{client}]: Patched Client Changed!\n{client.PatchedHash} / {hash}");
            return true;
        }
        return false;
    }

    public static string CreateBasePath(BLRClientModel client)
    {
        string[] pathParts = client.OriginalPath.Split('\\');
        string[] fileParts = pathParts[pathParts.Length - 1].Split('.');
        pathParts[pathParts.Length - 1] = fileParts[0] + "-BLREdit-Patched." + fileParts[1];
        string basePath = "";
        for (int i = pathParts.Length - 4; i >= 0; i--)
        {
            basePath = $"{pathParts[i]}\\{basePath}";
        }

        client.PatchedPath = $"{basePath}\\Binaries\\Win32\\{fileParts[0]}-BLREdit-Patched.{fileParts[1]}";

        return basePath;
    }

    public static bool ValidateOriginalPath(BLRClientModel client)
    {
        return !string.IsNullOrEmpty(client.OriginalPath) && File.Exists(client.OriginalPath);
    }
    public static bool ValidatePatchedPath(BLRClientModel client)
    {
        return !string.IsNullOrEmpty(client.PatchedPath) && File.Exists(client.PatchedPath);
    }

    public static void WriteProxyConfig(BLRClientModel client, List<ProxyModuleModel> enabledModules)
    {
        ProxyConfig config = IOResources.DeserializeFile<ProxyConfig>($"{client.ProxyConfigFolder}default.json") ?? new();
        config.Proxy.Modules.Server.Clear();
        config.Proxy.Modules.Client.Clear();
        foreach (var module in enabledModules)
        {
            if (module.IsServerModule.Is)
            {
                config.Proxy.Modules.Server.Add(module.Repository.FullName);
            }
            if (module.IsClientModule.Is)
            {
                config.Proxy.Modules.Client.Add(module.Repository.FullName);
            }
        }
        IOResources.SerializeFile($"{client.ProxyConfigFolder}default.json", config);
    }

    public static void StartClientWithArgs(BLRClientModel client, string args, bool isServer = false, bool watchDog = false, List<ProxyModuleModel>? enabledModules = null)
    {
        if (client.IsPatched.IsNot)
        {
            PatchClient(client);
        }

        if (enabledModules is null)
        {
            enabledModules = new List<ProxyModuleModel>();
            foreach (var module in App.AvailableProxyModules)
            {
                if (module.IsRequiredModule.Is)
                {
                    enabledModules.Add(module);
                }
            }
        }

        UpdateOrInstallModules(client, enabledModules);

        WriteProxyConfig(client, enabledModules);

        if (!isServer)
        {
            ApplyProfileSettings(client, ExportSystem.GetOrAddProfileSettings(BLREditSettings.Settings.PlayerName));
        }

        BLRProcess.CreateProcess(args, client, isServer, watchDog);
    }

    public static void LaunchServer(BLRClientModel client)
    {
        if (BLRProcess.IsServerRunning()) { return; }
        (var mode, var map, var canceled) = MapModeSelect.SelectMapAndMode(client.ClientVersion);
        if (canceled) { return; }
        string launchArgs = $"server {map.MapName}?Game=FoxGame.FoxGameMP_{mode?.ModeName ?? "DM"}?ServerName=BLREdit-{mode?.ModeName ?? "DM"}-Server?Port=7777?NumBots={BLREditSettings.Settings.BotCount}?MaxPlayers={BLREditSettings.Settings.PlayerCount}";
        StartClientWithArgs(client, launchArgs, true, BLREditSettings.Settings.ServerWatchDog.Is);
    }

    #region Overrides
    public override string ToString()
    {
        return $"[{ClientVersion}]:{OriginalPath?.Substring(0, Math.Min(OriginalPath.Length, 24))}";
    }
    public override bool Equals(object obj)
    {
        if (obj is BLRClientModel client)
        {
            return client.GetHashCode() == GetHashCode();
        }
        else
        { return false; }
    }

    public override int GetHashCode()
    {
        return OriginalPath.GetHashCode();
    }
    #endregion Overrides

    private ICommand? startServerCommand;
    [JsonIgnore] public ICommand StartServerCommand { get { startServerCommand ??=new RelayCommand(param => LaunchServer(this)); return startServerCommand; } }

    private ICommand? startBotMatchCommand;
    [JsonIgnore] public ICommand StartBotMatchCommand { get { startBotMatchCommand ??= new RelayCommand(param => { LaunchServer(this); StartClientWithArgs(this, $"127.0.0.1:7777?Name={BLREditSettings.Settings.PlayerName}"); } ); return startBotMatchCommand; } }

    private ICommand? modifyClientCommand;
    [JsonIgnore] public ICommand ModifyClientCommand { get { modifyClientCommand ??= new RelayCommand(param => { var clientWindow = new BLRClientWindow(this); clientWindow.ShowDialog(); }); return modifyClientCommand; } }
}
