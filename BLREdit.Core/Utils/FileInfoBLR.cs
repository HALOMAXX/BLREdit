using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Core;

public sealed class FileInfoBLR : ModelBase
{
    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is FileInfoBLR info)
        {
            return
                FullName.Equals(info.FullName, StringComparison.Ordinal) &&
                FileHash.Equals(info.FileHash, StringComparison.Ordinal);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(FullName);
        hash.Add(FileHash);

        return hash.ToHashCode();
    }
    #endregion Overrides

    public string OriginalString { get; }
    public string FileHash { get; set; } = "";
    [JsonIgnore] public FileInfo Info { get; }
    [JsonIgnore] public string Name { get { return Info.Name.AsSpan(0, Info.Name.Length - Info.Extension.Length).ToString(); } }
    [JsonIgnore] public string Extension { get { return Info.Extension; } }
    [JsonIgnore] public string FullName { get { return Info.FullName; } }


    public FileInfoBLR(string originalString)
    {
        OriginalString = originalString;
        Info = new(originalString);
    }

    public void UpdateFileHash()
    {
        if(Info.Exists) FileHash = IOResources.GetFileHash(Info.FullName);
    }
}
