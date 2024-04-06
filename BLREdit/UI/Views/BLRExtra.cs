using BLREdit.API.Utils;
using BLREdit.Import;

using static BLREdit.API.Utils.HelperFunctions;

namespace BLREdit.UI.Views;

public sealed class BLRExtra
{
    public BLRItem? Depot1 { get; set; }
    public BLRItem? Depot2 { get; set; }
    public BLRItem? Depot3 { get; set; }
    public BLRItem? Depot4 { get; set; }
    public BLRItem? Depot5 { get; set; }

    public BLRItem? Taunt1 { get; set; }
    public BLRItem? Taunt2 { get; set; }
    public BLRItem? Taunt3 { get; set; }
    public BLRItem? Taunt4 { get; set; }
    public BLRItem? Taunt5 { get; set; }
    public BLRItem? Taunt6 { get; set; }
    public BLRItem? Taunt7 { get; set; }
    public BLRItem? Taunt8 { get; set; }

    public BLRItem? TopIcon { get; set; }
    public BLRItem? TopColor { get; set; }
    public BLRItem? MiddleIcon { get; set; }
    public BLRItem? MiddleColor { get; set; }
    public BLRItem? BottomIcon { get; set; }
    public BLRItem? BottomColor { get; set; }

    public BLRItem? AnnouncerVoice { get; set; }
    public BLRItem? PlayerVoice { get; set; }
    public BLRItem? Title { get; set; }
}

public struct ExtraErrorReport(ItemReport depot1, ItemReport depot2, ItemReport depot3, ItemReport depot4, ItemReport depot5, ItemReport taunt1, ItemReport taunt2, ItemReport taunt3, ItemReport taunt4, ItemReport taunt5, ItemReport taunt6, ItemReport taunt7, ItemReport taunt8, ItemReport topIcon, ItemReport topColor, ItemReport middleIcon, ItemReport middleColor, ItemReport bottomIcon, ItemReport bottomColor, ItemReport announcer, ItemReport player, ItemReport title)
{
    public bool IsValid { get { return AllTruth(ItemReport.Valid, Depot1Report, Depot2Report, Depot3Report, Depot4Report, Depot5Report, Taunt1Report, Taunt2Report, Taunt3Report, Taunt4Report, Taunt5Report, Taunt6Report, Taunt7Report, Taunt8Report, TopIconReport, TopColorReport, MiddleIconReport, MiddleColorReport, BottomIconReport, BottomColorReport, AnnouncerReport, PlayerReport, TitleReport); } }

    public bool HasMissingItems { get { return Truth(ItemReport.Missing, Depot1Report, Depot2Report, Depot3Report, Depot4Report, Depot5Report, Taunt1Report, Taunt2Report, Taunt3Report, Taunt4Report, Taunt5Report, Taunt6Report, Taunt7Report, Taunt8Report, TopIconReport, TopColorReport, MiddleIconReport, MiddleColorReport, BottomIconReport, BottomColorReport, AnnouncerReport, PlayerReport, TitleReport) > 0; } }

    public ItemReport Depot1Report { get; } = depot1;
    public ItemReport Depot2Report { get; } = depot2;
    public ItemReport Depot3Report { get; } = depot3;
    public ItemReport Depot4Report { get; } = depot4;
    public ItemReport Depot5Report { get; } = depot5;

    public ItemReport Taunt1Report { get; } = taunt1;
    public ItemReport Taunt2Report { get; } = taunt2;
    public ItemReport Taunt3Report { get; } = taunt3;
    public ItemReport Taunt4Report { get; } = taunt4;
    public ItemReport Taunt5Report { get; } = taunt5;
    public ItemReport Taunt6Report { get; } = taunt6;
    public ItemReport Taunt7Report { get; } = taunt7;
    public ItemReport Taunt8Report { get; } = taunt8;

    public ItemReport TopIconReport { get; } = topIcon;
    public ItemReport TopColorReport { get; } = topColor;
    public ItemReport MiddleIconReport { get; } =  middleIcon;
    public ItemReport MiddleColorReport { get; } = middleColor;
    public ItemReport BottomIconReport { get; } = bottomIcon;
    public ItemReport BottomColorReport { get; } = bottomColor;

    public ItemReport AnnouncerReport { get; } = announcer;
    public ItemReport PlayerReport { get; } = player;
    public ItemReport TitleReport { get; } = title;
}