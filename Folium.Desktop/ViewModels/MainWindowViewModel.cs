using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Folium.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>The currently displayed page — ViewLocator maps this to its corresponding View.</summary>
    [ObservableProperty]
    private ViewModelBase _currentPage = null!;

    public MainWindowViewModel()
    {
        // App.Services is null at design-time (IDE preview); guard to avoid crash
        if (App.Services is null) return;
        _currentPage = App.Services.GetRequiredService<PdfMergeViewModel>();
    }

    [RelayCommand]
    private void NavigateToPdfMerge() =>
        CurrentPage = App.Services.GetRequiredService<PdfMergeViewModel>();

    [RelayCommand]
    private void NavigateToPdfSplit() =>
        CurrentPage = App.Services.GetRequiredService<PdfSplitViewModel>();

    [RelayCommand]
    private void NavigateToPdfCompress() =>
        CurrentPage = App.Services.GetRequiredService<PdfCompressViewModel>();

    [RelayCommand]
    private void NavigateToPdfRotate() =>
        CurrentPage = App.Services.GetRequiredService<PdfRotateViewModel>();

    [RelayCommand]
    private void NavigateToImageConvert() =>
        CurrentPage = App.Services.GetRequiredService<ImageConvertViewModel>();

    [RelayCommand]
    private void NavigateToImageResize() =>
        CurrentPage = App.Services.GetRequiredService<ImageResizeViewModel>();
}
