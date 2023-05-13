using System.Collections.Generic;
using System.IO;

namespace BLREdit.Models.BLR;

public sealed class BLRClientPatch
{
    public string PatchName { get; set; } = "ASLR Patch(v302)";
    public string ClientHash { get; set; } = "0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7";
    public SortedDictionary<int, List<byte>> PatchParts = new() { { 510, new(){ 0 } } };

    #region Overrides
    public override int GetHashCode()
    {
        var hash = PatchName.GetHashCode() ^ ClientHash.GetHashCode();
        foreach (var patch in PatchParts)
        { 
            hash ^= patch.Key.GetHashCode();
            foreach (var byt in patch.Value)
            { 
                hash ^= byt;
            }
        }
        return hash;
    }
    public override bool Equals(object? obj)
    {
        if (obj is BLRClientPatch patch)
        { 
            return patch.GetHashCode() == GetHashCode();
        }
        return false;
    }

    public override string ToString()
    {
        return $"[BinaryPatch]:{PatchName}";
    }
    #endregion Overrides

    public static Dictionary<string, List<BLRClientPatch>> ClientBinaryPatches = LoadPatches();

    private static Dictionary<string, List<BLRClientPatch>> LoadPatches()
    {
        var files = new DirectoryInfo("donwloads\\patches\\").GetFiles("*.json", SearchOption.AllDirectories);
        
        List<BLRClientPatch> allPatches = new();
        Dictionary<string, List<BLRClientPatch>> sortedPatches = new();

        foreach (var file in files)
        {
            if (IOResources.DeserializeFile<BLRClientPatch>(file.FullName) is BLRClientPatch patch)
            {
                allPatches.Add(patch);
            }
            if (IOResources.DeserializeFile<BLRClientPatch[]>(file.FullName) is BLRClientPatch[] patches)
            { 
                allPatches.AddRange(patches);
            }
        }

        foreach (var patch in allPatches)
        {
            if (sortedPatches.TryGetValue(patch.ClientHash, out var patchList))
            {
                patchList.Add(patch);
            }
            else
            {
                sortedPatches.Add(patch.ClientHash, new() { patch });
            }
        }

        return sortedPatches;
    }
}
