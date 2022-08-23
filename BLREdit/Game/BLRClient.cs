using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

using BLREdit.UI;

using PeNet;

namespace BLREdit.Game;

public class BLRClient : INotifyPropertyChanged
{
    private static SHA256 crypto = SHA256.Create();
    [JsonIgnore] public UIBool Patched { get; private set; } = new UIBool(false);
    [JsonIgnore] public UIBool CurrentClient { get; private set; } = new UIBool(false);

    private string clientHash;
    public string ClientHash {
        get { return clientHash; }
        set
        {
            if (clientHash != value && !string.IsNullOrEmpty(value))
            {
                clientHash = value;
                OnPropertyChanged(nameof(ClientVersion));
                OnPropertyChanged();
            }
        }
    }
    [JsonIgnore] public string ClientVersion { get { if (VersionHashes.TryGetValue(ClientHash, out string version)) { return version; } else { return "Unknown"; } } }

    private string originalPath;
    public string OriginalPath { get { return originalPath; } set { if (originalPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { originalPath = value; ClientHash = CreateClientHash(value); OnPropertyChanged(); } } }

    private string patchedPath;
    public string PatchedPath { get { return patchedPath; } set { if (patchedPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { patchedPath = value; Patched.SetBool(true); OnPropertyChanged(); } } }

    public ObservableCollection<BLRClientPatch> AppliedPatches { get; private set; } = new();

    public static Dictionary<string, List<BLRClientPatch>> AvailablePatches { get; private set; } = LoadPatches();

    private static Dictionary<string, List<BLRClientPatch>> LoadPatches()
    {
        List<BLRClientPatch> loadedPatches = new();
        Dictionary<string, List<BLRClientPatch>> sortedPatches = new();
        string[] patches = Directory.GetFiles(IOResources.ASSET_DIR+"patches\\");
        foreach (string file in patches)
        {
            if(file.EndsWith(".json"))
            loadedPatches.Add(IOResources.DeserializeFile<BLRClientPatch>(file));
        }
        if (loadedPatches.Count <= 0)
        {
            IOResources.SerializeFile("patch.json", new BLRClientPatch());
            loadedPatches.Add(new BLRClientPatch());
        }
        foreach (var loadedPatch in loadedPatches)
        {
            if (sortedPatches.TryGetValue(loadedPatch.ClientHash, out List<BLRClientPatch> patchList))
            {
                patchList.Add(loadedPatch);
            }
            else
            {
                List<BLRClientPatch> patchesList = new();
                patchesList.Add(loadedPatch);
                sortedPatches.Add(loadedPatch.ClientHash, patchesList);
            }
        }
        return sortedPatches;
    }

    public static Dictionary<string, string> VersionHashes => new()
    {
        {"0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7", "v3.02"},
        {"de08147e419ed89d6db050b4c23fa772338132587f6b533b6233733f9bce46c3", "v3.01"},
        {"1742df917761f9dc01b079ae2aad78ef2ff17562af1dad6ad6ea7cf3622fe7f6", "v3.00"},
        {"d4f9cec736a83f7930f04438344d35ff9f0e57212755974bd51f48ff89d303c4", "v1.20"},
        {"4032ed1c45e717757a280e4cfe2408bb0c4e366676b785f0ffd177c3054c13a5", "v1.40"},
        {"01890318303354f588d9b89bb1a34c5c49ff881d2515388fcc292b54eb036b58", "v1.30"},
        {"d0bc0ae14ab4dd9f407de400da4f333ee0b6dadf6d68b7504db3fc46c4baa59f", "v1.100"},
        {"9200705daddbbc10fee56db0586a20df1abf4c57a9384a630c578f772f1bd116", "v0.993"}
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

    private ICommand patchClientCommand;
    [JsonIgnore]
    public ICommand PatchClientCommand
    {
        get
        {
            if (patchClientCommand == null)
            {
                patchClientCommand = new RelayCommand(
                    param => this.PatchClient()
                );
            }
            return patchClientCommand;
        }
    }

    private ICommand launchClientCommand;
    [JsonIgnore]
    public ICommand LaunchClientCommand
    {
        get 
        {
            if (launchClientCommand == null)
            {
                launchClientCommand = new RelayCommand(
                    param => this.LaunchClient()
                );
            }
            return launchClientCommand;
        }
    }

    private ICommand launchServerCommand;
    [JsonIgnore]
    public ICommand LaunchServerCommand
    {
        get
        {
            if (launchServerCommand == null)
            {
                launchServerCommand = new RelayCommand(
                    param => this.LaunchServer()
                );
            }
            return launchServerCommand;
        }
    }

    private ICommand launchBotMatchCommand;
    [JsonIgnore]
    public ICommand LaunchBotMatchCommand
    {
        get
        {
            if (launchBotMatchCommand == null)
            {
                launchBotMatchCommand = new RelayCommand(
                    param => this.LaunchBotMatch()
                );
            }
            return launchBotMatchCommand;
        }
    }

    private void LaunchBotMatch()
    {
        string launchArgs = "server HeloDeck?Game=FoxGame.FoxGameMP_DM?SingleMatch?NumBots=12";
        ProcessStartInfo psi = new()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = PatchedPath,
            Arguments = launchArgs
        };
        Process game = new()
        {
            StartInfo = psi
        };
        game.Start();
    }

    private void LaunchServer()
    {
        string launchArgs = "server HeloDeck?Game=FoxGame.FoxGameMP_DM?ServerName=BLREdit-DM-Server?Port=7777?NumBots=12?MaxPlayers=16?Playlist=DM";
        ProcessStartInfo psi = new()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = PatchedPath,
            Arguments = launchArgs
        };
        Process game = new()
        {
            StartInfo = psi
        };
        game.Start();
    }

    public void LaunchClient()
    {
        LaunchClient(BLREditSettings.GetLaunchOptions());
    }

    public void LaunchClient(LaunchOptions options)
    {
        string launchArgs = options.Server.IPAddress + ':' + options.Server.Port;
        launchArgs += $"?Name={options.UserName}";
        ProcessStartInfo psi = new()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = PatchedPath,
            Arguments = launchArgs
        };
        Process game = new()
        {
            StartInfo = psi
        };
        game.Start();
    }

    private string GetNewPatchedFile()
    {
        string[] pathParts = OriginalPath.Split('\\');
        string[] fileParts = pathParts[pathParts.Length - 1].Split('.');
        pathParts[pathParts.Length - 1] = fileParts[0] + "-BLREdit-Patched." + fileParts[1];
        string newPath = "";
        for (int i = 0; i < pathParts.Length; i++)
        { 
            newPath += pathParts[i];
            if (i < pathParts.Length - 1)
            {
                newPath += '\\';
            }
        }
        return newPath;
    }

    /// <summary>
    /// Patch this GameClient
    /// </summary>
    public void PatchClient()
    {
        if (Patched.Is) { return; }
        string outFile = GetNewPatchedFile();
        File.Copy(OriginalPath, outFile, true);
        using var PatchedFile = File.Open(outFile, FileMode.Open);
        using BinaryWriter binaryWriter = new((Stream)PatchedFile);
        if (AvailablePatches.TryGetValue(this.ClientHash, out List<BLRClientPatch> patches))
        {
            try
            {
                foreach (BLRClientPatch patch in patches)
                {
                    LoggingSystem.LogInfo($"Applying Patch:{patch.PatchName} to Client:{ClientHash}");
                    foreach (var part in patch.PatchParts)
                    {
                        binaryWriter.Seek(part.Key, SeekOrigin.Begin);
                        binaryWriter.Write(part.Value.ToArray());
                    }
                    this.AppliedPatches.Add(patch);
                }
            }
            catch (Exception error)
            {
                LoggingSystem.LogError(error.Message + '\n' + error.StackTrace);
            }
        }
        else
        {
            LoggingSystem.LogWarning($"No patches found for {ClientHash}");
        }
        binaryWriter.Close();
        binaryWriter.Dispose();
        PatchedFile.Close();
        PatchedFile.Dispose();
        PatchedPath = outFile;
        MainWindow.Self.CheckGameClientSetup();
    }


    public static string CreateClientHash(string path)
    {
        using var stream = File.OpenRead(path);
        return BitConverter.ToString(crypto.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}