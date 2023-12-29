using BLREdit.UI;

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
using System.Windows.Forms;

namespace BLREdit.Game;

public sealed class BLRProcess : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    public static ObservableCollection<BLRProcess> RunningGames { get; } = new();

    private Process gameProcess;
    public Process GameProcess { get { return gameProcess; } private set { gameProcess = value; OnPropertyChanged(); } }
    private BLRClient client;
    public BLRClient Client { get { return client; } private set { client = value; OnPropertyChanged(); } }
    private bool isServer = false;
    public bool IsServer { get { return isServer; } private set { isServer = value; OnPropertyChanged(); } }
    private bool watchdog = false;
    public bool Watchdog { get { return watchdog; } private set { watchdog = value; OnPropertyChanged(); } }
    private BLRServer? connectedServer;
    public BLRServer? ConnectedServer { get { return connectedServer; } private set { connectedServer = value; OnPropertyChanged(); } }
    private string? launchArguments;

    static BLRProcess()
    {
        RunningGames.CollectionChanged += RunningGamesChanged;
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

    private BLRProcess(string launchArgs, BLRClient client,bool isServer, bool watchdog = false, BLRServer? server = null)
    {
        ConnectedServer = server;
        IsServer= isServer;
        this.client = client;
        Watchdog = watchdog;
        ProcessStartInfo psi = new()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = Client.SDKType == "BLRevive" ? Client.OriginalPath : Client.PatchedPath,
            Arguments = launchArgs
        };
        this.gameProcess = new()
        {
            EnableRaisingEvents = true,
            StartInfo = psi
        };
        GameProcess.Exited += ProcessExit;
    }

    public static void CreateProcess(string launchArgs, BLRClient client, bool isServer, bool watchdog = false, BLRServer? server = null)
    {
        RunningGames.Add(new BLRProcess(launchArgs, client, isServer, watchdog, server));
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
        LoggingSystem.Log($"[{this.Client}]({(this.IsServer ? "Server" : "Client")}): has Exited with {GameProcess.ExitCode}");
        if (!IsServer)
        {
            Client.UpdateProfileSettings();
            MainWindow.MainView.UpdateWindowTitle();
            LoggingSystem.Log($"[{this.Client}]: Grabbing Settings from client!");
        }

        if (!Watchdog) { this.Remove(); }
        else
        { LoggingSystem.Log($"[{this.Client}]: Restarting!"); this.GameProcess.Start(); }
    }

    private void Start()
    {
        LoggingSystem.Log($"Starting client:\nArgs: {GameProcess.StartInfo.Arguments}");
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
        if (!IsServer)
        {
            Client.UpdateProfileSettings();
            MainWindow.MainView.UpdateWindowTitle();
        }
    }
}
