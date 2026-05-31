using Folium.Core.Models;
using ImageMagick;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

namespace Folium.Core.Services;

/// <summary>iText7-based implementation of <see cref="IPdfService"/>.</summary>
public sealed class PdfService : IPdfService
{
    /// <inheritdoc/>
    public async Task<OperationResult<string>> MergeAsync(
        IReadOnlyList<string> inputPaths,
        string outputPath,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (inputPaths.Count < 2)
            return OperationResult<string>.Failure("Au moins 2 fichiers sont requis pour fusionner.");

        foreach (var path in inputPaths)
        {
            if (!File.Exists(path))
                return OperationResult<string>.Failure($"Fichier introuvable : {path}");
        }

        var outputDir = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            return OperationResult<string>.Failure($"Dossier de sortie inaccessible : {outputDir}");

        try
        {
            return await Task.Run(() =>
            {
                using var writer = new PdfWriter(outputPath);
                using var destDoc = new PdfDocument(writer);
                var merger = new PdfMerger(destDoc);

                for (int i = 0; i < inputPaths.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var reader = new PdfReader(inputPaths[i]);
                    using var srcDoc = new PdfDocument(reader);
                    merger.Merge(srcDoc, 1, srcDoc.GetNumberOfPages());
                    progress?.Report((i + 1) * 100 / inputPaths.Count);
                }

                return OperationResult<string>.Success(outputPath);
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure("Opération annulée.");
        }
        catch (Exception ex)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure($"Erreur lors de la fusion : {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IReadOnlyList<string>>> SplitAsync(
        string inputPath,
        string outputDirectory,
        string? pageRanges = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPath))
            return OperationResult<IReadOnlyList<string>>.Failure($"Fichier introuvable : {inputPath}");

        if (!Directory.Exists(outputDirectory))
            return OperationResult<IReadOnlyList<string>>.Failure($"Dossier de sortie introuvable : {outputDirectory}");

        try
        {
            return await Task.Run(() =>
            {
                using var reader = new PdfReader(inputPath);
                using var srcDoc = new PdfDocument(reader);

                int totalPages = srcDoc.GetNumberOfPages();
                var ranges = ParsePageRanges(pageRanges, totalPages);
                var baseName = Path.GetFileNameWithoutExtension(inputPath);
                var outputFiles = new List<string>();

                for (int i = 0; i < ranges.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (from, to) = ranges[i];
                    var outFile = Path.Combine(outputDirectory, $"{baseName}_part{i + 1}.pdf");

                    using var partWriter = new PdfWriter(outFile);
                    using var partDoc = new PdfDocument(partWriter);
                    srcDoc.CopyPagesTo(from, to, partDoc);
                    outputFiles.Add(outFile);

                    progress?.Report((i + 1) * 100 / ranges.Count);
                }

                return OperationResult<IReadOnlyList<string>>.Success(outputFiles);
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<IReadOnlyList<string>>.Failure("Opération annulée.");
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<string>>.Failure($"Erreur lors de la division : {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<string>> CompressAsync(
        string inputPath,
        string outputPath,
        int imageQuality = 75,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPath))
            return OperationResult<string>.Failure($"Fichier introuvable : {inputPath}");

        var outputDir = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            return OperationResult<string>.Failure($"Dossier de sortie inaccessible : {outputDir}");

        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var writerProps = new WriterProperties()
                    .SetCompressionLevel(9)
                    .SetFullCompressionMode(true);

                using var pdfReader = new PdfReader(inputPath);
                pdfReader.SetUnethicalReading(true);
                using var pdfWriter = new PdfWriter(outputPath, writerProps);
                using var doc = new PdfDocument(pdfReader, pdfWriter);

                int total = doc.GetNumberOfPages();
                var visited = new HashSet<PdfObject>(ReferenceEqualityComparer.Instance);

                for (int i = 1; i <= total; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    RecompressPageImages(doc.GetPage(i), imageQuality, visited);
                    progress?.Report(i * 100 / total);
                }

                return OperationResult<string>.Success(outputPath);
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure("Opération annulée.");
        }
        catch (Exception ex)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure($"Erreur lors de la compression : {ex.Message}");
        }
    }

    private static void RecompressPageImages(PdfPage page, int quality, HashSet<PdfObject> visited)
    {
        var xObjDict = page.GetResources().GetResource(PdfName.XObject);
        if (xObjDict != null)
            RecompressXObjectDict(xObjDict, quality, visited);
    }

    private static void RecompressXObjectDict(PdfDictionary xObjDict, int quality, HashSet<PdfObject> visited)
    {
        foreach (var key in xObjDict.KeySet())
        {
            var stream = xObjDict.GetAsStream(key);
            if (stream == null || !visited.Add(stream)) continue;

            var subtype = stream.GetAsName(PdfName.Subtype);

            // Les PDFs Word encapsulent souvent les images dans des Form XObjects — on descend récursivement
            if (PdfName.Form.Equals(subtype))
            {
                var nestedXObjDict = stream.GetAsDictionary(PdfName.Resources)
                                           ?.GetAsDictionary(PdfName.XObject);
                if (nestedXObjDict != null)
                    RecompressXObjectDict(nestedXObjDict, quality, visited);
                continue;
            }

            if (!PdfName.Image.Equals(subtype)) continue;
            if (stream.GetAsBoolean(PdfName.ImageMask)?.GetValue() == true) continue;

            int width = stream.GetAsNumber(PdfName.Width)?.IntValue() ?? 0;
            int height = stream.GetAsNumber(PdfName.Height)?.IntValue() ?? 0;
            int bpc = stream.GetAsNumber(PdfName.BitsPerComponent)?.IntValue() ?? 8;
            if (width == 0 || height == 0 || bpc != 8) continue;

            var filter = stream.Get(PdfName.Filter);
            bool isDct = PdfName.DCTDecode.Equals(filter) ||
                         (filter is PdfArray a1 && a1.Size() == 1 && PdfName.DCTDecode.Equals(a1.Get(0)));
            bool isFlate = PdfName.FlateDecode.Equals(filter) ||
                           (filter is PdfArray a2 && a2.Size() == 1 && PdfName.FlateDecode.Equals(a2.Get(0)));

            if (!isDct && !isFlate) continue;

            byte[] storedBytes = stream.GetBytes(false);
            if (storedBytes.Length < 5_000) continue;

            byte[]? recompressed = null;

            if (isDct)
            {
                recompressed = RecompressJpeg(storedBytes, quality);
            }
            else // FlateDecode → convertir en JPEG
            {
                var magickFormat = ResolveMagickFormat(stream.Get(PdfName.ColorSpace));
                if (magickFormat == null) continue;

                try
                {
                    byte[] rawPixels = stream.GetBytes(true); // pixels décodés (predictor appliqué)
                    var settings = new MagickReadSettings
                    {
                        Format = magickFormat.Value,
                        Width = (uint)width,
                        Height = (uint)height,
                        Depth = 8
                    };
                    using var img = new MagickImage(rawPixels, settings);
                    img.Quality = (uint)quality;
                    img.Strip();
                    using var ms = new MemoryStream();
                    img.Write(ms, MagickFormat.Jpeg);
                    var jpegBytes = ms.ToArray();

                    if (jpegBytes.Length >= storedBytes.Length) continue;

                    recompressed = jpegBytes;
                    var outCs = img.ColorSpace == ColorSpace.Gray ? PdfName.DeviceGray : PdfName.DeviceRGB;
                    stream.Put(PdfName.ColorSpace, outCs);
                    stream.Remove(PdfName.DecodeParms);
                }
                catch { continue; }
            }

            if (recompressed == null || recompressed.Length >= storedBytes.Length) continue;

            stream.SetData(recompressed, false);
            stream.Put(PdfName.Filter, PdfName.DCTDecode);
            stream.Put(PdfName.BitsPerComponent, new PdfNumber(8));
        }
    }

    private static MagickFormat? ResolveMagickFormat(PdfObject? colorSpace)
    {
        if (colorSpace is PdfName csName)
        {
            if (PdfName.DeviceRGB.Equals(csName)) return MagickFormat.Rgb;
            if (PdfName.DeviceGray.Equals(csName)) return MagickFormat.Gray;
            if (PdfName.DeviceCMYK.Equals(csName)) return null; // CMYK → skip (complexité colorimétrique)
        }
        if (colorSpace is PdfArray csArr && csArr.Size() >= 1)
        {
            var first = csArr.Get(0);
            if (PdfName.ICCBased.Equals(first) && csArr.Size() >= 2)
            {
                var n = csArr.GetAsStream(1)?.GetAsNumber(PdfName.N)?.IntValue();
                return n switch { 1 => MagickFormat.Gray, 3 => MagickFormat.Rgb, _ => null };
            }
            if (PdfName.CalRGB.Equals(first)) return MagickFormat.Rgb;
            if (PdfName.CalGray.Equals(first)) return MagickFormat.Gray;
        }
        return MagickFormat.Rgb; // fallback raisonnable
    }

    private static byte[]? RecompressJpeg(byte[] jpegBytes, int quality)
    {
        try
        {
            using var image = new MagickImage(jpegBytes);
            image.Quality = (uint)quality;
            image.Strip();
            using var ms = new MemoryStream();
            image.Write(ms, MagickFormat.Jpeg);
            return ms.ToArray();
        }
        catch { return null; }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<string>> RotateAsync(
        string inputPath,
        string outputPath,
        int degrees,
        IReadOnlyList<int>? pageNumbers = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!new[] { 90, 180, 270 }.Contains(degrees))
            return OperationResult<string>.Failure("L'angle de rotation doit être 90, 180 ou 270 degrés.");

        if (!File.Exists(inputPath))
            return OperationResult<string>.Failure($"Fichier introuvable : {inputPath}");

        var outputDir = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            return OperationResult<string>.Failure($"Dossier de sortie inaccessible : {outputDir}");

        try
        {
            return await Task.Run(() =>
            {
                using var reader = new PdfReader(inputPath);
                using var writer = new PdfWriter(outputPath);
                using var doc = new PdfDocument(reader, writer);

                var rotateSet = pageNumbers?.ToHashSet() ?? new HashSet<int>();
                int total = doc.GetNumberOfPages();

                for (int i = 1; i <= total; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (rotateSet.Count == 0 || rotateSet.Contains(i))
                    {
                        var page = doc.GetPage(i);
                        page.SetRotation((page.GetRotation() + degrees) % 360);
                    }
                    progress?.Report(i * 100 / total);
                }

                return OperationResult<string>.Success(outputPath);
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure("Opération annulée.");
        }
        catch (Exception ex)
        {
            TryDeleteFile(outputPath);
            return OperationResult<string>.Failure($"Erreur lors de la rotation : {ex.Message}");
        }
    }

    private static List<(int From, int To)> ParsePageRanges(string? pageRanges, int totalPages)
    {
        if (pageRanges is null)
            return Enumerable.Range(1, totalPages).Select(p => (p, p)).ToList();

        var ranges = new List<(int, int)>();
        foreach (var segment in pageRanges.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = segment.Trim();
            var dashIndex = trimmed.IndexOf('-');
            if (dashIndex > 0)
            {
                var from = int.Parse(trimmed[..dashIndex]);
                var to = int.Parse(trimmed[(dashIndex + 1)..]);
                ranges.Add((from, to));
            }
            else
            {
                var page = int.Parse(trimmed);
                ranges.Add((page, page));
            }
        }
        return ranges;
    }

    private static void TryDeleteFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* best-effort */ }
    }
}
