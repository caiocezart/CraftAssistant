using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace CraftAssistant;

public class PoeData
{
    [JsonPropertyName("base_groups")]
    public List<PoeDataBaseGroup> BaseGroups { get; set; }
}

public class PoeDataBaseGroup
{
    [JsonPropertyName("base_group")]
    public string BaseGroup { get; set; }

    [JsonPropertyName("prefixes")]
    public List<PoeDataMod> Prefixes { get; set; }

    [JsonPropertyName("suffixes")]
    public List<PoeDataMod> Suffixes { get; set; }

    [JsonPropertyName("base_type")]
    public string BaseType { get; set; }

    [JsonPropertyName("bgroup")]
    public string ClassName { get; set; }
}

public class PoeDataMod
{
    [JsonPropertyName("tiers")]
    public List<PoeDataModTier> Tiers { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("mod_groups")]
    public List<string> ModGroups { get; set; }
    public string SplitDescription => string.Join("\n", Description.Split(','));
}

public class PoeDataModTier
{
    [JsonPropertyName("tier")]
    public string Name { get; set; }

    [JsonPropertyName("ilvl")]
    public int ItemLevel { get; set; }

    [JsonPropertyName("weighting")]
    public int Weighting { get; set; }

    [JsonPropertyName("values")]
    public List<PoeDataModValues> Values { get; set; }

    [JsonPropertyName("weight_percent")]
    public float WeightPercent { get; set; }

    [JsonPropertyName("affix_percent")]
    public float AffixPercent { get; set; }

    [JsonPropertyName("float")]
    public bool Float { get; set; }

    public string DescriptionWithValues(string description)
    {
        if (string.IsNullOrEmpty(description) || Values == null)
            return description;

        int valueIndex = 0;
        var parts = description.Split(',');
        var result = new List<string>();

        foreach (var part in parts)
        {
            var processed = part.Trim();
            if (valueIndex < Values.Count)
            {
                var value = Values[valueIndex++];
                var minValue = Float ? ((float)value.Min / 60).ToString("0.#") : value.Min.ToString();
                var maxValue = Float ? ((float)value.Max / 60).ToString("0.#") : value.Max.ToString();
                processed = processed.Replace("#", $"[{minValue}-{maxValue}]");
            }
            result.Add(processed);
        }

        return string.Join("\n", result);
    }
}

public class PoeDataModValues
{
    public int Min { get; set; }

    public int Max { get; set; }
}