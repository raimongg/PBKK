using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CampusApp.ViewModels;
using CampusApp.Views;

namespace CampusApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var registrasiViewModel = new RegistrasiViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = registrasiViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
