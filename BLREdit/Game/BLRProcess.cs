using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game;

public sealed class BLRProcess : IDisposable
{
    private static ObservableCollection<BLRProcess> RunningGames { get; } = new();

    private Process GameProcess { get;  set; }
    private BLRClient Client { get; set; }
    private bool Watchdog { get; set; }
    private BLRProcess(string launchArgs, BLRClient client, bool watchdog = false)
    {
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
        GameProcess.Start();
    }

    public static void CreateProcess(string launchArgs, BLRClient client, bool watchdog = false)
    {
        RunningGames.Add(new BLRProcess(launchArgs, client, watchdog));
    }

    private void ProcessExit(object sender, EventArgs args)
    {
        LoggingSystem.Log($"[{this.Client}]: has Exited with {GameProcess.ExitCode}");
        if (!Watchdog) { this.Dispose(); }
        else
        { LoggingSystem.Log($"[{this.Client}]: Restarting!"); this.GameProcess.Start(); }
    }

    public void Dispose()
    {
        GameProcess.Exited -= ProcessExit;
        GameProcess.Dispose();
        RunningGames.Remove(this);
    }
}
