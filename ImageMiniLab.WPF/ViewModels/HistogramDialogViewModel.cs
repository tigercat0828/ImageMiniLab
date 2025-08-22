
using CommunityToolkit.Mvvm.ComponentModel;
using ImageMiniLab.WPF.Models;
using System.Windows.Media.Imaging;

namespace ImageMiniLab.WPF.ViewModels; 
public partial class HistogramDialogViewModel : ObservableObject {

    private readonly RawImage _target;
    [ObservableProperty] private BitmapSource? _redChannelImageSource;
    [ObservableProperty] private BitmapSource? _greenChannelImageSource;
    [ObservableProperty] private BitmapSource? _blueChannelImageSource;

    public HistogramDialogViewModel(RawImage image) {
        _target = image;
        var redChannel = ImageProcessing.RedChannel(image);
        var greenChannel = ImageProcessing.GreenChannel(image);
        var blueChannel = ImageProcessing.BlueChannel(image);
        RedChannelImageSource = redChannel.ToImageSource();
        GreenChannelImageSource = greenChannel.ToImageSource();
        BlueChannelImageSource = blueChannel.ToImageSource();
    }
}
