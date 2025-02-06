using ExileCore2.Shared.Enums;
using Logger = ExileCore2CustomLogger.Logger;

namespace CraftAssistant;
public class CraftingService(Logger logger)
{
    private readonly Logger _logger = logger;


    public List<DisplayMod> GetAvailablePrefixes(Item item)
    {
        _logger.Debug($"Getting available prefixes for {item.Name}");

        return item.Mods
            .Where(p => !item.Mods.Any(m =>
                m.AffixType == ModType.Prefix &&
                m.Description == p.Description))
            .ToList();
    }

    public List<DisplayMod> GetAvailableSuffixes(Item item)
    {
        _logger.Debug($"Getting available suffixes for {item.Name}");

        return item.Mods
            .Where(p => !item.Mods.Any(m =>
                m.AffixType == ModType.Suffix &&
                m.Description == p.Description))
            .ToList();
    }

    public void Craft(Item item)
    {
        // TODO: Implement crafting logic
        _logger.Info($"Crafting item: {item.Name}");
    }
}