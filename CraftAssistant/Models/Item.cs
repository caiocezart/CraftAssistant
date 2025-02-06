using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;

namespace CraftAssistant;

public class Item
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string Metadata { get; set; }
    public string BaseName { get; set; }
    public string ClassName { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public bool Identified { get; set; }
    public bool IsMirrored { get; set; }
    public int ItemLevel { get; set; }
    public ItemRarity ItemRarity { get; set; }
    public int RequiredLevel { get; set; }
    public string UniqueName { get; set; }
    public IntPtr TextureId { get; set; }
    public IEnumerable<string> Tags { get; set; }
    public IEnumerable<string> MoreTagsFromPath { get; set; }
    public Requirements Requirements { get; set; }
    public int Quality { get; set; }
    public string ResourcePath { get; set; }
    public List<DisplayMod> Mods { get; set; }

    [JsonIgnore]
    public string Name => $"{UniqueName} {BaseName}";
    [JsonIgnore]
    public PoeDataBaseGroup PoeDataBaseGroup { get; set; }
    [JsonIgnore]
    public GameData GameData { get; set; }
    
    public string ItemRequirements()
    {
        var requirements = new List<string>();

        if (RequiredLevel > 0)
            requirements.Add($"Level {RequiredLevel}");

        if (Requirements != null)
        {
            if (Requirements.Dexterity > 0)
                requirements.Add($"{Requirements.Dexterity} Dex");
            if (Requirements.Intelligence > 0)
                requirements.Add($"{Requirements.Intelligence} Int");
            if (Requirements.Strength > 0)
                requirements.Add($"{Requirements.Strength} Str");
        }

        return requirements.Any() ? string.Join(", ", requirements) : "";
    }
    public List<string> BaseGroups()
    {
        var baseGroups = new List<string>();
        var itemClassNameNormalized = ClassName.Replace(" ", "_").ToLower();

        // Handle plural to singular conversion for item class names
        if (itemClassNameNormalized.EndsWith("s"))
        {
            itemClassNameNormalized = itemClassNameNormalized[..^1];
        }

        if (Tags.Any()) baseGroups.Add($"{itemClassNameNormalized}_{Tags.First()}");

        baseGroups.Add(ClassName.ToLower());

        return baseGroups;
    }
}

public class GameData
{

    public List<Mod> Mods { get; set; }

    public RenderItem RenderItem { get; set; }
}

public class Requirements
{
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Strength { get; set; }
}

public class Mod
{
    public string Name { get; set; }
    public string Group { get; set; }
    public List<int> Values { get; set; }
    public ModRecord ModRecord { get; set; }
    public List<string> ModGroups => new List<string> { Name, Group };
    public string Translation { get; set; }
    public ModType AffixType { get; set; }
}

public class ModRecord
{
    public ModType AffixType { get; set; }
    public List<string> Groups { get; set; }
    public string Key { get; set; }
    public string Group { get; set; }
}