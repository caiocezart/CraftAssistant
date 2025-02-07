using System.IO.Compression;
using LibBundle3.Records;
using LibBundledGGPK3;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace ExileCore2TexturesHandler;

public static class TexturesHandler
{
    public static bool TexturesArchiveAvailable(string archiveZipPath)
    {
        return File.Exists(archiveZipPath);
    }

    public static void ExtractTexturesToArchive(string contentPath, string nodePath, string archiveZipPath)
    {
        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tempExtractionPath = Path.Combine(currentDirectory, "temp");
        EnsureDirectoryExists(tempExtractionPath);

        // Extract and convert textures
        ExtractContent(contentPath, nodePath, tempExtractionPath);

        // Create the final archive        
        CreateTextureArchive(tempExtractionPath, archiveZipPath);

        // Delete the temporary extraction path
        Directory.Delete(tempExtractionPath, true);
    }

    public static Image<Rgba32> LoadImageFromArchive(string archiveZipPath, string imagePath)
    {
        string normalizedSearchPath = imagePath.Trim()
            .ToLowerInvariant()
            .Replace(".dds", ".png")
            .Replace("/", "\\");

        // Open the zip archive
        using (var zipArchive = ZipFile.OpenRead(archiveZipPath))
        {
            var entry = zipArchive.GetEntry(normalizedSearchPath);
            if (entry != null)
            {
                using var stream = entry.Open();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);

                memoryStream.Position = 0;
                return Image.Load<Rgba32>(memoryStream);
            }
        }

        return null;
    }

    private static void ExtractContent(string contentPath, string nodePath, string tempExtractionPath)
    {
        LibBundle3.Index? index = null;
        if (contentPath.EndsWith(".ggpk"))
        {
            var ggpk = new BundledGGPK(contentPath, false);
            index = ggpk.Index;
            index.ParsePaths();
        }
        else if (contentPath.EndsWith(".index.bin"))
        {
            index = new LibBundle3.Index(contentPath, false);// false to parsePaths manually
            index.ParsePaths();
        }
        else
        {
            throw new ArgumentException("Invalid content path", nameof(contentPath));
        }

        foreach (var file in index.Files)
        {
            try
            {
                ProcessFile(file.Value, nodePath, tempExtractionPath);
            }
            catch
            {
                // fail silently
            }

        }
    }

    private static void ProcessFile(FileRecord file, string nodePath, string tempExtractionPath)
    {
        var filePath = file.Path;
        if (!filePath.Contains(nodePath)) return;

        if (Path.GetExtension(filePath) != ".dds") return;

        var outputFilePath = Path.Combine(tempExtractionPath,
            filePath.Replace(".dds", ".png").Replace("/", "\\"));

        var directoryName = Path.GetDirectoryName(outputFilePath);
        if (directoryName == null) return;

        // each file has its own path, so we need to ensure the directory exists
        Directory.CreateDirectory(directoryName);

        ConvertDDStoPNG(file, outputFilePath);
    }

    private static void ConvertDDStoPNG(FileRecord file, string outputPath)
    {
        unsafe
        {
            byte[] ddsBytes = file.Read().ToArray();

            fixed (byte* ptr = ddsBytes)
            {
                using var stream = new UnmanagedMemoryStream(ptr, ddsBytes.Length);
                using var image = Dds.Create(stream, new PfimConfig());
                using var img = Image.LoadPixelData<Rgba32>(
                    image.Data,
                    image.Width,
                    image.Height);

                img.SaveAsync(outputPath);
            }
        }
    }

    private static void CreateTextureArchive(string tempExtractionPath, string archiveZipPath)
    {
        using var archive = ZipFile.Open(archiveZipPath, ZipArchiveMode.Create);
        foreach (var file in Directory.GetFiles(tempExtractionPath, "*.png", SearchOption.AllDirectories))
        {
            try
            {
                var relativePath = Path.GetRelativePath(tempExtractionPath, file);
                archive.CreateEntryFromFile(file, relativePath);
            }
            catch
            {
                // fail silently
            }
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
}