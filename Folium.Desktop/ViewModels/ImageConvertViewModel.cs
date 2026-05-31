using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Folium.Core.Services;
using Folium.Desktop.Services;

namespace Folium.Desktop.ViewModels;

public partial class ImageConvertViewModel : ViewModelBase
{
    private readonly IImageService _imageService;
    private readonly IFilePickerService _filePicker;

    public IReadOnlyList<string> AvailableFormats { get; } = ["jpg", "png", "webp", "avif", "tiff"];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConvertCommand))]
    private string _inputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConvertCommand))]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private string _selectedFormat = "jpg";

    [ObservableProperty]
    private int _quality = 85;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConvertCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    public ImageConvertViewModel(IImageService imageService, IFilePickerService filePicker)
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
        var path = await _filePicker.SaveFileAsync(
            "Enregistrer l'image convertie", $"output.{SelectedFormat}", SelectedFormat);
        if (path is not null)
            OutputPath = path;
    }

    [RelayCommand(CanExecute = nameof(CanConvert))]
    private async Task ConvertAsync(CancellationToken ct)
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        IsError = false;

        var result = await _imageService.ConvertAsync(InputPath, OutputPath, Quality, ct);

        IsBusy = false;

        if (result.IsSuccess)
        {
            StatusMessage = $"Conversion réussie : {result.Data}";
            IsError = false;
        }
        else
        {
            StatusMessage = result.ErrorMessage ?? "Erreur inconnue.";
            IsError = true;
        }
    }

    private bool CanConvert() =>
        !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath) && !IsBusy;
}
