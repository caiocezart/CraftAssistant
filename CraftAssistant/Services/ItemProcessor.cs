using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using Logger = ExileCore2CustomLogger.Logger;

namespace CraftAssistant;

public class ItemProcessor
{
    private readonly Logger _logger;
    private readonly GameController _gameController;

    public ItemProcessor(Logger logger, GameController gameController)

    {
        _logger = logger;
        _gameController = gameController;
    }

    public (Item item, StatusMessage status) ProcessEntity(State state, Entity entity)
    {
        try
        {
            _logger.Debug($"Processing entity: {entity.Metadata}");

            var item = new Item
            {
                GameData = new GameData(),
                Mods = new List<DisplayMod>()
            };

            item.Id = Guid.NewGuid();
            item.Metadata = entity.Metadata;


            // Process base item data
            if (!ProcessMetadataEntity(item, entity))
            {
                return (null, new StatusMessage("Invalid base item type", StatusCode.CraftingError));
            }

            // Process components
            if (!ProcessModsComponent(item, entity))
            {
                return (null, new StatusMessage("Invalid mods component", StatusCode.CraftingError));
            }

            ProcessAttributeRequirementsComponent(item, entity);
            ProcessQualityComponent(item, entity);
            ProcessRenderItemComponents(item, entity);
            
            item.FileName = item.Name;

            // Validate processed item
            var validationStatus = Helpers.ValidateItem(item);
            if (validationStatus.Code != StatusCode.Success)
            {
                _logger.Debug(validationStatus.Message);
                return (item, validationStatus);
            }

            // Find and set item base group from PoeData
            item.PoeDataBaseGroup = FindPoeDataItemGroup(
                item.BaseGroups(),
                state.PoeData
            );

            if (item.PoeDataBaseGroup == null)
            {
                return (item, new StatusMessage("No matching base group found", StatusCode.CraftingError));
            }

            CreateDisplayMods(item);

            return (item, new StatusMessage("Item processed successfully", StatusCode.Success));
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing entity: {ex.Message}");
            return (null, new StatusMessage($"Internal error: {ex.Message}", StatusCode.InternalError));
        }
    }

    private bool ProcessMetadataEntity(Item item, Entity entity)
    {
        if (!_gameController.Files.BaseItemTypes.Contents.TryGetValue(entity.Metadata, out var metadataEntity))
        {
            return false;
        }

        item.BaseName = metadataEntity.BaseName;
        item.ClassName = metadataEntity.ClassName;
        item.Height = metadataEntity.Height;
        item.Width = metadataEntity.Width;

        item.Tags = metadataEntity.Tags;
        item.MoreTagsFromPath = metadataEntity.MoreTagsFromPath;

        return true;
    }

    private bool ProcessModsComponent(Item item, Entity entity)
    {
        // Mods component
        if (!entity.TryGetComponent<Mods>(out var modsComponent))
        {
            return false;
        }

        item.Identified = modsComponent.Identified;
        item.IsMirrored = modsComponent.IsMirrored;
        item.ItemLevel = modsComponent.ItemLevel;
        item.ItemRarity = modsComponent.ItemRarity;
        item.RequiredLevel = modsComponent.RequiredLevel;
        item.UniqueName = modsComponent.UniqueName;

        var itemMods = new List<Mod>();

        try
        {
            itemMods = ProcessItemMods(itemMods, modsComponent.ExplicitMods);
            itemMods = ProcessItemMods(itemMods, modsComponent.ImplicitMods);
            itemMods = ProcessItemMods(itemMods, modsComponent.EnchantedMods);
            itemMods = ProcessItemMods(itemMods, modsComponent.CorruptionImplicitMods);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing item mods: {ex.Message}");
            return false;
        }

        item.GameData.Mods = itemMods;

        return true;
    }

    private List<Mod> ProcessItemMods(List<Mod> itemMods, List<ItemMod> mods)
    {
        foreach (var mod in mods)
        {
            _logger.Debug($"Processing item mod: {mod.Name}");
            var newMod = new Mod
            {
                Name = mod.Name.ToLower().Trim(),
                Group = mod.Group.ToLower().Trim(),
                Values = mod.Values,
                // ModRecord = new ModRecord
                // {
                //     AffixType = mod.ModRecord.AffixType,
                //     Groups = mod.ModRecord.Groups,

                //     Key = mod.ModRecord.Key,
                //     Group = mod.ModRecord.Group
                // },
                Translation = mod.Translation,
                AffixType = mod.ModRecord.AffixType

            };

            // newMod.ModGroups.AddRange(newMod.ModRecord.Groups);
            // newMod.ModGroups.Add(newMod.ModRecord.Key);
            // newMod.ModGroups.Add(newMod.Name);
            // newMod.ModGroups.Add(newMod.ModRecord.Group);
            
            itemMods.Add(newMod);
        }

        return itemMods;
    }

    private void ProcessAttributeRequirementsComponent(Item item, Entity entity)
    {
        if (!entity.TryGetComponent<AttributeRequirements>(out var attrRequirements))
        {
            item.Requirements = new Requirements();
        }
        else
        {
            item.Requirements = new Requirements
            {
                Dexterity = attrRequirements.dexterity,
                Intelligence = attrRequirements.intelligence,
                Strength = attrRequirements.strength
            };
        }
    }

    private void ProcessQualityComponent(Item item, Entity entity)
    {
        if (!entity.TryGetComponent<Quality>(out var qualityComponent))
        {
            item.Quality = 0;
        }
        else
        {
            item.Quality = qualityComponent.ItemQuality;
        }
    }

    private void ProcessRenderItemComponents(Item item, Entity entity)
    {
        if (!entity.TryGetComponent<RenderItem>(out var renderComponent))
        {
            item.ResourcePath = "";
        }
        else
        {
            item.ResourcePath = renderComponent.ResourcePath;
        }
    }

    private bool CreateDisplayMods(Item item)
    {
        try
        {
            foreach (var mod in item.GameData.Mods)
            {
                var displayMod = new DisplayMod
                {
                    AffixType = mod.AffixType,
                    Values = mod.Values
                };

                displayMod = FindPoeDataAffixroup(mod, item.PoeDataBaseGroup, displayMod);
                displayMod.Tier = FindMatchingTier(displayMod);
                displayMod.Float = displayMod.BaseData?.Tiers[0]?.Float ?? false;

                if (displayMod.Tier != null) item.Mods.Add(displayMod);
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error creating display mods: {ex.Message}");
            return false;
        }

        return true;
    }

    public PoeDataBaseGroup FindPoeDataItemGroup(List<string> itemBaseGroups, List<PoeDataBaseGroup> poeDataItemBaseGroups)
    {
        foreach (var itemBaseGroup in itemBaseGroups)
        {

            foreach (var poeDataItemBaseGroup in poeDataItemBaseGroups)
            {
                if (poeDataItemBaseGroup.BaseGroup.ToLower().Contains(itemBaseGroup))
                {
                    return poeDataItemBaseGroup;
                }
            }
        }

        return null;
    }

    private DisplayMod FindPoeDataAffixroup(Mod mod, PoeDataBaseGroup itemBaseGroup, DisplayMod displayMod)
    {
        _logger.Debug($"Finding PoeDataAffixroup for {mod.Translation}");
        _logger.Debug($"ModGroups: {string.Join(", ", mod.ModGroups)}");
        var poeDataAffixes = itemBaseGroup.Prefixes.Concat(itemBaseGroup.Suffixes);

        var found = false;
        foreach (var itemAffixGroup in mod.ModGroups)
        {
            if(found) break;

            foreach (var poeDataAffix in poeDataAffixes)
            {
                if(found) break;

                _logger.Debug($"Checking {itemAffixGroup} against {string.Join(", ", poeDataAffix.Description)}");
                foreach (var poeDataAffixGroup in poeDataAffix.ModGroups)
                {                  
                    if(found) break;

                    _logger.Debug($"Checking {itemAffixGroup} against {poeDataAffixGroup}");
                    _logger.Debug($"Checking {itemAffixGroup} against local{poeDataAffixGroup}");
                    if (itemAffixGroup == poeDataAffixGroup || itemAffixGroup == $"local{poeDataAffixGroup}")
                    {
                        displayMod.Description = poeDataAffix.Description ?? "";
                        displayMod.BaseData = poeDataAffix ?? new PoeDataMod();
                        _logger.Debug($"Found {itemAffixGroup} matches with {poeDataAffixGroup}");
                        found = true;
                    }
                }
            }
        }

        return displayMod;
    }

    private PoeDataModTier FindMatchingTier(DisplayMod displayMod)
    {
        try
        {
            var tiers = displayMod.BaseData?.Tiers ?? [];
            foreach (var tier in tiers)
            {
                _logger.Debug($"Checking tier {tier.Values[0].Min} - {tier.Values[0].Max}");
                if (displayMod.Values.Count == 1)
                {
                    if (Helpers.IsWithinRange(displayMod.Values[0], tier.Values[0].Min, tier.Values[0].Max))
                    {
                        return tier;
                    }

                }

                if (Helpers.IsWithinRange(displayMod.Values[0], tier.Values[0].Min, tier.Values[0].Max)
                    && Helpers.IsWithinRange(displayMod.Values[1], tier.Values[1].Min, tier.Values[1].Max))
                {
                    return tier;
                }

            }

            var defaultTier = tiers[tiers.Count - 1];

            return defaultTier;
        }
        catch
        {
            _logger.Debug("Error finding matching tier");
            return null;
        }
    }
}