using System.Windows;
using System.Windows.Controls;

namespace ImageMiniLab.WPF.Views;
/// <summary>
/// ConvolutioMaskDialog.xaml 的互動邏輯
/// </summary>
public partial class ConvolutioMaskDialog : Window {

    private TextBox[,] maskCells = new TextBox[5, 5];
    public string KernelSize = "3x3"; // 預設值
    private int currentSize = 3;
    public float[] ResultMask { get; private set; }
    public ConvolutioMaskDialog() {
        InitializeComponent();
        BuildMaskTextBoxGrid(3);
    }

    private void Ok_Click(object sender, RoutedEventArgs e) {
        List<float> values = [];
        foreach (var tb in maskCells) {
            if (float.TryParse(tb.Text, out float v))
                values.Add(v);
            else
                values.Add(0);
        }
        ResultMask = [.. values];
        DialogResult = true;
        Close();
    }

    private void BuildMaskTextBoxGrid(int size) {
        MaskGrid.Children.Clear();
        MaskGrid.Rows = size;
        MaskGrid.Columns = size;

        maskCells = new TextBox[size, size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                var tb = new TextBox {
                    Text = "0",
                    Margin = new Thickness(2),
                    FontSize = 16,
                    TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                maskCells[i, j] = tb;
                MaskGrid.Children.Add(tb);
            }
        }
    }

    private void KernelSize_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item) {
            string sizeStr = item.Content.ToString() ?? "3 x 3";
            currentSize = sizeStr == "5 x 5" ? 5 : 3;
            BuildMaskTextBoxGrid(currentSize);
        }
    }
}
