using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMiniLab.WPF.Interfaces;
using ImageMiniLab.WPF.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageMiniLab.WPF.ViewModels;
public partial class MainWindowViewModel : ObservableObject {
    private readonly IFileService _fileService;

    private RawImage _rawInput = new();
    private RawImage _rawOutput = new();

    [ObservableProperty] private bool _hasImage;
    partial void OnHasImageChanged(bool value) {
        GrayscaleAverageCommand.NotifyCanExecuteChanged();
        GrayscaleWeightedCommand.NotifyCanExecuteChanged();
        RotateRightCommand.NotifyCanExecuteChanged();
        RotateLeftCommand.NotifyCanExecuteChanged();
        FlipVerticalCommand.NotifyCanExecuteChanged();
        FlipHorizontalCommand.NotifyCanExecuteChanged();
        SaveImageCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty] private BitmapSource? _imgInput;

    [ObservableProperty] private BitmapSource? _imgOutput;

    public MainWindowViewModel(IFileService service) {
        _fileService = service;
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void GrayscaleAverage() {
        _rawOutput = ImageProcessing.GrayScaleAverage(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();

    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void GrayscaleWeighted() {
        _rawOutput = ImageProcessing.GrayScaleWeighted(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void RotateRight() { 
        _rawOutput = ImageProcessing.RotateRight(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void RotateLeft() {
        _rawOutput = ImageProcessing.RotateLeft(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void FlipVertical() { 
        _rawOutput = ImageProcessing.FlipVertical(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void FlipHorizontal() {
        _rawOutput = ImageProcessing.FlipHorizontal(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    // File operations
    [RelayCommand]
    private void LoadImage() {
        var path = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            _rawInput.LoadFile(path);
            ImgInput = _rawInput.ToImageSource();
            HasImage = true;
        }
    }
    [RelayCommand(CanExecute = nameof(HasImage))]
    private void SaveImage() {
        var path = _fileService.SaveFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            _rawInput.SaveFile(path);
        }
    }
    [RelayCommand] private void Exit() {
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
