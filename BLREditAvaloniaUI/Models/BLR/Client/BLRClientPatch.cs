using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;

namespace BLREdit.Models.BLR;

public sealed class BLRClientPatch : ModelBase
{
    private static DirectoryInfo PatchLocation { get; } 
    public static Dictionary<string, RangeObservableCollection<BLRClientPatch>> ClientPatches { get; }

    public string PatchName { get; set; } = "ASLR Patch(v302)";
    public string ClientHash { get; set; } = "0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7";
    public RangeObservableCollection<BLRClientPatchPart> PatchParts { get; set; } = new() { new() { Position = 510, BytesToOverwrite = new() { 0 } } };

    #region Overrides
    public override int GetHashCode()
    {
        var hash = PatchName.GetHashCode() ^ ClientHash.GetHashCode();
        foreach (var patch in PatchParts)
        { 
            hash ^= patch.Position.GetHashCode();
            foreach (var byt in patch.BytesToOverwrite)
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

    static BLRClientPatch()
    { 
        PatchLocation = new DirectoryInfo("Data\\Patches");
        ClientPatches = LoadPatches();
    }

    private static Dictionary<string, RangeObservableCollection<BLRClientPatch>> LoadPatches()
    {
        Dictionary<string, RangeObservableCollection<BLRClientPatch>> sortedPatches = new();
        if (PatchLocation.Exists)
        {
            var allPatches = IOResources.DeserializeDirectory<BLRClientPatch>(PatchLocation);

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
        }
        return sortedPatches;
    }
}

public sealed class BLRClientPatchPart : ModelBase
{
    public int Position { get; set; } = 0;
    public RangeObservableCollection<byte> BytesToOverwrite { get; set; } = new();
}
