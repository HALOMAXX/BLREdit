using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit;

public sealed class FileInfoBLR : ModelBase
{
    private FileInfo? _info;
    public string OriginalString { get; }
    public string FileHash { get; set; } = "";
    [JsonIgnore] public FileInfo Info { get { _info ??= new(OriginalString); return _info; } }
    [JsonIgnore] public string Name { get { return Info.Name.AsSpan(0, Info.Name.Length - Info.Extension.Length).ToString(); } }
    [JsonIgnore] public string Extension { get { return Info.Extension; } }
    [JsonIgnore] public string FullName { get { return Info.FullName; } }


    public FileInfoBLR(string originalString)
    {
        OriginalString = originalString;
    }

    public void UpdateFileHash()
    { 
        FileHash = IOResources.GetFileHash(Info.FullName);
    }
}
