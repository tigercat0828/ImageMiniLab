using ImageMiniLab.WPF.Interfaces;
using ImageMiniLab.WPF.Services;
using ImageMiniLab.WPF.ViewModels;
using ImageMiniLab.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace ImageMiniLab.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private readonly IHost _host;

        public App() {
            var builder = Host.CreateApplicationBuilder();
            // view
            builder.Services.AddSingleton<IFileService, FileService>();
            builder.Services.AddSingleton<MainWindow>();
            builder.Services.AddSingleton<MainWindowViewModel>();
            // services
            _host = builder.Build();
        }
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var Window = _host.Services.GetRequiredService<MainWindow>();
            Window.Show();
        }

    }

}
