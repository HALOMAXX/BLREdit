using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace BLREdit.Models;

public sealed class BLRMap : ModelBase
{
    private static DirectoryInfo MapInfoLocation { get; }
    public static Dictionary<string, RangeObservableCollection<BLRMap>> MapsForVersion { get; }
    public static RangeObservableCollection<BLRMap> Maps { get; }

    public string DisplayName { get; set; } = "Lobby";
    public string MapFilename { get; set; } = "FoxEntry";
    public string MagiCowInfoName { get; set; } = "Lobby";
    public RangeObservableCollection<string> SupportedGameHashes { get; set; } = new();
    public RangeObservableCollection<string> SupportedPlaylists { get; set; } = new();

    static BLRMap()
    {
        MapInfoLocation = new DirectoryInfo("Data\\MapInfos");
        Maps = new(IOResources.DeserializeDirectory<BLRMap>(MapInfoLocation));
        MapsForVersion = SortMapsByVersion(Maps);
        
    }

    public static BLRMap GetMapForVersion(string versionHash, string mapName)
    {
        if (MapsForVersion.TryGetValue(versionHash, out var maps))
        {
            foreach (var map in maps)
            {
                if (mapName.Equals(map.MapFilename) || mapName.Equals(map.MagiCowInfoName))
                {
                    return map;
                }
            }
        }
        return new();
    }

    public static BLRMap GetMap(string mapName)
    {
        foreach (var map in Maps)
        {
            if (mapName.Equals(map.MapFilename) || mapName.Equals(map.MagiCowInfoName))
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