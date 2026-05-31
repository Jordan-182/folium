using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Folium.Core.Services;
using Folium.Desktop.Services;

namespace Folium.Desktop.ViewModels;

public partial class ImageResizeViewModel : ViewModelBase
{
    private readonly IImageService _imageService;
    private readonly IFilePickerService _filePicker;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
    private string _inputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
    private int _width;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
    private int _height;

    [ObservableProperty]
    private bool _maintainAspectRatio = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    public ImageResizeViewModel(IImageService imageService, IFilePickerService filePicker)
    {
        _imageService = imageService;
        _filePicker = filePicker;
    }

    [RelayCommand]
    private async Task BrowseInputAsync()
    {
        var paths = await _filePicker.OpenFilesAsync(
            "Sélectionner une image", "jpg", "jpeg", "png", "webp", "avif", "tiff", "gif", "bmp");
        if (paths.Count > 0)
            InputPath = paths[0];
    }

    [RelayCommand]
    private async Task BrowseOutputAsync()
    {
        var ext = System.IO.Path.GetExtension(InputPath).TrimStart('.');
        if (string.IsNullOrEmpty(ext)) ext = "png";
        var path = await _filePicker.SaveFileAsync("Enregistrer l'image redimensionnée", $"resized.{ext}", ext);
        if (path is not null)
            OutputPath = path;
    }

    [RelayCommand(CanExecute = nameof(CanResize))]
    private async Task ResizeAsync(CancellationToken ct)
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        IsError = false;

        var result = await _imageService.ResizeAsync(InputPath, OutputPath, Width, Height, MaintainAspectRatio, ct);

        IsBusy = false;

        if (result.IsSuccess)
        {
            StatusMessage = $"Redimensionnement réussi : {result.Data}";
            IsError = false;
        }
        else
        {
            StatusMessage = result.ErrorMessage ?? "Erreur inconnue.";
            IsError = true;
        }
    }

    private bool CanResize() =>
        !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath)
        && (Width > 0 || Height > 0) && !IsBusy;
}
