using Folium.Core.Models;

namespace Folium.Core.Services;

/// <summary>
/// Defines PDF manipulation operations. All methods are async and must never block the UI thread.
/// Implementations must not call external services or write telemetry.
/// </summary>
public interface IPdfService
{
    /// <summary>Merges multiple PDF files into a single output file.</summary>
    /// <param name="inputPaths">Absolute paths to the source PDFs, in merge order.</param>
    /// <param name="outputPath">Absolute path for the merged output PDF.</param>
    /// <param name="progress">Optional progress reporter (0–100).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<OperationResult<string>> MergeAsync(
        IReadOnlyList<string> inputPaths,
        string outputPath,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Splits a PDF into multiple files by page ranges.</summary>
    /// <param name="inputPath">Absolute path to the source PDF.</param>
    /// <param name="outputDirectory">Directory where split files will be written.</param>
    /// <param name="pageRanges">
    /// Page ranges to extract, e.g. "1-3,5,7-9".
    /// Pass null to produce one file per page.
    /// </param>
    /// <param name="progress">Optional progress reporter (0–100).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<OperationResult<IReadOnlyList<string>>> SplitAsync(
        string inputPath,
        string outputDirectory,
        string? pageRanges = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Compresses a PDF to reduce its file size.</summary>
    /// <param name="inputPath">Absolute path to the source PDF.</param>
    /// <param name="outputPath">Absolute path for the compressed output PDF.</param>
    /// <param name="imageQuality">JPEG recompression quality for embedded images (1–100, default 75).</param>
    /// <param name="progress">Optional progress reporter (0–100).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<OperationResult<string>> CompressAsync(
        string inputPath,
        string outputPath,
        int imageQuality = 75,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Rotates specified pages of a PDF.</summary>
    /// <param name="inputPath">Absolute path to the source PDF.</param>
    /// <param name="outputPath">Absolute path for the rotated output PDF.</param>
    /// <param name="degrees">Rotation angle in degrees: 90, 180, or 270.</param>
    /// <param name="pageNumbers">
    /// 1-based page numbers to rotate.
    /// Pass null or empty to rotate all pages.
    /// </param>
    /// <param name="progress">Optional progress reporter (0–100).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<OperationResult<string>> RotateAsync(
        string inputPath,
        string outputPath,
        int degrees,
        IReadOnlyList<int>? pageNumbers = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}
