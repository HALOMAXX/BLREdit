using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BLREdit 
{
    public class IOResources
    {
        public const string PROFILE_DIR = "Profiles\\";
        public const string ASSET_DIR = "Assets\\";
        public const string JSON_DIR = "json\\";
        public const string GEAR_FILE = ASSET_DIR + JSON_DIR + "gear.json";
        public const string MOD_FILE = ASSET_DIR + JSON_DIR + "mods.json";
        public const string WEAPON_FILE = ASSET_DIR + JSON_DIR + "weapons.json";

        public static Encoding FILE_ENCODING { get; } = Encoding.UTF8;
        public static JsonSerializerOptions JSO { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = false, Converters = { new JsonStringEnumConverter() } };
        public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    }
}