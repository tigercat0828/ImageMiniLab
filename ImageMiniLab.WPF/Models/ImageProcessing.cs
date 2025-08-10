using System.Runtime.CompilerServices;
using System;

namespace ImageMiniLab.WPF.Models;
public static class ImageProcessing {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RawImage GrayScaleAverage(RawImage input) {
        var w = input.Width;
        var h = input.Height;

        var dst = new RawImage(w, h);
        var srcPixels = input.Pixels;
        var dstPixels = dst.Pixels;

        Parallel.For(0, h, y => {
            for (int x = 0; x < w; x++) {
                int i = (y * w + x) * 4;
                byte b = srcPixels[i + 0];
                byte g = srcPixels[i + 1];
                byte r = srcPixels[i + 2];
                byte gray = (byte)((b + g + r) / 3);
                dstPixels[i + 0] = gray;
                dstPixels[i + 1] = gray;
                dstPixels[i + 2] = gray;
                dstPixels[i + 3] = 255;
            }
        });
        return dst;
    }
    public static RawImage GrayScaleWeighted(RawImage input) {
        var w = input.Width;
        var h = input.Height;

        var dst = new RawImage(w, h);
        var srcPixels = input.Pixels;
        var dstPixels = dst.Pixels;

        Parallel.For(0, h, y => {
            for (int x = 0; x < w; x++) {
                int i = (y * w + x) * 4;
                byte b = srcPixels[i + 0];
                byte g = srcPixels[i + 1];
                byte r = srcPixels[i + 2];
                byte yv = (byte)Math.Clamp(0.114f * b + 0.587f * g + 0.299f * r, 0, 255);
                dstPixels[i + 0] = yv;
                dstPixels[i + 1] = yv;
                dstPixels[i + 2] = yv;
                dstPixels[i + 3] = 255;
            }
        });
        return dst;
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

    public static unsafe RawImage FlipVertical(RawImage input) {
        int width = input.Width;
        int height = input.Height;
        var output = new RawImage(width, height);
        fixed (byte* src = input.Pixels)
        fixed (byte* dst = output.Pixels) {
            uint* s = (uint*)src;
            uint* d = (uint*)dst;
            Parallel.For(0,height, y => {
                    uint* sRow = s + (y * width);
                    uint* dRow = d + (height - 1 - y) * width;

                System.Buffer.MemoryCopy();     
            });
        }
        return output;
    }

    public static unsafe RawImage FlipHorizontal(RawImage input) {
        return default;
    }

}
