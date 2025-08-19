using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageMiniLab.WPF.ViewModels.Tools;
public partial class SaltPepperNoiseTool : ToolViewModelBase {

    [ObservableProperty] private int _probability = 5;
    partial void OnProbabilityChanged(int value) { 
        if(value < 0) _probability = 0;
        else if(value > 100) _probability = 100;
    }

}
