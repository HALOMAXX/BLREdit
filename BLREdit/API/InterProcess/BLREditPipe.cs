using BLREdit.Game;
using BLREdit.UI;

using Microsoft.Win32;

using PeNet.Header.Net.MetaDataTables;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BLREdit.API.InterProcess;

public sealed class BLREditPipe
{
    public const string PIPE_NAME = "BLREdit";
    public static bool IsServer { get; private set; } = true;

    private static List<Thread> ServerThreads { get; } = new List<Thread>();

    private static Process Self { get; } = Process.GetCurrentProcess();

    static BLREditPipe()
    {
        //Test if BLRedit is already running if not we can prepare a PipeServer
        ValidateServerState();

        if (IsServer || IsElevated())
        {
            AddOrUpdateProtocol();
        }

        SpawnPipes();
    }

    static void SpawnPipes()
    {
        if (IsServer)
        {
            LoggingSystem.Log($"[PipeServer]: Starting {Environment.ProcessorCount} PipeThreads");
            for (int i = Environment.ProcessorCount; i > 0; i--)
            {
                var thread = new Thread(PipeServer) { IsBackground = true, Name = $"Pipe Server[{i}]" };
                ServerThreads.Add(thread);
                thread.Start();
            }
        }
    }

    static void ValidateServerState()
    {
        var running = Process.GetProcessesByName("BLREdit");

        if (running.Length <= 0) { IsServer = true; return; }
        IsServer = true;
        foreach (var run in running)
        {
            if (run.StartTime < Self.StartTime)
            {
                IsServer = false;
            }
        }
    }

    static void AddOrUpdateProtocol()
    {
        var root = Registry.ClassesRoot.OpenSubKey("blredit");
        RegistryKey command = null;
        if (root is not null)
        {
            command = root.OpenSubKey(@"shell\open\command");
        }

        if (root is null && IsElevated())
        {
            LoggingSystem.Log("Installing App Protocol");

            root = Registry.ClassesRoot.CreateSubKey("blredit");
            root.SetValue(string.Empty, "URL: BLREdit Protocol");
            root.SetValue("URL Protocol", string.Empty);

            command = root.CreateSubKey(@"shell\open\command");
            command.SetValue(string.Empty, $"\"{App.BLREditLocation}BLREdit.exe\" \"%1\"");

            LoggingSystem.Log("Finished installing App Protocol");

            command.Close();
            root.Close();
            Environment.Exit(0);
        }
        else if (root is null && !IsElevated())
        {
            LoggingSystem.Log("Protocol is not installed");
            RunAsAdmin();
        }
        else if (command is null && IsElevated())
        {
            LoggingSystem.Log("Creating Missing App path");

            command = root.CreateSubKey(@"shell\open\command");
            command.SetValue(string.Empty, $"\"{App.BLREditLocation}BLREdit.exe\" \"%1\"");

            LoggingSystem.Log("Created App path");

            command.Close();
            root.Close();
            Environment.Exit(0);
        }
        else if (command is null && !IsElevated())
        {
            LoggingSystem.Log("Command Key is missing");
            RunAsAdmin();
        }
        else if (command is not null && IsElevated())
        {
            LoggingSystem.Log("Updating App path");

            //run clean up and recreate total
            command?.Close();
            root?.Close();


            System.IO.File.WriteAllText("clean.reg", 
                @"[-HKEY_CLASSES_ROOT\blredit\shell\open\command]" +
                @"[-HKEY_CLASSES_ROOT\blredit\shell\open]" +
                @"[-HKEY_CLASSES_ROOT\blredit\shell]" +
                @"[-HKEY_CLASSES_ROOT\blredit]");

            var p = Process.Start("regedit.exe", "/s clean.reg");
            p.WaitForExit();

            System.IO.File.Delete("clean.reg");

            root = Registry.ClassesRoot.CreateSubKey("blredit");
            root.SetValue(string.Empty, "URL: BLREdit Protocol");
            root.SetValue("URL Protocol", string.Empty);

            command = root.CreateSubKey(@"shell\open\command");
            command.SetValue(string.Empty, $"\"{App.BLREditLocation}BLREdit.exe\" \"%1\"");

            LoggingSystem.Log("Updated App path");

            command?.Close();
            root?.Close();
            Environment.Exit(0);
        }
        else if (command is not null && !IsElevated())
        {
            var tokens = (command.GetValue(string.Empty, string.Empty) as string).Split('"');
            if (tokens.Length >= 2)
            {
                if (tokens[1] != $"{App.BLREditLocation}BLREdit.exe")
                {
                    LoggingSystem.Log("Protocol Path is wrong");
                    RunAsAdmin();
                }
            }
        }

        command?.Close();
        root?.Close();
    }

    static void RunAsAdmin()
    {
        ProcessStartInfo info = new(App.BLREditLocation + "BLREdit.exe") { Verb = "runas" };
        var p = Process.Start(info);
        p.WaitForExit();
    }

    static bool IsElevated()
    {
        using (var identity = WindowsIdentity.GetCurrent())
        { 
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    static void PipeServer()
    {
        using (var server = new NamedPipeServerStream(PIPE_NAME, PipeDirection.In, Environment.ProcessorCount))
        {
            using (var reader = new StreamReader(server))
            {
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
                        App.Current.Dispatcher.Invoke(() => { ProcessArgs(args.ToArray()); } );
                    }
                    catch(Exception error)
                    {
                        LoggingSystem.Log($"[{Thread.CurrentThread.Name}](Error): {error}");
                    }
                    finally 
                    {
                        server.Disconnect();
                    }
                }
            }
        }
    }

    const string API = "blredit://";
    public static void ProcessArgs(string[] args)
    {
        Dictionary<string, string> argDict = new();

        for (var i = 0; i < args.Length; i++)
        {
            LoggingSystem.Log($"{args[i]}");

            if (!args[i].StartsWith(API)) continue;

            int index = args[i].IndexOf('/', API.Length);

            int offset = 1;
            if (args[i].EndsWith("/"))
            {
                offset++;
            }

            int end = args[i].Length - (index + offset);

            if (end <= 0) { LoggingSystem.Log($"Failed to Parse Arg:(E:{end} / O:{offset} / A:{args[i].Length} / L:{API.Length}): {args[i]}"); continue; }
            string name = args[i].Substring(API.Length, index - API.Length);
            string value = Uri.UnescapeDataString(args[i].Substring(index + 1, end));

            LoggingSystem.Log($"{name}[{index}]: {value}");

            argDict.Add(name, value);
        }

        if (argDict.ContainsKey("add-server"))
        {
            var json = argDict["add-server"];
            LoggingSystem.Log($"[{Thread.CurrentThread.Name}]: Adding Server ({json})");
            var server = IOResources.Deserialize<BLRServer>(json);
            if (server != null && MainWindow.Self != null)
            {
                MainWindow.Self.AddServer(server);
            }
        }
        if (argDict.ContainsKey("connect-server"))
        {
            var json = argDict["connect-server"];
            LoggingSystem.Log($"[{Thread.CurrentThread.Name}]: Adding Server ({json})");
            var server = IOResources.Deserialize<BLRServer>(json);
            if (server != null && MainWindow.Self != null)
            {
                //MainWindow.Self.AddServer(server);
                server.ConnectToServerCommand.Execute(null);
            }
        }
    }

    public static void StopServerThreads()
    {
        foreach (var thread in ServerThreads)
        {
            if (thread.IsAlive)
            { 
                thread.Abort();
            }
        }
    }

    public static bool ForwardLaunchArgs(string[] args)
    {
        if (IsServer) { return false; }
        int linesWritten = 0;
        while (linesWritten < args.Length)
        {
            using (var client = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out))
            {
                try
                {
                    LoggingSystem.Log("trying to forward launch args over Pipe");
                    client.Connect(1000);
                    LoggingSystem.Log($"Connected to Pipe. Started to transfer launch args:");
                    using (var writer = new StreamWriter(client))
                    {
                        foreach (var line in args)
                        {
                            writer.WriteLine(line);
                            linesWritten++;
                        }
                    }
                    LoggingSystem.Log("Finished transfering launch args");
                    return true;
                }
                catch (Exception error)
                {
                    LoggingSystem.Log(error.ToString());

                    linesWritten = 0;

                    ValidateServerState();

                    if (IsServer)
                    {
                        SpawnPipes();
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
