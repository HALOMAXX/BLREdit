using BLREdit.API.Export;
using BLREdit.API.Utils;
using BLREdit.Export;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.UI;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;

namespace BLREdit.API.InterProcess;

public sealed class BLREditPipe
{
    public const string PIPE_NAME = "BLREdit";
    public const string API = "blredit://";
    public static bool IsServer { get; private set; } = true;
    private static Process Self { get; } = Process.GetCurrentProcess();
    public static Dictionary<string, Action<string>> ApiEndPoints { get; } = new();

    static BLREditPipe()
    {
        ValidateServerState();

        if (DataStorage.Settings.EnableAPI.Is)
        {
            if (IsServer || IsElevated())
            {
                AddOrUpdateProtocol();
            }
        }
    }

    static void SpawnPipes()
    {
        if (IsServer)
        {
            LoggingSystem.Log($"[PipeServer]: Starting {Environment.ProcessorCount} PipeThreads");
            for (int i = Environment.ProcessorCount; i > 0; i--)
            {
                var thread = new Thread(PipeServer) { IsBackground = true, Name = $"Pipe Server[{i}]" };
                App.AppThreads.Add(thread);
                thread.Start();
            }
        }
    }

    static DateTime LastTriedTime = Self.StartTime;
    static void ValidateServerState()
    {
        var running = Process.GetProcessesByName("BLREdit");

        if (running.Length <= 0) { IsServer = true; return; }
        IsServer = true;
        foreach (var run in running)
        {
            if (run.StartTime < LastTriedTime)
            {
                LastTriedTime = run.StartTime;
                IsServer = false;
                return;
            }
        }
    }

    static void AddOrUpdateProtocol()
    {
        if (Registry.ClassesRoot.OpenSubKey("blredit") is RegistryKey root)
        {
            if (root.OpenSubKey(@"shell\open\command") is RegistryKey command)
            {
                var tokens = (command.GetValue(string.Empty, string.Empty) as string)?.Split('"');

                if (IsElevated())
                {
                    if (tokens is not null && tokens.Length >= 2 && tokens[1] != $"{App.BLREditLocation}BLREdit.exe")
                    {
                        LoggingSystem.Log("Updating App path");

                        //run clean up and recreate total
                        command.Close();
                        root.Close();

                        command.Dispose();
                        root.Dispose();

                        File.WriteAllText("clean.reg",
                            @"[-HKEY_CLASSES_ROOT\blredit\shell\open\command]" +
                            @"[-HKEY_CLASSES_ROOT\blredit\shell\open]" +
                            @"[-HKEY_CLASSES_ROOT\blredit\shell]" +
                            @"[-HKEY_CLASSES_ROOT\blredit]");

                        var p = Process.Start("regedit.exe", "/s clean.reg");
                        p.WaitForExit();

                        File.Delete("clean.reg");

                        root = Registry.ClassesRoot.CreateSubKey("blredit");
                        root.SetValue(string.Empty, "URL: BLREdit Protocol");
                        root.SetValue("URL Protocol", string.Empty);

                        command = root.CreateSubKey(@"shell\open\command");
                        command.SetValue(string.Empty, $"\"{App.BLREditLocation}BLREdit.exe\" \"%1\"");

                        LoggingSystem.Log("Updated App path");

                        command.Close();
                        root.Close();

                        if (!IsServer) { Environment.Exit(0); }
                    }
                    command.Close();
                    root.Close();

                    command.Dispose();
                    root.Dispose();

                    LoggingSystem.Log("Running BLREdit with Admin privileges!"); 
                    return;
                }
                else
                {
                    if (tokens is not null && tokens.Length >= 2 && tokens[1] != $"{App.BLREditLocation}BLREdit.exe")
                    {
                        LoggingSystem.Log("Protocol Path is wrong");
                        command.Close();
                        root.Close();
                        RunAsAdmin();
                    }
                }
            }
            else
            {
                if (IsElevated())
                {
                    LoggingSystem.Log("Creating Missing App path");

                    command = root.CreateSubKey(@"shell\open\command");
                    command.SetValue(string.Empty, $"\"{App.BLREditLocation}BLREdit.exe\" \"%1\"");

                    LoggingSystem.Log("Created App path");

                    command.Close();
                    root.Close();
                    Environment.Exit(0);
                }
                else
                {
                    LoggingSystem.Log("Command Key is missing");
                    root.Close();
                    RunAsAdmin();
                }
            }
        }
        else
        {
            if (IsElevated())
            {
                LoggingSystem.Log("Installing App Protocol");

                root = Registry.ClassesRoot.CreateSubKey("blredit");
                root.SetValue(string.Empty, "URL: BLREdit Protocol");
                root.SetValue("URL Protocol", string.Empty);

                var command = root.CreateSubKey(@"shell\open\command");
                command.SetValue(string.Empty, $"\"{App.BLREditLocation}BLREdit.exe\" \"%1\"");

                LoggingSystem.Log("Finished installing App Protocol");

                command.Close();
                root.Close();
                Environment.Exit(0);
            }
            else
            {
                LoggingSystem.Log("Protocol is not installed");
                RunAsAdmin();
            }
        }
    }

    private static bool AddedApiEndpoints = false;
    static void AddApiEndpoints()
    {
        if (AddedApiEndpoints) { return; }
        AddedApiEndpoints = true;
        ApiEndPoints.Add("add-server", (json) => {
            if (json.StartsWith("{"))
            {
                LoggingSystem.Log($"[BLREdit API](add-server): Adding Server ({json})");
                var server = IOResources.Deserialize<BLRServer>(json);
                if (server != null && MainWindow.Instance != null)
                {
                    MainWindow.AddServer(server);
                }
            }
            else
            {
                LoggingSystem.Log($"[BLREdit API](add-server): Recieved malformed json!\n{json}");
            }
        });
        ApiEndPoints.Add("connect-server", (json) => {
            if (json.StartsWith("{"))
            {
                LoggingSystem.Log($"[BLREdit API](connect-server): Connecting to Server ({json})");
                var server = IOResources.Deserialize<BLRServer>(json);
                //MainWindow.AddServer(server);
                if (server != null && MainWindow.Instance != null)
                {
                    server.ConnectToServerCommand.Execute(null);
                }
            }
            else
            {
                LoggingSystem.Log($"[BLREdit API](connect-server): Connecting to ServerAddress ({json})");
                foreach (var server in DataStorage.ServerList)
                {
                    if (server.ServerAddress == json)
                    { 
                        server.ConnectToServerCommand.Execute(null);
                    }
                }
            }
        });
        ApiEndPoints.Add("start-server", (json) => {

            if (json.StartsWith("{"))
            {
                LoggingSystem.Log($"[BLREdit API](start-server): Starting Server ({json})");
                var serverConfig = IOResources.Deserialize<ServerLaunchParameters>(json) ?? new();
                BLRClient? client;
                if (serverConfig.ClientId < 0)
                { client = DataStorage.Settings.DefaultClient; }
                else
                { client = DataStorage.GameClients[serverConfig.ClientId]; }
                if (client is null)
                { LoggingSystem.Log("No client Available to launch server!"); return; }

                //TODO: Transform Required Modules of ServerConfig to Modules to send to StartProcess as Enabled Modules list

                string launchArgs = $"server {serverConfig.Map}?ServerName=\"{serverConfig.ServerName}\"?Port={serverConfig.Port}?NumBots={serverConfig.BotCount}?MaxPlayers={serverConfig.MaxPlayers}?Playlist={serverConfig.Playlist}?SCP={serverConfig.SCP}?TimeLimit={serverConfig.TimeLimit}";
                client.StartProcess(launchArgs, true, serverConfig.WatchDog);
            }
            else
            {
                LoggingSystem.Log($"[BLREdit API](start-server): Recieved malformed json!\n{json}");
            }
        });
        ApiEndPoints.Add("import-profile", (compressedBase64) => {
            LoggingSystem.Log($"[BLREdit API](import-profile): Importing Profile");
            if (string.IsNullOrEmpty(compressedBase64)) { LoggingSystem.Log($"[BLREdit API](import-profile): Recieved Empty string!"); return; }
            var compressedData = IOResources.Base64ToData(compressedBase64);
            var json = IOResources.Unzip(compressedData);
            if (string.IsNullOrEmpty(json) || (!json.StartsWith("{") && !json.StartsWith("["))) { LoggingSystem.Log("[BLREdit API](import-profile): Recieved invalid json"); return; }
            var sharedProfile = IOResources.Deserialize<Shareable3LoadoutSet>(json);
            if (sharedProfile is null) { LoggingSystem.Log("[BLREdit API](import-profile): failed to deserialize shareable profile!"); return; }
            var profile = sharedProfile.ToBLRProfile();
            var newProfile = BLRLoadoutStorage.AddNewLoadoutSet($"Imported-Profile{DataStorage.Loadouts.Count}", profile);
            MainWindow.ShowAlert($"{newProfile.Shareable.Name} has been Imported!");
        });
        //TODO: Add more api endpoints like add-weapon, import-loadout, select-loadout(for tournaments) and more
    }

    static void RunAsAdmin()
    {
        try
        {
            ProcessStartInfo info = new(App.BLREditLocation + "BLREdit.exe") { Verb = "runas" };
            var p = Process.Start(info);
            p.WaitForExit();
        }
        catch 
        {
            LoggingSystem.Log("Failed to Launch BLREdit as Admin will have to do without the API");
            DataStorage.Settings.EnableAPI.Set(false);
            BLREditSettings.Save();
        }
    }

    static bool IsElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static void PipeServer()
    {
        using var server = new NamedPipeServerStream(PIPE_NAME, PipeDirection.In, Environment.ProcessorCount);
        using var reader = new StreamReader(server);
        while (App.IsRunning)
        {
            server.WaitForConnection();
            LoggingSystem.Log($"[{Thread.CurrentThread.Name}]: Recieved Connection");
            try
            {
                var args = new List<string>();
                while (!reader.EndOfStream)
                {
                    args.Add(reader.ReadLine());
                }
                foreach (var line in args)
                {
                    LoggingSystem.Log($"[{Thread.CurrentThread.Name}]:{line}");
                }
                App.Current.Dispatcher.Invoke(() => { ProcessArgs(args.ToArray()); });
            }
            catch (Exception error)
            {
                LoggingSystem.Log($"[{Thread.CurrentThread.Name}](Error): {error}");
            }
            finally
            {
                server.Disconnect();
            }
        }
    }

    public static void ProcessArgs(string[] args)
    {
        AddApiEndpoints();
        if (args.Length <= 0) { return; }
        Dictionary<string, string> argDict = new();

        foreach (var arg in args)
        {
            if (!arg.StartsWith(API)) { continue; }

            int index = arg.IndexOf('/', API.Length);
            int offset = 1;
            if (arg.EndsWith("/"))
            {
                offset++;
            }

            int end = arg.Length - (index + offset);

            if (end <= 0) { LoggingSystem.Log($"Failed to Parse Arg:(E:{end} / O:{offset} / A:{arg.Length} / L:{API.Length}): {arg}"); continue; }
            string name = arg.Substring(API.Length, index - API.Length);
            string value = Uri.UnescapeDataString(arg.Substring(index + 1, end));

            if (ApiEndPoints.TryGetValue(name, out Action<string> action))
            {
                try
                {
                    action(value);
                }
                catch (Exception error)
                {
                    LoggingSystem.Log($"[API]({name}): {error}");
                }
            }
        }
    }

    public static bool ForwardLaunchArgs(string[] args)
    {
        if (IsServer || App.ForceStart) { SpawnPipes(); return false; }
        while (true)
        {
            using var client = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out);
            try
            {
                LoggingSystem.Log("trying to forward launch args over Pipe");
                client.Connect(100);
                LoggingSystem.Log($"Connected to Pipe. Started to transfer launch args:");
                using (var writer = new StreamWriter(client))
                {
                    foreach (var line in args)
                    {
                        writer.WriteLine(line);
                    }
                }
                LoggingSystem.Log("Finished transfering launch args");
                return true;
            }
            catch (Exception error)
            {
                LoggingSystem.Log(error.ToString());

                ValidateServerState();

                if (IsServer)
                {
                    SpawnPipes();
                    return false;
                }
            }
        }
    }
}
