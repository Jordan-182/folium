using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Folium.Core.Services;
using Folium.Desktop.Services;
using Folium.Desktop.ViewModels;
using Folium.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Folium.Desktop;

public partial class App : Application
{
    /// <summary>DI container — available after OnFrameworkInitializationCompleted.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Core services (stateless — singletons)
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<IImageService, ImageService>();

        // Desktop services
        services.AddSingleton<IFilePickerService, FilePickerService>();

        // ViewModels (transient — fresh instance per navigation)
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PdfMergeViewModel>();
        services.AddTransient<PdfSplitViewModel>();
        services.AddTransient<PdfCompressViewModel>();
        services.AddTransient<PdfRotateViewModel>();
        services.AddTransient<ImageConvertViewModel>();
        services.AddTransient<ImageResizeViewModel>();

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
