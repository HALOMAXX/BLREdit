using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit;

[AttributeUsage(AttributeTargets.Property)]
public sealed class WeaponPartAttribute : Attribute
{
    private readonly int _weaponPartOrder;
    public int WeaponPartOrder { get { return _weaponPartOrder; } }
    public WeaponPartAttribute([CallerLineNumber] int order = 0)
    { 
        _weaponPartOrder = order;
    }
}
