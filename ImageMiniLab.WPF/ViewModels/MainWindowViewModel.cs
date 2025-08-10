using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMiniLab.WPF.Interfaces;
using ImageMiniLab.WPF.Models;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageMiniLab.WPF.ViewModels;
public partial class MainWindowViewModel : ObservableObject {
    private readonly IFileService _fileService;

    private RawImage rawInput = new();
    private RawImage rawOutput = new();



    [ObservableProperty] private BitmapSource? _imgInput;

    [ObservableProperty] private BitmapSource? _imgOutput;

    [ObservableProperty] private bool _hasImage;
    public MainWindowViewModel(IFileService service) {
        _fileService = service;
    }


    [RelayCommand(CanExecute = nameof(HasImage))]
    private void GrayscaleAverage() {
        rawOutput = ImageProcessing.GrayScaleAverage(rawInput);
        ImgOutput = rawOutput.ToImageSource();

    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void GrayscaleWeighted() {
        rawOutput = ImageProcessing.GrayScaleWeighted(rawInput);
        ImgOutput = rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void RotateRight() { 
        rawOutput = ImageProcessing.RotateRight(rawInput);
        ImgOutput = rawOutput.ToImageSource();
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void RotateLeft() {
        rawOutput = ImageProcessing.RotateLeft(rawInput);
        ImgOutput = rawOutput.ToImageSource();
    }

    [RelayCommand]
    private void LoadImage() {
        var path = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            rawInput.LoadFile(path);
            ImgInput = rawInput.ToImageSource();
        }
        HasImage = true;
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void SaveImage() {
        var path = _fileService.SaveFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            rawInput.SaveFile(path);
        }
    }
    [RelayCommand]
    private void Exit() {
        if (ImgOutput != null) {
            var result = MessageBox.Show(
                "結束應用程式前儲存圖片?",
                "尚未存檔",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Cancel) return;
            if (result == MessageBoxResult.Yes) {
                SaveImage();
            }
            Application.Current.Shutdown();
        }
        else {
            Application.Current.Shutdown();
        }
    }
   
}
