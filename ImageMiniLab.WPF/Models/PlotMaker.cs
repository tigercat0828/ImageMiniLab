using ScottPlot;



namespace ImageMiniLab.WPF.Models;
public static class PlotMaker {

    public static void DrawHistogram(Plot plot, RawImage image, int channel) {

        // 建立 256 個 bin
        double[] histo = new double[256];
        for (int i = 0; i < image.PixelCount; i++) {
            int index = i * 4;
            byte value = image[index + channel];
            histo[value]++;
        }

        // X 軸是 0..255
        double[] xs = Enumerable.Range(0, 256).Select(x => (double)x).ToArray();

        plot.Clear(); // 清掉舊圖
        plot.Add.Bars(xs, histo);  // 直接畫直方圖

        plot.XLabel("Intensity");
        plot.YLabel("Count");
        plot.Axes.SetLimitsX(0, 255);
        plot.Axes.SetLimits(bottom: 0);


    }


}
