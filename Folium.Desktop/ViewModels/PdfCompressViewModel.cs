using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Folium.Core.Services;
using Folium.Desktop.Services;

namespace Folium.Desktop.ViewModels;

public partial class PdfCompressViewModel : ViewModelBase
{
    private readonly IPdfService _pdfService;
    private readonly IFilePickerService _filePicker;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompressCommand))]
    private string _inputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompressCommand))]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompressCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private int _imageQuality = 75;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    public PdfCompressViewModel(IPdfService pdfService, IFilePickerService filePicker)
    {
        _pdfService = pdfService;
        _filePicker = filePicker;
    }

    [RelayCommand]
    private async Task BrowseInputAsync()
    {
        var paths = await _filePicker.OpenFilesAsync("Sélectionner un PDF", "pdf");
        if (paths.Count > 0)
            InputPath = paths[0];
    }

    [RelayCommand]
    private async Task BrowseOutputAsync()
    {
        var path = await _filePicker.SaveFileAsync("Enregistrer le PDF compressé", "compressed.pdf", "pdf");
        if (path is not null)
            OutputPath = path;
    }

    [RelayCommand(CanExecute = nameof(CanCompress))]
    private async Task CompressAsync(CancellationToken ct)
    {
        IsBusy = true;
        ProgressValue = 0;
        StatusMessage = string.Empty;
        IsError = false;

        var progress = new Progress<int>(p => ProgressValue = p);
        var result = await _pdfService.CompressAsync(InputPath, OutputPath, ImageQuality, progress, ct);

        IsBusy = false;

        if (result.IsSuccess)
        {
            var inputSize = new FileInfo(InputPath).Length;
            var outputSize = new FileInfo(OutputPath).Length;
            var reduction = (1.0 - (double)outputSize / inputSize) * 100;
            StatusMessage = $"{inputSize / 1024.0 / 1024:F2} Mo → {outputSize / 1024.0 / 1024:F2} Mo (−{reduction:F0}%) · {OutputPath}";
            IsError = false;
        }
        else
        {
            StatusMessage = result.ErrorMessage ?? "Erreur inconnue.";
            IsError = true;
        }
    }

    private bool CanCompress() =>
        !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath) && !IsBusy;
}
