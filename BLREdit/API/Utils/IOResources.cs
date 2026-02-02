using BLREdit.Export;
using BLREdit.UI.Windows;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
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

    private static string? baseDirectory;
    public static string BaseDirectory { get { if (baseDirectory is null) { if (AppDomain.CurrentDomain.BaseDirectory.EndsWith("\\", StringComparison.InvariantCulture)) { baseDirectory = AppDomain.CurrentDomain.BaseDirectory; } else { baseDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\"; } } return baseDirectory; } }

    public const string GAME_APPID = "209870";

    #region FILES
    public const string GEAR_FILE = "gear.json";
    public const string MOD_FILE = "mods.json";
    public const string WEAPON_FILE = "weapons.json";
    public const string ITEM_LIST_FILE = "itemList.json";
    public const string SETTINGS_FILE = "settings.json";
    public const string PROXY_FILE = "Proxy.dll";
    public const string GAME_DEFAULT_EXE = "FoxGame-win32-Shipping.exe";
    #endregion FILES

    public static string Steam32InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", "") as string ?? string.Empty;
    public static string Steam6432InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", "") as string ?? string.Empty;
    public static readonly List<string> GameFolders = [];
    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter(), new JsonDoubleConverter(), new JsonFloatConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter(), new JsonDoubleConverter(), new JsonFloatConverter() } };
    public static Regex RemoveWhiteSpacesFromJson { get; } = new Regex("(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+");

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

    enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }

    public static bool CreateSymbolicLink(FileInfo SymLink, FileInfo Source)
    {
        if (SymLink is null || Source is null) { LoggingSystem.LogNull(); return false; }
        return CreateSymbolicLink(SymLink.FullName, Source.FullName, SymbolicLink.File);
    }

    public static bool CreateSymbolicLink(DirectoryInfo SymLink, DirectoryInfo Source)
    {
        if (SymLink is null || Source is null) { LoggingSystem.LogNull(); return false; }
        return CreateSymbolicLink(SymLink.FullName, Source.FullName, SymbolicLink.Directory);
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
            file.CopyTo(targetFilePath);
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
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern void AllocConsole();

    [DllImport("Kernel32")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern void FreeConsole();
    public static void SpawnConsole()
    { 
        AllocConsole();
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    private static void GetGamePathFromVDF(string vdfPath, string appID)
    {
        if (string.IsNullOrEmpty(vdfPath) || string.IsNullOrEmpty(appID)) { return; }
        if (!File.Exists(vdfPath)) { return; }

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
            LoggingSystem.MessageLog(string.Format(Properties.Resources.msg_VDFGamePathError, vdfPath, GameFolders.Count, error), Properties.Resources.msgT_Error);
            GameFolders.Clear();
            return;
        }
    }

    public static void DownloadFileMessageBox(string url, string filename)
    {
        var dlWindow = new DownloadInfoWindow(url, filename);
        dlWindow.ShowDialog();
    }

    public static string? CreateFileHash(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) { return null; }
        using var stream = File.OpenRead(path);
        using var crypto = SHA256.Create();
        return BitConverter.ToString(crypto.ComputeHash(stream)).Replace("-", string.Empty).ToUpperInvariant();
    }

    public static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
    {
        unchecked {
            if (a1 == a2) { return true; }
            if (a1 == null || a2 == null || a1.Length != a2.Length) { return false; }
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8) {
                    if (*((long*)x1) != *((long*)x2)) return false;
                }
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; }
                x1 += 4; x2 += 4;
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; }
                x1 += 2; x2 += 2;
                if ((l & 1) != 0) { if (*((byte*)x1) != *((byte*)x2)) return false; }
                return true;
            }
        }
    }

    public static bool CompareFiles(string file1, string file2)
    {
        try
        {
            FileInfo fileInfo1 = new(file1);
            FileInfo fileInfo2 = new(file2);

            if (!fileInfo1.Exists || !fileInfo2.Exists) { LoggingSystem.MessageLog($"failed to CompareFiles one or both files don't Exist\nFile1:{fileInfo1.FullName}\nFile2:{fileInfo2.FullName}", "Error"); return false; }
            var file1data = File.ReadAllBytes(fileInfo1.FullName);
            var file2data = File.ReadAllBytes(fileInfo2.FullName);
            return UnsafeCompare(file1data, file2data);

        }
        catch(Exception error) { LoggingSystem.MessageLog($"failed to CompareFiles\nError:{error.Message}\nStacktrace:{error.StackTrace}", "Error"); return false; }
    }

    public static string DataToBase64(byte[] data)
    {
        return Base64UrlEncoder.Encode(data);
    }

    public static byte[] Base64ToData(string base64)
    {
        return Base64UrlEncoder.DecodeBytes(base64);
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
        if(src is null || dest is null) { LoggingSystem.FatalLog("either src or dest stream is null"); return; }
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
        {
            dest.Write(bytes, 0, cnt);
        }
    }

    public static void SerializeFile<T>(string filePath, T obj, bool compact = false)
    {
        if (string.IsNullOrEmpty(filePath)) { LoggingSystem.LogNull(); return; }
        if (obj is null) { LoggingSystem.LogNull(); return; }
        var info = new FileInfo(filePath);
        if (!info.Directory.Exists) { info.Directory.Create(); }
        using var file = new StreamWriter(File.Open(filePath, FileMode.Create), Encoding.UTF8);
        file.Write(Serialize(obj, compact));
        file.Close();
        LoggingSystem.Log($"[Serializer]: {typeof(T).Name} serialize succes!");
    }

    public static string Serialize<T>(T obj, bool compact = false)
    {
        if (obj == null) { return ""; }
        if (compact)
        {
            return JsonSerializer.Serialize<T>(obj, JSOCompacted);
        }
        else
        {
            return JsonSerializer.Serialize<T>(obj, JSOFields);
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

        if (!File.Exists("BLREdit.exe")) { Directory.SetCurrentDirectory(App.BLREditLocation); }

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

    public static BlockingCollection<string> FilesToClipboard { get; } = [];
    [STAThread]
    public static void ClipboardThread()
    {
        while(App.IsRunning)
        {
            try
            {
                var file = FilesToClipboard.Take();
                if (string.IsNullOrEmpty(file)) { continue; }

                Clipboard.SetFileDropList([file]);
                //Clipboard.Flush();
            }
            catch (Exception error)
            {
                LoggingSystem.Log($"[Clipboard Thread]Message:{error.Message}\n[ClipboardThread]Stacktrace:{error.StackTrace}");
            }
        }
    }
}