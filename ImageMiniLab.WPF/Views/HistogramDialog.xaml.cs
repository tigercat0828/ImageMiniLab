using ImageMiniLab.WPF.Models;
using ImageMiniLab.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageMiniLab.WPF.Views {
    /// <summary>
    /// HistogramDialog.xaml 的互動邏輯
    /// </summary>
    public partial class HistogramDialog : Window {

        public HistogramDialog(RawImage image) {
            InitializeComponent();
            this.DataContext = new HistogramDialogViewModel(image);
        }
    }
}
