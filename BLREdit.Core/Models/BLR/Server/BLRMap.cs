using BLREdit.Core.Utils;
using System.Collections.ObjectModel;

namespace BLREdit.Core.Models.BLR.Server;

public sealed class BLRMap : ModelBase
{
    private static DirectoryInfo MapInfoLocation { get; }
    public static Dictionary<string, RangeObservableCollection<BLRMap>> MapsForVersion { get; }
    public static RangeObservableCollection<BLRMap> Maps { get; } = new();

    public string DisplayName { get; set; } = "Lobby";
    public string MapFilename { get; set; } = "FoxEntry";
    public string MagiCowInfoName { get; set; } = "Lobby";
    public RangeObservableCollection<string> SupportedGameHashes { get; set; } = new();
    public RangeObservableCollection<string> SupportedPlaylists { get; set; } = new();

    static BLRMap()
    {
        MapInfoLocation = new DirectoryInfo("Data\\Maps");

        IOResources.DeserializeDirectoryInto(Maps, MapInfoLocation);

        MapsForVersion = SortMapsByVersion(Maps);
    }

    public static BLRMap GetMapForVersion(string versionHash, string mapName)
    {
        if (versionHash is null || mapName is null) return new();
        if (MapsForVersion.TryGetValue(versionHash, out var maps))
        {
            foreach (var map in maps)
            {
                if (mapName.Equals(map.MapFilename, StringComparison.Ordinal) || mapName.Equals(map.MagiCowInfoName, StringComparison.Ordinal))
                {
                    return map;
                }
            }
        }
        return new();
    }

    public static BLRMap GetMap(string mapName)
    {
        if (mapName is null) return new();
        foreach (var map in Maps)
        {
            if (mapName.Equals(map.MapFilename, StringComparison.Ordinal) || mapName.Equals(map.MagiCowInfoName, StringComparison.Ordinal))
            {
                return map;
            }
        }
        return new();
    }

    private static Dictionary<string, RangeObservableCollection<BLRMap>> SortMapsByVersion(RangeObservableCollection<BLRMap> allMaps)
    {
        Dictionary<string, RangeObservableCollection<BLRMap>> sortedMaps = new();
        foreach (var map in allMaps)
        {
            foreach (var hash in map.SupportedGameHashes)
            {
                if (sortedMaps.TryGetValue(hash, out var mapList))
                {
                    mapList.Add(map);
                }
                else
                {
                    sortedMaps.Add(hash, new() { map });
                }
            }
        }
        return sortedMaps;
    }
}

public class JsonBLRMapConverter : JsonGenericConverter<BLRMap>
{
    static JsonBLRMapConverter()
    {
        Default = new BLRMap() { DisplayName = "", MagiCowInfoName = "", MapFilename = "" };
        IOResources.JSOSerialization.Converters.Add(new JsonBLRMapConverter());
        IOResources.JSOSerializationCompact.Converters.Add(new JsonBLRMapConverter());
    }
}