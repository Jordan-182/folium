using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Folium.Core.Services;
using Folium.Desktop.Services;

namespace Folium.Desktop.ViewModels;

public partial class PdfRotateViewModel : ViewModelBase
{
    private readonly IPdfService _pdfService;
    private readonly IFilePickerService _filePicker;

    public IReadOnlyList<int> AvailableDegrees { get; } = [90, 180, 270];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RotateCommand))]
    private string _inputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RotateCommand))]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private int _selectedDegrees = 90;

    [ObservableProperty]
    private string _pageNumbersText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RotateCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    public PdfRotateViewModel(IPdfService pdfService, IFilePickerService filePicker)
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
        var path = await _filePicker.SaveFileAsync("Enregistrer le PDF pivoté", "rotated.pdf", "pdf");
        if (path is not null)
            OutputPath = path;
    }

    [RelayCommand(CanExecute = nameof(CanRotate))]
    private async Task RotateAsync(CancellationToken ct)
    {
        IsBusy = true;
        ProgressValue = 0;
        StatusMessage = string.Empty;
        IsError = false;

        var pageNumbers = ParsePageNumbers();
        var progress = new Progress<int>(p => ProgressValue = p);
        var result = await _pdfService.RotateAsync(InputPath, OutputPath, SelectedDegrees, pageNumbers, progress, ct);

        IsBusy = false;

        if (result.IsSuccess)
        {
            StatusMessage = $"Rotation réussie : {result.Data}";
            IsError = false;
        }
        else
        {
            StatusMessage = result.ErrorMessage ?? "Erreur inconnue.";
            IsError = true;
        }
    }

    private bool CanRotate() =>
        !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath) && !IsBusy;

    private IReadOnlyList<int>? ParsePageNumbers()
    {
        if (string.IsNullOrWhiteSpace(PageNumbersText))
            return null;

        return PageNumbersText
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.Parse(s.Trim()))
            .ToList();
    }
}
