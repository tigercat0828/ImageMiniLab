using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMiniLab.WPF.Interfaces;
using ImageMiniLab.WPF.Models;
using ImageMiniLab.WPF.ViewModels.Tools;
using ImageMiniLab.WPF.Views;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageMiniLab.WPF.ViewModels;
public partial class MainWindowViewModel : ObservableObject {
    private readonly IFileService _fileService;
    public MainWindowViewModel(IFileService service) {
        _fileService = service;
    }

    private RawImage _rawOutput = new();

    private readonly Stack<RawImage> _undoStack = [];
    private readonly Stack<RawImage> _redoStack = [];

    [ObservableProperty] private bool _canUndo;
    [ObservableProperty] private bool _canRedo;

    private void PushHistory() {
        _undoStack.Push(_rawOutput.Clone());
        _redoStack.Clear();
        UpdateUndoRedoState();
    }
    private void UpdateUndoRedoState() {
        CanRedo = _redoStack.Count > 0;
        CanUndo = _undoStack.Count > 0;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo() {
        _redoStack.Push(_rawOutput.Clone());
        _rawOutput = _undoStack.Pop();
        ImgOutput = _rawOutput.ToImageSource();
        UpdateUndoRedoState();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo() {
        _undoStack.Push(_rawOutput.Clone());
        _rawOutput = _undoStack.Pop();
        ImgOutput = _rawOutput.ToImageSource();
        UpdateUndoRedoState();
    }

    // status bar
    [ObservableProperty] private string _imageSizeText = "請載入圖片...";
    [ObservableProperty] private string _imageFilename = "Unknown";

    // Image Box
    [ObservableProperty] private BitmapSource? _imgInput;
    [ObservableProperty] private BitmapSource? _imgOutput;

    // tool 
    [ObservableProperty] private ToolViewModelBase? _activeTool;
    private readonly SaltPepperNoiseTool _saltNoiseTool = new();
    private readonly GaussianNoiseTool _gaussianNoiseTool = new();
    private readonly MosaicTool _mosaicTool = new();
    private readonly ScaleTool _scaleTool = new();


    [ObservableProperty] private bool _hasImage;
    partial void OnHasImageChanged(bool value) {
        GrayscaleAverageCommand.NotifyCanExecuteChanged();
        RotateRightCommand.NotifyCanExecuteChanged();
        RotateLeftCommand.NotifyCanExecuteChanged();
        FlipVerticalCommand.NotifyCanExecuteChanged();
        FlipHorizontalCommand.NotifyCanExecuteChanged();
        SaltPepperNoiseCommand.NotifyCanExecuteChanged();
        GaussianNoiseCommand.NotifyCanExecuteChanged();
        HistogramEqualizeCommand.NotifyCanExecuteChanged();
        SaveImageCommand.NotifyCanExecuteChanged();
        SmoothCommand.NotifyCanExecuteChanged();
        EdgeDetectionCommand.NotifyCanExecuteChanged();

        OpenGaussianNoiseToolCommand.NotifyCanExecuteChanged();
        OpenSaltPepperNoiseToolCommand.NotifyCanExecuteChanged();
        OpenMosaicToolCommand.NotifyCanExecuteChanged();
        OpenConvolutionMaskDialogCommand.NotifyCanExecuteChanged();

        OpenHistogramDialogCommand.NotifyCanExecuteChanged();

        TestMethodCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void OpenHistogramDialog() {
        var dialog = new HistogramDialog(_rawOutput) {
            Owner = Application.Current.MainWindow
        };
        dialog.Show();
    }


    [RelayCommand(CanExecute = nameof(HasImage))]
    private void OpenConvolutionMaskDialog() {
        var dialog = new ConvolutioMaskDialog() {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true) {
            float[] mask = dialog.ResultMask;
            MaskKernel maskKernel = new(mask);
            PushHistory();
            _rawOutput = ImageProcessing.ConvolutionFullColor(_rawOutput, maskKernel);
            ImgOutput = _rawOutput.ToImageSource();
        }
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void EdgeDetection() {
        PushHistory();
        _rawOutput = ImageProcessing.EdgeDetection(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void Smooth() {
        PushHistory();
        _rawOutput = ImageProcessing.Smooth(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void GrayscaleAverage() {
        PushHistory();
        ClearToolPanel();
        _rawOutput = ImageProcessing.GrayScaleAverage(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();

    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void RotateRight() {
        PushHistory();
        ClearToolPanel();
        _rawOutput = ImageProcessing.RotateRight(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void RotateLeft() {
        PushHistory();
        ClearToolPanel();
        _rawOutput = ImageProcessing.RotateLeft(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void FlipVertical() {
        PushHistory();
        ClearToolPanel();
        _rawOutput = ImageProcessing.FlipVertical(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void FlipHorizontal() {
        PushHistory();
        ClearToolPanel();
        _rawOutput = ImageProcessing.FlipHorizontal(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void GaussianNoise() {

        PushHistory();
        _rawOutput = ImageProcessing.GaussianNoise(_rawOutput, _gaussianNoiseTool.Sigma, out var noiseMask);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void SaltPepperNoise() {
        PushHistory();
        _rawOutput = ImageProcessing.SaltPepperNoise(_rawOutput, _saltNoiseTool.Probability, out var _);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void Mosaic() {
        PushHistory();
        _rawOutput = ImageProcessing.Mosaic(_rawOutput, _mosaicTool.CellSize);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void HistogramEqualize() {
        PushHistory();
        _rawOutput = ImageProcessing.HistogramEqualize(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void Scale() {

        PushHistory();
        _rawOutput = ImageProcessing.ScaleBilinearInterpolation(_rawOutput, _scaleTool.ScaleFactor / 100.0);
        ImgOutput = _rawOutput.ToImageSource();
    }

    [RelayCommand] private void OpenSaltPepperNoiseTool() => ActiveTool = _saltNoiseTool;
    [RelayCommand] private void OpenGaussianNoiseTool() => ActiveTool = _gaussianNoiseTool;
    [RelayCommand] private void OpenMosaicTool() => ActiveTool = _mosaicTool;
    [RelayCommand] private void OpenScaleTool() => ActiveTool = _scaleTool;
    [RelayCommand] private void ClearToolPanel() => ActiveTool = null;

    // File operations
    [RelayCommand]
    private void LoadImage() {
        var path = _fileService.OpenFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            _rawOutput.LoadFile(path);
            ImgOutput = _rawOutput.ToImageSource();
            HasImage = true;
        }
        ImageSizeText = $"{ImgOutput.Width} x {ImgOutput.Height}";
        ImageFilename = path;
    }

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void SaveImage() {
        var path = _fileService.SaveFileDialog();
        if (!string.IsNullOrEmpty(path)) {
            _rawOutput.SaveFile(path);
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

    [RelayCommand(CanExecute = nameof(HasImage))]
    private void TestMethod() {
        PushHistory();
        _rawOutput = ImageProcessing.Reverse(_rawOutput);
        ImgOutput = _rawOutput.ToImageSource();
    }

}
