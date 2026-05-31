using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Folium.Core.Services;
using Folium.Desktop.Services;

namespace Folium.Desktop.ViewModels;

public partial class PdfSplitViewModel : ViewModelBase
{
    private readonly IPdfService _pdfService;
    private readonly IFilePickerService _filePicker;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SplitCommand))]
    private string _inputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SplitCommand))]
    private string _outputDirectory = string.Empty;

    [ObservableProperty]
    private string _pageRanges = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SplitCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    public PdfSplitViewModel(IPdfService pdfService, IFilePickerService filePicker)
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
    private async Task BrowseOutputFolderAsync()
    {
        var folder = await _filePicker.OpenFolderAsync("Choisir le dossier de sortie");
        if (folder is not null)
            OutputDirectory = folder;
    }

    [RelayCommand(CanExecute = nameof(CanSplit))]
    private async Task SplitAsync(CancellationToken ct)
    {
        IsBusy = true;
        ProgressValue = 0;
        StatusMessage = string.Empty;
        IsError = false;

        var ranges = string.IsNullOrWhiteSpace(PageRanges) ? null : PageRanges.Trim();
        var progress = new Progress<int>(p => ProgressValue = p);
        var result = await _pdfService.SplitAsync(InputPath, OutputDirectory, ranges, progress, ct);

        IsBusy = false;

        if (result.IsSuccess)
        {
            StatusMessage = $"{result.Data!.Count} fichier(s) créé(s) dans : {OutputDirectory}";
            IsError = false;
        }
        else
        {
            StatusMessage = result.ErrorMessage ?? "Erreur inconnue.";
            IsError = true;
        }
    }

    private bool CanSplit() =>
        !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputDirectory) && !IsBusy;
}
