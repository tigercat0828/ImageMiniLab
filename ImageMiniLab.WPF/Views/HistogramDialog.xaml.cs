using ImageMiniLab.WPF.Models;
using ImageMiniLab.WPF.ViewModels;
using System.Windows;

namespace ImageMiniLab.WPF.Views;
/// <summary>
/// HistogramDialog.xaml 的互動邏輯
/// </summary>
public partial class HistogramDialog : Window {

    public HistogramDialog(RawImage image) {
        InitializeComponent();
        var vm = new HistogramDialogViewModel(image);
        DataContext = vm;

        PlotMaker.DrawHistogram(RedChannel_Histogram.Plot, vm.RedImage, 2);
        PlotMaker.DrawHistogram(GreenChannel_Histogram.Plot, vm.GreenImage, 1);
        PlotMaker.DrawHistogram(BlueChannel_Histogram.Plot, vm.BlueImage, 0);
        BlueChannel_Histogram.Refresh();
        GreenChannel_Histogram.Refresh();
        RedChannel_Histogram.Refresh();
    }
}
