using System;

namespace BLREdit.API.Utils;

[Flags]
public enum ItemReport
{
    None        = 0,
    Valid       = 1,
    Invalid     = 2,
    Missing     = 4,
    Duplicate   = 8
}
