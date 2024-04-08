using BLREdit.API.Utils;
using BLREdit.Import;

using static BLREdit.API.Utils.HelperFunctions;

namespace BLREdit.UI.Views;

public sealed class BLRGear
{
    public BLRItem? Helmet { get; set; }
    public BLRItem? UpperBody { get; set; }
    public BLRItem? LowerBody { get; set; }
    public BLRItem? Tactical { get; set; }
    public BLRItem? Gear1 { get; set; }
    public BLRItem? Gear2 { get; set; }
    public BLRItem? Gear3 { get; set; }
    public BLRItem? Gear4 { get; set; }
    public BLRItem? BodyCamo { get; set; }
    public BLRItem? Avatar { get; set; }
    public BLRItem? Trophy { get; set; }
    public bool IsFemale { get; set; }
    public bool IsBot { get; set; }
}

public struct GearErrorReport(ItemReport helmet,  ItemReport upper, ItemReport lower, ItemReport tactical, ItemReport camo, ItemReport avatar, ItemReport trophy, ItemReport gear1, ItemReport gear2, ItemReport gear3, ItemReport gear4)
{ 
    public bool IsValid { get { return AllTruth(ItemReport.Valid, HelmetReport, UpperBodyReport, LowerBodyReport, TacticalReport, BodyCamoReport, AvatarReport, TrophyReport, Gear1Report, Gear2Report, Gear3Report, Gear4Report); } }

    public bool HasDuplicates { get { return Truth(ItemReport.Duplicate, Gear1Report, Gear2Report, Gear3Report, Gear4Report) > 0; } }

    public ItemReport HelmetReport { get; } = helmet;
    public ItemReport UpperBodyReport { get; } = upper;
    public ItemReport LowerBodyReport { get; } = lower;
    public ItemReport TacticalReport { get; } = tactical;
    public ItemReport BodyCamoReport { get; } = camo;
    public ItemReport AvatarReport { get; } = avatar;
    public ItemReport TrophyReport { get; } = trophy;

    public ItemReport Gear1Report { get; } = gear1;
    public ItemReport Gear2Report { get; } = gear2;
    public ItemReport Gear3Report { get; } = gear3;
    public ItemReport Gear4Report { get; } = gear4;
}