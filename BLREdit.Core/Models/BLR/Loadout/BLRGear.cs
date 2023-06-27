using PropertyChanged;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    private bool _isFemale = false;
    private bool _isBot = false;

    public string ClientVersion { get; set; } = "v302";


    [DoNotNotify] public BLRItem? Helmet { get { return _helmet; } set { if (IsValidPrimary(value)) { _helmet = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? UpperBody { get { return _upperBody; } set { if (IsValidPrimary(value)) { _upperBody = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? LowerBody { get { return _lowerBody; } set { if (IsValidPrimary(value)) { _lowerBody = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Tactical { get { return _tactical; } set { if (IsValidPrimary(value)) { _tactical = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_R1 { get { return _gear_R1; } set { if (IsValidPrimary(value)) { _gear_R1 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_R2 { get { return _gear_R2; } set { if (IsValidPrimary(value)) { _gear_R2 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_L1 { get { return _gear_L1; } set { if (IsValidPrimary(value)) { _gear_L1 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Gear_L2 { get { return _gear_L2; } set { if (IsValidPrimary(value)) { _gear_L2 = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? BodyCamo { get { return _bodyCamo; } set { if (IsValidPrimary(value)) { _bodyCamo = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Avatar { get { return _avatar; } set { if (IsValidPrimary(value)) { _avatar = value; OnPropertyChanged(); } } }
    [DoNotNotify] public BLRItem? Badge { get { return _badge; } set { if (IsValidPrimary(value)) { _badge = value; OnPropertyChanged(); } } }

    #region ItemValidation


    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidUpperBody(BLRItem? item)
    {
        if (item is null || item.CategoryName is "UpperBodies") return true;
        return false;
    }
    public bool IsValidLowerBody(BLRItem? item)
    {
        if (item is null || item.CategoryName is "LowerBodies") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    public bool IsValidHelmet(BLRItem? item)
    {
        if (item is null || item.CategoryName is "Helmets") return true;
        return false;
    }
    #endregion ItemValidation
}
