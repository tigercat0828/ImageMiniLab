
using CommunityToolkit.Mvvm.ComponentModel;
using ImageMiniLab.WPF.Models;
using System.Windows.Media.Imaging;

namespace ImageMiniLab.WPF.ViewModels;
public partial class HistogramDialogViewModel : ObservableObject {

    private readonly RawImage _target;
    [ObservableProperty] private BitmapSource? _redChannelImageSource;
    [ObservableProperty] private BitmapSource? _greenChannelImageSource;
    [ObservableProperty] private BitmapSource? _blueChannelImageSource;
    public RawImage RedImage { get; }
    public RawImage GreenImage { get; }
    public RawImage BlueImage { get; }

    public HistogramDialogViewModel(RawImage image) {
        _target = image;

        RedImage = ImageProcessing.RedChannel(image);
        GreenImage = ImageProcessing.GreenChannel(image);
        BlueImage = ImageProcessing.BlueChannel(image);
        RedChannelImageSource = RedImage.ToImageSource();
        GreenChannelImageSource = GreenImage.ToImageSource();
        BlueChannelImageSource = BlueImage.ToImageSource();
    }
}
