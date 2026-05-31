using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Folium.Desktop.Services;

/// <summary>Avalonia StorageProvider-backed implementation of <see cref="IFilePickerService"/>.</summary>
public sealed class FilePickerService : IFilePickerService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> OpenFilesAsync(string title, params string[] extensions)
    {
        var topLevel = GetTopLevel();
        if (topLevel is null) return [];

        var fileTypes = BuildFileTypes(extensions);
        var result = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = true,
            FileTypeFilter = fileTypes.Count > 0 ? fileTypes : null
        });

        return result.Select(f => f.Path.LocalPath).ToList();
    }

    /// <inheritdoc/>
    public async Task<string?> OpenFolderAsync(string title)
    {
        var topLevel = GetTopLevel();
        if (topLevel is null) return null;

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return result.FirstOrDefault()?.Path.LocalPath;
    }

    /// <inheritdoc/>
    public async Task<string?> SaveFileAsync(string title, string defaultFileName, params string[] extensions)
    {
        var topLevel = GetTopLevel();
        if (topLevel is null) return null;

        var fileTypes = BuildFileTypes(extensions);
        var result = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultFileName,
            FileTypeChoices = fileTypes.Count > 0 ? fileTypes : null
        });

        return result?.Path.LocalPath;
    }

    private static TopLevel? GetTopLevel()
    {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return lifetime?.MainWindow is { } window ? TopLevel.GetTopLevel(window) : null;
    }

    private static List<FilePickerFileType> BuildFileTypes(string[] extensions)
    {
        if (extensions.Length == 0) return [];

        return
        [
            new FilePickerFileType(string.Join(", ", extensions).ToUpperInvariant())
            {
                Patterns = extensions.Select(e => $"*.{e.TrimStart('.')}").ToArray()
            }
        ];
    }
}
