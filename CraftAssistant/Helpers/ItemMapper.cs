using System;
using System.Collections.Generic;
using System.Linq;
using CraftAssistant;

namespace CraftAssistant;
public static class ItemMapper
{
    public static Item ToItem(StoredItem storedItem)
    {
        if (storedItem == null) return null;

        return new StoredItem
        {
            Id = storedItem.Id,
            FileName = storedItem.FileName,
            Metadata = storedItem.Metadata,
            BaseName = storedItem.BaseName,
            ClassName = storedItem.ClassName,
            Height = storedItem.Height,
            Width = storedItem.Width,
            Identified = storedItem.Identified,
            IsMirrored = storedItem.IsMirrored,
            ItemLevel = storedItem.ItemLevel,
            ItemRarity = storedItem.ItemRarity,
            RequiredLevel = storedItem.RequiredLevel,
            UniqueName = storedItem.UniqueName,
            TextureId = storedItem.TextureId,
            Tags = storedItem.Tags,
            MoreTagsFromPath = storedItem.MoreTagsFromPath,
            Requirements = storedItem.Requirements,
            Quality = storedItem.Quality,
            ResourcePath = storedItem.ResourcePath,            
            Mods = storedItem.Mods,
            StoredAt = DateTime.UtcNow
        };
    }

    public static StoredItem ToStoredItem(Item item)
    {
        if (item == null) return null;

        return new StoredItem
        {
            Id = item.Id,
            FileName = item.FileName,
            Metadata = item.Metadata,
            BaseName = item.BaseName,
            ClassName = item.ClassName,
            Height = item.Height,
            Width = item.Width,
            Identified = item.Identified,
            IsMirrored = item.IsMirrored,
            ItemLevel = item.ItemLevel,
            ItemRarity = item.ItemRarity,
            RequiredLevel = item.RequiredLevel,
            UniqueName = item.UniqueName,
            TextureId = item.TextureId,
            Tags = item.Tags,
            MoreTagsFromPath = item.MoreTagsFromPath,
            Requirements = item.Requirements,
            Quality = item.Quality,
            ResourcePath = item.ResourcePath,
            Mods = item.Mods,
            StoredAt = DateTime.UtcNow
        };
    }

    public static Item CloneItem(Item item)
    {
        if (item == null) return null;

        var clonedItem = new Item
        {
            Id = Guid.NewGuid(),
            FileName = item.FileName,
            Metadata = item.Metadata,
            BaseName = item.BaseName,
            ClassName = item.ClassName,
            Height = item.Height,
            Width = item.Width,
            Identified = item.Identified,
            IsMirrored = item.IsMirrored,
            ItemLevel = item.ItemLevel,
            ItemRarity = item.ItemRarity,
            RequiredLevel = item.RequiredLevel,
            UniqueName = item.UniqueName,
            TextureId = item.TextureId,
            Tags = CloneTags(item.Tags),
            MoreTagsFromPath = CloneMoreTagsFromPath(item.MoreTagsFromPath),
            Requirements = CloneItemRequirements(item.Requirements),
            Quality = item.Quality,
            ResourcePath = item.ResourcePath,
            Mods = CloneItemMods(item.Mods),
            PoeDataBaseGroup = item.PoeDataBaseGroup,
            GameData = item.GameData
        };

        if (clonedItem.Mods != null)
        {
            foreach (var mod in clonedItem.Mods)
            {
                mod.BaseData = item.Mods.First(m => m.Description == mod.Description).BaseData;
            }
        }

        return clonedItem;
    }    

    private static List<DisplayMod> CloneItemMods(List<DisplayMod> sourceMods)
    {
        return sourceMods?.Select(mod => new DisplayMod
        {
            AffixType = mod.AffixType,
            Values = mod.Values,
            Description = mod.Description,
            Tier = mod.Tier,
            Float = mod.Float,
            BaseData = mod.BaseData,
        }).ToList() ?? new List<DisplayMod>();
    }  

    private static Requirements CloneItemRequirements(Requirements sourceRequirements)
    {
        if (sourceRequirements == null) return new Requirements();
        return new Requirements
        {
            Dexterity = sourceRequirements.Dexterity,
            Intelligence = sourceRequirements.Intelligence,
            Strength = sourceRequirements.Strength
        };
    }

    private static IEnumerable<string> CloneTags(IEnumerable<string> sourceTags)
    {
        return sourceTags?.ToList() ?? new List<string>();
    }    

    private static IEnumerable<string> CloneMoreTagsFromPath(IEnumerable<string> sourceMoreTagsFromPath)
    {
        return sourceMoreTagsFromPath?.ToList() ?? new List<string>();
    }
}