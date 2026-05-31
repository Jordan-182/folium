using Folium.Core.Models;

namespace Folium.Core.Services;

/// <summary>
/// Defines image manipulation operations. All methods are async and must never block the UI thread.
/// Implementations use Magick.NET and must not call external services.
/// </summary>
public interface IImageService
{
    /// <summary>Converts an image to a different format.</summary>
    /// <param name="inputPath">Absolute path to the source image.</param>
    /// <param name="outputPath">
    /// Absolute path for the converted image.
    /// The file extension determines the output format (e.g. ".webp", ".png").
    /// </param>
    /// <param name="quality">Quality 1–100 for lossy formats (JPEG, WebP). Ignored for lossless.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<OperationResult<string>> ConvertAsync(
        string inputPath,
        string outputPath,
        int quality = 85,
        CancellationToken cancellationToken = default);

    /// <summary>Resizes an image, optionally preserving aspect ratio.</summary>
    /// <param name="inputPath">Absolute path to the source image.</param>
    /// <param name="outputPath">Absolute path for the resized image.</param>
    /// <param name="width">Target width in pixels. Pass 0 to derive from height and aspect ratio.</param>
    /// <param name="height">Target height in pixels. Pass 0 to derive from width and aspect ratio.</param>
    /// <param name="maintainAspectRatio">When true, the image is not stretched.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<OperationResult<string>> ResizeAsync(
        string inputPath,
        string outputPath,
        int width,
        int height,
        bool maintainAspectRatio = true,
        CancellationToken cancellationToken = default);
}
