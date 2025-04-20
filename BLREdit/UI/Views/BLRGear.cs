using BLREdit.API.Utils;
using BLREdit.Import;

using static BLREdit.API.Utils.HelperFunctions;

namespace BLREdit.UI.Views;

public sealed class BLRGear
{
    public BLREditItem? Helmet { get; set; }
    public BLREditItem? UpperBody { get; set; }
    public BLREditItem? LowerBody { get; set; }
    public BLREditItem? Tactical { get; set; }
    public BLREditItem? Gear1 { get; set; }
    public BLREditItem? Gear2 { get; set; }
    public BLREditItem? Gear3 { get; set; }
    public BLREditItem? Gear4 { get; set; }
    public BLREditItem? BodyCamo { get; set; }
    public BLREditItem? Avatar { get; set; }
    public BLREditItem? Trophy { get; set; }
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