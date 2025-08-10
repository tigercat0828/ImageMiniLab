using ImageMiniLab.WPF.Interfaces;
using Microsoft.Win32;

namespace ImageMiniLab.WPF.Services;
public class FileService : IFileService {
    public string OpenFileDialog(string filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp") {
        var dialog = new OpenFileDialog() {
            Filter = filter
        };
        return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
    }
    public string SaveFileDialog(string filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp") {
        var dialog = new SaveFileDialog() {
            Filter = filter,
            DefaultExt = ".png",
            FilterIndex = 1
        };
        return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
    }
}
