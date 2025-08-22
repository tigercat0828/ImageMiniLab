using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageMiniLab.WPF.ViewModels.Tools;
public partial class MosaicTool : ToolViewModelBase {
    [ObservableProperty] int _cellSize = 10;
    partial void OnCellSizeChanged(int value) {
        if (CellSize < 0) CellSize = 0;
        CellSize = value;
    }
}
