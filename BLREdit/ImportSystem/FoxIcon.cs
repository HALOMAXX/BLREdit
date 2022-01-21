using System;
using System.IO;
using System.Windows.Media;
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
            // Define parameters used to create the BitmapSource.
            PixelFormat pf = PixelFormats.Bgr32;
            int width = 128;
            int height = 128;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            // Initialize the image with data.
            for (int i = 0; i < rawImage.Length; i++)
            {
                rawImage[i] = (byte)(128);
            }

            // Create a BitmapSource.
            BitmapSource bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null,
                rawImage, rawStride);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream stream = new MemoryStream();
            BitmapImage bitmapImage = new BitmapImage();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream);

            stream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(stream.ToArray());
            bitmapImage.EndInit();

            stream.Close();

            return bitmapImage;
        }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
