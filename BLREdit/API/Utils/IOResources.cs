using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BLREdit;

public class IOResources
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

    public static Encoding FILE_ENCODING { get; } = Encoding.UTF8;
    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };

    public static WebClient WebClient { get; } = new WebClient();
    public static HttpClient HttpClient { get; } = new HttpClient() { Timeout = new TimeSpan(0,0,10) };
    public static HttpClient HttpClientWeb { get; } = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };

    static IOResources()
    {
        
        WebClient.Headers.Add("User-Agent", $"BLREdit-{App.CurrentVersion}");
        HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}");
        HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        HttpClientWeb.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}");
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
        VToken libraryInfo;
        try
        {
            libraryInfo = VdfConvert.Deserialize(File.ReadAllText(vdfPath)).Value;
        }
        catch (System.Exception ex)
        {
            LoggingSystem.Log($"failed reading {vdfPath}, steam library parsing aborted");
            LoggingSystem.Log(ex.Message);
            return;
        }
        foreach (VProperty library in libraryInfo.Children().Cast<VProperty>())
        {
            if (library.Key != "contentstatsid")
            {
                //0 = path /// 6=apps
                var tokens = library.Value.Children();
                if (tokens.Count() >= 7)
                {
                    foreach (VProperty app in ((VProperty)tokens.ElementAt(6)).Value.Children().Cast<VProperty>())
                    {
                        if (app.Key == appID)
                        {
                            GameFolders.Add(((VValue)((VProperty)tokens.ElementAt(0)).Value).Value + "\\" + GAME_PATH_SUFFIX);
                        }
                    }
                }
            }
        }
    }

    public static void CopyToBackup(string file)
    {
        FileInfo info = new(file);
        File.Copy(file, ExportSystem.CurrentBackupFolder.FullName + info.Name, true);
    }

    public static void SerializeFile<T>(string filePath, T obj, bool compact = false)
    {
        //if the object we want to serialize is null we can instantly exit this function as we dont have anything to do as well the filePath
        if (string.IsNullOrEmpty(filePath)) { LoggingSystem.Log("filePath was empty!"); return; }

        bool writeFile;
        bool deleteFile;
        if (obj is ExportSystemProfile prof)
        {
            if (prof.IsDirty)
            {
                LoggingSystem.Log($"{prof.Name}❕");
                deleteFile = File.Exists(filePath);
                writeFile = true;
            }
            else
            {
                LoggingSystem.Log($"{prof.Name}✔");
                deleteFile = false;
                writeFile = false;
            }
        }
        else
        {
            deleteFile = File.Exists(filePath);
            writeFile = true;
        }

        //remove file before we write to it to prevent resedue data
        if (deleteFile)
        { File.Delete(filePath); }

        if (writeFile)
        {
            using var file = File.CreateText(filePath);
            file.Write(Serialize(obj, compact));
            file.Close();
            LoggingSystem.Log($"{typeof(T).Name} serialize succes!");
        }
    }

    public static string Serialize<T>(T obj, bool compact = false)
    {
        //if the object we want to serialize is null we can instantly exit this function as we dont have anything to do as well the filePath
        if (obj == null) { LoggingSystem.Log("object were empty!"); return ""; }
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
        { LoggingSystem.Log($"File:({filePath}) was not found for Deserialization!"); return temp; }



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
            LoggingSystem.Log($"{error.Message}\n{error.StackTrace}");
        }
        return default;
    }
}