namespace Folium.Desktop.Services;

/// <summary>
/// Abstracts OS file/folder picker dialogs so ViewModels stay decoupled from Avalonia's StorageProvider.
/// </summary>
public interface IFilePickerService
{
    /// <summary>Opens a file picker allowing the user to select one or more files.</summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="extensions">Allowed extensions without dot (e.g. "pdf", "jpg"). Empty = all files.</param>
    Task<IReadOnlyList<string>> OpenFilesAsync(string title, params string[] extensions);

    /// <summary>Opens a folder picker and returns the selected folder path, or null if cancelled.</summary>
    Task<string?> OpenFolderAsync(string title);

    /// <summary>Opens a save-file dialog and returns the chosen path, or null if cancelled.</summary>
    /// <param name="defaultFileName">Pre-filled file name in the dialog.</param>
    /// <param name="extensions">Allowed extensions without dot. Empty = all files.</param>
    Task<string?> SaveFileAsync(string title, string defaultFileName, params string[] extensions);
}
