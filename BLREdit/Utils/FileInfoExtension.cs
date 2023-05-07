using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.Utils;

public sealed class FileInfoExtension
{
    public readonly FileInfo Info;
    public FileInfoExtension(string file)
    {
        Info = new(file);
    }

    private string name;
    public string Name
    {
        get
        {
            name ??= Info.Name.Substring(0, Info.Name.Length - Info.Extension.Length);
            return name;
        }
    }
}
