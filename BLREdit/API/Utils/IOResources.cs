using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

using Microsoft.Win32;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BLREdit;

public class IOResources
{
    public const string PROFILE_DIR = "Profiles\\";
    public const string SEPROFILE_DIR = "SEProfiles\\";
    public const string ASSET_DIR = "Assets\\";
    public const string JSON_DIR = "json\\";
    public const string GEAR_FILE = ASSET_DIR + JSON_DIR + "gear.json";
    public const string MOD_FILE = ASSET_DIR + JSON_DIR + "mods.json";
    public const string WEAPON_FILE = ASSET_DIR + JSON_DIR + "weapons.json";
    public const string ITEM_LIST_FILE = ASSET_DIR + JSON_DIR + "itemList.json";
    public const string SETTINGS_FILE = "settings.json";
    public const string GAME_APPID = "209870";
    public const string GAME_PATH_SUFFIX = "\\steamapps\\common\\blacklightretribution\\Binaries\\Win32\\";
    public const string GAME_DEFAULT_EXE = "FoxGame-win32-Shipping.exe";
    public static string Steam32InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", "") as string;
    public static string Steam6432InstallFolder { get; private set; } = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", "") as string;
    public static List<string> GameFolders = new();

    public static Encoding FILE_ENCODING { get; } = Encoding.UTF8;
    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };

    public static void GetGameLocationsFromSteam()
    {
        string steampath;
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
        var libraryInfo = VdfConvert.Deserialize(File.ReadAllText(vdfPath)).Value;
        foreach (VProperty library in libraryInfo.Children())
        {
            if (library.Key != "contentstatsid")
            {
                //0 = path /// 6=apps
                var tokens = library.Value.Children();
                if (tokens.Count() >= 7)
                {
                    foreach (VProperty app in ((VProperty)tokens.ElementAt(6)).Value.Children())
                    {
                        if (app.Key == appID)
                        {
                            GameFolders.Add(((VValue)((VProperty)tokens.ElementAt(0)).Value).Value + GAME_PATH_SUFFIX);
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
        if (string.IsNullOrEmpty(filePath)) { LoggingSystem.LogWarning("filePath was empty!"); return; }

        bool writeFile;
        bool deleteFile;
        if (obj is ExportSystemProfile prof)
        {
            if (prof.IsDirty)
            {
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(prof.Name + "❕");
                deleteFile = File.Exists(filePath);
                writeFile = true;
            }
            else
            {
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(prof.Name + "✔");
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
            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(typeof(T).Name + " serialize succes!");
        }
    }

    public static string Serialize<T>(T obj, bool compact = false)
    {
        //if the object we want to serialize is null we can instantly exit this function as we dont have anything to do as well the filePath
        if (obj == null) { if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogWarning("object were empty!"); return ""; }
        if (compact)
        {
            return JsonSerializer.Serialize<T>(obj, JSOCompacted);
        }
        else
        {
            return JsonSerializer.Serialize<T>(obj, JSOFields);
        }
    }

    public static T DeserializeFile<T>(string filePath)
    {
        T temp = default;
        if (string.IsNullOrEmpty(filePath)) { return temp; }

        //check if file exist's before we try to read it if it doesn't exist return and Write an error to log
        if (!File.Exists(filePath))
        { if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogError("File:(" + filePath + ") was not found for Deserialization!"); return temp; }



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
        return JsonSerializer.Deserialize<T>(json, JSOFields);
    }
}