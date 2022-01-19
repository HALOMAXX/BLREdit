using System;
using System.Windows.Media.Imaging;

namespace BLREdit
{
    public class FoxIcon
    {
        public string Name { get; set; } = "";
        public Uri Icon { get; set; } = null;

        public FoxIcon(string file)
        {
            string[] fileparts = file.Split('\\');
            string[] iconparts = fileparts[fileparts.Length - 1].Split('.');
            string iconname = "";
            if (iconparts.Length > 2)
            {
                for (int i = 0; i < iconparts.Length - 1; i++)
                {
                    if (i != 0)
                    {
                        iconname += ".";
                    }
                    iconname += iconparts[i];
                }
            }
            else
            { 
                iconname = iconparts[0];
            }
            Name = iconname;
            Icon = new Uri(AppDomain.CurrentDomain.BaseDirectory + file, UriKind.Absolute);
        }

        public static BitmapImage CreateEmptyBitmap()
        {
            return new BitmapImage();
        }
    }
}
