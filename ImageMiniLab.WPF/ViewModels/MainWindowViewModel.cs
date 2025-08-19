using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMiniLab.WPF.Interfaces;
using ImageMiniLab.WPF.Models;
using ImageMiniLab.WPF.ViewModels.Tools;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageMiniLab.WPF.ViewModels;
public partial class MainWindowViewModel : ObservableObject {
    private readonly IFileService _fileService;
    public MainWindowViewModel(IFileService service) {
        _fileService = service;
    }

    private RawImage _rawInput = new();
    private RawImage _rawOutput = new();


    [ObservableProperty] private string _imageSizeText = "請載入圖片...";
    [ObservableProperty] private string _imageFilename = "Unknown";
    [ObservableProperty] private BitmapSource? _imgInput;
    [ObservableProperty] private BitmapSource? _imgOutput;

    // tool 
    [ObservableProperty] private ToolViewModelBase? _activeTool; // tool 
    private readonly SaltPepperNoiseTool _saltNoiseTool = new();
    private readonly GaussianNoiseTool _gaussianNoiseTool = new();

    [ObservableProperty] private bool _hasImage;
    partial void OnHasImageChanged(bool value) {
        GrayscaleAverageCommand.NotifyCanExecuteChanged();
        GrayscaleWeightedCommand.NotifyCanExecuteChanged();
        RotateRightCommand.NotifyCanExecuteChanged();
        RotateLeftCommand.NotifyCanExecuteChanged();
        FlipVerticalCommand.NotifyCanExecuteChanged();
        FlipHorizontalCommand.NotifyCanExecuteChanged();
        SaltPepperNoiseCommand.NotifyCanExecuteChanged();
        GaussianNoiseCommand.NotifyCanExecuteChanged();
        HistogramEqualizeCommand.NotifyCanExecuteChanged();
        SaveImageCommand.NotifyCanExecuteChanged();
    }
    
    [RelayCommand(CanExecute = nameof(HasImage))] private void GrayscaleAverage() {
        _rawOutput = ImageProcessing.GrayScaleAverage(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();

    }
    
    [RelayCommand(CanExecute = nameof(HasImage))] private void GrayscaleWeighted() {
        _rawOutput = ImageProcessing.GrayScaleWeighted(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    
    [RelayCommand(CanExecute = nameof(HasImage))] private void RotateRight() { 
        _rawOutput = ImageProcessing.RotateRight(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    
    [RelayCommand(CanExecute = nameof(HasImage))] private void RotateLeft() {
        _rawOutput = ImageProcessing.RotateLeft(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    
    [RelayCommand(CanExecute = nameof(HasImage))] private void FlipVertical() { 
        _rawOutput = ImageProcessing.FlipVertical(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    
    [RelayCommand(CanExecute = nameof(HasImage))] private void FlipHorizontal() {
        ActiveTool = null;
        _rawOutput = ImageProcessing.FlipHorizontal(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))] private void GaussianNoise() {
        _rawOutput = ImageProcessing.GaussianNoise(_rawInput, _gaussianNoiseTool.Sigma , out var noiseMask);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))] private void SaltPepperNoise() { 
        _rawOutput = ImageProcessing.SaltPepperNoise(_rawInput, _saltNoiseTool.Probability, out var _);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand] private void OpenSaltPepperNoiseTool() => ActiveTool = _saltNoiseTool;
    [RelayCommand] private void OpenGaussianNoiseTool() => ActiveTool = _gaussianNoiseTool;
    [RelayCommand] private void ClearToolPanel() => ActiveTool = null;    

    [RelayCommand(CanExecute = nameof(HasImage))] private void HistogramEqualize() {
        _rawOutput = ImageProcessing.HistogramEqualize(_rawInput);
        ImgOutput = _rawOutput.ToImageSource();
    }
    
    // File operations
    [RelayCommand] private void LoadImage() {
        var path = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            _rawInput.LoadFile(path);
            ImgInput = _rawInput.ToImageSource();
            HasImage = true;
        }
        ImageSizeText = $"{_rawInput.Width} x {_rawInput.Height}";
        ImageFilename = path;
    }

    [RelayCommand(CanExecute = nameof(HasImage))] private void SaveImage() {
        var path = _fileService.SaveFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            _rawOutput.SaveFile(path);
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
