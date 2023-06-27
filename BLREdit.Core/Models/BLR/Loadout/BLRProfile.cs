using System.Collections.ObjectModel;

namespace BLREdit.Core.Models.BLR.Loadout;

public sealed class BLRProfile : ModelBase
{
    public string ProfileName { get; set; } = "New Profile";
    public RangeObservableCollection<BLRLoadout> Loadouts { get; set; } = new() { 
        new BLRLoadout() { ClientVersion = "v302", }
    };
}
