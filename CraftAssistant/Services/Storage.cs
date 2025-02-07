using System.Text.Json;
using System.Text.Json.Serialization;
using Logger = ExileCore2CustomLogger.Logger;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO.Compression;
using Image = SixLabors.ImageSharp.Image;

namespace CraftAssistant;

public class Storage
{
    private readonly Logger _logger;
    private readonly string _itemsDirectory;
    private readonly string _poeDataJsonPath;

    public Storage(Logger logger, string dataPath, string poeDataJsonFile = "poe.json")
    {
        _logger = logger;
        _itemsDirectory = Path.Combine(dataPath, "items");
        _poeDataJsonPath = Path.Combine(dataPath, poeDataJsonFile);
        
        EnsureDirectoryExists(_itemsDirectory);
        _logger.Debug($"Using POE data from {_poeDataJsonPath}");
    }

    public bool IsPoeDataAvailable()
    {
        return File.Exists(_poeDataJsonPath);
    }

    public List<string> GetItemsSaved()
    {
        _logger.Debug($"Loading items saved");

        var itemsSaved = new List<string>();
        foreach (var file in Directory.GetFiles(_itemsDirectory))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            itemsSaved.Add(fileName);
        }

        return itemsSaved;
    }

    public void SaveItem(Item item)
    {
        try
        {
            var storedItem = ItemMapper.ToStoredItem(item);
            var filePath = GetItemPath(item.FileName);
            _logger.Debug($"Saving item {filePath}");

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(storedItem, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);

            _logger.Info($"Saved item {item.FileName}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error saving item: {ex.Message}");
        }
    }

    public StoredItem LoadItem(string itemToLoad)
    {
        var itemFilePath = Path.Combine(_itemsDirectory, $"{itemToLoad}.json");
        _logger.Debug($"Loading item {itemFilePath}");

        var json = File.ReadAllText(Path.Combine(itemFilePath));
        var item = Newtonsoft.Json.JsonConvert.DeserializeObject<StoredItem>(json);

        _logger.Debug($"Loaded item {item.FileName}");

        return item;
    }

    public void DeleteItem(string fileName)
    {
        try
        {
            var filePath = GetItemPath(fileName);
            _logger.Debug($"Deleting item {filePath}");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.Info($"Deleted item {fileName}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error deleting item: {ex.Message}");
        }
    }

    public List<PoeDataBaseGroup> LoadPoeData()
    {
        _logger.Debug("Loading POE Data");
        string jsonString = File.ReadAllText(_poeDataJsonPath);

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        var poeData = JsonSerializer.Deserialize<PoeData>(jsonString, options);

        return poeData.BaseGroups;
    }

    private string GetItemPath(string fileName)
    {
        return Path.Combine(_itemsDirectory, $"{fileName}.json");
    }

    private void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public Image<Rgba32> LoadImageFromArchive(string archiveZipPath, string imagePath)
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

}

public class StoredItem : Item
{
    public DateTime StoredAt { get; set; }
}