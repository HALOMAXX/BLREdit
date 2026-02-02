using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.Utils;

public static class FileInfoExtension
{
    public static string GetNameWithoutExtension(this FileInfo info)
    { 
        return info.Name.Substring(0, info.Name.Length - info.Extension.Length);
    }
}