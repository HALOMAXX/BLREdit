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
public sealed class ProfileSettingAttribute(int id, [CallerMemberName] string? property = null) : Attribute
{
    public string Property => property ?? string.Empty;
    public int ID => id;
}