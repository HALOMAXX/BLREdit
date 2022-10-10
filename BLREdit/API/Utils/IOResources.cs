using BLREdit.Export;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BLREdit;

public sealed class IOResources
{
    #region DIRECTORIES
    public const string PROFILE_DIR = "Profiles\\";
    public const string SEPROFILE_DIR = "SEProfiles\\";
    public const string ASSET_DIR = "Assets\\";
    public const string LOCAL_DIR = "localization\\";
    public const string JSON_DIR = "json\\";
    public const string GAME_PATH_SUFFIX = "steamapps\\common\\blacklightretribution\\Binaries\\Win32\\";
    public const string PROXY_MODULES_DIR = "Modules\\";
    #endregion DIRECTORIES

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

    public static string Steam32InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", "") as string;
    public static string Steam6432InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", "") as string;
    public static readonly List<string> GameFolders = new();

    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };

    public static WebClient WebClient { get; } = new WebClient();
    public static HttpClient HttpClient { get; } = new HttpClient() { Timeout = new TimeSpan(0,0,10) };
    public static HttpClient HttpClientWeb { get; } = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };

    static IOResources()
    {
        WebClient.Headers.Add(HttpRequestHeader.UserAgent, $"BLREdit-{App.CurrentVersion}");
        if (!HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { LoggingSystem.Log($"Failed to add {HttpRequestHeader.UserAgent} to HttpClient"); };
        HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        if (!HttpClientWeb.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { LoggingSystem.Log($"Failed to add {HttpRequestHeader.UserAgent} to HttpClientWeb"); };
        HttpClientWeb.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
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

    private static void GetGamePathFromVDF(string vdfPath, string appID)
    {
        if (string.IsNullOrEmpty(vdfPath) || string.IsNullOrEmpty(appID)) { LoggingSystem.Log($"vdfPath or AppID is empty"); return; }
        if (!File.Exists(vdfPath)) { LoggingSystem.Log($"vdfPath file doesn't exist"); return; }
        string data;
        try
        {
            data = File.ReadAllText(vdfPath);
        }
        catch (Exception ex)
        {
            LoggingSystem.Log($"failed reading {vdfPath}, steam library parsing aborted reason:{ex.Message}\n{ex.StackTrace}");
            return;
        }

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

    public static void SerializeFile<T>(string filePath, T obj, bool compact = false)
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

    public static string Serialize<T>(T obj, bool compact = false)
    {
        if (obj == null) { LoggingSystem.Log("[Serializer]: object was null!"); return ""; }
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
    public static T DeserializeFile<T>(string filePath)
    {
        T temp = default;
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

    public static T Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JSOFields);
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"[Serializer]: {error.Message}\n{error.StackTrace}");
        }
        return default;
    }
}