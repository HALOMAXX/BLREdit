using System;
using System.Collections.Generic;
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

using PeNet;

namespace BLREdit.Game;

public class GameClient : INotifyPropertyChanged
{
    [JsonIgnore] public bool IsPatched { get { return !string.IsNullOrEmpty(PatchedPath) && File.Exists(PatchedPath); } set { IsNotPatched = false; OnPropertyChanged(); } }
    [JsonIgnore] public bool IsNotPatched { get { return !IsPatched; } private set { OnPropertyChanged(); } }
    [JsonIgnore] public bool IsCurrentClient { get { return PatchedPath == BLREditSettings.Settings?.DefaultClient?.PatchedPath && IsPatched; } set { IsNotCurrentClient = false; OnPropertyChanged(); } }
    [JsonIgnore] public bool IsNotCurrentClient { get { return !IsCurrentClient; } private set { OnPropertyChanged(); } }

    [JsonIgnore] private string clientHash;
    public string ClientHash { get { return clientHash; } 
        set 
        { 
            if (clientHash != value && !string.IsNullOrEmpty(value)) 
            { 
                clientHash = value;
                VersionHashes.TryGetValue(ClientHash, out int version);
                ClientVersion = version;
                OnPropertyChanged(); 
            }
        }
    }

    [JsonIgnore] private int version;
    public int ClientVersion { get { return version; } set { version = value; OnPropertyChanged(); } }

    [JsonIgnore] private string originalPath;
    public string OriginalPath { get { return originalPath; } set { if (originalPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { originalPath = value; IsPatched = false; ClientHash = CreateClientHash(value);  OnPropertyChanged(); } } }
    
    [JsonIgnore] private string patchedPath;
    public string PatchedPath { get { return patchedPath; } set { if (patchedPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { patchedPath = value; IsPatched = false; OnPropertyChanged(); } } }

    [JsonIgnore] private bool patchEmblem = true;
    public bool PatchEmblem { get { return patchEmblem; } set { patchEmblem = value; OnPropertyChanged(); } }

    [JsonIgnore] private bool patchProxy = false;
    public bool PatchProxy { get { return patchProxy; } set { patchProxy = value; OnPropertyChanged(); } }

    [JsonIgnore] private bool isProxy = false;
    public bool IsProxyPatched { get { return isProxy; } private set { isProxy = value; OnPropertyChanged(); } }

    [JsonIgnore] private bool isEmblem = false;
    public bool IsEmblemPatched { get { return isEmblem; } private set { isEmblem = value; OnPropertyChanged(); } }

    public static Dictionary<string, int> VersionHashes => new()
    {
        {"0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7", 302},
        {"de08147e419ed89d6db050b4c23fa772338132587f6b533b6233733f9bce46c3", 301},
        {"1742df917761f9dc01b079ae2aad78ef2ff17562af1dad6ad6ea7cf3622fe7f6", 300},
        {"d4f9cec736a83f7930f04438344d35ff9f0e57212755974bd51f48ff89d303c4", 120}
    };

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    private static string CreateClientHash(string filePath)
    {
        using (var crypto = SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            { 
                return BitConverter.ToString(crypto.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
            }
        }
    }

    public void LaunchClient()
    {
        LaunchClient(BLREditSettings.GetLaunchOptions());
    }

    public void LaunchClient(LaunchOptions options)
    {
        string launchArgs = options.Server.IPAddress + ':' + options.Server.Port;
        launchArgs += $"?Name={options.UserName}";
        ProcessStartInfo psi = new ProcessStartInfo()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        psi.FileName = PatchedPath;
        psi.Arguments = launchArgs;
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
        //if (IsPatched) { return; }
        string outFile = GetNewPatchedFile();
        File.Copy(OriginalPath, outFile, true);
        var PatchedFile = new FileStream(outFile, FileMode.Open);
        BinaryWriter binaryWriter = new((Stream)PatchedFile);
        try
        {
            binaryWriter.Seek(510, SeekOrigin.Begin);
            binaryWriter.Write((byte)0);
            if (PatchEmblem)
            {
                byte[] buffer = new byte[4]
                {
                (byte) 144,
                (byte) 144,
                (byte) 144,
                (byte) 144
                };
                binaryWriter.Seek(11766694, SeekOrigin.Begin);
                binaryWriter.Write(buffer);
            }
            
            binaryWriter.Close();
        }
        catch (Exception error)
        {
            LoggingSystem.LogError(error.Message + '\n' + error.StackTrace);
        }
        PatchedFile.Close();
        PatchedPath = outFile;
    }
}