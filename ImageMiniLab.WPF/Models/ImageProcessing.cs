using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Markup.Localizer;
using System.Diagnostics;
using ScottPlot;
using System.Numerics;
using System.Windows.Media.Imaging;
using System.Runtime.ExceptionServices;
using OpenTK.Graphics.OpenGL;

namespace ImageMiniLab.WPF.Models;
public static class ImageProcessing {

    public static RawImage GrayScaleAverage(RawImage input) {
        var w = input.Width;
        var h = input.Height;
        var output = new RawImage(w, h);
        Parallel.For(0, h, y => {

            for (int x = 0; x < w; x++) {
                int i = (y * w + x) * 4;
                byte b = input[i + 0];
                byte g = input[i + 1];
                byte r = input[i + 2];
                byte gray = (byte)((b + g + r) / 3);
                output[i + 0] = gray;
                output[i + 1] = gray;
                output[i + 2] = gray;
                output[i + 3] = 255;
            }
        });
        return output;
    }
    public static RawImage GrayScaleWeighted(RawImage input) {
        var w = input.Width;
        var h = input.Height;
        var output = new RawImage(w, h);

        Parallel.For(0, h, y => {
            for (int x = 0; x < w; x++) {
                int i = (y * w + x) * 4;
                byte b = input[i + 0];
                byte g = input[i + 1];
                byte r = input[i + 2];
                byte yv = (byte)Math.Clamp(0.114f * b + 0.587f * g + 0.299f * r, 0, 255);
                output[i + 0] = yv;
                output[i + 1] = yv;
                output[i + 2] = yv;
                output[i + 3] = 255;
            }
        });
        return output;
    }
    public static unsafe RawImage RotateRight(RawImage input) { 
     
        int width = input.Width; 
        int height = input.Height; 
        var output = new RawImage(height, width);
        fixed (byte* src = input.Pixels)
        fixed (byte* dst = output.Pixels) {
            uint* s = (uint*)src;
            uint* d = (uint*)dst;
            Parallel.For(0, height, y => { 
            
                for(int x =0; x < width; x++) {
                    int si = (y * width + x);
                    
                    int dstX = height - 1 - y;
                    int dstY = x;

                    int di = dstY * height + dstX;
                    d[di] = s[si];
                }
            });
        }
        return output;
    }
    public static unsafe RawImage RotateLeft(RawImage input) {

        int width = input.Width;
        int height = input.Height;
        var output = new RawImage(height, width);
        fixed (byte* src = input.Pixels)
        fixed (byte* dst = output.Pixels) {
            uint* s = (uint*)src;
            uint* d = (uint*)dst;
            Parallel.For(0, height, y => {

                for (int x = 0; x < width; x++) {
                    int si = (y * width + x);
                    int dstX = y;
                    int dstY = width - 1 - x;

                    int di = dstY * height + dstX;
                    d[di] = s[si];
                }
            });
        }
        return output;
    }
    public static RawImage FlipVertical(RawImage input) {
        int w = input.Width;
        int h = input.Height;
        RawImage output = new(w, h);
        byte[] srcPixels = input.Pixels;
        byte[] dstPixels = output.Pixels;

        Parallel.For(0, h, y => {
            Span<uint> srcSpan = MemoryMarshal.Cast<byte, uint>(srcPixels);
            Span<uint> dstSpan = MemoryMarshal.Cast<byte, uint>(dstPixels);
            int srcIndex = y * w;
            int dstIndex = (h - 1 - y) * w;
            Span<uint> srcRow = srcSpan.Slice(srcIndex, w);
            Span<uint> dstRow = dstSpan.Slice(dstIndex, w);
            srcRow.CopyTo(dstRow);
        });
        return output;
    }
    public static RawImage FlipHorizontal(RawImage input) {
      int w = input.Width;
        int h = input.Height;
        RawImage output = new(w, h);
        byte[] srcPixels = input.Pixels;
        byte[] dstPixels = output.Pixels;
        Parallel.For(0, h, y => {
            Span<uint> srcSpan = MemoryMarshal.Cast<byte, uint>(srcPixels);
            Span<uint> dstSpan = MemoryMarshal.Cast<byte, uint>(dstPixels);
            int srcIndex = y * w;
            int dstIndex = y * w;
            Span<uint> srcRow = srcSpan.Slice(srcIndex, w);
            Span<uint> dstRow = dstSpan.Slice(dstIndex, w);
            srcRow.CopyTo(dstRow);
            dstRow.Reverse();
        });
        return output;
    }
    public static unsafe RawImage SaltPepperNoise(RawImage input, int noiseValue, out RawImage noiseMask) {
        Random random = new();
        int width = input.Width;
        int height = input.Height;
        RawImage mask = new(width, height);
        RawImage output = new(input);

        // 0xAARRGGBB
        const uint BLACK = 0xFF000000;
        const uint WHITE = 0xFFFFFFFF;
        const uint GRAY = 0xFF808080;

        double prob = noiseValue / 2.0f;

        fixed (byte* msk = mask.Pixels)
        fixed (byte* dst = output.Pixels) {
            uint* m = (uint*)msk;
            uint* d = (uint*)dst;
            Parallel.For(0, input.Length, i => {
                int rand = Random.Shared.Next(0, 100);
                if (rand <= prob) {
                    m[i] = BLACK;
                    d[i] = BLACK;
                }
                else if (rand >= 100 - prob) {
                    m[i] = WHITE;
                    d[i] = WHITE;
                }
                else {
                    m[i] = GRAY;
                }
            });
        }
        noiseMask = mask;
        return output;
    }
    public static RawImage GaussianNoise(RawImage input, float sigma, out RawImage noiseMask) {
        int width = input.Width;
        int height = input.Height;

        RawImage output = new(input); // 複製原圖
        RawImage noise = new(width, height);

        int total = width * height;

        Parallel.For(0, total / 2, i => {
            Span<uint> src = MemoryMarshal.Cast<byte, uint>(input.Pixels);
            Span<uint> dst = MemoryMarshal.Cast<byte, uint>(output.Pixels);
            Span<uint> nse = MemoryMarshal.Cast<byte, uint>(noise.Pixels);

            // Box-Muller
            double u1 = Random.Shared.NextDouble();
            double u2 = Random.Shared.NextDouble();

            double sqrt = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;

            double z1 = sigma * Math.Cos(theta) * sqrt;
            double z2 = sigma * Math.Sin(theta) * sqrt;

            // grab the three channel value
            int idx1 = i * 2;
            int idx2 = i * 2 + 1;

            uint p1 = src[idx1];
            uint p2 = src[idx2];

            byte b1 = (byte)(p1 & 0xFF);
            byte g1 = (byte)((p1 >> 8) & 0xFF);
            byte r1 = (byte)((p1 >> 16) & 0xFF);

            byte b2 = (byte)(p2 & 0xFF);
            byte g2 = (byte)((p2 >> 8) & 0xFF);
            byte r2 = (byte)((p2 >> 16) & 0xFF);

            // apply noise
            b1 = (byte)Math.Clamp(b1 + z1, 0, 255);
            g1 = (byte)Math.Clamp(g1 + z1, 0, 255);
            r1 = (byte)Math.Clamp(r1 + z1, 0, 255);

            b2 = (byte)Math.Clamp(b2 + z2, 0, 255);
            g2 = (byte)Math.Clamp(g2 + z2, 0, 255);
            r2 = (byte)Math.Clamp(r2 + z2, 0, 255);

            dst[idx1] = 0xFF000000 | (uint)(r1 << 16) | (uint)(g1 << 8) | b1;
            dst[idx2] = 0xFF000000 | (uint)(r2 << 16) | (uint)(g2 << 8) | b2;

            // make noise mask 
            byte n1 = (byte)Math.Clamp(128 + z1, 0, 255);
            byte n2 = (byte)Math.Clamp(128 + z2, 0, 255);

            nse[idx1] = 0xFF000000 | (uint)(n1 << 16) | (uint)(n1 << 8) | n1;
            nse[idx2] = 0xFF000000 | (uint)(n2 << 16) | (uint)(n2 << 8) | n2;
        });

        noiseMask = noise;
        return output;
    }
    public static RawImage HistogramEqualize(RawImage input, bool isGrayscale) {
        RawImage output = isGrayscale ? 
            EqualizeForGrayscale(input) : 
            EqualizeForFullColor(input);
        return output;
    }

    public static RawImage EqualizeForGrayscale(RawImage input) {
        // build histogram
        int[] hist = new int[256];
        for (int i = 0; i < input.Length * 4; i += 4) {
            hist[input[i]]++;
        }
        
        // build CDF and elect min CDF
        int[] CDF = new int[256];
        int minCDF = 0;
        CDF[0] = hist[0];
        for (int i = 1; i < 256; i++) CDF[i] = hist[i] + CDF[i - 1];
        for (int i = 0; i < 256; i++) { // first non-zero CDF
            if (CDF[i] != 0) {
                minCDF = CDF[i]; 
                break;
            }
        }
        // build map
        int total = CDF[255];
        if (minCDF == total) return new(input);

        byte[] map = new byte[256];
        double factor = 255.0 / (total - minCDF);
        for (int i = 0; i < 256; i++) {
            int m = (int)Math.Round((CDF[i] - minCDF) * factor);
            map[i] = (byte)Math.Clamp(m, 0, 255);
        }
        // retrieve map value
        RawImage output = new(input.Width, input.Height);
        for (int i = 0; i < input.Length * 4; i+=4) {
            byte value = map[input[i]];
            output[i + 0] = value;
            output[i + 1] = value;
            output[i + 2] = value;
            output[i + 3] = input[i+3];
        }
        return output;
    }

    public static RawImage EqualizeForFullColor(RawImage input) {
        // build histogram
        int[] hist = new int[256];
        for (int i = 0; i < input.Length*4; i+=4) {
            // RGB -> YCbCr
            int b = input[i + 0];
            int g = input[i + 1];
            int r = input[i + 2];
            int yBin = (int)Math.Clamp(Math.Round(Luma(r, g, b)), 0, 255);
            hist[yBin]++;
        }
        // build CDF and get minCDF
        int[] CDF = new int[256];
        CDF[0] = hist[0];
        for (int i = 1; i < 256; i++) CDF[i] = CDF[i - 1] + hist[i];
        int minCDF = 0;
        for (int i = 0; i < 256; i++) {
            if (CDF[i] != 0) {
                minCDF = CDF[i];
                break;
            }
        }
        int total = CDF[255];
        if (minCDF == total || total == 0) return new(input); // all the value are same value
        // build map
        int[] map = new int[256];
        double factor = 255.0f / (total - minCDF);
        for (int i = 0; i < 256; i++) {
            int m = (int)Math.Round((CDF[i] - minCDF) * factor);
            map[i] = m;
        }
        // retrieve map value
        RawImage output = new (input.Width, input.Height);
        for (int i = 0; i < input.Length * 4; i += 4) {
            int b = input[i + 0];
            int g = input[i + 1];
            int r = input[i + 2];
            byte alpha = input[i + 3];

            int yBin = (int)Math.Round(Luma(r, g, b));
            double newY = map[yBin];

            RgbToCbCr(r, g, b, out double cb, out double cr);
            CbCrToRGB(newY, cb, cr, out int newR, out int newG, out int newB);
            output[i + 0] = (byte)newB;
            output[i + 1] = (byte)newG;
            output[i + 2] = (byte)newR;
            output[i + 3] = alpha;
        }

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Luma(int r, int g, int b) => 0.299 * r + 0.587 * g + 0.114 * b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RgbToCbCr(int r, int g, int b, out double cb, out double cr) {
        cb = -0.168736 * r - 0.331264 * g + 0.5 * b + 128.0;
        cr = 0.5 * r - 0.418688 * g - 0.081312 * b + 128.0;
        cb = Math.Clamp(cb, 0, 255);
        cr = Math.Clamp(cr, 0, 255);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CbCrToRGB(double y, double cb, double cr, out int r, out int g, out int b) {
        double dcb = cb - 128.0;
        double dcr = cr - 128.0;
        r = Math.Clamp((int)Math.Round(y + 1.402 * dcr), 0, 255);
        g = Math.Clamp((int)Math.Round(y - 0.344136 * dcb - 0.714136 * dcr), 0, 255);
        b = Math.Clamp((int)Math.Round(y + 1.772 * dcb), 0, 255);
    }

}
