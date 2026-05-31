namespace Folium.Core.Models;

/// <summary>Represents a file to be processed by Folium.</summary>
public sealed record FoliumFile
{
    /// <summary>Absolute path to the file on disk.</summary>
    public required string AbsolutePath { get; init; }

    /// <summary>File name without directory (e.g. "document.pdf").</summary>
    public string FileName => Path.GetFileName(AbsolutePath);

    /// <summary>File extension in lowercase (e.g. ".pdf", ".jpg").</summary>
    public string Extension => Path.GetExtension(AbsolutePath).ToLowerInvariant();

    /// <summary>File size in bytes. -1 if not yet resolved.</summary>
    public long SizeBytes { get; init; } = -1;
}
