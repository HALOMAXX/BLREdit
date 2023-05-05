using BLREdit.Export;
using BLREdit.Model.BLR;

using PeNet;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game;

public sealed class BLRProcess : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    private static ObservableCollection<BLRProcess> RunningGames { get; } = new();

    private Process gameProcess;
    private Process GameProcess { get { return gameProcess; }  set { gameProcess = value; OnPropertyChanged(); } }
    private BLRClientModel client;
    private BLRClientModel Client { get { return client; } set { client = value; OnPropertyChanged(); } }
    private bool isServer = false;
    private bool IsServer { get { return isServer; } set { isServer = value; OnPropertyChanged(); } }
    private bool watchdog = false;
    private bool Watchdog { get { return watchdog; } set { watchdog = value; OnPropertyChanged(); } }

    static BLRProcess()
    {
        RunningGames.CollectionChanged += RunningGamesChanged;
    }

    public static bool IsServerRunning()
    {
        foreach (var process in RunningGames)
        {
            if (process.IsServer) { return true; }
        }
        return false;
    }

    private static void RunningGamesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
                foreach (BLRProcess process in e.OldItems)
                { 
                    process.Cleanup();
                }
                break;
            case NotifyCollectionChangedAction.Add:
                foreach (BLRProcess process in e.NewItems)
                { 
                    process.Start();
                }
                break;
        }
    }

    private BLRProcess(string launchArgs, BLRClientModel client,bool isServer, bool watchdog = false)
    {
        IsServer= isServer;
        Client = client;
        Watchdog = watchdog;
        ProcessStartInfo psi = new()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = Client.PatchedPath,
            Arguments = launchArgs
        };
        GameProcess = new()
        {
            EnableRaisingEvents = true,
            StartInfo = psi
        };
        GameProcess.Exited += ProcessExit;
    }

    public static void CreateProcess(string launchArgs, BLRClientModel client, bool isServer, bool watchdog = false)
    {
        RunningGames.Add(new BLRProcess(launchArgs, client, isServer, watchdog));
    }

    public static void KillAll()
    {
        for (int i = RunningGames.Count - 1; i >= 0; i--)
        {
            RunningGames.RemoveAt(i);
        }
    }

    private void ProcessExit(object sender, EventArgs args)
    {
        LoggingSystem.Log($"[{this.Client}]: has Exited with {GameProcess.ExitCode}");
        if (!IsServer)
        {
            BLRClientModel.LoadOrUpdateProfileSettings(Client);
            foreach (var profile in Client.ProfileSettings)
            {
                ExportSystem.UpdateOrAddProfileSettings(profile.Value.ProfileName, profile.Value);
            }
        }

        if (!Watchdog) { this.Remove(); }
        else
        { LoggingSystem.Log($"[{this.Client}]: Restarting!"); this.GameProcess.Start(); }
    }

    private void Start()
    {
        GameProcess.Start();
    }

    public void Remove()
    {
        RunningGames.Remove(this);
    }

    public void Cleanup()
    {
        GameProcess.Exited -= ProcessExit;
        if (!GameProcess.HasExited)
        { 
            GameProcess.Kill();
        }
        GameProcess.Dispose();
    }
}
