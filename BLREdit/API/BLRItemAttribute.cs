#pragma warning disable CA1822 // Mark members as static

using System;
using System.Runtime.CompilerServices;

namespace BLREdit;

[AttributeUsage(AttributeTargets.Property)]
public sealed class BLRItemAttribute(string itemType, [CallerLineNumber] int propertyOrder = 0, [CallerMemberName] string? propertyName = null) : Attribute
{
    public string? PropertyName => propertyName;
    public int PropertyOrder => propertyOrder;
    public string ItemType => itemType;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ProfileSettingAttribute(int id, [CallerMemberName] string? name = null) : Attribute
{
    public string MemberName => name ?? string.Empty;
    public int ID => id;
}


#pragma warning restore CA1822 // Mark members as static