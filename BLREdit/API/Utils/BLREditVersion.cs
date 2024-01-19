using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BLREdit.API.Utils;

public sealed class BLREditVersion
{
    public string Name { get; private set; } = "v";
    public List<short> VersionParts { get; } = new();
    public BLREditVersion? SubVersion { get; private set; }
    public BLREditVersion(string? versionTag, string? subName = null)
    {
        if (versionTag is null) return;
        if (!string.IsNullOrEmpty(subName)) { Name = subName; }
        var fullVersion = string.Join("v", versionTag.Split('v').Skip(1));
        var splitFullVersion = fullVersion.Split('-');
        var subVersion = $"v{string.Join("-", splitFullVersion.Skip(1))}";
        if (subVersion.Length>1) { SubVersion = new BLREditVersion(subVersion, splitFullVersion.Skip(1).First().Split('.').First()); }

        var stringVersionParts = splitFullVersion.First().Split('.');
        for (int i = 0; i < stringVersionParts.Length; i++)
        {
            if (short.TryParse(stringVersionParts[i], out short result))
            {
                VersionParts.Add(result);
            }
        }
    }

    public override string ToString()
    {
        var versionParts = VersionParts.Skip(1);
        string version = $"{Name}{VersionParts.First()}";
        
        foreach (var num in versionParts)
        { 
            version += $".{num}";  
        }

        if (SubVersion is not null)
        {
            version += $"-{SubVersion}";
        }

        return version;
    }

    public static bool operator <(BLREditVersion v1, BLREditVersion v2)
    {
        if (v1.VersionParts.Count < v2.VersionParts.Count) { return true; }
        if (v1.VersionParts.Count > v2.VersionParts.Count) { return false; }
        for (int i = 0; i < v1.VersionParts.Count; i++)
        {
            if (v1.VersionParts[i] < v2.VersionParts[i]) { return true; }
            else if (v1.VersionParts[i] > v2.VersionParts[i]) { return false; }
        }
        if (v1.SubVersion is null && v2.SubVersion is null) { return false; } //they are equal
        else if (v1.SubVersion is null && v2.SubVersion is not null) { return true; }
        else if (v1.SubVersion is not null && v2.SubVersion is not null) { return v1.SubVersion < v2.SubVersion; }
        else return false;
    }

    public static bool operator >(BLREditVersion v1, BLREditVersion v2)
    {
        if (v1.VersionParts.Count < v2.VersionParts.Count) { return false; }
        if (v1.VersionParts.Count > v2.VersionParts.Count) { return true; }
        for (int i = 0; i < v1.VersionParts.Count; i++)
        {
            if (v1.VersionParts[i] > v2.VersionParts[i]) { return true; }
            else if (v1.VersionParts[i] < v2.VersionParts[i]) { return false; }
        }
        if (v1.SubVersion is null && v2.SubVersion is null) { return false; }  //they are equal
        else if (v1.SubVersion is null && v2.SubVersion is not null) { return false; }
        else if (v1.SubVersion is not null && v2.SubVersion is not null) { return v1.SubVersion > v2.SubVersion; }
        else return true;
    }

    public static bool operator <=(BLREditVersion v1, BLREditVersion v2)
    {
        if (v1.VersionParts.Count < v2.VersionParts.Count) { return true; }
        if (v1.VersionParts.Count > v2.VersionParts.Count) { return false; }
        for (int i = 0; i < v1.VersionParts.Count; i++)
        {
            if (v1.VersionParts[i] < v2.VersionParts[i]) { return true; }
            else if (v1.VersionParts[i] > v2.VersionParts[i]) { return false; }
        }
        if (v1.SubVersion is null && v2.SubVersion is null) { return true; } //they are equal
        else if (v1.SubVersion is null && v2.SubVersion is not null) { return true; }
        else if (v1.SubVersion is not null && v2.SubVersion is not null) { return v1.SubVersion <= v2.SubVersion; }
        else return false;
    }

    public static bool operator >=(BLREditVersion v1, BLREditVersion v2)
    {
        if (v1.VersionParts.Count < v2.VersionParts.Count) { return false; }
        if (v1.VersionParts.Count > v2.VersionParts.Count) { return true; }
        for (int i = 0; i < v1.VersionParts.Count; i++)
        {
            if (v1.VersionParts[i] > v2.VersionParts[i]) { return true; }
            else if (v1.VersionParts[i] < v2.VersionParts[i]) { return false; }
        }
        if (v1.SubVersion is null && v2.SubVersion is null) { return true; }  //they are equal
        else if (v1.SubVersion is null && v2.SubVersion is not null) { return false; }
        else if (v1.SubVersion is not null && v2.SubVersion is not null) { return v1.SubVersion >= v2.SubVersion; }
        else return true;
    }
}
