using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

                for (int x = 0; x < width; x++) {
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
            Parallel.For(0, input.PixelCount, i => {
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
    public static RawImage HistogramEqualize(RawImage input) {
        // build histogram
        int[] hist = new int[256];
        for (int i = 0; i < input.PixelCount * 4; i += 4) {
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
        RawImage output = new(input.Width, input.Height);
        for (int i = 0; i < input.PixelCount * 4; i += 4) {
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
    public static RawImage Mosaic(RawImage input, int cellSize) {

        if (cellSize > input.Width || cellSize > input.Height || cellSize < 2) return new(input);
        int width = input.Width;
        int height = input.Height;
        RawImage output = new(width, height);

        int rows = (int)Math.Ceiling(height / (float)cellSize);
        int cols = (int)Math.Ceiling(width / (float)cellSize);

        Parallel.For(0, rows, r => {
            for (int c = 0; c < cols; c++) {
                int startX = c * cellSize;
                int endX = Math.Min(startX + cellSize, width);
                int startY = r * cellSize;
                int endY = Math.Min(startY + cellSize, height);

                (int avgR, int avgG, int avgB) = AverageBrightness(input, startX, endX, startY, endY);

                // 填滿這個 block
                for (int y = startY; y < endY; y++) {
                    for (int x = startX; x < endX; x++) {
                        int index = (y * width + x) * 4;
                        output[index + 0] = (byte)avgB;
                        output[index + 1] = (byte)avgG;
                        output[index + 2] = (byte)avgR;
                        output[index + 3] = input[index + 3]; // alpha 固定為不透明
                    }
                }
            }
        });
        return output;
    }
    public static RawImage ScaleBilinearInterpolation(RawImage input, double factor) {

        if (factor < 0) return new RawImage(input);
        int newWidth = (int)(input.Width * factor);
        int newHeight = (int)(input.Height * factor);
        int srcWidth = input.Width;
        int srcHeight = input.Height;

        RawImage output = new(newWidth, newHeight);

        Parallel.For(0, newHeight, ny => {                  // newY
            double srcY = ny / factor;
            int y1 = (int)srcY;
            int y2 = Math.Min(y1 + 1, input.Height - 1);
            double dy = srcY - y1;

            for (int nx = 0; nx < newWidth; nx++) {         // newX
                double srcX = nx / factor;
                int x1 = (int)srcX;
                int x2 = Math.Min(x1 + 1, input.Width - 1);
                double dx = srcX - x1;

                int i11 = (y1 * srcWidth + x1) * 4;
                int i12 = (y1 * srcWidth + x2) * 4;
                int i21 = (y2 * srcWidth + x1) * 4;
                int i22 = (y2 * srcWidth + x2) * 4;

                int index = (ny * newWidth + nx) * 4;    // dst index
                output[index + 0] = (byte)BilinearInterpolate(input[i11 + 0], input[i12 + 0], input[i21 + 0], input[i22 + 0], dx, dy);  // B
                output[index + 1] = (byte)BilinearInterpolate(input[i11 + 1], input[i12 + 1], input[i21 + 1], input[i22 + 1], dx, dy);  // G
                output[index + 2] = (byte)BilinearInterpolate(input[i11 + 2], input[i12 + 2], input[i21 + 2], input[i22 + 2], dx, dy);  // R
                output[index + 3] = (byte)BilinearInterpolate(input[i11 + 3], input[i12 + 3], input[i21 + 3], input[i22 + 3], dx, dy);  // A
            }
        });
        return output;
    }
    public static RawImage ConvolutionFullColor(RawImage input, MaskKernel mask) {
        int kernelSize = mask.Size;
        int kernelOffset = kernelSize / 2;
        int width = input.Width;
        int height = input.Height;
        RawImage output = new(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {

                float sumB = 0, sumG = 0, sumR = 0;

                for (int i = -kernelOffset; i <= kernelOffset; i++) {
                    for (int j = -kernelOffset; j <= kernelOffset; j++) {
                        int pX = Math.Clamp(x + i, 0, width - 1);
                        int pY = Math.Clamp(y + j, 0, height - 1);
                        int pIndex = (pY * width + pX) * 4;

                        float kernelValue = mask[i + kernelOffset][j + kernelOffset];

                        sumB += input[pIndex + 0] * kernelValue;
                        sumG += input[pIndex + 1] * kernelValue;
                        sumR += input[pIndex + 2] * kernelValue;
                    }
                }
                int index = (y * width + x) * 4;
                output[index + 0] = (byte)Math.Clamp(sumB / mask.Scalar, 0, 255);
                output[index + 1] = (byte)Math.Clamp(sumG / mask.Scalar, 0, 255);
                output[index + 2] = (byte)Math.Clamp(sumR / mask.Scalar, 0, 255);
                output[index + 3] = input[index + 3];
            }
        });
        return output;
    }
    public static RawImage ConvolutionGrayscale(RawImage input, MaskKernel mask) {
        int kernelOffset = mask.Size / 2;
        int width = input.Width;
        int height = input.Height;
        RawImage output = new(width, height);

        Parallel.For(0, height, y => {
            for (int x = 0; x < width; x++) {
                float gray = 0.0f;

                for (int i = -kernelOffset; i <= kernelOffset; i++) {
                    for (int j = -kernelOffset; j <= kernelOffset; j++) {
                        int pX = Math.Clamp(x + i, 0, width - 1);
                        int pY = Math.Clamp(y + j, 0, height - 1);
                        int pIndex = (pY * width + pX) * 4;
                        float kernelValue = mask[i + kernelOffset][j + kernelOffset];
                        gray += (input[pIndex] & 0xFF) * kernelValue;
                    }
                }
                int index = (y * width + x) * 4;
                byte grayScale = (byte)Math.Clamp(gray / mask.Scalar, 0, 255);
                output[index + 0] = grayScale;
                output[index + 1] = grayScale;
                output[index + 2] = grayScale;
                output[index + 3] = input[index + 3];
            }
        });
        return output;
    }
    public static RawImage Reverse(RawImage input) {
        RawImage output = new(input);
        for (int i = 0; i < input.PixelCount * 4; i += 4) {
            output[i + 0] = (byte)(255 - input[i + 0]);
            output[i + 1] = (byte)(255 - input[i + 1]);
            output[i + 2] = (byte)(255 - input[i + 2]);
        }
        return output;
    }
    public static RawImage Smooth(RawImage input) {
        MaskKernel smoothMask = MaskKernel.LoadPreBuiltMask(DefaultMask.GaussianSmooth);
        return ConvolutionFullColor(input, smoothMask);
    }
    public static RawImage EdgeDetection(RawImage input) {
        // Canny Edge Detection 
        // Step 1: Smooth
        RawImage grayscale = GrayScaleWeighted(input);
        RawImage smooth = Smooth(grayscale);
        // Step 2: Calculate Gradient
        MaskKernel sobelXmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelX);
        MaskKernel sobelYmask = MaskKernel.LoadPreBuiltMask(DefaultMask.SobelY);
        RawImage sobelXimage = ConvolutionGrayscale(smooth, sobelXmask);
        RawImage sobelYimage = ConvolutionGrayscale(smooth, sobelYmask);

        Func<byte, byte, byte> grad = (gx, gy) => {
            int dx = gx - 128; // 補回負數方向，因為原本被壓成 0~255
            int dy = gy - 128;
            double g = Math.Sqrt(dx * dx + dy * dy);
            return (byte)Math.Clamp(g, 0, 255);
        };

        RawImage gradientData = OverlayCalculate(sobelXimage, sobelYimage, grad);
        return gradientData;
        // return Reverse(gradientData);
    }
    public static RawImage OverlayCalculate(RawImage input1, RawImage input2, Func<byte, byte, byte> func) {

        if (input1.Width != input2.Width || input1.Height != input2.Height) {
            return null!;
        }
        int width = input1.Width;
        int height = input1.Height;
        int length = input1.PixelCount;
        RawImage output = new(width, height);

        Parallel.For(0, length, i => {

            int index = i * 4;
            output[index + 0] = func(input1[index + 0], input2[index + 0]);
            output[index + 1] = func(input1[index + 1], input2[index + 1]);
            output[index + 2] = func(input1[index + 2], input2[index + 2]);
            output[index + 3] = 255;
        });

        return output;
    }
    public static RawImage BlueChannel(RawImage input) => GetChannel(input, 0);
    public static RawImage GreenChannel(RawImage input) => GetChannel(input, 1);
    public static RawImage RedChannel(RawImage input) => GetChannel(input, 2);
    private static RawImage GetChannel(RawImage input, int channel) {
        RawImage output = new(input.Width, input.Height);
        Parallel.For(0, input.PixelCount, i => {
            int index = i * 4;
            output[index + channel] = input[index + channel];
            output[index + 3] = input[index + 3];
        });
        return output;
    }

    // tool method 
    private static double BilinearInterpolate(float v11, float v12, float v21, float v22, double dx, double dy) {
        double value =
            (1 - dx) * (1 - dy) * v11 +
            dx * (1 - dy) * v12 +
            (1 - dx) * dy * v21 +
            dx * dy * v22;
        return value;
    }
    private static (int, int, int) AverageBrightness(RawImage input, int startX, int endX, int startY, int endY) {
        int totalR = 0, totalG = 0, totalB = 0, count = 0;

        for (int y = startY; y < endY; y++) {
            for (int x = startX; x < endX; x++) {
                int index = (y * input.Width + x) * 4;
                byte b = input[index + 0];
                byte g = input[index + 1];
                byte r = input[index + 2];
                totalR += r;
                totalG += g;
                totalB += b;
                count++;
            }
        }

        return (totalR / count, totalG / count, totalB / count);
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
