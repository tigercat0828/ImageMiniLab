using ImageMiniLab.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CodeDom;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ImageMiniLab.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private readonly IHost _host;

        public App() {
            var builder = Host.CreateApplicationBuilder();

            builder.Services.AddSingleton<MainWindow>();
            _host = builder.Build();
            
        }
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var Window = _host.Services.GetRequiredService<MainWindow>();
            Window.Show();
        }

    }

}
