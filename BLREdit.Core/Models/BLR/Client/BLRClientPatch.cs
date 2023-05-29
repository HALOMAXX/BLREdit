using BLREdit.Core.Utils;

using System.Collections.ObjectModel;

namespace BLREdit.Core.Models.BLR.Client;

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
        var hash = new HashCode();

        hash.Add(PatchName);
        hash.Add(ClientHash);
        foreach (var patch in PatchParts)
        {
            hash.Add(patch);
        }
        return hash.ToHashCode();
    }
    public override bool Equals(object? obj)
    {
        if (obj is BLRClientPatch patch)
        {
            return
                PatchName.Equals(patch.PatchName, StringComparison.Ordinal) &&
                ClientHash.Equals(patch.ClientHash, StringComparison.Ordinal) &&
                PatchParts.ContentsAreEqual(patch.PatchParts);
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
    public int Position { get; set; }
    public RangeObservableCollection<byte> BytesToOverwrite { get; set; } = new();

    #region Overrides
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Position);
        foreach (var patch in BytesToOverwrite)
        {
            hash.Add(patch);
        }
        return hash.ToHashCode();
    }
    public override bool Equals(object? obj)
    {
        if (obj is BLRClientPatchPart patch)
        {
            return
                Position.Equals(patch.Position) &&
                BytesToOverwrite.ContentsAreEqual(patch.BytesToOverwrite);
        }
        return false;
    }
    #endregion Overrides
}
