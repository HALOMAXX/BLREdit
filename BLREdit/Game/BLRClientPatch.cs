using System.Collections.Generic;

namespace BLREdit.Game
{
    public class BLRClientPatch
    {
        public string PatchName { get; set; } = "ASLR Patch(v302)";
        public string ClientHash { get; set; } = "0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7";
        public bool Default { get; set; } = true;
        public SortedDictionary<int, List<byte>> PatchParts = new() { { 510, new(){ 0 } } };
    }
}
