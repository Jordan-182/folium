using Folium.Core.Models;
using ImageMagick;

namespace Folium.Core.Services;

/// <summary>Magick.NET-based implementation of <see cref="IImageService"/>.</summary>
public sealed class ImageService : IImageService
{
    /// <inheritdoc/>
    public async Task<OperationResult<string>> ConvertAsync(
        string inputPath,
        string outputPath,
        int quality = 85,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPath))
            return OperationResult<string>.Failure($"Fichier introuvable : {inputPath}");

        var outputDir = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            return OperationResult<string>.Failure($"Dossier de sortie inaccessible : {outputDir}");

        if (quality < 1 || quality > 100)
            return OperationResult<string>.Failure("La qualité doit être comprise entre 1 et 100.");

        try
        {
            using var image = new MagickImage(inputPath);
            image.Quality = (uint)quality;
            await image.WriteAsync(outputPath, cancellationToken);
            return OperationResult<string>.Success(outputPath);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure("Opération annulée.");
        }
        catch (Exception ex)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure($"Erreur lors de la conversion : {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<string>> ResizeAsync(
        string inputPath,
        string outputPath,
        int width,
        int height,
        bool maintainAspectRatio = true,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPath))
            return OperationResult<string>.Failure($"Fichier introuvable : {inputPath}");

        var outputDir = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            return OperationResult<string>.Failure($"Dossier de sortie inaccessible : {outputDir}");

        if (width <= 0 && height <= 0)
            return OperationResult<string>.Failure("La largeur ou la hauteur doit être supérieure à 0.");

        try
        {
            using var image = new MagickImage(inputPath);
            var geometry = new MagickGeometry((uint)Math.Max(width, 0), (uint)Math.Max(height, 0))
            {
                IgnoreAspectRatio = !maintainAspectRatio
            };
            image.Resize(geometry);
            await image.WriteAsync(outputPath, cancellationToken);
            return OperationResult<string>.Success(outputPath);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure("Opération annulée.");
        }
        catch (Exception ex)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure($"Erreur lors du redimensionnement : {ex.Message}");
        }
    }

    private static void TryDeleteFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* best-effort */ }
    }
}
