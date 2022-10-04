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

    public readonly Lazy<BitmapSource> WideImage;
    public readonly Lazy<BitmapSource> LargeImage;
    public readonly Lazy<BitmapSource> SmallImage;

    public static readonly BitmapImage WideEmpty = CreateEmptyBitmap(WideImageWidth, WideImageHeight);
    public static readonly BitmapImage LargeEmpty = CreateEmptyBitmap(LargeSquareImageWidth, LargeSquareImageWidth);
    public static readonly BitmapImage SmallEmpty = CreateEmptyBitmap(SmallSquareImageWidth, SmallSquareImageWidth);

    private const int WideImageWidth = 256;
    private const int WideImageHeight = 128;

    private const int LargeSquareImageWidth = 128;

    private const int SmallSquareImageWidth = 64;

    static FoxIcon()
    {
        WideEmpty.Freeze();
        LargeEmpty.Freeze();
        SmallEmpty.Freeze();
    }

    public FoxIcon(string file)
    {
        IconFileInfo = new FileInfo(file);
        IconName = IconFileInfo.Name.Split('.')[0];
        WideImage = new(() => { var img = GetWideImage(); img.Freeze(); return img; });
        LargeImage = new(() => { var img = GetLargeSquareImage(); img.Freeze(); return img; });
        SmallImage = new(() => { var img = GetSmallSquareImage(); img.Freeze(); return img; });
    }

    public BitmapSource GetWideImage()
    {
        return GetImage(WideImageWidth, WideImageHeight, WideEmpty.Clone(), true);
    }

    public BitmapSource GetLargeSquareImage()
    {
        return GetSquareImage(LargeSquareImageWidth, LargeEmpty.Clone());
    }

    public BitmapSource GetSmallSquareImage()
    {
        return GetSquareImage(SmallSquareImageWidth, SmallEmpty.Clone());
    }

    public BitmapSource GetSquareImage(int square, BitmapImage empty)
    {
        return GetImage(square, square, empty, true);
    }

    private BitmapSource GetImage(int Width, int Height, BitmapImage empty, bool Uniform)
    {
        DrawingGroup group = new();


        ImageDrawing baseImage = new() //background for Image
        {
            Rect = new Rect(0, 0, Width, Height),
            ImageSource = empty
        };
        group.Children.Add(baseImage);

        var tmp = GetImage(); //Load the actual image we want to draw

        ImageDrawing actualImage = new()
        {
            Rect = CreateRectForImage(Width, Height, tmp.PixelWidth, tmp.PixelHeight, Uniform),
            ImageSource = tmp
        };
        group.Children.Add(actualImage);

        var finished = new DrawingImage(group);
        return ToBitmapSource(finished);
    }

    public static Rect CreateRectForImage(double BaseWidth, double BaseHeight, double InsertWidth, double InsertHeight, bool Uniform)
    {
        double offsetX = (BaseWidth - InsertWidth);
        double offsetY = (BaseHeight - InsertHeight);

        double scaleX = BaseWidth / InsertWidth;
        double scaleY = BaseHeight / InsertHeight;

        if (Uniform)
        {
            if (scaleX <= scaleY)
            {
                double finalOffset = 0;
                if (offsetX != offsetY)
                {
                    double scaledOffset = BaseHeight - (InsertHeight * scaleX);
                    finalOffset = scaledOffset - (scaledOffset / 2.0);
                }
                return new Rect(0, finalOffset, InsertWidth * scaleX, InsertHeight * scaleX);
            }
            else
            {
                return new Rect((offsetX * scaleY) / 2.0, 0, InsertWidth * scaleY, InsertHeight * scaleY);
            }
        }
        else
        {
            return new Rect(0, 0, InsertWidth * scaleX, InsertHeight * scaleY);
        }
    }

    private BitmapSource GetImage()
    {
        return new BitmapImage(new Uri(IconFileInfo.FullName, UriKind.Absolute));
    }

    public static BitmapSource ToBitmapSource(DrawingImage source)
    {
        DrawingVisual drawingVisual = new();
        DrawingContext drawingContext = drawingVisual.RenderOpen();
        drawingContext.DrawImage(source, new Rect(new Point(0, 0), new Size(source.Width, source.Height)));
        drawingContext.Close();

        RenderTargetBitmap bmp = new((int)source.Width, (int)source.Height, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(drawingVisual);
        return bmp;
    }

    public static BitmapImage CreateEmptyBitmap(int width, int height)
    {
        // Define parameters used to create the BitmapSource.
        PixelFormat pf = PixelFormats.Bgra32;

        int rawStride = (width * pf.BitsPerPixel) / 8;
        byte[] rawImage = new byte[rawStride * height];

        // Initialize the image with data.
        for (int i = 0; i < rawImage.Length; i += 4)
        {
            rawImage[i] = 0;    //Blue
            rawImage[i + 1] = 0;  //Green
            rawImage[i + 2] = 0;  //Red
            rawImage[i + 3] = 0; //Alpha
        }

        // Create a BitmapSource.
        BitmapSource bitmap = BitmapSource.Create(width, height,
            96, 96, pf, null,
            rawImage, rawStride);
        PngBitmapEncoder encoder = new();
        MemoryStream stream = new();
        BitmapImage bitmapImage = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(stream);

        stream.Position = 0;
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = new MemoryStream(stream.ToArray());
        bitmapImage.EndInit();

        stream.Close();

        return bitmapImage;
    }
}
