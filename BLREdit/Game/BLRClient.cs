using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BLREdit.Game.Proxy;
using BLREdit.UI;
using BLREdit.UI.Views;
using BLREdit.UI.Windows;

using PeNet;
using PeNet.Header.Net.MetaDataTables;

using File = System.IO.File;

namespace BLREdit.Game;

public sealed class BLRClient : INotifyPropertyChanged
{
    [JsonIgnore] public UIBool Patched { get; private set; } = new UIBool(false);
    [JsonIgnore] public UIBool CurrentClient { get; private set; } = new UIBool(false);
    [JsonIgnore] public string ClientVersion { get { if (VersionHashes.TryGetValue(ClientHash, out string version)) { return version; } else { return "Unknown"; } } }
    [JsonIgnore] public ObservableCollection<Process> RunningClients = new();


    [JsonIgnore] public BitmapImage ClientVersionPart0 { get { return new BitmapImage(new Uri(@"pack://application:,,,/UI/Resources/V.png", UriKind.Absolute)); } }
    [JsonIgnore] public BitmapImage ClientVersionPart1 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 2) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[1])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage ClientVersionPart2 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 3) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[2])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage ClientVersionPart3 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 4) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[3])}.png", UriKind.Absolute)); } return null; } }
    [JsonIgnore] public BitmapImage ClientVersionPart4 { get { if (ClientVersion != "Unknown" && ClientVersion.Length >= 5) { return new BitmapImage(new Uri($"pack://application:,,,/UI/Resources/{char.GetNumericValue(ClientVersion[4])}.png", UriKind.Absolute)); } return null; } }

    private string clientHash;
    public string ClientHash {
        get { return clientHash; }
        set { if (clientHash != value && !string.IsNullOrEmpty(value)) { clientHash = value; OnPropertyChanged(nameof(ClientVersion)); OnPropertyChanged(); } }
    }

    private string patchedHash;
    public string PatchedHash { 
        get { return patchedHash; } 
        set { if (patchedHash != value && !string.IsNullOrEmpty(value)) { patchedHash = value; OnPropertyChanged(); } }
    }

    private string originalPath;
    public string OriginalPath { 
        get { return originalPath; } 
        set { if (originalPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { originalPath = value; ClientHash ??= IOResources.CreateFileHash(value); OnPropertyChanged(); } } 
    }

    private string patchedPath;
    public string PatchedPath { 
        get { return patchedPath; } 
        set { if (patchedPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { patchedPath = value; Patched.SetBool(true); OnPropertyChanged(); } } 
    }

    public string ConfigFolder { get; set; }
    public string ModulesFolder { get; set; }

    public ObservableCollection<BLRClientPatch> AppliedPatches { get; set; } = new();

    public ObservableCollection<ProxyModule> InstalledModules { get; set; } = new();

    [JsonIgnore] public static VisualProxyModule[] AvailabeModules { get { return App.AvailableProxyModules; } }

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

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override bool Equals(object obj)
    {
        if (obj is BLRClient client)
        {
            return (client.OriginalPath == OriginalPath && client.ClientVersion == ClientVersion && client.Patched.Is == Patched.Is);
        }
        else
        {
            return false;
        }
    }

    public override string ToString()
    {
        return $"[{ClientVersion}]:{OriginalPath?.Substring(0, Math.Min(OriginalPath.Length, 24))}";
    }

    #region ClientValidation
    public bool ValidateClient()
    {
        bool isValid = true;
        bool NeedsPatching = false;
        if (OriginalFileValidation())
        {
            //this is super overkill
            if (!ValidateClientHash(ClientHash, OriginalPath, out string NewHash))
            {
                LoggingSystem.Log($"Client has changed was {ClientHash} is now {NewHash} needs patching!");
                ClientHash = NewHash;
                NeedsPatching = true;
            }
            else
            { LoggingSystem.Log($"Client is still the Same! {OriginalPath}"); }
        }
        else
        {
            LoggingSystem.Log($"Client is not valid missing original file path or file is missing!");
            isValid = false;
        }

        if (PatchedFileValidation())
        {
            if (!ValidateClientHash(PatchedHash, PatchedPath, out string NewHash))
            {
                LoggingSystem.Log($"Patched client changed/corrupted was {PatchedHash} is now {NewHash} needs patching!");
                NeedsPatching = true;
            }
            else
            {
                LoggingSystem.Log($"Patched file is still the same!");
                if (ValidatePatches()) { NeedsPatching = true; }
            }
        }
        else
        {
            LoggingSystem.Log($"Client hasn't been patched yet!");
            NeedsPatching=true;
        }

        if (NeedsPatching)
        {
            PatchClient();
        }

        return isValid;
    }

    public bool OriginalFileValidation()
    {
        return !string.IsNullOrEmpty(OriginalPath) && File.Exists(OriginalPath);
    }

    public bool PatchedFileValidation()
    {
        return !string.IsNullOrEmpty(PatchedPath) && File.Exists(PatchedPath);
    }

    public bool ValidatePatches()
    {
        bool needUpdatedPatches = false;

        if (BLRClientPatch.AvailablePatches.TryGetValue(this.ClientHash, out List<BLRClientPatch> patches))
        {
            if (AppliedPatches.Count > 0)
            {
                foreach (var installedPatch in AppliedPatches)
                {
                    bool isValid = false;
                    foreach (var patch in patches)
                    {
                        if (installedPatch.Equals(patch)) isValid = true;
                    }
                    if (!isValid) { needUpdatedPatches = true; LoggingSystem.Log($"found old patch {installedPatch.PatchName}"); }
                }

                var proxySource = $"{AppDomain.CurrentDomain.BaseDirectory}{IOResources.ASSET_DIR}\\dlls\\Proxy.dll";
                var proxyTarget = $"{Path.GetDirectoryName(PatchedPath)}\\Proxy.dll";
                if (File.Exists(proxySource) && File.Exists(proxyTarget))
                {
                    var sourceHash = IOResources.CreateFileHash(proxySource);
                    var targetHash = IOResources.CreateFileHash(proxyTarget);
                    if (sourceHash != targetHash)
                    {
                        File.Copy(proxySource, proxyTarget, true);
                    }
                }
            }
            else
            {
                LoggingSystem.Log($"no installed patches for {ClientHash}");
                needUpdatedPatches = true;
            }
        }
        else
        {
            LoggingSystem.Log($"No patches found for {ClientHash}");
            needUpdatedPatches=true;
        }
        return needUpdatedPatches;
    }

    public void ValidateModules()
    {
        LoggingSystem.Log($"Validating Modules({InstalledModules.Count}) of {this}");

        // TODO Remove old / other modules
        foreach (var file in Directory.EnumerateFiles(ModulesFolder))
        { 
            var info = new FileInfo(file);
            if (info.Extension == ".dll")
            {
                var name = info.Name.Split('.')[0];
                bool isInstalled = false;
                foreach (var module in InstalledModules)
                {
                    if (name == module.InstallName)
                    { isInstalled = true; break; }
                }
                if (!isInstalled) { info.Delete(); }
            }
        }

        ProxyConfig config = IOResources.DeserializeFile<ProxyConfig>($"{ConfigFolder}\\default.json") ?? new();
        config.Proxy.Modules.Server.Clear();
        config.Proxy.Modules.Client.Clear();
        foreach (var module in InstalledModules)
        {
            LoggingSystem.Log($"\t{module.InstallName}:");
            LoggingSystem.Log($"\t\tClient:{module.Client}");
            LoggingSystem.Log($"\t\tServer:{module.Server}");

            if(module.Client) config.Proxy.Modules.Client.Add(module.InstallName);
            if(module.Server) config.Proxy.Modules.Server.Add(module.InstallName);
        }

        IOResources.SerializeFile($"{ConfigFolder}\\default.json", config);
        LoggingSystem.Log($"Finished Validating Modules of {this}");
    }

    public static bool ValidateClientHash(string currentHash, string fileLocation, out string newHash)
    {
        if (string.IsNullOrEmpty(currentHash) || string.IsNullOrEmpty(fileLocation)) { newHash = null; return false; }
        newHash = IOResources.CreateFileHash(fileLocation);
        return currentHash.Equals(newHash);
    }

    #endregion ClientValidation

    #region Commands
    private ICommand patchClientCommand;
    [JsonIgnore]
    public ICommand PatchClientCommand
    {
        get
        {
            patchClientCommand ??= new RelayCommand(
                    param => this.PatchClient()
                );
            return patchClientCommand;
        }
    }

    private ICommand launchClientCommand;
    [JsonIgnore]
    public ICommand LaunchClientCommand
    {
        get 
        {
            launchClientCommand ??= new RelayCommand(
                    param => this.LaunchClient()
                );
            return launchClientCommand;
        }
    }

    private ICommand launchServerCommand;
    [JsonIgnore]
    public ICommand LaunchServerCommand
    {
        get
        {
            launchServerCommand ??= new RelayCommand(
                    param => this.LaunchServer()
                );
            return launchServerCommand;
        }
    }

    private ICommand launchBotMatchCommand;
    [JsonIgnore]
    public ICommand LaunchBotMatchCommand
    {
        get
        {
            launchBotMatchCommand ??= new RelayCommand(
                    param => this.LaunchBotMatch()
                );
            return launchBotMatchCommand;
        }
    }

    private ICommand modifyClientCommand;
    [JsonIgnore]
    public ICommand ModifyClientCommand
    {
        get
        {
            modifyClientCommand ??= new RelayCommand(
                    param => this.ModifyClient()
                );
            return modifyClientCommand;
        }
    }

    private ICommand currentClientCommand;
    [JsonIgnore]
    public ICommand CurrentClientCommand
    {
        get
        {
            currentClientCommand ??= new RelayCommand(
                    param => this.SetCurrentClient()
                );
            return currentClientCommand;
        }
    }
    #endregion Commands

    #region Launch/Exit
    private void LaunchBotMatch()
    {
        //TODO Map/Mode Select
        (var mode, var map) = MapModeSelect.SelectMapAndMode(this.ClientVersion);

        string launchArgs = $"server {map.MapName}?Game=FoxGame.FoxGameMP_{mode.ModeName}?SingleMatch?NumBots=12";
        StartProcess(launchArgs);
    }

    private void LaunchServer()
    {
        string launchArgs = "server HeloDeck?Game=FoxGame.FoxGameMP_DM?ServerName=BLREdit-DM-Server?Port=7777?NumBots=12?MaxPlayers=16?Playlist=DM";
        StartProcess(launchArgs);
    }

    public void LaunchClient()
    {
        LaunchClient(BLREditSettings.GetLaunchOptions());
    }

    public void LaunchClient(LaunchOptions options)
    {
        string launchArgs = options.Server.IPAddress + ':' + options.Server.Port;
        launchArgs += $"?Name={options.UserName}";
        StartProcess(launchArgs);
    }
    private void StartProcess(string launchArgs)
    {
        ValidateClient();
        ValidateModules();
        //Write Proxy Config for installed modules

        ProcessStartInfo psi = new()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = PatchedPath,
            Arguments = launchArgs
        };
        Process game = new()
        {
            EnableRaisingEvents = true,
            StartInfo = psi
        };
        RunningClients.Add(game);
        game.Exited += ClientExit;
        game.Start();
    }
    private void ClientExit(object sender, EventArgs args)
    {
        var process = (Process)sender;
        RunningClients.Remove(process);
        LoggingSystem.Log($"[{this}]: has Exited with {process.ExitCode}");
        process.Dispose();
    }
    #endregion Launch/Exit

    private void ModifyClient()
    {
        MainWindow.ClientWindow.Client = this;
        MainWindow.ClientWindow.ShowDialog();
    }

    private void SetCurrentClient()
    {
        LoggingSystem.Log($"Setting Current Client:{this}");
        if (this.Patched.Is)
        {
            BLREditSettings.Settings.DefaultClient = this;
            foreach (BLRClient c in MainWindow.GameClients)
            {
                c.CurrentClient.SetBool(false);
            }
            this.CurrentClient.SetBool(true);
        }
    }

    private string CreateFolderStructure()
    {
        string[] pathParts = OriginalPath.Split('\\');
        string[] fileParts = pathParts[pathParts.Length - 1].Split('.');
        pathParts[pathParts.Length - 1] = fileParts[0] + "-BLREdit-Patched." + fileParts[1];
        string basePath = "";
        for (int i = pathParts.Length-4; i >= 0; i--)
        { 
            basePath = $"{pathParts[i]}\\{basePath}";
        }
        LoggingSystem.Log($"found root BLR Directory {basePath}");
        ConfigFolder = Directory.CreateDirectory($"{basePath}\\FoxGame\\Config\\BLRevive\\").FullName;
        ModulesFolder = Directory.CreateDirectory($"{basePath}\\Binaries\\Win32\\Modules\\").FullName;

        return $"{basePath}\\Binaries\\Win32\\{fileParts[0]}-BLREdit-Patched.{fileParts[1]}";
    }

    /// <summary>
    /// Patch this GameClient
    /// </summary>
    public void PatchClient()
    {
        string outFile = CreateFolderStructure();
        File.Copy(OriginalPath, outFile, true);

        AppliedPatches.Clear();

        if (BLRClientPatch.AvailablePatches.TryGetValue(this.ClientHash, out List<BLRClientPatch> patches))
        {
            try
            {
                using var PatchedFile = File.Open(outFile, FileMode.Open);
                using BinaryWriter binaryWriter = new((Stream)PatchedFile);
                foreach (BLRClientPatch patch in patches)
                {
                    LoggingSystem.Log($"Applying Patch:{patch.PatchName} to Client:{ClientHash}");
                    foreach (var part in patch.PatchParts)
                    {
                        binaryWriter.Seek(part.Key, SeekOrigin.Begin);
                        binaryWriter.Write(part.Value.ToArray());
                    }
                    this.AppliedPatches.Add(patch);
                }
                binaryWriter.Close();
                binaryWriter.Dispose();
                PatchedFile.Close();
                PatchedFile.Dispose();

                File.Copy($"{AppDomain.CurrentDomain.BaseDirectory}{IOResources.ASSET_DIR}\\dlls\\Proxy.dll", $"{Path.GetDirectoryName(outFile)}\\Proxy.dll", true);

                var peFile = new PeFile(outFile);
                peFile.AddImport("Proxy.dll", "InitializeThread");
                File.WriteAllBytes(outFile, peFile.RawFile.ToArray());
            }
            catch (Exception error)
            {
                LoggingSystem.Log(error.Message + '\n' + error.StackTrace);
            }
        }
        else
        {
            LoggingSystem.Log($"No patches found for {ClientHash}");
        }

        PatchedPath = outFile;
        PatchedHash = IOResources.CreateFileHash(outFile);
    }

    

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}