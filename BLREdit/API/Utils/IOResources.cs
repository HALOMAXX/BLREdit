using System.IO;
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

    public static Encoding FILE_ENCODING { get; } = Encoding.UTF8;
    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };


    public static void CopyToBackup(string file)
    {
        FileInfo info = new(file);
        File.Copy(file, ExportSystem.CurrentBackupFolder.FullName + info.Name, true);
    }

    public static void SerializeFile<T>(string filePath, T obj, bool compact = false)
    {
        //if the object we want to serialize is null we can instantly exit this function as we dont have anything to do as well the filePath
        if (string.IsNullOrEmpty(filePath)) { LoggingSystem.LogWarning("filePath was empty!"); return; }

        //remove file before we write to it to prevent resedue data
        if (File.Exists(filePath)) { if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("file already exist's deleting it"); File.Delete(filePath); }
        else
        { if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("file doesn't exist were good to create it"); }

        using var file = File.CreateText(filePath);
        file.Write(Serialize(obj, compact));
        file.Close();
        if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Serialize Succes!");
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
        return temp;
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JSOFields);
    }
}