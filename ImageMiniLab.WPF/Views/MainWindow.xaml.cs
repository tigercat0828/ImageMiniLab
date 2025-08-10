using ImageMiniLab.WPF.ViewModels;
using System.Windows;

namespace ImageMiniLab.WPF.Views {
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window {

        private readonly MainWindowViewModel _viewModel;
        public MainWindow(MainWindowViewModel vm) {
            InitializeComponent();
            this.DataContext = _viewModel = vm;
        }

    }
}
