using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit;

[AttributeUsage(AttributeTargets.Property)]
public sealed class WeaponPartAttribute([CallerLineNumber] int order = 0) : Attribute
{
#pragma warning disable CA1822 // Mark members as static
    public int WeaponPartOrder => order;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ProfileSettingAttribute(int id, [CallerMemberName] string? name = null) : Attribute
{
    public string MemberName => name ?? string.Empty;
    public int ID => id;
#pragma warning restore CA1822 // Mark members as static
}
