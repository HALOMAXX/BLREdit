using System.IO;
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
        public static BLREditSettings Settings { get; } = LoadSettings();

        public static BLREditSettings LoadSettings()
        {
            if (File.Exists("Settings.json"))
            {
                BLREditSettings settings;
                using (var file = File.OpenText("Settings.json"))
                {
                    settings = JsonSerializer.Deserialize<BLREditSettings>(file.ReadToEnd(), JSO);
                    file.Close();
                }
                File.Delete("Settings.json");
                using (var file = File.OpenWrite("Settings.json"))
                {
                    JsonSerializer.Serialize<BLREditSettings>(file, settings, JSO);
                    file.Close();
                }
                return settings;
            }
            else
            {
                using (var file = File.OpenWrite("Settings.json"))
                {
                    var tmp = new BLREditSettings();
                    JsonSerializer.Serialize<BLREditSettings>(file, tmp, JSO);
                    file.Close();
                    return tmp;
                }
            }
        }


    }

    public class BLREditSettings
    {
        public bool ShowUpdateNotice { get; set; } = true;
        public bool DoRuntimeCheck { get; set; } = true;
        public bool ForceRuntimeCheck { get; set; } = false;
        public ImageSize WideImageSize { get; set; } = new ImageSize() { Width=256, Height=128};
        public ImageSize LargeSquareImageSize { get; set; } = new ImageSize() { Width = 128, Height = 128 };
        public ImageSize SmallSquareImageSize { get; set; } = new ImageSize() { Width = 64, Height = 64 };
        public ByteColor BackGroundItemColor { get; set; } = new ByteColor();
    }

    public class ImageSize
    { 
        public int Width { get; set; }
        public int Height { get; set; }

        public ImageSize Copy()
        {
            return new ImageSize() { Width = this.Width, Height = this.Height };
        }
    }

    public class ByteColor
    {
        public byte Red { get; set; } = 128;
        public byte Green { get; set; } = 128;
        public byte Blue { get; set; } = 128;
        public byte Alpha { get; set; } = 255;
    }
}