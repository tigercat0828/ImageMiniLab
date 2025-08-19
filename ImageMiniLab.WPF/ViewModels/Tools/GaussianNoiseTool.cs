using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMiniLab.WPF.ViewModels.Tools; 
public partial class GaussianNoiseTool : ToolViewModelBase
{
    [ObservableProperty] private float _sigma;
    partial void OnSigmaChanged(float value) {
        if(value < 0) Sigma = 0;
        if(value > 255) Sigma = 255;
    }
}
