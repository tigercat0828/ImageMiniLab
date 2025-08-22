using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageMiniLab.WPF.ViewModels.Tools;
public partial class GaussianNoiseTool : ToolViewModelBase {
    [ObservableProperty] private float _sigma;
    partial void OnSigmaChanged(float value) {
        if (value < 0) Sigma = 0;
        if (value > 255) Sigma = 255;
    }
}
