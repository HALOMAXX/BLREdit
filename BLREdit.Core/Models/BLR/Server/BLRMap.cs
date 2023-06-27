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
    public RangeObservableCollection<string> SupportedGameVersions { get; set; } = new();
    public RangeObservableCollection<string> SupportedPlaylists { get; set; } = new();

    static BLRMap()
    {
        MapInfoLocation = new DirectoryInfo("Data\\Maps");

        IOResources.DeserializeDirectoryInto(Maps, MapInfoLocation);

        MapsForVersion = SortMapsByVersion(Maps);
    }

    public static BLRMap GetMapForVersion(string version, string mapName)
    {
        if (version is null || mapName is null) return new();
        if (MapsForVersion.TryGetValue(version, out var maps))
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
            foreach (var version in map.SupportedGameVersions)
            {
                if (sortedMaps.TryGetValue(version, out var mapList))
                {
                    mapList.Add(map);
                }
                else
                {
                    sortedMaps.Add(version, new() { map });
                }
            }
        }
        return sortedMaps;
    }
}

public sealed class JsonBLRMapConverter : JsonGenericConverter<BLRMap>
{
    static JsonBLRMapConverter()
    {
        Default = new BLRMap() { DisplayName = "", MagiCowInfoName = "", MapFilename = "" };
    }
}