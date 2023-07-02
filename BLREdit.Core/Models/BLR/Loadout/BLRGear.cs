using BLREdit.Core.Models.BLR.Item;

using PropertyChanged;

using System.Collections.ObjectModel;

namespace BLREdit.Core.Models.BLR.Loadout;

public sealed class BLRGear : ModelBase
{
    private BLRItem? _helmet;
    private BLRItem? _upperBody;
    private BLRItem? _lowerBody;
    private BLRItem? _tactical;
    private BLRItem? _gear_R1;
    private BLRItem? _gear_R2;
    private BLRItem? _gear_L1;
    private BLRItem? _gear_L2;
    private BLRItem? _bodyCamo;
    private BLRItem? _avatar;
    private BLRItem? _badge;

    private bool _isFemale;
    private bool _isBot;

    public RangeObservableCollection<string> MetaData { get; } = new() { "v302" };

    [DoNotNotify] public BLRItem? Helmet { get { return _helmet; } set { if (IsValidHelmet(value)) { _helmet = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? UpperBody { get { return _upperBody; } set { if (IsValidUpperBody(value)) { _upperBody = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? LowerBody { get { return _lowerBody; } set { if (IsValidLowerBody(value)) { _lowerBody = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Tactical { get { return _tactical; } set { if (IsValidTactical(value)) { _tactical = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_R1 { get { return _gear_R1; } set { if (IsValidGear(value)) { _gear_R1 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_R2 { get { return _gear_R2; } set { if (IsValidGear(value)) { _gear_R2 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_L1 { get { return _gear_L1; } set { if (IsValidGear(value)) { _gear_L1 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_L2 { get { return _gear_L2; } set { if (IsValidGear(value)) { _gear_L2 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? BodyCamo { get { return _bodyCamo; } set { if (IsValidBodyCamo(value)) { _bodyCamo = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Avatar { get { return _avatar; } set { if (IsValidAvatar(value)) { _avatar = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Badge { get { return _badge; } set { if (IsValidBadge(value)) { _badge = value; OnPropertyChanged(); } } }
    [DoNotNotify] public bool IsFemale { get { return _isFemale; } set { _isFemale = value; OnPropertyChanged(); } }
    [DoNotNotify] public bool IsBot { get { return _isBot; } set { _isBot = value; OnPropertyChanged(); } }

    #region ItemValidation
    public static bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public static bool IsValidUpperBody(BLRItem? item)
    {
        if (item is null || item.CategoryName is "UpperBodies") return true;
        return false;
    }
    public static bool IsValidLowerBody(BLRItem? item)
    {
        if (item is null || item.CategoryName is "LowerBodies") return true;
        return false;
    }
    public static bool IsValidTactical(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Tactical") return true;
        return false;
    }
    public static bool IsValidBodyCamo(BLRItem? item)
    {
        if (item is null || item.CategoryName is "BodyCamos") return true;
        return false;
    }
    public static bool IsValidAvatar(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Avatars") return true;
        return false;
    }
    public static bool IsValidBadge(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Badges") return true;
        return false;
    }
    public static bool IsValidGear(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Attachments") return true;
        return false;
    }
    #endregion ItemValidation
}
