using SkiaSharp;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageMiniLab.WPF.Models;
public class RawImage {
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Length => Width * Height;
    
    public byte[] Pixels;

    public byte this[int index] { 
        get { return Pixels[index]; }
        set { Pixels[index] = value; }
    }
    public RawImage() { }
    public RawImage(string filename) {
        LoadFile(filename);
    }
    public RawImage(int width, int height) {
        Width = width;
        Height = height;
        Pixels = new byte[width * height * 4]; // BGRA
    }
    public RawImage(RawImage image) {
        Width = image.Width;
        Height = image.Height;
        Pixels = [.. image.Pixels];
    }
    public unsafe void LoadFile(string filename) {

        using var src = SKBitmap.Decode(filename) ?? throw new IOException($"Failed to load image : {filename}");

        Width = src.Width;
        Height = src.Height;
        Pixels = new byte[Width * Height * 4];  // packed RGBA

        byte* srcBase = (byte*)src.GetPixels();
        if (srcBase == null) throw new InvalidOperationException("Source pixels is null.");

        int srcStride = (int)src.RowBytes;
        int dstStride = Width * 4;

        // adjust padding
        fixed (byte* dstBase = Pixels) {
            for (int y = 0; y < Height; y++) {
                Buffer.MemoryCopy(
                    srcBase + y * srcStride, dstBase + y * dstStride,
                    dstStride, dstStride
                );
            }
        }
        // or just call
        // fixed (byte* dst = Pixels) Buffer.MemoryCopy((void*)bmp.GetPixels(), dst, Pixels.Length, Pixels.Length);
    }
    public unsafe void SaveFile(string filename) {

        if (Pixels == null || Pixels.Length != Width * Height * 4)
            throw new InvalidOperationException("Invalid pixel buffer.");

        // support .png .jpg .bmp
        var extesion = Path.GetExtension(filename).ToLowerInvariant();
        var format = extesion switch {
            ".png" => SKEncodedImageFormat.Png,
            ".jpg" => SKEncodedImageFormat.Jpeg,
            ".bmp" => SKEncodedImageFormat.Bmp,
            ".webp" => SKEncodedImageFormat.Webp,
            _ => throw new NotSupportedException($"Unsupported file format: {extesion}")
        };

        var info = new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        fixed (byte* p = Pixels) {
            using var bitmap = new SKBitmap();
            bitmap.InstallPixels(info, (IntPtr)p, info.RowBytes);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(format, format == SKEncodedImageFormat.Jpeg ? 90 : 100);
            using var fs = File.Create(filename);
            data.SaveTo(fs);
        }

    }
    public BitmapSource ToImageSource() {

        var src = BitmapSource.Create(
            Width, Height,
            96, 96,         // DPI
            PixelFormats.Bgra32, null,
            Pixels, Width * 4);

        src.Freeze();
        return src;
    }
}
