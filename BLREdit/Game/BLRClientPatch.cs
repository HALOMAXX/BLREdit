using System.Collections.Generic;
using System.IO;

namespace BLREdit.Game;

public sealed class BLRClientPatch
{
    public string PatchName { get; set; } = "ASLR Patch(v302)";
    public string ClientHash { get; set; } = "0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7";
    public SortedDictionary<int, List<byte>> PatchParts = new() { { 510, new(){ 0 } } };

    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(BLRClientPatch)) return false;
        
        BLRClientPatch patch = (BLRClientPatch)obj;

        if (ClientHash != patch.ClientHash) return false;
        if (PatchParts.Count != patch.PatchParts.Count) return false;

        int[] thisKeys = new int[PatchParts.Count];
        int[] otherKeys = new int[PatchParts.Count];

        List<byte>[] thisBytes = new List<byte>[PatchParts.Count];
        List<byte>[] otherBytes = new List<byte>[PatchParts.Count];

        PatchParts.Keys.CopyTo(thisKeys, 0);
        patch.PatchParts.Keys.CopyTo(otherKeys, 0);

        PatchParts.Values.CopyTo(thisBytes, 0);
        patch.PatchParts.Values.CopyTo(otherBytes, 0);

        for (int i = 0; i < PatchParts.Count; i++)
        {
            if (thisKeys[i] != otherKeys[i]) return false;
            if (thisBytes[i].Count != otherBytes[i].Count) return false;
            for (int x = 0; x < thisBytes[i].Count; x++)
            {
                if (thisBytes[i][x] != otherBytes[i][x]) return false;
            }
        }

        return true;
    }

    public static Dictionary<string, List<BLRClientPatch>> AvailablePatches { get; private set; } = LoadPatches();
    private static Dictionary<string, List<BLRClientPatch>> LoadPatches()
    {
        List<BLRClientPatch> loadedPatches = new();
        Dictionary<string, List<BLRClientPatch>> sortedPatches = new();
        string[] patches = Directory.GetFiles($"{IOResources.ASSET_DIR}patches\\");
        foreach (string file in patches)
        {
            if (file.EndsWith(".json"))
            {
                var patch = IOResources.DeserializeFile<BLRClientPatch>(file);
                if (patch is not null) loadedPatches.Add(patch);
            }
                
        }
        if (loadedPatches.Count <= 0)
        {
            IOResources.SerializeFile($"patch.json", new BLRClientPatch());
            loadedPatches.Add(new BLRClientPatch());
        }
        foreach (var loadedPatch in loadedPatches)
        {
            if (sortedPatches.TryGetValue(loadedPatch.ClientHash, out List<BLRClientPatch> patchList))
            {
                patchList.Add(loadedPatch);
            }
            else
            {
                sortedPatches.Add(loadedPatch.ClientHash, new() { loadedPatch });
            }
        }
        return sortedPatches;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
