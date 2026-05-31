using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Folium.Core.Services;
using Folium.Desktop.Services;

namespace Folium.Desktop.ViewModels;

public partial class PdfMergeViewModel : ViewModelBase
{
    private readonly IPdfService _pdfService;
    private readonly IFilePickerService _filePicker;

    public ObservableCollection<string> InputFiles { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeCommand))]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string? _selectedFile;

    public PdfMergeViewModel(IPdfService pdfService, IFilePickerService filePicker)
    {
        _pdfService = pdfService;
        _filePicker = filePicker;
        InputFiles.CollectionChanged += (_, _) => MergeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var paths = await _filePicker.OpenFilesAsync("Sélectionner des PDFs", "pdf");
        foreach (var path in paths)
        {
            if (!InputFiles.Contains(path))
                InputFiles.Add(path);
        }
    }

    [RelayCommand]
    private void RemoveFile(string path) => InputFiles.Remove(path);

    [RelayCommand]
    private void MoveUp(string path)
    {
        var index = InputFiles.IndexOf(path);
        if (index > 0) InputFiles.Move(index, index - 1);
    }

    [RelayCommand]
    private void MoveDown(string path)
    {
        var index = InputFiles.IndexOf(path);
        if (index >= 0 && index < InputFiles.Count - 1) InputFiles.Move(index, index + 1);
    }

    [RelayCommand]
    private async Task BrowseOutputAsync()
    {
        var path = await _filePicker.SaveFileAsync("Enregistrer le PDF fusionné", "fusion.pdf", "pdf");
        if (path is not null)
            OutputPath = path;
    }

    [RelayCommand(CanExecute = nameof(CanMerge))]
    private async Task MergeAsync(CancellationToken ct)
    {
        IsBusy = true;
        ProgressValue = 0;
        StatusMessage = string.Empty;
        IsError = false;

        var progress = new Progress<int>(p => ProgressValue = p);
        var result = await _pdfService.MergeAsync(InputFiles.ToList(), OutputPath, progress, ct);

        IsBusy = false;

        if (result.IsSuccess)
        {
            StatusMessage = $"Fusion réussie : {result.Data}";
            IsError = false;
        }
        else
        {
            StatusMessage = result.ErrorMessage ?? "Erreur inconnue.";
            IsError = true;
        }
    }

    private bool CanMerge() => InputFiles.Count >= 2 && !string.IsNullOrEmpty(OutputPath) && !IsBusy;
}
