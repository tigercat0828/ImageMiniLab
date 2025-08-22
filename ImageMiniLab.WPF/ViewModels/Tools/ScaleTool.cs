using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageMiniLab.WPF.ViewModels.Tools;
public partial class ScaleTool : ToolViewModelBase {

    [ObservableProperty] double _scaleFactor = 100;
    partial void OnScaleFactorChanged(double value) {
        if (value < 0) ScaleFactor = 100;
        else ScaleFactor = value;
    }
}
