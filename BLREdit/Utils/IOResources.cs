using BLREdit.Export;
using BLREdit.UI.Windows;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BLREdit;

public sealed class IOResources
{
    #region DIRECTORIES
    public const string PROFILE_DIR = "Profiles\\";
    public const string SEPROFILE_DIR = "SEProfiles\\";
    public const string ASSET_DIR = "Assets\\";
    public const string LOCAL_DIR = "localizations\\";
    public const string JSON_DIR = "json\\";
    public const string TEXTURE_DIR = "textures\\";
    public const string DLL_DIR = "dlls\\";
    public const string PATCH_DIR = "patches\\";
    public const string PREVIEW_DIR = "crosshairs\\";
    public const string UPDATE_DIR = "updates\\";
    public const string PACKAGE_DIR = "packaged\\";
    public const string GAME_PATH_SUFFIX = "steamapps\\common\\blacklightretribution\\Binaries\\Win32\\";
    public const string PROXY_MODULES_DIR = "Modules\\";
    #endregion DIRECTORIES

    private static string baseDirectory = null;
    public static string BaseDirectory { get { if (baseDirectory is null) { if (AppDomain.CurrentDomain.BaseDirectory.EndsWith("\\")) { baseDirectory = AppDomain.CurrentDomain.BaseDirectory; } else { baseDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\"; } } return baseDirectory; } }

    public const string GAME_APPID = "209870";

    static string wineVersion;
    public static string WineVersion { get { wineVersion ??= GetWineVersionString(); return wineVersion; } }

    public static bool IsWine { get { return !(WineVersion == "Not running under wine!"); } }

    private static string GetWineVersionString()
    {
        try
        {
            return GetWineVersion();
        }
        catch 
        {
            return "Not running under wine!";
        }
    }

    [DllImport("ntdll.dll", EntryPoint="wine_get_version", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    private static extern string GetWineVersion();

    #region FILES
    public const string GEAR_FILE = "gear.json";
    public const string MOD_FILE = "mods.json";
    public const string WEAPON_FILE = "weapons.json";
    public const string ITEM_LIST_FILE = "itemList.json";
    public const string SETTINGS_FILE = "settings.json";
    public const string PROXY_FILE = "Proxy.dll";
    public const string GAME_DEFAULT_EXE = "FoxGame-win32-Shipping.exe";
    #endregion FILES

    public static string Steam32InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", "") as string;
    public static string Steam6432InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", "") as string;
    public static readonly List<string> GameFolders = new();

    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter(), new JsonDoubleConverter(), new JsonFloatConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter(), new JsonDoubleConverter(), new JsonFloatConverter() } };

    public static WebClient WebClient { get; } = new WebClient();
    public static HttpClient HttpClient { get; } = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };
    public static HttpClient HttpClientWeb { get; } = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };

    static IOResources()
    {
        System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        WebClient.Headers.Add(HttpRequestHeader.UserAgent, $"BLREdit-{App.CurrentVersion}");
        if (!HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { LoggingSystem.Log($"Failed to add {HttpRequestHeader.UserAgent} to HttpClient"); };
        HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        if (!HttpClientWeb.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { LoggingSystem.Log($"Failed to add {HttpRequestHeader.UserAgent} to HttpClientWeb"); };
        HttpClientWeb.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
        Thread downloadClient = new(DownloadFiles);
        App.AppThreads.Add(downloadClient);
        downloadClient.Start();
    }



    private static BlockingCollection<DownloadRequest> DownloadRequests { get; } = new();
    public static void DownloadFile(string url, string filename)
    {
        DownloadRequest req = new(url, filename);
        DownloadRequests.Add(req);
        WaitHandle.WaitAll(new WaitHandle[] { req.locked });
    }

    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    private static void DownloadFiles()
    {
        while (App.IsRunning)
        {
            var request = DownloadRequests.Take();
            try
            {
                if(!request.file.Directory.Exists) request.file.Directory.Create();
                if (!request.file.Exists) request.file.Delete();
                WebClient.DownloadFile(request.url, request.file.FullName);
            }
            catch (Exception error)
            {
                LoggingSystem.Log($"[WebClient]Failed to Download({request.url})\nReason:{error}");
            }
            finally 
            { request.locked.Set(); }
        }
    }

    private class DownloadRequest
    {
        public string url;
        public FileInfo file;
        public AutoResetEvent locked;

        public DownloadRequest(string url, string filename)
        {
            this.url = url;
            this.file = new FileInfo(filename);
            locked = new AutoResetEvent(false);
        }

        public DownloadRequest(string url, FileInfo file)
        {
            this.url = url;
            this.file = file;
            locked = new AutoResetEvent(false);
        }
    }

    public static void GetGameLocationsFromSteam()
    {
        string steampath;
        if (string.IsNullOrEmpty(Steam32InstallFolder) && string.IsNullOrEmpty(Steam6432InstallFolder))
        {
            LoggingSystem.Log("not performing steam library probing because no steam install is found");
            return;
        }
        if (string.IsNullOrEmpty(Steam32InstallFolder))
        {
            steampath = Steam6432InstallFolder;
        }
        else
        {
            steampath = Steam32InstallFolder;
        }

        steampath += "\\steamapps\\";

        GetGamePathFromVDF(steampath + "libraryfolders.vdf", GAME_APPID);
    }

    [DllImport("Kernel32")]
    private static extern void AllocConsole();

    [DllImport("Kernel32")]
    private static extern void FreeConsole();
    public static void SpawnConsole()
    { 
        AllocConsole();
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    private static void GetGamePathFromVDF(string vdfPath, string appID)
    {
        if (string.IsNullOrEmpty(vdfPath) || string.IsNullOrEmpty(appID)) { LoggingSystem.Log($"vdfPath or AppID is empty"); return; }
        if (!File.Exists(vdfPath)) { LoggingSystem.Log($"vdfPath file doesn't exist"); return; }
        
        try
        {
            string data;
            data = File.ReadAllText(vdfPath);
            VToken libraryInfo = VdfConvert.Deserialize(data).Value;

            foreach (VProperty library in libraryInfo.Children().Cast<VProperty>())
            {
                if (library.Key != "contentstatsid")
                {
                    //0 = path /// 6=apps
                    GameFolders.Add($"{((VValue)((VProperty)library.Value.Children().First()).Value).Value}\\{GAME_PATH_SUFFIX}");
                }
            }
        }
        catch (Exception error)
        {
            LoggingSystem.MessageLog($"failed reading {vdfPath}, found Folders:{GameFolders.Count}, steam library parsing aborted reason:\n{error}");
            GameFolders.Clear();
            return;
        }
    }

    public static void DownloadFileMessageBox(string url, string filename)
    {
        var dlWindow = new DownloadInfoWindow(url, filename);
        dlWindow.ShowDialog();
    }

    public static string CreateFileHash(string path)
    {
        if (!File.Exists(path)) { LoggingSystem.Log($"[BLRClient]: Hashing failed reason: Can't find {path}"); return null; }
        using var stream = File.OpenRead(path);
        using var crypto = SHA256.Create();
        return BitConverter.ToString(crypto.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
    }

    public static void CopyToBackup(string file)
    {
        if (string.IsNullOrEmpty(file)) return;
        FileInfo info = new(file);
        File.Copy(file, ExportSystem.CurrentBackupFolder.FullName + info.Name, true);
    }

    public static string JsonToBase64(string json)
    {
        return Base64UrlEncoder.Encode(IOResources.Zip(json));
    }

    public static string Base64ToJson(string base64)
    {
        return IOResources.Unzip(Base64UrlEncoder.DecodeBytes(base64));
    }

    public static byte[] Zip(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);

        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            CopyStreamToStream(msi, gs);
        }

        return mso.ToArray();
    }

    public static string Unzip(byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            CopyStreamToStream(gs, mso);
        }

        return Encoding.UTF8.GetString(mso.ToArray());
    }

    public static void CopyStreamToStream(Stream src, Stream dest)
    {
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
        {
            dest.Write(bytes, 0, cnt);
        }
    }

    public static void SerializeFile<T>(string filePath, T? obj, bool compact = false)
    {
        if (string.IsNullOrEmpty(filePath)) { LoggingSystem.Log("[Serializer]: filePath was empty!"); return; }
        if (obj is null) { LoggingSystem.Log("[Serializer]: obj was null!"); return; }

        bool writeFile;
        if (obj is ExportSystemProfile prof)
        {
            if (prof.IsDirty)
            {
                LoggingSystem.Log($"[Serializer]: {prof.Name}❕");
                writeFile = true;
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
            else
            {
                LoggingSystem.Log($"[Serializer]: {prof.Name}✔");
                writeFile = false;
            }
        }
        else
        {
            writeFile = true;
            if (File.Exists(filePath)) { File.Delete(filePath); }
        }

        if (writeFile)
        {
            using var file = File.CreateText(filePath);
            file.Write(Serialize(obj, compact));
            file.Close();
            LoggingSystem.Log($"[Serializer]: {typeof(T).Name} serialize succes!");
        }
    }

    public static string Serialize<T>(T? obj, bool compact = false)
    {
        if (obj is null) { LoggingSystem.Log("[Serializer]: object was null!"); return ""; }
        if (compact)
        {
            return JsonSerializer.Serialize<T?>(obj, JSOCompacted);
        }
        else
        {
            return JsonSerializer.Serialize<T?>(obj, JSOFields);
        }
    }

    /// <summary>
    /// Deserializes the file and returns the object
    /// </summary>
    /// <typeparam name="T">Type that is contained within the file</typeparam>
    /// <param name="filePath">the filepath</param>
    /// <returns>will return object of type on success otherwise will return the default object of said type also if the file is not readable/existent will also return default</returns>
    public static T? DeserializeFile<T>(string filePath)
    {
        T? temp = default;
        if (string.IsNullOrEmpty(filePath)) { return temp; }
        
        //check if file exist's before we try to read it if it doesn't exist return and Write an error to log
        if (!File.Exists(filePath))
        { LoggingSystem.Log($"[Serializer]: File({filePath}) was not found for Deserialization!"); return temp; }

        using (var file = File.OpenText(filePath))
        {
            temp = Deserialize<T>(file.ReadToEnd());
            file.Close();
        }
        
        if (temp is ExportSystemProfile prof)
        {
            prof.IsDirty = false;
        }

        return temp;
    }

    public static T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JSOFields);
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"[Serializer]: {error}");
        }
        return default;
    }
}