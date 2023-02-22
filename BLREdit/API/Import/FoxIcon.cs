using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BLREdit.Import;

public sealed class FoxIcon
{
    public FileInfo IconFileInfo { get; private set; }
    public string IconName { get; private set; }

    public FoxIcon(string file)
    {
        IconFileInfo = new FileInfo(file);
        IconName = IconFileInfo.Name.Substring(0, IconFileInfo.Name.Length - IconFileInfo.Extension.Length);
    }
}
