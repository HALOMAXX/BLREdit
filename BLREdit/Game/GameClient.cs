using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    private string originalPath;
    public string OriginalPath { get { return originalPath; } set { if (originalPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { originalPath = value; IsPatched = false; OnPropertyChanged(); } } }
    private string patchedPath;
    public string PatchedPath { get { return patchedPath; } set { if (patchedPath != value && !string.IsNullOrEmpty(value) && File.Exists(value)) { patchedPath = value; IsPatched = false; OnPropertyChanged(); } } }

    
    private bool patchEmblem = true;
    public bool PatchEmblem { get { return patchEmblem; } set { patchEmblem = value; OnPropertyChanged(); } }
    
    private bool patchProxy = false;
    public bool PatchProxy { get { return patchProxy; } set { patchProxy = value; OnPropertyChanged(); } }

    private bool isProxy = false;
    public bool IsProxyPatched { get { return isProxy; } private set { isProxy = value; OnPropertyChanged(); } }
    
    private bool isEmblem = false;
    public bool IsEmblemPatched { get { return isEmblem; } private set { isEmblem = value; OnPropertyChanged(); } }

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
        Process game = new();
        game.StartInfo = psi;
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