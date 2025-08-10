namespace ImageMiniLab.WPF.Interfaces {
    public interface IFileService {
        string OpenFileDialog(string filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp");
        string SaveFileDialog(string filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp");
    }
}